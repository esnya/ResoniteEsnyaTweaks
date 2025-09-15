using System.Collections.Generic;
using EsnyaTweaks.SceneAuditor.Rules;
using Xunit;
using FluentAssertions;

namespace EsnyaTweaks.SceneAuditor.Tests;

public sealed class DetectionPrimitivesTests
{
    private static readonly int[] ExpectedIndices = [0, 2];
    [Fact]
    public void FindNonDescendingIndices_Should_Find_Violations()
    {
        float[] heights = [0.8f, 0.8f, 0.7f, 0.9f, 0.1f];
        var indices = DetectionPrimitives.FindNonDescendingIndices(heights);
        indices.Should().BeEquivalentTo(ExpectedIndices);
    }

    [Fact]
    public void BuildDuplicateOwnersIndex_Should_Index_Duplicates()
    {
        var a = "A";
        var b = "B";
        var c = "C";
        var ab = new List<string> { a, b };
        var bc = new List<string> { b, c };
        var cOnly = new List<string> { c };
        (string, IEnumerable<string>)[] groups =
        [
            ("G1", ab),
            ("G2", bc),
            ("G3", cOnly)
        ];

        var index = DetectionPrimitives.BuildDuplicateOwnersIndex(groups);
        index[a].Should().HaveCount(1);
        index[b].Should().HaveCount(2);
        index[c].Should().HaveCount(2);
    }
}
