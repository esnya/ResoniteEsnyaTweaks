using System.Collections.Generic;
using Elements.Core;
using FrooxEngine;

namespace EsnyaTweaks.LODGroupTweaks.Internal;

internal static class LODHelpers
{
    public static float GetBoundingMagnitude(Slot space, MeshRenderer renderer)
    {
        return renderer.GetBoundingBoxInSpace(space).Size.Magnitude;
    }

    public static float GetBoundingMagnitude(Slot slot)
    {
        return slot.ComputeBoundingBox(filter: static c => c is MeshRenderer).Size.Magnitude;
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
                if (r != null && seen.Add(r))
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
}
