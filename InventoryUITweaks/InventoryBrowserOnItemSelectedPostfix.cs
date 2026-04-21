using System.Diagnostics.CodeAnalysis;
using Elements.Core;
using FrooxEngine;
using FrooxEngine.UIX;
using HarmonyLib;

namespace EsnyaTweaks.InventoryUITweaks;

[HarmonyPatch(typeof(InventoryBrowser), methodName: "OnItemSelected")]
internal static class InventoryBrowserOnItemSelectedPostfix
{
    [HarmonyPostfix]
    internal static void Postfix(
        [SuppressMessage("Style", "SA1313", Justification = "Harmony magic parameter")] InventoryBrowser __instance,
        BrowserItem previousItem,
        BrowserItem currentItem)
    {
        _ = previousItem;

        var selected = currentItem as InventoryItemUI;
        var items = Pool.BorrowList<InventoryItemUI>();
        __instance.Slot.GetComponentsInChildren(items);
        var dirField = ReflectionCache.InventoryItemUIDirectory;
        foreach (var it in items)
        {
            var isDir = dirField != null && dirField.GetValue(it) != null;
            if (!isDir)
            {
                continue;
            }

            var helperSlot = it.Slot.FindChild("ET_SelectFolderButton");
            if (helperSlot == null)
            {
                continue;
            }

            var helper = helperSlot.GetComponent<Button>();
            if (helper == null)
            {
                continue;
            }

            var isSelected = ReferenceEquals(it, selected);
            helper.BaseColor.Value = isSelected ? InventoryBrowser.SELECTED_COLOR : RadiantUI_Constants.BUTTON_COLOR;
            var checkImg = helperSlot.FindChild("ET_SelectFolderCheck")?.GetComponent<Image>();
            if (checkImg != null)
            {
                checkImg.Tint.Value = isSelected ? colorX.White : ((colorX)checkImg.Tint).SetA(0f);
            }
        }

        Pool.Return(ref items);
    }
}
