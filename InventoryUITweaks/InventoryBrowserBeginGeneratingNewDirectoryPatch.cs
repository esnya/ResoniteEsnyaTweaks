using FrooxEngine;
using FrooxEngine.UIX;
using HarmonyLib;

namespace EsnyaTweaks.InventoryUITweaks;

[HarmonyPatch]
internal static class InventoryBrowserBeginGeneratingNewDirectoryPatch
{
    internal static System.Reflection.MethodInfo TargetMethod()
    {
        return AccessTools.Method(
            typeof(InventoryBrowser),
            "BeginGeneratingNewDirectory",
            [typeof(RecordDirectory), typeof(GridLayout).MakeByRefType(), typeof(GridLayout).MakeByRefType(), typeof(SlideSwapRegion.Slide)]);
    }

    [HarmonyPostfix]
    internal static void Postfix(RecordDirectory directory, GridLayout folders)
    {
        if (directory != null || folders == null)
        {
            return;
        }

        foreach (var child in folders.Slot.Children)
        {
            foreach (var relay in child.GetComponents<ButtonRelayBase>())
            {
                relay.DoublePressDelay.Value = 0f;
            }
        }
    }
}
