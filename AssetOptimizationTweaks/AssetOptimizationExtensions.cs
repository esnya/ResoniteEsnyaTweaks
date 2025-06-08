using System;
using System.Runtime.CompilerServices;
using Elements.Core;
using FrooxEngine;
using FrooxEngine.UIX;

namespace EsnyaTweaks.AssetOptimizationTweaks;

internal static class AssetOptimizationExtensions
{
    public static int DeduplicateProceduralAssets(this Slot root, Slot? replaceRoot = null)
    {
        var num = 0;
        var dictionary = Pool.BorrowDictionary<IWorldElement, IWorldElement>();
        var allProviders = Pool.BorrowList<IAssetProvider>();

        root.GetComponentsInChildren(allProviders);

        var dictionary2 = Pool.BorrowDictionaryList<Type, IAssetProvider>();
        foreach (var provider in allProviders)
        {
            if (IsProceduralAssetProvider(provider))
            {
                dictionary2.Add(provider.GetType(), provider);
            }
        }

        Pool.Return(ref allProviders);

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
                        dictionary.Add(component2, component);
                        item2.Value.RemoveAt(num2);
                        num++;
                    }
                }
            }
        }

        Pool.Return(ref dictionary2);
        root.World.ReplaceReferenceTargets(
            dictionary,
            nullIfIncompatible: false,
            replaceRoot ?? root.World.RootSlot
        );

        foreach (var item3 in dictionary)
        {
            if (((IAssetProvider)item3.Key).AssetReferenceCount <= 0)
            {
                ((Component)item3.Key).Destroy();
            }
        }

        Pool.Return(ref dictionary);
        return num;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsProceduralAssetProvider(IAssetProvider provider)
    {
        var type = provider.GetType();
        return type.IsGenericType
            && type.GetGenericTypeDefinition()
                .Name.StartsWith("ProceduralAssetProvider", StringComparison.Ordinal);
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
