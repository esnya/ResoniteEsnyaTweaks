using System.Diagnostics.CodeAnalysis;
using FrooxEngine;
using HarmonyLib;

namespace EsnyaTweaks.InventoryUITweaks;

[HarmonyPatch(typeof(BrowserItem), nameof(BrowserItem.Pressed))]
internal static class BrowserItemPressedPatch
{
    [SuppressMessage("Style", "SA1313", Justification = "Harmony magic parameter")]
    internal static bool Prefix(BrowserItem __instance)
    {
        if (__instance is InventoryItemUI inventoryItem)
        {
            var directoryField = ReflectionCache.InventoryItemUIDirectory;
            if (directoryField?.GetValue(inventoryItem) != null)
            {
                inventoryItem.Browser.Target.SelectedItem.Target = null!;
                inventoryItem.Open();
                return false;
            }
        }

        return true;
    }
}
