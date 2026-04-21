using Elements.Core;
using FrooxEngine;
using FrooxEngine.UIX;
using HarmonyLib;

namespace EsnyaTweaks.InventoryUITweaks;

[HarmonyPatch(typeof(InventoryBrowser), methodName: "ProcessItem")]
internal static class InventoryBrowserProcessItemPostfix
{
    [HarmonyPostfix]
    internal static void Postfix(InventoryItemUI item)
    {
        if (item == null)
        {
            return;
        }

        var dirField = ReflectionCache.InventoryItemUIDirectory;
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
                img.FlipHorizontally.Value = false;
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
                var btn = ui.Button(string.Empty);
                btn.Slot.Name = HelperName;
                btn.Slot.PersistentSelf = false;
                var rt = btn.RectTransform;
                var inv = item.Inventory;
                var itemSize = inv?.ItemSize.Value ?? BrowserDialog.DEFAULT_ITEM_SIZE;
                var pad = BrowserDialog.PADDING;
                var folderHeight = (0.5f * itemSize) + pad;
                var side = (folderHeight * 0.5f) - (pad * 0.25f);
                if (side < 20f)
                {
                    side = 20f;
                }

                rt.AnchorMin.Value = new float2(0.0f, 0.5f);
                rt.AnchorMax.Value = new float2(0.0f, 0.5f);
                var leftPad = pad * 2f;
                rt.OffsetMin.Value = new float2(leftPad, -side * 0.5f);
                rt.OffsetMax.Value = new float2(leftPad + side, side * 0.5f);
                var mainButton = slot.GetComponent<Button>();
                var labelRT = mainButton?.Label?.RectTransform;
                if (labelRT != null)
                {
                    var cur = labelRT.OffsetMin.Value;
                    labelRT.OffsetMin.Value = new float2(cur.x + side + leftPad + pad, cur.y);
                }

                ui.NestInto(btn.Slot);
                var check = ui.Image(ui.CheckSprite, colorX.White);
                check.Slot.Name = "ET_SelectFolderCheck";
                var checkRT = check.RectTransform;
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
