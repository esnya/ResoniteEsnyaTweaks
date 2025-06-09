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
        name.Should().NotBe("Unknown");
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
        author.Should().NotBe("Unknown");
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
        version.Should().NotBe("Unknown");
        version.Should().NotBe("0.0.0");
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
