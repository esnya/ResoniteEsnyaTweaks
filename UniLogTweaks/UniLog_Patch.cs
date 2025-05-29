using System.Runtime.CompilerServices;
using Elements.Core;
using HarmonyLib;

namespace EsnyaTweaks.UniLogeTweaks;

#pragma warning disable IDE0060

[HarmonyPatch(typeof(UniLog))]
internal static class UniLog_Patch
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static void Patch(ref string message, ref bool stackTrace, bool allowStackTrace)
    {
        if (UniLogTweaksMod.AddIndent)
        {
            message = message.Replace("\n", "\n\t");
        }

        if (!allowStackTrace)
        {
            stackTrace = false;
        }
    }


    [HarmonyPatch(nameof(UniLog.Log), new[] { typeof(string), typeof(bool) })]
    [HarmonyPrefix]
    internal static void Log_Prefix(ref string message, ref bool stackTrace) => Patch(ref message, ref stackTrace, !UniLogTweaksMod.AllowInfo);


    [HarmonyPatch(nameof(UniLog.Warning))]
    [HarmonyPrefix]
    internal static void Warning_Prefix(ref string message, ref bool stackTrace) => Patch(ref message, ref stackTrace, !UniLogTweaksMod.AllowWarning);

    [HarmonyPatch(nameof(UniLog.Error))]
    [HarmonyPrefix]
    internal static void Error_Prefix(ref string message, ref bool stackTrace) => Patch(ref message, ref stackTrace, !UniLogTweaksMod.AllowError);
}
