using System;
using System.Collections.Generic;

namespace EsnyaTweaks.SceneAuditor.Rules;

/// <summary>
/// Pure helpers for rule evaluation. Engine-agnostic and unit-testable.
/// </summary>
public static class DetectionPrimitives
{
    public static IReadOnlyList<int> FindNonDescendingIndices(IReadOnlyList<float> heights)
    {
        ArgumentNullException.ThrowIfNull(heights);
        if (heights.Count <= 1)
        {
            return [];
        }

        List<int> result = [];
        for (var i = 0; i < heights.Count - 1; i++)
        {
            if (heights[i] <= heights[i + 1])
            {
                result.Add(i);
            }
        }
        return result;
    }

    public static Dictionary<TRenderer, HashSet<TGroup>> BuildDuplicateOwnersIndex<TGroup, TRenderer>(
        IEnumerable<(TGroup Group, IEnumerable<TRenderer> Renderers)> groups
    ) where TRenderer : notnull
    {
        ArgumentNullException.ThrowIfNull(groups);
        Dictionary<TRenderer, HashSet<TGroup>> index = [];
        foreach (var (group, renderers) in groups)
        {
            if (renderers is null)
            {
                continue;
            }
            foreach (var r in renderers)
            {
                if (!index.TryGetValue(r, out var owners))
                {
                    owners = [];
                    index[r] = owners;
                }
                owners.Add(group);
            }
        }
        return index;
    }
}
