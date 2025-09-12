using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Elements.Core;
using FrooxEngine;
using FrooxEngine.UIX;
using HarmonyLib;
using ResoniteModLoader;

namespace EsnyaTweaks.InventoryUITweaks;

internal static class ReflectionCache
{
    internal static readonly MethodInfo BrowserDialog_GoUp = AccessTools.Method(typeof(BrowserDialog), "GoUp", [typeof(int)]);

    // Fields for runtime checks
    [SuppressMessage(
        "Globalization",
        "CA1303:Do not pass literals as localized parameters",
        Justification = "Mod logs are not localized"
    )]
    private static FieldInfo? GetInventoryItemUIDirectoryField()
    {
        var field = AccessTools.Field(typeof(InventoryItemUI), "Directory");
        if (field == null)
        {
            ResoniteMod.Msg(
                "[InventoryUITweaks] Error: Could not find field 'Directory' on InventoryItemUI. Reflection access failed."
            );
        }
        return field;
    }

    internal static readonly FieldInfo? InventoryItemUI_Directory = GetInventoryItemUIDirectoryField();
}

// 1) Disable double-click requirement for the Back/Up button (always single press)
[HarmonyPatch(typeof(BrowserDialog), methodName: "GenerateBackButton")]
[HarmonyPatch([typeof(UIBuilder), typeof(LocaleString)])]
internal static class BrowserDialog_GenerateBackButton_Patch
{
    [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Harmony patch method")]
    [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Harmony patch signature")]
    private static bool Prefix(BrowserDialog __instance, UIBuilder ui, LocaleString label)
    {
        // Recreate original button without SyncDelegate callback and handle locally to avoid data-model target requirement
        var btn = ui.Button(in label, RadiantUI_Constants.Sub.RED);
        btn.LocalPressed += (b, e) =>
        {
            ReflectionCache.BrowserDialog_GoUp.Invoke(__instance, [1]);
        };
        btn.Label.Color.Value = RadiantUI_Constants.Hero.RED;
        return false; // Skip original
    }
}

// 1) Disable double-click requirement for opening folders in Inventory (single click opens directories)
[HarmonyPatch(typeof(BrowserItem), nameof(BrowserItem.Pressed))]
internal static class BrowserItem_Pressed_Patch
{
    [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Harmony patch signature")]
    private static bool Prefix(BrowserItem __instance, IButton button, ButtonEventData eventData)
    {
        // Single-click open only for folders (InventoryItemUI with non-null Directory)
        if (__instance is InventoryItemUI inv)
        {
            var dirField = ReflectionCache.InventoryItemUI_Directory;
            if (dirField?.GetValue(inv) != null)
            {
                inv.Browser.Target.SelectedItem.Target = null!;
                inv.Open();
                return false; // Skip original double-click logic for folders
            }
        }
        return true;
    }
}

// Add a small overlay "Select" button on folder entries so user can select directories without opening them.
[HarmonyPatch(typeof(InventoryBrowser), methodName: "ProcessItem")]
internal static class InventoryBrowser_ProcessItem_Postfix
{
    [HarmonyPostfix]
    [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Harmony patch method")]
    private static void Postfix(InventoryItemUI item)
    {
        if (item == null)
        {
            return;
        }
        var dirField = ReflectionCache.InventoryItemUI_Directory;
        var isDirectory = dirField != null && dirField.GetValue(item) != null;
        var slot = item.Slot;
        if (slot == null || slot.IsDestroyed)
        {
            return;
        }

        // 1) Favorite indicator (top-left corner), for items marked as favorite via ProcessItem coloring
        const string FavIndicatorName = "ET_FavoriteIndicator";
        var isFavorite = item.NormalColor.Value == InventoryBrowser.FAVORITE_COLOR;
        var favChild = slot.FindChild(FavIndicatorName);
        if (isFavorite)
        {
            if (favChild == null)
            {
                var uiFav = new UIBuilder(slot);
                uiFav.NestInto(slot);
                var img = uiFav.Image(slot.AttachSprite(OfficialAssets.Common.Indicators.TopLeftCornerTriangle), InventoryBrowser.FAVORITE_COLOR);
                img.Slot.Name = FavIndicatorName;
                img.Slot.PersistentSelf = false;
                img.FlipHorizontally.Value = false; // keep on left side
                var rtImg = img.RectTransform;
                rtImg.AnchorMin.Value = new float2(0.0f, 0.85f);
                rtImg.AnchorMax.Value = new float2(0.15f, 1.0f);
                rtImg.OffsetMin.Value = float2.Zero;
                rtImg.OffsetMax.Value = float2.Zero;
                uiFav.NestOut();
            }
            else
            {
                favChild.ActiveSelf = true;
            }
        }
        else
        {
            favChild?.Destroy();
        }

        // 2) Selection helper button for directories (left side rectangular area)
        if (isDirectory)
        {
            const string HelperName = "ET_SelectFolderButton";
            if (slot.FindChild(HelperName) == null)
            {
                var ui = new UIBuilder(slot);
                ui.NestInto(slot);
                RadiantUI_Constants.SetupDefaultStyle(ui);
                // Plain button; we add a checkmark image inside
                var btn = ui.Button("");
                btn.Slot.Name = HelperName;
                btn.Slot.PersistentSelf = false;
                var rt = btn.RectTransform;
                // Compute square size ~ 1/3 of folder cell height: side ~= (0.5*ItemSize + PADDING)/3 - PADDING
                var inv = item.Inventory;
                var itemSize = inv?.ItemSize.Value ?? BrowserDialog.DEFAULT_ITEM_SIZE;
                var pad = BrowserDialog.PADDING;
                // Target ~50% of folder button height, with a small inset
                var folderHeight = (0.5f * itemSize) + pad;
                var side = (folderHeight * 0.5f) - (pad * 0.25f);
                if (side < 20f)
                {
                    side = 20f;
                }
                // Place square at left with small padding
                rt.AnchorMin.Value = new float2(0.0f, 0.5f);
                rt.AnchorMax.Value = new float2(0.0f, 0.5f);
                var leftPad = pad * 2f;
                rt.OffsetMin.Value = new float2(leftPad, -side * 0.5f);
                rt.OffsetMax.Value = new float2(leftPad + side, side * 0.5f);
                // Shift label to the right by the square width + padding
                var mainButton = slot.GetComponent<Button>();
                var labelRT = mainButton?.Label?.RectTransform;
                if (labelRT != null)
                {
                    var cur = labelRT.OffsetMin.Value;
                    labelRT.OffsetMin.Value = new float2(cur.x + side + leftPad + pad, cur.y);
                }
                // Add centered check image (hidden by default)
                ui.NestInto(btn.Slot);
                var check = ui.Image(ui.CheckSprite, colorX.White);
                check.Slot.Name = "ET_SelectFolderCheck";
                var checkRT = check.RectTransform;
                // Centered, fit inside square (70%)
                var inset = side * 0.15f;
                checkRT.AnchorMin.Value = new float2(0f, 0f);
                checkRT.AnchorMax.Value = new float2(1f, 1f);
                checkRT.OffsetMin.Value = new float2(inset, inset);
                checkRT.OffsetMax.Value = new float2(-inset, -inset);
                check.Tint.Value = ((colorX)check.Tint).SetA(0f);
                ui.NestOut();
                btn.LocalPressed += (b, e) =>
                {
                    if (item.Browser?.Target != null)
                    {
                        item.Browser.Target.SelectedItem.Target = item;
                    }
                };
                ui.NestOut();
            }
        }
    }
}

// Keep folder selection button visuals in sync with selection
[HarmonyPatch(typeof(InventoryBrowser), methodName: "OnItemSelected")]
internal static class InventoryBrowser_OnItemSelected_Postfix
{
    [HarmonyPostfix]
    [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Harmony patch method")]
    [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Harmony patch signature")]
    private static void Postfix(InventoryBrowser __instance, BrowserItem previousItem, BrowserItem currentItem)
    {
        var selected = currentItem as InventoryItemUI;
        var items = Pool.BorrowList<InventoryItemUI>();
        __instance.Slot.GetComponentsInChildren(items);
        var dirField = ReflectionCache.InventoryItemUI_Directory;
        foreach (var it in items)
        {
            var isDir = dirField != null && dirField.GetValue(it) != null;
            if (!isDir)
            {
                continue;
            }
            var helperSlot = it.Slot.FindChild("ET_SelectFolderButton");
            var helper = helperSlot?.GetComponent<Button>();
            if (helper == null)
            {
                continue;
            }
            var isSelected = ReferenceEquals(it, selected);
            helper.BaseColor.Value = isSelected ? InventoryBrowser.SELECTED_COLOR : RadiantUI_Constants.BUTTON_COLOR;
            var checkImg = helperSlot?.FindChild("ET_SelectFolderCheck")?.GetComponent<Image>();
            if (checkImg != null)
            {
                checkImg.Tint.Value = isSelected ? colorX.White : ((colorX)checkImg.Tint).SetA(0f);
            }
        }
        Pool.Return(ref items);
    }
}

// 1-b) Make inventories list items single-click by zeroing double-press delay on generated entries
[HarmonyPatch]
internal static class InventoryBrowser_BeginGeneratingNewDirectory_Patch
{
    [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Harmony patch method")]
    private static MethodInfo TargetMethod()
    {
        return AccessTools.Method(
            typeof(InventoryBrowser),
            "BeginGeneratingNewDirectory",
            [
                typeof(RecordDirectory),
                typeof(GridLayout).MakeByRefType(),
                typeof(GridLayout).MakeByRefType(),
                typeof(SlideSwapRegion.Slide)
            ]
        );
    }

    [HarmonyPostfix]
    [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Harmony patch method")]
    private static void Postfix(RecordDirectory directory, GridLayout folders)
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
