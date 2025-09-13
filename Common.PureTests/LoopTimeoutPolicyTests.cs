using EsnyaTweaks.Common.Flux;
using FluentAssertions;
using Xunit;

namespace EsnyaTweaks.Common.PureTests;

public sealed class LoopTimeoutPolicyTests
{
    [Theory]
    [InlineData(0L, 0L, 10L, 5, false, true)]
    [InlineData(0L, 1L, 1000L, 1, false, false)]
    [InlineData(5L, 5L, 0L, 100, true, true)]
    public void ShouldAbort_Works_As_Expected(long prev, long curr, long elapsed, int timeout, bool abortReq, bool expected)
    {
        LoopTimeoutPolicy.ShouldAbort(prev, curr, elapsed, timeout, abortReq)
            .Should().Be(expected);
    }
}

