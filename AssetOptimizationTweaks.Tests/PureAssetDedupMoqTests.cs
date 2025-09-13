using System.Collections.Generic;
using EsnyaTweaks.AssetOptimizationTweaks.Internal;
using FluentAssertions;
using FrooxEngine;
using Moq;
using Xunit;

namespace EsnyaTweaks.AssetOptimizationTweaks.Tests;

public sealed class PureAssetDedupMoqTests
{
    [Fact]
    public void AddRedirections_WithIWorldElementMocks_ShouldMapOnce()
    {
        var dup1 = new Mock<IWorldElement>().Object;
        var orig1 = new Mock<IWorldElement>().Object;

        var map = new Dictionary<IWorldElement, IWorldElement>();
        PureAssetDedup.AddRedirections(map, [dup1], [orig1]);
        PureAssetDedup.AddRedirections(map, [dup1], [orig1]);

        map.Should().ContainSingle().Which.Should().Be(new KeyValuePair<IWorldElement, IWorldElement>(dup1, orig1));
    }
}
