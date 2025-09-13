using System;
using System.Collections.Generic;
using System.Linq;

namespace EsnyaTweaks.AssetOptimizationTweaks.Internal;

/// <summary>
/// Pure, engine-agnostic helpers for asset deduplication logic.
/// </summary>
internal static class PureAssetDedup
{
    /// <summary>
    /// Finds duplicate pairs using the provided equality comparer.
    /// </summary>
    public static (T Original, T Duplicate)[] FindDuplicatePairs<T>(
        IReadOnlyList<T> items,
        Func<T, T, bool> equals
    )
        where T : notnull
    {
        List<(T, T)> pairs = [];

        for (var i = 0; i < items.Count; i++)
        {
            var original = items[i];
            for (var j = i + 1; j < items.Count; j++)
            {
                var candidate = items[j];
                if (equals(original, candidate))
                {
                    pairs.Add((original, candidate));
                }
            }
        }

        return [.. pairs];
    }

    /// <summary>
    /// Adds redirections from duplicates to originals in order, ignoring duplicates and keeping existing entries.
    /// </summary>
    public static void AddRedirections<TKey>(
        IDictionary<TKey, TKey> map,
        IEnumerable<TKey> duplicates,
        IEnumerable<TKey> originals
    )
        where TKey : notnull
    {
        foreach (var (dup, orig) in duplicates.Zip(originals, (d, o) => (d, o)))
        {
            if (!map.TryAdd(dup, orig) && !Equals(map[dup], orig))
            {
                // keep existing value; no-op
            }
        }
    }
}
