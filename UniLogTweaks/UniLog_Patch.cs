using System.Runtime.CompilerServices;
using Elements.Core;
using HarmonyLib;
using EsnyaTweaks.Common.Logging;

namespace EsnyaTweaks.UniLogTweaks;

[HarmonyPatch(typeof(UniLog))]
internal static class UniLog_Patch
{
    [HarmonyPatch(nameof(UniLog.Log), [typeof(string), typeof(bool)])]
    [HarmonyPrefix]
    internal static void Log_Prefix(ref string message, ref bool stackTrace)
    {
        Patch(ref message, ref stackTrace, !UniLogTweaksMod.AllowInfo);
    }

    [HarmonyPatch(nameof(UniLog.Warning))]
    [HarmonyPrefix]
    internal static void Warning_Prefix(ref string message, ref bool stackTrace)
    {
        Patch(ref message, ref stackTrace, !UniLogTweaksMod.AllowWarning);
    }

    [HarmonyPatch(nameof(UniLog.Error))]
    [HarmonyPrefix]
    internal static void Error_Prefix(ref string message, ref bool stackTrace)
    {
        Patch(ref message, ref stackTrace, !UniLogTweaksMod.AllowError);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Patch(ref string message, ref bool stackTrace, bool allowStackTrace)
    {
        message = LogMessageTransform.ApplyIndent(message, UniLogTweaksMod.AddIndent);

        if (!allowStackTrace)
        {
            stackTrace = false;
        }
    }
}
