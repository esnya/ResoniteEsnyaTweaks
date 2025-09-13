using System.Diagnostics.CodeAnalysis;
using Elements.Core;
using FrooxEngine;
using FrooxEngine.UIX;
using HarmonyLib;

namespace EsnyaTweaks.InventoryUITweaks;

[HarmonyPatch(typeof(BrowserDialog), methodName: "GenerateBackButton")]
[HarmonyPatch([typeof(UIBuilder), typeof(LocaleString)])]
internal static class BrowserDialogGenerateBackButtonPatch
{
    internal static bool Prefix(
        [SuppressMessage("Style", "SA1313", Justification = "Harmony magic parameter")] BrowserDialog __instance,
        UIBuilder ui,
        LocaleString label)
    {
        var btn = ui.Button(in label, RadiantUI_Constants.Sub.RED);
        btn.LocalPressed += (b, e) =>
        {
            ReflectionCache.BrowserDialogGoUp.Invoke(__instance, [1]);
        };
        btn.Label.Color.Value = RadiantUI_Constants.Hero.RED;
        return false;
    }
}
