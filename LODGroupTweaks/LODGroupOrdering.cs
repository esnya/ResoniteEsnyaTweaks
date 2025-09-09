using System;
using System.Collections.Generic;
using System.Linq;

namespace EsnyaTweaks.LODGroupTweaks;

internal interface IReorderableList<T>
{
    int Count { get; }
    T this[int index] { get; }
    int IndexOf(T item);
    void Move(int from, int to);
}

internal static class LODGroupOrdering
{
    public static int[] DesiredOrderByDescending<T>(IReadOnlyList<T> items, Func<T, float> key)
    {
        return items.Count <= 1
            ? [.. Enumerable.Range(0, items.Count)]
            : [.. items.Select((x, i) => (i, v: key(x))).OrderByDescending(p => p.v).Select(p => p.i)];
    }

    public static void ReorderTo<T>(IReorderableList<T> list, IReadOnlyList<T> desired)
    {
        var current = new List<T>(capacity: list.Count);
        for (var i = 0; i < list.Count; i++)
        {
            current.Add(list[i]);
        }

        for (var i = 0; i < desired.Count; i++)
        {
            if (ReferenceEquals(list[i], desired[i]))
            {
                continue;
            }

            var from = current.IndexOf(desired[i]);
            if (from < 0)
            {
                continue;
            }

            if (from != i)
            {
                list.Move(from, i);
                current.RemoveAt(from);
                current.Insert(i, desired[i]);
            }
        }
    }
}
