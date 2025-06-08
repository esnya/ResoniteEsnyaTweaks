using FluentAssertions;
using Xunit;

namespace EsnyaTweaks.AssetOptimizationTweaks.Tests;

/// <summary>
/// Tests for the AssetOptimizationTweaksMod class.
/// </summary>
public class AssetOptimizationTweaksModTests
{
    [Fact]
    public void Name_ShouldReturnAssemblyTitle()
    {
        // Arrange
        var mod = new AssetOptimizationTweaksMod();

        // Act
        var name = mod.Name;

        // Assert
        name.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Author_ShouldReturnAssemblyCompany()
    {
        // Arrange
        var mod = new AssetOptimizationTweaksMod();

        // Act
        var author = mod.Author;

        // Assert
        author.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Version_ShouldReturnAssemblyVersion()
    {
        // Arrange
        var mod = new AssetOptimizationTweaksMod();

        // Act
        var version = mod.Version;

        // Assert
        version.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Link_ShouldReturnRepositoryUrl()
    {
        // Arrange
        var mod = new AssetOptimizationTweaksMod();

        // Act
        var link = mod.Link;

        // Assert
        link.Should().NotBeNullOrEmpty();
        link.Should().StartWith("https://");
    }
}
