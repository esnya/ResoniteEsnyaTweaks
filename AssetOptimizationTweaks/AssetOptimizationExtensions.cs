using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Runtime.CompilerServices;
using Elements.Core;
using FrooxEngine;
using FrooxEngine.UIX;
using FrooxEngine.Undo;
using ResoniteModLoader;

namespace EsnyaTweaks.AssetOptimizationTweaks;

internal static class AssetOptimizationExtensions
{
    private static readonly ConcurrentDictionary<Type, bool> _proceduralAssetProviderCache = new();

    public static int DeduplicateProceduralAssets(this Slot root, Slot? replaceRoot = null)
    {
        var num = 0;
        var dictionary = Pool.BorrowDictionary<IWorldElement, IWorldElement>();
        var allProviders = Pool.BorrowList<IAssetProvider>();

        root.GetComponentsInChildren(allProviders);
        ResoniteMod.DebugFunc(() => $"{allProviders.Count} asset providers found under {root}");

        var dictionary2 = Pool.BorrowDictionaryList<Type, IAssetProvider>();
        foreach (var provider in allProviders)
        {
            if (IsProceduralAssetProvider(provider) && IsNotDriven(provider) && provider.Slot.AllowOptimization())
            {
                dictionary2.Add(provider.GetType(), provider);
            }
        }

        if (ResoniteMod.IsDebugEnabled())
        {
            foreach (var item in dictionary2)
            {
                ResoniteMod.Debug(
                    $"Found {item.Value.Count} procedural asset providers of type {item.Key.Name}"
                );
            }
        }

        Pool.Return(ref allProviders);

        var undoManager = root.World.GetUndoManager();
        undoManager?.BeginBatch("[Mod] Deduplicate Procedural Asset Providers");
        foreach (var item2 in dictionary2)
        {
            for (var i = 0; i < item2.Value.Count; i++)
            {
                var component = (Component)item2.Value[i];
                for (var num2 = item2.Value.Count - 1; num2 > i; num2--)
                {
                    var component2 = (Component)item2.Value[num2];
                    if (component.PublicMembersEqual(component2))
                    {
                        ResoniteMod.DebugFunc(() =>
                            $"Deduplicating procedural asset provider {component2} in favor of {component}"
                        );
                        foreach (var (e2, e1) in component2.SyncMembers.Zip(component.SyncMembers, (e2, e1) => (e2, e1)))
                        {
                            if (e2 == null || e1 == null)
                            {
                                continue;
                            }

                            ResoniteMod.DebugFunc(() =>
                                $"Redirecting reference from {e2} to {e1}"
                            );
                            dictionary.Add(e2, e1);
                        }
                        dictionary.Add(component2, component);
                        item2.Value.RemoveAt(num2);
                        num++;
                    }
                }
            }
        }

        Pool.Return(ref dictionary2);

        if (ResoniteMod.IsDebugEnabled())
        {
            ResoniteMod.DebugFunc(() => $"{num} procedural asset providers deduplicated");
        }
        root.World.ReplaceReferenceTargets(
            dictionary,
            nullIfIncompatible: false,
            replaceRoot ?? root.World.RootSlot
        );

        foreach (var item3 in dictionary)
        {
            if (item3.Key is Component component3)
            {
                ResoniteMod.DebugFunc(() =>
                    $"Redirecting references from {item3.Key} to {item3.Value} in {component3.Slot}"
                );

                component3?.UndoableDestroy();
            }
        }
        undoManager?.EndBatch();

        Pool.Return(ref dictionary);

        ResoniteMod.Msg($"{num} procedural asset providers deduplicated");
        return num;
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
        // より効率的なアプローチ：基底型の階層を辿る
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

    public static Button LocalButton(
        this UIBuilder ui,
        string label,
        ButtonEventHandler localAction
    )
    {
        var button = ui.Button(label);
        button.LocalPressed += localAction;
        return button;
    }
}
