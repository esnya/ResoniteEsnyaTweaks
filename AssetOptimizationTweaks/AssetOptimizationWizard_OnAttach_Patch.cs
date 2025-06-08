using FrooxEngine;
using FrooxEngine.UIX;
using HarmonyLib;
using ResoniteModLoader;

namespace EsnyaTweaks.AssetOptimizationTweaks;

[HarmonyPatch(typeof(AssetOptimizationWizard), "OnAttach")]
internal static class AssetOptimizationWizard_OnAttach_Patch
{
    public static void Postfix(AssetOptimizationWizard __instance)
    {
        var uiBuilder = new UIBuilder(__instance.Slot);
        var message = __instance.TryGetField<Text>("_message")?.Value;

        if (message is null)
        {
            ResoniteMod.Warn("AssetOptimizationWizard message field not found.");
            return;
        }

        uiBuilder.ForceNext = message.RectTransform;

        uiBuilder.LocalButton(
            "[Mod] Deduplicate Procedural Assets",
            (_, _) =>
            {
                var removedCount = __instance.Slot.DeduplicateProceduralAssets();
                message.Content.Value = $"Deduplicated {removedCount} procedural assets.";
                ResoniteMod.DebugFunc(() =>
                    $"AssetOptimizationWizard: Deduplicated {removedCount} procedural assets"
                );
            }
        );
    }
}
