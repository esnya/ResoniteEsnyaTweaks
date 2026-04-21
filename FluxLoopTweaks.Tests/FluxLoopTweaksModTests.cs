using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Xunit;
using FluentAssertions;
using AutoFixture;

namespace EsnyaTweaks.FluxLoopTweaks.Tests;

public static class FluxLoopTweaksModTests
{
    [Fact]
    public static void Mod_Should_Have_Valid_Name()
    {
        // Arrange
        var mod = new FluxLoopTweaksMod();

        // Act
        var name = mod.Name;

        // Assert
        name.Should().NotBeNullOrEmpty();
        name.Should().Be("EsnyaTweaks.FluxLoopTweaks");
        name.Should().NotBe("Unknown");
    }

    [Fact]
    public static void Mod_Should_Have_Valid_Author()
    {
        // Arrange
        var mod = new FluxLoopTweaksMod();

        // Act
        var author = mod.Author;

        // Assert
        author.Should().NotBeNullOrEmpty();
        author.Should().Be("esnya");
        author.Should().NotBe("Unknown");
    }

    [Fact]
    public static void Mod_Should_Have_Valid_Version()
    {
        // Arrange
        var mod = new FluxLoopTweaksMod();

        // Act
        var version = mod.Version;

        // Assert
        version.Should().NotBeNullOrEmpty();
        version.Should().NotBe("Unknown");
        version.Should().NotBe("0.0.0");
        version
            .Should()
            .MatchRegex(
                @"^\d+\.\d+\.\d+(?:\+[A-Za-z][A-Za-z0-9]*)?$",
                "Version must be x.y.z with an optional +alpha suffix (no +hash allowed)");
    }

    [Theory]
    [InlineData(30000)] // デフォルト値
    public static void TimeoutMs_Should_Return_Expected_Value_When_Config_Is_Null(
        int expectedTimeout)
    {
        // Act
        var timeout = FluxLoopTweaksMod.TimeoutMs;

        // Assert
        timeout.Should().Be(expectedTimeout);
        timeout.Should().BePositive();
    }

    // HarmonyId の検証は共通基底のテスト（Common.Tests）へ集約

    [Fact]
    public static void Mod_Should_Implement_ResoniteModBase()
    {
        // Arrange
        var mod = new FluxLoopTweaksMod();

        // Assert
        mod.Should().BeAssignableTo<ResoniteModLoader.ResoniteMod>();
    }

    [Fact]
    public static void Mod_Properties_Should_Not_Change_Between_Instances()
    {
        // Arrange
        var mod1 = new FluxLoopTweaksMod();
        var mod2 = new FluxLoopTweaksMod();

        // Assert
        mod1.Name.Should().Be(mod2.Name);
        mod1.Author.Should().Be(mod2.Author);
        mod1.Version.Should().Be(mod2.Version);
    }

    [Fact]
    public static void Fixture_Should_Create_Mock_Objects()
    {
        // Arrange
        var fixture = new Fixture();

        // Act & Assert
        var randomString = fixture.Create<string>();
        var randomInt = fixture.Create<int>();

        randomString.Should().NotBeNullOrEmpty();
        randomInt.Should().NotBe(0);
    }

    [Fact]
    public static void Assembly_Should_Expose_Internals_To_Tests()
    {
        var assembly = typeof(FluxLoopTweaksMod).Assembly;

        var attribute = assembly
            .GetCustomAttributes<InternalsVisibleToAttribute>()
            .FirstOrDefault(attr => attr.AssemblyName == "EsnyaTweaks.FluxLoopTweaks.Tests");

        attribute
            .Should()
            .NotBeNull(
                "internals should be visible to the test assembly so that private members can be tested");
    }
}
