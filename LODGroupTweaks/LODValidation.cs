using System;
using System.Collections.Generic;
using Elements.Core;
using FrooxEngine;
using ResoniteModLoader;

namespace EsnyaTweaks.LODGroupTweaks;

internal static class LODValidation
{
    public static float[] GetHeights(LODGroup group)
    {
        var lods = group?.LODs;
        if (lods == null)
        {
            return [];
        }
        var arr = new float[lods.Count];
        for (var i = 0; i < lods.Count; i++)
        {
            arr[i] = lods[i]?.ScreenRelativeTransitionHeight.Value ?? 0f;
        }
        return arr;
    }

    public static bool HasOrderViolation(IReadOnlyList<float> heights)
    {
        for (var i = 0; i < heights.Count - 1; i++)
        {
            if (heights[i] <= heights[i + 1])
            {
                return true;
            }
        }
        return false;
    }

    public static IEnumerable<MeshRenderer> EnumerateRenderers(LODGroup group)
    {
        var lods = group?.LODs;
        if (lods == null)
        {
            yield break;
        }
        var seen = new HashSet<MeshRenderer>();
        foreach (var lod in lods)
        {
            if (lod == null)
            {
                continue;
            }
            foreach (var r in EnumerateRenderers(lod))
            {
                if (r == null)
                {
                    continue;
                }
                if (seen.Add(r))
                {
                    yield return r;
                }
            }
        }
    }

    public static IEnumerable<MeshRenderer> EnumerateRenderers(LODGroup.LOD lod)
    {
        if (lod == null)
        {
            yield break;
        }
        var list = lod.Renderers;
        if (list == null)
        {
            yield break;
        }
        foreach (var r in list)
        {
            if (r != null)
            {
                yield return r;
            }
        }
    }

    public static HashSet<MeshRenderer> CollectAssignedRenderersInOtherGroups(LODGroup current)
    {
        var set = new HashSet<MeshRenderer>();
        var root = current?.Slot?.World?.RootSlot;
        if (root == null)
        {
            return set;
        }
        var all = Pool.BorrowList<LODGroup>();
        try
        {
            root.GetComponentsInChildren(all);
            foreach (var g in all)
            {
                if (g == null || g == current)
                {
                    continue;
                }
                foreach (var r in EnumerateRenderers(g))
                {
                    if (r != null)
                    {
                        set.Add(r);
                    }
                }
            }
        }
        finally
        {
            Pool.Return(ref all);
        }
        return set;
    }

    public static void ValidateLODs(IEnumerable<LODGroup.LOD> src)
    {
        if (!ResoniteMod.IsDebugEnabled())
        {
            return;
        }
        var hasNull = false;
        var notDescending = false;
        var prev = float.PositiveInfinity;
        var heights = Pool.BorrowList<float>();
        try
        {
            foreach (var l in src)
            {
                if (l == null)
                {
                    hasNull = true;
                    continue;
                }
                var h = l.ScreenRelativeTransitionHeight.Value;
                heights.Add(h);
                if (h >= prev)
                {
                    notDescending = true;
                }
                prev = h;
            }
            if (hasNull || notDescending)
            {
                ResoniteMod.DebugFunc(() =>
                    $"LOD issue: nulls={(hasNull ? 1 : 0)}, order={(notDescending ? "non-desc" : "ok")}, heights=[{string.Join(", ", heights)}]");
            }
        }
        catch (Exception ex)
        {
            ResoniteMod.Warn($"ValidateLODs failed: {ex}");
        }
        finally
        {
            Pool.Return(ref heights);
        }
    }

    // no helpers needed
}

