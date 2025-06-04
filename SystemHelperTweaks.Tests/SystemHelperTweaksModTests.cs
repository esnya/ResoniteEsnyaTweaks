using System.Reflection;
using FluentAssertions;
using Xunit;

namespace EsnyaTweaks.SystemHelperTweaks.Tests;

public static class SystemHelperTweaksModTests
{
    [Fact]
    public static void Mod_Should_Have_Valid_Name()
    {
        var mod = new SystemHelperTweaksMod();
        mod.Name.Should().Be("EsnyaTweaks.SystemHelperTweaks");
    }

    [Fact]
    public static void Mod_Should_Have_Valid_Author()
    {
        var mod = new SystemHelperTweaksMod();
        mod.Author.Should().Be("esnya");
    }

    [Fact]
    public static void Mod_Should_Have_Valid_Version()
    {
        var mod = new SystemHelperTweaksMod();
        mod.Version.Should().MatchRegex(@"^\d+\.\d+\.\d+(?:\+[A-Za-z][A-Za-z0-9]*)?$");
    }

    [Fact]
    public static void HarmonyId_Should_Be_Valid()
    {
        var harmonyId = typeof(SystemHelperTweaksMod)
            .GetProperty("HarmonyId", BindingFlags.NonPublic | BindingFlags.Static)
            ?.GetValue(null)
            ?.ToString();

        harmonyId.Should().NotBeNullOrEmpty();
        harmonyId.Should().Contain("com.nekometer.esnya");
        harmonyId.Should().Contain("SystemHelperTweaks");
    }

    [Fact]
    public static void Mod_Should_Implement_ResoniteMod()
    {
        var mod = new SystemHelperTweaksMod();
        mod.Should().BeAssignableTo<ResoniteModLoader.ResoniteMod>();
    }
}
