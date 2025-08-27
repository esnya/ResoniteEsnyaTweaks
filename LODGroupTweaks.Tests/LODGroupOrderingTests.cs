using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace EsnyaTweaks.LODGroupTweaks.Tests;

public sealed class LODGroupOrderingTests
{
    private static readonly int[] Indices12 = [1, 2];

    private sealed class FakeList<T>(IEnumerable<T> items) : IReorderableList<T>
    {
        private readonly List<T> _items = [.. items];
        public int MoveCalls { get; private set; }

        public int Count => _items.Count;
        public T this[int index] => _items[index];
        public int IndexOf(T item)
        {
            return _items.IndexOf(item);
        }
        public void Move(int from, int to)
        {
            if (from == to)
            {
                return;
            }
            var it = _items[from];
            _items.RemoveAt(from);
            _items.Insert(to, it);
            MoveCalls++;
        }

        public T[] Snapshot()
        {
            return [.. _items];
        }
    }

    [Fact]
    public void DesiredOrderByDescending_Should_Return_Indices_By_Height_Desc()
    {
        float[] items = [0f, 0.7f, 0.7f, 0.2f, 1.0f];

        var order = LODGroupOrdering.DesiredOrderByDescending(items, x => x);

        // 1.0 -> index 4, 0.7 -> indices 1,2 (any order, unstable ok), 0.2 -> 3, 0 -> 0
        order[0].Should().Be(4);
        order.Should().Contain(Indices12);
        order.Last().Should().Be(0);
    }

    [Fact]
    public void ReorderTo_Should_Rearrange_List_To_Desired_Order()
    {
        string[] items = ["A", "B", "C", "D"];
        var list = new FakeList<string>(items);
        string[] desired = ["C", "A", "D", "B"];

        LODGroupOrdering.ReorderTo(list, desired);

        list.Snapshot().Should().ContainInOrder(desired);
        list.MoveCalls.Should().BeGreaterThan(0);
    }

    [Fact]
    public void ReorderTo_Should_Do_Nothing_When_Already_In_Order()
    {
        int[] items = [10, 5, 1];
        var list = new FakeList<int>(items);

        LODGroupOrdering.ReorderTo(list, items);

        list.Snapshot().Should().ContainInOrder(items);
        list.MoveCalls.Should().Be(0);
    }
}
