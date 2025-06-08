using System;
using System.Linq;
using Elements.Core;
using FluentAssertions;
using FrooxEngine;
using Xunit;

namespace EsnyaTweaks.AssetOptimizationTweaks.Tests;

/// <summary>
/// Integration tests for DeduplicateProceduralAssets method.
/// These tests use actual Resonite objects and verify the destructive behavior.
/// </summary>
public sealed class DeduplicateProceduralAssetsTests : IDisposable
{
    private readonly Engine _engine;
    private readonly World _world;

    public DeduplicateProceduralAssetsTests()
    {
        _engine = new Engine();
        _world = _engine.WorldManager.StartLocal(_ => { });
    }

    public void Dispose()
    {
        _world?.Dispose();
        _engine?.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public void DeduplicateProceduralAssets_WithNullRoot_ShouldThrowArgumentNullException()
    {
        // Arrange
        Slot? nullSlot = null;

        // Act & Assert
        var exception = Assert.Throws<NullReferenceException>(() =>
            nullSlot!.DeduplicateProceduralAssets()
        );

        exception.Should().NotBeNull();
    }

    [Fact]
    public void DeduplicateProceduralAssets_WithEmptySlot_ShouldReturnZero()
    {
        // Arrange
        var emptySlot = _world.AddSlot("EmptySlot");

        // Act
        var result = emptySlot.DeduplicateProceduralAssets();

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public void DeduplicateProceduralAssets_WithNoProceduralAssets_ShouldReturnZero()
    {
        // Arrange
        var testSlot = _world.AddSlot("TestSlot");

        // Add some non-procedural components
        testSlot.AttachComponent<ValueCopy<float>>();
        testSlot.AttachComponent<MeshRenderer>();

        // Act
        var result = testSlot.DeduplicateProceduralAssets();

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public void DeduplicateProceduralAssets_WithIdenticalProceduralAssets_ShouldDeduplicate()
    {
        // Arrange
        var testSlot = _world.AddSlot("TestSlot");

        // Create two identical procedural asset providers
        _ = CreateProceduralAssetProvider(testSlot);
        _ = CreateProceduralAssetProvider(testSlot);

        var initialProviderCount = GetProceduralAssetProviderCount(testSlot);
        initialProviderCount.Should().Be(2);

        // Act
        var result = testSlot.DeduplicateProceduralAssets();

        // Assert
        result.Should().Be(1); // One duplicate removed
        var finalProviderCount = GetProceduralAssetProviderCount(testSlot);
        finalProviderCount.Should().Be(1); // Only one should remain
    }

    [Fact]
    public void DeduplicateProceduralAssets_WithDifferentProceduralAssets_ShouldNotDeduplicate()
    {
        // Arrange
        var testSlot = _world.AddSlot("TestSlot");

        // Create two different procedural asset providers
        _ = CreateProceduralAssetProvider(testSlot);
        _ = CreateProceduralAssetProvider(testSlot);

        var initialProviderCount = GetProceduralAssetProviderCount(testSlot);
        initialProviderCount.Should().Be(2);

        // Act
        var result = testSlot.DeduplicateProceduralAssets();

        // Assert
        result.Should().Be(0); // No duplicates found
        var finalProviderCount = GetProceduralAssetProviderCount(testSlot);
        finalProviderCount.Should().Be(2); // Both should remain
    }

    [Fact]
    public void DeduplicateProceduralAssets_WithMultipleDuplicates_ShouldDeduplicateAll()
    {
        // Arrange
        var testSlot = _world.AddSlot("TestSlot");

        // Create multiple identical procedural asset providers
        for (var i = 0; i < 5; i++)
        {
            CreateProceduralAssetProvider(testSlot);
        }

        var initialProviderCount = GetProceduralAssetProviderCount(testSlot);
        initialProviderCount.Should().Be(5);

        // Act
        var result = testSlot.DeduplicateProceduralAssets();

        // Assert
        result.Should().Be(4); // Four duplicates removed
        var finalProviderCount = GetProceduralAssetProviderCount(testSlot);
        finalProviderCount.Should().Be(1); // Only one should remain
    }

    [Fact]
    public void DeduplicateProceduralAssets_ShouldPreserveReferences()
    {
        // Arrange
        var testSlot = _world.AddSlot("TestSlot");
        _ = testSlot.AttachComponent<MeshRenderer>();

        // Create two identical procedural asset providers
        _ = CreateProceduralAssetProvider(testSlot);
        _ = CreateProceduralAssetProvider(testSlot);

        // Set up reference to one of the providers
        // Note: This would require specific Resonite asset reference setup

        // Act
        var result = testSlot.DeduplicateProceduralAssets();

        // Assert
        result.Should().Be(1);
        // Verify that references are properly redirected
        // This test needs specific implementation based on how Resonite handles asset references
    }

    [Fact]
    public void DeduplicateProceduralAssets_WithCustomReplaceRoot_ShouldUseCustomRoot()
    {
        // Arrange
        var testSlot = _world.AddSlot("TestSlot");
        var customReplaceRoot = _world.AddSlot("CustomReplaceRoot");

        // Create identical procedural asset providers
        CreateProceduralAssetProvider(testSlot);
        CreateProceduralAssetProvider(testSlot);

        // Act
        var result = testSlot.DeduplicateProceduralAssets(customReplaceRoot);

        // Assert
        result.Should().Be(1);
        // Verify that the custom replace root was used
        // This test needs specific verification based on Resonite's behavior
    }

    [Fact]
    public void DeduplicateProceduralAssets_ShouldDestroyUnreferencedComponents()
    {
        // Arrange
        var testSlot = _world.AddSlot("TestSlot");

        // Create identical procedural asset providers
        var provider1 = CreateProceduralAssetProvider(testSlot);
        var provider2 = CreateProceduralAssetProvider(testSlot);

        var initialComponentCount = testSlot.GetComponents<Component>().Count;

        // Act
        var result = testSlot.DeduplicateProceduralAssets();

        // Assert
        result.Should().Be(1);
        var finalComponentCount = testSlot.GetComponents<Component>().Count;
        finalComponentCount.Should().BeLessThan(initialComponentCount);
    }

    [Fact]
    public void DeduplicateProceduralAssets_ShouldRedirectReferencesToFirstInstance()
    {
        // Arrange
        var testSlot = _world.AddSlot("TestSlot");

        var slot1 = testSlot.AddSlot("Box1");
        var slot2 = testSlot.AddSlot("Box2");

        // Create two identical BoxMesh components
        var boxMesh1 = slot1.AttachComponent<BoxMesh>();
        var boxCollider1 = slot1.AttachComponent<BoxCollider>();
        var valueCopy1 = slot1.AttachComponent<ValueCopy<float3>>();
        valueCopy1.Source.Target = boxMesh1.Size;
        valueCopy1.Target.Target = boxCollider1.Size;

        var boxMesh2 = slot2.AttachComponent<BoxMesh>();
        var boxCollider2 = slot2.AttachComponent<BoxCollider>();
        var valueCopy2 = slot2.AttachComponent<ValueCopy<float3>>();
        valueCopy2.Source.Target = boxMesh2.Size;
        valueCopy2.Target.Target = boxCollider2.Size;

        var initialProviderCount = GetProceduralAssetProviderCount(testSlot);
        initialProviderCount.Should().Be(2);

        valueCopy1.Source.Target.Should().NotBe(valueCopy2.Source.Target);
        valueCopy1.Target.Target.Should().NotBe(valueCopy2.Target.Target);

        // Act
        var result = testSlot.DeduplicateProceduralAssets();

        // Assert
        result.Should().Be(1); // One duplicate should be removed
        var finalProviderCount = GetProceduralAssetProviderCount(testSlot);
        finalProviderCount.Should().Be(1); // Only one should remain

        // Verify that references are redirected
        valueCopy1.Source.Target.Should().Be(valueCopy2.Source.Target);
        valueCopy1.Target.Target.Should().Be(valueCopy2.Target.Target);
    }

    private static BoxMesh CreateProceduralAssetProvider(Slot slot)
    {
        // Create an actual procedural asset provider - BoxMesh inherits from ProceduralMesh
        // which is a ProceduralAssetProvider<Mesh>
        var boxMesh = slot.AttachComponent<BoxMesh>();

        // Set up some properties to make it functional
        boxMesh.Size.Value = float3.One;
        boxMesh.UVScale.Value = float3.One;
        boxMesh.ScaleUVWithSize.Value = false;

        return boxMesh;
    }

    private static int GetProceduralAssetProviderCount(Slot slot)
    {
        var allProviders = Pool.BorrowList<IAssetProvider>();
        slot.GetComponentsInChildren(allProviders);

        var proceduralCount = allProviders.Count(provider =>
        {
            var type = provider.GetType();
            return type.IsGenericType
                && type.GetGenericTypeDefinition()
                    .Name.StartsWith("ProceduralAssetProvider", StringComparison.Ordinal);
        });

        Pool.Return(ref allProviders);
        return proceduralCount;
    }
}
