using EsnyaTweaks.Common.LOD;
using FluentAssertions;
using Xunit;

namespace EsnyaTweaks.Common.PureTests;

public sealed class OrderValidationPureTests
{
    [Theory]
    [InlineData(new float[] { }, false)]
    [InlineData(new float[] { 1f }, false)]
    [InlineData(new float[] { 1f, 0.5f }, false)]
    [InlineData(new float[] { 1f, 1f }, true)]
    [InlineData(new float[] { 0.5f, 1f }, true)]
    public void HasNonDescending_Works(float[] data, bool expected)
    {
        OrderValidation.HasNonDescending(data).Should().Be(expected);
    }
}
