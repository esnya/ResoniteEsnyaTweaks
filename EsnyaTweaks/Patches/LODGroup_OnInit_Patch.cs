using FrooxEngine;
using HarmonyLib;
using System.ComponentModel;
using System.Reflection;

namespace EsnyaTweaks.Patches;

[HarmonyPatchCategory("LODGroup UpdateOrder")]
[Description("Set LODGroup update order to 1000 to prevent rendering issues.")]
[HarmonyPatch]
internal static class LODGroup_OnInit_Patch
{
    internal static MethodInfo TargetMethod()
    {
        return AccessTools.Method(typeof(LODGroup), "OnInit");
    }

    internal static void Postfix(object __instance)
    {
        if (__instance is LODGroup lodGroup)
        {
            lodGroup.UpdateOrder = 1000;
            ResoniteModLoader.ResoniteMod.DebugFunc(() => $"LODGroup {lodGroup} found. Patching update order to {lodGroup.UpdateOrder}...");
        }
    }
}
