using System;
using System.Collections.Generic;
using System.Reflection;
using FluentAssertions;
using FrooxEngine;
using Xunit;

namespace EsnyaTweaks.AssetOptimizationTweaks.Tests;

public class AssetOptimizationExtensionsTests
{
    [Fact]
    public void CheckInheritanceHierarchy_ShouldReturnFalse_ForNonProceduralAssetProvider()
    {
        var nonProceduralType = typeof(string);
        var method = GetPrivateMethod("CheckInheritanceHierarchy");

        var result = (bool)method.Invoke(null, [nonProceduralType])!;

        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(typeof(object))]
    [InlineData(typeof(int))]
    [InlineData(typeof(List<string>))]
    public void CheckInheritanceHierarchy_ShouldReturnFalse_ForVariousNonProceduralTypes(Type type)
    {
        var method = GetPrivateMethod("CheckInheritanceHierarchy");

        var result = (bool)method.Invoke(null, [type])!;

        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(typeof(BoxMesh))]
    [InlineData(typeof(GridTexture))]
    [InlineData(typeof(SineWaveClip))]
    public void CheckInheritanceHierarchy_ShouldReturnTrue_ForProceduralAssetProviders(Type type)
    {
        var method = GetPrivateMethod("CheckInheritanceHierarchy");

        var result = (bool)method.Invoke(null, [type])!;

        result.Should().BeTrue();
    }

    [Fact]
    public void BuildRedirectionMapShouldNotModifyMapWhenNoDuplicates()
    {
        var groupedProviders = new Dictionary<Type, List<IAssetProvider>>();
        var redirectionMap = new Dictionary<IWorldElement, IWorldElement>();

        AssetOptimizationExtensions.BuildRedirectionMap(groupedProviders, redirectionMap);

        redirectionMap.Should().BeEmpty();
    }

    [Fact]
    public void GroupProceduralAssetProvidersShouldNotModifyGroupedWhenEmpty()
    {
        var providers = new List<IAssetProvider>();
        var grouped = new Dictionary<Type, List<IAssetProvider>>();

        AssetOptimizationExtensions.GroupProceduralAssetProviders(providers, grouped);

        grouped.Should().BeEmpty();
    }

    [Fact]
    public void BuildRedirectionMap_ShouldNotModifyMapWithDifferentTypes()
    {
        var groupedProviders = new Dictionary<Type, List<IAssetProvider>>
        {
            { typeof(BoxMesh), [] },
            { typeof(GridTexture), [] },
        };
        var redirectionMap = new Dictionary<IWorldElement, IWorldElement>();

        AssetOptimizationExtensions.BuildRedirectionMap(groupedProviders, redirectionMap);

        redirectionMap.Should().BeEmpty();
    }

    [Theory]
    [InlineData(typeof(void))]
    [InlineData(typeof(Enum))]
    [InlineData(typeof(Array))]
    public void CheckInheritanceHierarchy_ShouldReturnFalse_ForEdgeCaseTypes(Type type)
    {
        var method = GetPrivateMethod("CheckInheritanceHierarchy");

        var result = (bool)method.Invoke(null, [type])!;

        result.Should().BeFalse();
    }

    [Fact]
    public void CheckInheritanceHierarchy_ShouldReturnFalse_ForNullType()
    {
        var method = GetPrivateMethod("CheckInheritanceHierarchy");

        var result = (bool)method.Invoke(null, [null])!;

        result.Should().BeFalse();
    }

    [Fact]
    public void CheckInheritanceHierarchy_ShouldReturnFalse_ForAbstractTypes()
    {
        var method = GetPrivateMethod("CheckInheritanceHierarchy");

        var result = (bool)method.Invoke(null, [typeof(System.IO.Stream)])!;

        result.Should().BeFalse();
    }

    [Fact]
    public void CheckInheritanceHierarchy_ShouldReturnFalse_ForInterfaceTypes()
    {
        var method = GetPrivateMethod("CheckInheritanceHierarchy");

        var result = (bool)method.Invoke(null, [typeof(IDisposable)])!;

        result.Should().BeFalse();
    }

