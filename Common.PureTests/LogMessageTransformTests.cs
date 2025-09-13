using EsnyaTweaks.Common.Logging;
using FluentAssertions;
using Xunit;

namespace EsnyaTweaks.Common.PureTests;

public sealed class LogMessageTransformTests
{
    [Theory]
    [InlineData(null, true, "")]
    [InlineData("single line", true, "single line")]
    [InlineData("a\nb", true, "a\n\tb")]
    [InlineData("a\n\tb", false, "a\n\tb")]
    public void ApplyIndent_Works(string? input, bool addIndent, string expected)
    {
        LogMessageTransform.ApplyIndent(input, addIndent).Should().Be(expected);
    }
}

