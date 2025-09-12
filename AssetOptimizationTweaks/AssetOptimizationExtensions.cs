using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Elements.Core;
using FrooxEngine;
using ResoniteModLoader;

namespace EsnyaTweaks.AssetOptimizationTweaks;

internal static class AssetOptimizationExtensions
{
    private static readonly ConcurrentDictionary<Type, bool> _proceduralAssetProviderCache = new();

    public static int DeduplicateProceduralAssets(this Slot root, Slot? replaceRoot = null)
    {
        var allProviders = Pool.BorrowList<IAssetProvider>();
        var groupedProviders = new Dictionary<Type, List<IAssetProvider>>();
        var redirectionMap = new Dictionary<IWorldElement, IWorldElement>();

        root.GetComponentsInChildren(allProviders);
        ResoniteMod.DebugFunc(() => $"{allProviders.Count} asset providers found under {root}");

        try
        {
            GroupProceduralAssetProviders(allProviders, groupedProviders);
            LogGroupedProviders(groupedProviders);

            BuildRedirectionMap(groupedProviders, redirectionMap);
            var deduplicatedCount = redirectionMap.Count(kvp => kvp.Key is Component);

            if (redirectionMap.Count > 0)
            {
                ApplyRedirections(root, redirectionMap, replaceRoot);
            }

            ResoniteMod.Msg($"{deduplicatedCount} procedural asset providers deduplicated");
            return deduplicatedCount;
        }
        finally
        {
            Pool.Return(ref redirectionMap);
            Pool.Return(ref groupedProviders);
            Pool.Return(ref allProviders);
        }
    }

    internal static void GroupProceduralAssetProviders(
        List<IAssetProvider> providers,
        Dictionary<Type, List<IAssetProvider>> grouped
    )
    {
        foreach (var provider in providers)
        {
            if (
                IsProceduralAssetProvider(provider)
                && IsNotDriven(provider)
                && provider.Slot.AllowOptimization()
            )
            {
                if (!grouped.TryGetValue(provider.GetType(), out var list))
                {
                    list = [];
                    grouped[provider.GetType()] = list;
                }
                list.Add(provider);
            }
        }
    }

    internal static void BuildRedirectionMap(
        Dictionary<Type, List<IAssetProvider>> groupedProviders,
        Dictionary<IWorldElement, IWorldElement> redirectionMap
    )
    {
        foreach (var providerGroup in groupedProviders)
        {
            var duplicatePairs = FindDuplicatePairs(providerGroup.Value);

            foreach (var (original, duplicate) in duplicatePairs)
            {
                AddSyncMemberRedirections(redirectionMap, duplicate, original);
                redirectionMap.TryAdd(duplicate, original);
            }
        }
    }

    internal static (Component Original, Component Duplicate)[] FindDuplicatePairs(
        List<IAssetProvider> providers
    )
    {
        var pairs = Pool.BorrowList<(Component, Component)>();

        for (var i = 0; i < providers.Count; i++)
        {
            var original = (Component)providers[i];

            for (var j = i + 1; j < providers.Count; j++)
            {
                var candidate = (Component)providers[j];

                if (original.PublicMembersEqual(candidate))
                {
                    ResoniteMod.DebugFunc(() => $"Found duplicate: {candidate} matches {original}");
                    pairs.Add((original, candidate));
                }
            }
        }

        var result = pairs.ToArray();
        Pool.Return(ref pairs);
        return result;
    }

    internal static void AddSyncMemberRedirections(
        Dictionary<IWorldElement, IWorldElement> redirectionMap,
        Component duplicate,
        Component original
    )
    {
        foreach (
            var (duplicateMember, originalMember) in duplicate.SyncMembers.Zip(
                original.SyncMembers,
                (d, o) => (d, o)
            )
        )
        {
            if (duplicateMember != null && originalMember != null)
            {
                ResoniteMod.DebugFunc(() =>
                    $"Redirecting reference from {duplicateMember} to {originalMember}"
                );
                if (!redirectionMap.TryAdd(duplicateMember, originalMember)
                    && redirectionMap[duplicateMember] != originalMember)
                {
                    ResoniteMod.DebugFunc(() =>
                        $"Duplicate redirection detected for {duplicateMember}; existing target {redirectionMap[duplicateMember]} kept over {originalMember}"
                    );
                }
            }
        }
    }

    private static void LogGroupedProviders(Dictionary<Type, List<IAssetProvider>> groupedProviders)
    {
        if (ResoniteMod.IsDebugEnabled())
        {
            foreach (var group in groupedProviders)
            {
                ResoniteMod.Debug(
                    $"Found {group.Value.Count} procedural asset providers of type {group.Key.Name}"
                );
            }
        }
    }

    private static void ApplyRedirections(
        Slot root,
        Dictionary<IWorldElement, IWorldElement> redirectionMap,
        Slot? replaceRoot
    )
    {
        if (ResoniteMod.IsDebugEnabled())
        {
            var componentCount = redirectionMap.Count(kvp => kvp.Key is Component);
            ResoniteMod.DebugFunc(() =>
                $"{componentCount} procedural asset providers will be deduplicated"
            );
        }

        root.World.ReplaceReferenceTargets(
            redirectionMap,
            nullIfIncompatible: false,
            replaceRoot ?? root.World.RootSlot
        );

        foreach (var kvp in redirectionMap)
        {
            if (kvp.Key is Component component)
            {
                ResoniteMod.DebugFunc(() =>
                    $"Destroying duplicate component {kvp.Key} (redirected to {kvp.Value}) in {component.Slot}"
                );
                component.Destroy();
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsNotDriven(IAssetProvider provider)
    {
        return (provider as Component)?.SyncMembers.All(e => !e.IsDriven) ?? false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsProceduralAssetProvider(IAssetProvider provider)
    {
        var type = provider.GetType();

        return _proceduralAssetProviderCache.GetOrAdd(
            type,
            static t => CheckInheritanceHierarchy(t)
        );
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static bool CheckInheritanceHierarchy(Type type)
    {
        var currentType = type;
        while (currentType != null && currentType != typeof(object))
        {
            if (currentType.IsGenericType)
            {
                var genericTypeDef = currentType.GetGenericTypeDefinition();
                var typeName = genericTypeDef.Name;

                if (typeName.Equals("ProceduralAssetProvider`1", StringComparison.Ordinal))
                {
                    return true;
                }
            }
            currentType = currentType.BaseType;
        }
        return false;
    }

}
