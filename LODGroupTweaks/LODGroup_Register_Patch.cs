using System;
using System.Collections.Generic;
using System.Linq;
using FrooxEngine;
using HarmonyLib;
using Elements.Core;
using ResoniteModLoader;

namespace EsnyaTweaks.LODGroupTweaks;

[HarmonyPatch(typeof(LODGroup))]
internal static class LODGroup_Register_Patch
{
    private static readonly HashSet<LODGroup> s_reordering = [];
    private const float EPSILON = 1e-6f; // minimal adjustment to ensure strict descending

#pragma warning disable IDE0051 // used by Harmony via reflection
    [HarmonyPrefix]
    [HarmonyPatch("RegisterChanged")]
    private static void RegisterChanged_Prefix(LODGroup __instance)
    {
        TryReorderByHeight(__instance);
    }

    [HarmonyPrefix]
    [HarmonyPatch("RegisterAddedOrRemoved")]
    private static void RegisterAddedOrRemoved_Prefix(LODGroup __instance)
    {
        TryReorderByHeight(__instance);
    }
#pragma warning restore IDE0051

    private static void TryReorderByHeight(LODGroup group)
    {
        if (!s_reordering.Add(group)) { return; }
        try
        {
            if (group.LODs.Count <= 1) { return; }

            var lods = Pool.BorrowList<LODGroup.LOD>();
            try
            {
                foreach (var l in group.LODs)
                {
                    lods.Add(l);
                }
                var beforeHeights = lods
                    .Select(l => l.ScreenRelativeTransitionHeight.Value)
                    .ToArray();

                // Minimally adjust thresholds to be strictly descending (no clamp)
                // Unity error: "SetLODs: Attempting to set LOD where the screen relative size is greater then or equal to a higher detail LOD level."
                // Fix by ensuring h[i] >= h[i+1] + EPSILON going from high detail (index 0) to low detail.
                var count = group.LODs.Count;
                if (count > 0)
                {
                    var adjusted = false;
                    for (var i = count - 2; i >= 0; i--)
                    {
                        var nextVal = group.LODs[i + 1].ScreenRelativeTransitionHeight.Value;
                        var cur = group.LODs[i].ScreenRelativeTransitionHeight;
                        var curVal = cur.Value;

                        if (curVal <= nextVal)
                        {
                            curVal = nextVal + EPSILON;
                        }

                        if (curVal != cur.Value)
                        {
                            cur.Value = curVal;
                            adjusted = true;
                        }
                    }

                    if (adjusted)
                    {
                        var afterHeights = group.LODs
                            .Select(l => l.ScreenRelativeTransitionHeight.Value)
                            .ToArray();
                        ResoniteMod.DebugFunc(() =>
                            $"[LODGroupTweaks] Updated {group}: Adjusted=yes, Before=[{string.Join(",", beforeHeights)}], After=[{string.Join(",", afterHeights)}]"
                        );
                    }
                }
            }
            finally
            {
                Pool.Return(ref lods);
            }
        }
        catch (Exception)
        {
        }
        finally
        {
            s_reordering.Remove(group);
        }
    }
}
