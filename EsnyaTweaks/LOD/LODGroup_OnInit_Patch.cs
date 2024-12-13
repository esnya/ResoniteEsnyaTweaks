using FrooxEngine;
using HarmonyLib;
using System.Reflection;

namespace EsnyaTweaks.LOD;

[HarmonyPatchCategory(nameof(PatchCategory.LOD))]
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
            EsnyaTweaksMod.DebugFunc(() => $"LODGroup {lodGroup} found. Patching update order to {lodGroup.UpdateOrder}...");
        }
    }
}

