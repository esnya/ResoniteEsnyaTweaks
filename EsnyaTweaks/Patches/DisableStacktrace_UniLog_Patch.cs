using Elements.Core;
using EsnyaTweaks.Attributes;
using HarmonyLib;

namespace EsnyaTweaks.Patches;

internal static class DisableStacktrace
{
    [HarmonyPatchCategory("Disable Stacktrace on Log"), TweakDescription("Disable stacktrace on UniLog.Log")]
    [HarmonyPatch(typeof(UniLog), nameof(UniLog.Log), new[] { typeof(string), typeof(bool) })]
    internal static class Log_Patch
    {
        internal static void Prefix(string message, ref bool stackTrace)
        {
            stackTrace = false;
        }
    }

    [HarmonyPatchCategory("Disable Stacktrace on Warning"), TweakDescription("Disable stacktrace on UniLog.Warning")]
    [HarmonyPatch(typeof(UniLog), nameof(UniLog.Warning), new[] { typeof(string), typeof(bool) })]
    internal static class Warning_Patch
    {
        internal static void Prefix(string message, ref bool stackTrace)
        {
            stackTrace = false;
        }
    }

    [HarmonyPatchCategory("Disable Stacktrace on Error"), TweakDescription("Disable stacktrace on UniLog.Error")]
    [HarmonyPatch(typeof(UniLog), nameof(UniLog.Error), new[] { typeof(string), typeof(bool) })]
    internal static class Error_Patch
    {
        internal static void Prefix(string message, ref bool stackTrace)
        {
            stackTrace = false;
        }
    }
}

