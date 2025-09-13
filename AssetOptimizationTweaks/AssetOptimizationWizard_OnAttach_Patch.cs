using System.Linq;
using FrooxEngine;
using FrooxEngine.UIX;
using HarmonyLib;
using ResoniteModLoader;
using EsnyaTweaks.Common.UIX;

namespace EsnyaTweaks.AssetOptimizationTweaks;

[HarmonyPatch(typeof(AssetOptimizationWizard), "OnAttach")]
internal static class AssetOptimizationWizard_OnAttach_Patch
{
    public static void Postfix(AssetOptimizationWizard __instance)
    {
        var lastElement = __instance
            .Slot.GetComponentsInChildren<RectTransform>()
            .LastOrDefault()
            ?.Slot?.Parent;
        if (lastElement is null)
        {
            ResoniteMod.Warn("Failed to find the last element in AssetOptimizationWizard.");
            return;
        }

        var uiBuilder = new UIBuilder(lastElement);
        RadiantUI_Constants.SetupEditorStyle(uiBuilder);
        uiBuilder.Style.MinHeight = 24;
        uiBuilder.Style.PreferredHeight = 24;
        uiBuilder.LocalButton(
            "[Mod] Deduplicate Procedural Assets",
            (_, _) =>
            {
                var root = __instance.Root.Target;
                if (root is null)
                {
                    ResoniteMod.Warn("AssetOptimizationWizard root is null.");
                    return;
                }

                var removedCount = root.DeduplicateProceduralAssets();
                ResoniteMod.DebugFunc(() =>
                    $"AssetOptimizationWizard: Deduplicated {removedCount} procedural assets"
                );
            }
        );
    }
}
