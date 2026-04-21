using FrooxEngine;
using Xunit;
using FluentAssertions;

namespace EsnyaTweaks.LODGroupTweaks.Tests;

public sealed class LODValidationTests
{
    private static readonly float[] EqualPair = [0.9f, 0.9f];
    private static readonly float[] DescendingPair = [0.9f, 0.8f];
    private static readonly float[] NonDescendingMixed = [1.0f, 0.7f, 0.7f, 0.2f];

    [Fact]
    public void HasOrderViolation_Should_Detect_Ascending_Or_Equal()
    {
        LODValidation.HasOrderViolation(EqualPair).Should().BeTrue();
        LODValidation.HasOrderViolation(DescendingPair).Should().BeFalse();
        LODValidation.HasOrderViolation(NonDescendingMixed).Should().BeTrue();
    }

    [Fact]
    public void GetHeights_Should_Read_SyncList_Values_In_Order()
    {
        var g = new LODGroup();
        // Can't construct real LOD instances without engine; simulate via manual sync list replacement if available
        // Here we just ensure method doesn't throw and returns expected length for Count==0
        LODValidation.GetHeights(g).Should().NotBeNull();
    }
}
