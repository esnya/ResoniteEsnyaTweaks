using System.Collections.Generic;

namespace EsnyaTweaks.Common.LOD;

/// <summary>
/// Pure helpers for LOD order validation.
/// </summary>
public static class OrderValidation
{
    /// <summary>
    /// Returns true when any adjacent pair is non-descending (i.e., a[i] &lt;= a[i+1]).
    /// </summary>
    /// <param name="heights">Sequence of LOD screen-relative heights.</param>
    /// <returns>True when a non-descending pair exists; otherwise false.</returns>
    public static bool HasNonDescending(IReadOnlyList<float> heights)
    {
        System.ArgumentNullException.ThrowIfNull(heights);
        for (var i = 0; i < heights.Count - 1; i++)
        {
            if (heights[i] <= heights[i + 1])
            {
                return true;
            }
        }
        return false;
    }
}