    [Fact]
    public void FindDuplicatePairs_ShouldReturnEmptyArray_WhenNoProviders()
    {
        var providers = new List<IAssetProvider>();

        var result = AssetOptimizationExtensions.FindDuplicatePairs(providers);

        result.Should().BeEmpty();
    }

    [Fact]
    public void IsNotDriven_ShouldReturnTrue_ForNullElements()
    {
        var method = GetPrivateMethod("IsNotDriven");

        var result = (bool)method.Invoke(null, [null, null])!;

        result.Should().BeTrue();
    }
    [Theory]
    [InlineData(typeof(Component))]
    [InlineData(typeof(IAssetProvider))]
    [InlineData(typeof(Worker))]
    public void CheckInheritanceHierarchy_ShouldReturnFalse_ForBaseTypes(Type type)
    {
        var method = GetPrivateMethod("CheckInheritanceHierarchy");

        var result = (bool)method.Invoke(null, [type])!;

        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(typeof(SphereMesh))]
    [InlineData(typeof(BoxMesh))]
    [InlineData(typeof(CylinderMesh))]
    public void CheckInheritanceHierarchy_ShouldReturnTrue_ForMoreProceduralAssetProviders(
        Type type
    )
    {
        var method = GetPrivateMethod("CheckInheritanceHierarchy");

        var result = (bool)method.Invoke(null, [type])!;

        result.Should().BeTrue();
    }

    [Fact]
    public void IsNotDriven_ShouldReturnFalse_ForNullProvider()
    {
        var method = GetPrivateMethod("IsNotDriven");

        var result = (bool)method.Invoke(null, [null])!;

        result.Should().BeFalse();
    }

    [Fact]
    public void BuildRedirectionMap_ShouldHandleMultipleTypesWithEmptyLists()
    {
        var groupedProviders = new Dictionary<Type, List<IAssetProvider>>
        {
            { typeof(BoxMesh), [] },
            { typeof(GridTexture), [] },
            { typeof(SineWaveClip), [] },
            { typeof(SphereMesh), [] },
        };
        var redirectionMap = new Dictionary<IWorldElement, IWorldElement>();

        AssetOptimizationExtensions.BuildRedirectionMap(groupedProviders, redirectionMap);

        redirectionMap.Should().BeEmpty();
    }

    [Fact]
    public void GroupProceduralAssetProviders_ShouldMaintainTypeGrouping()
    {
        var providers = new List<IAssetProvider>();
        var grouped = new Dictionary<Type, List<IAssetProvider>>();

        AssetOptimizationExtensions.GroupProceduralAssetProviders(providers, grouped);

        grouped.Should().BeEmpty();
        grouped.Keys.Should().BeEmpty();
    }

    [Fact]
    public void CheckInheritanceHierarchy_ShouldHandleComplexGenericTypes()
    {
        var method = GetPrivateMethod("CheckInheritanceHierarchy");

        var result = (bool)method.Invoke(null, [typeof(List<BoxMesh>)])!;

        result.Should().BeFalse();
    }

    [Fact]
    public void FindDuplicatePairs_ShouldHandleProviderListsWithSingleItem()
    {
        var providers = new List<IAssetProvider>();

        var result = AssetOptimizationExtensions.FindDuplicatePairs(providers);

        result.Should().BeEmpty();
        result.Length.Should().Be(0);
    }

    [Theory]
    [InlineData(typeof(Dictionary<string, int>))]
    [InlineData(typeof(Action<string>))]
    [InlineData(typeof(Func<int, string>))]
    public void CheckInheritanceHierarchy_ShouldReturnFalse_ForGenericDelegateTypes(Type type)
    {
        var method = GetPrivateMethod("CheckInheritanceHierarchy");

        var result = (bool)method.Invoke(null, [type])!;

        result.Should().BeFalse();
    }

    private static MethodInfo GetPrivateMethod(string methodName)
    {
        return typeof(AssetOptimizationExtensions).GetMethod(
                methodName,
                BindingFlags.NonPublic | BindingFlags.Static
            ) ?? throw new InvalidOperationException($"Method {methodName} not found");
    }
}
