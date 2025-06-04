using System.Reflection;
using FluentAssertions;
using Xunit;

namespace EsnyaTweaks.UniLogTweaks.Tests;

internal static class UniLogTweaksModTests
{
    [Fact]
    public static void Mod_Should_Have_Valid_Name()
    {
        var mod = new UniLogTweaksMod();
        mod.Name.Should().Be("EsnyaTweaks.UniLogTweaks");
    }

    [Fact]
    public static void Mod_Should_Have_Valid_Author()
    {
        var mod = new UniLogTweaksMod();
        mod.Author.Should().Be("esnya");
    }

    [Fact]
    public static void Mod_Should_Have_Valid_Version()
    {
        var mod = new UniLogTweaksMod();
        mod.Version.Should().MatchRegex(@"^\d+\.\d+\.\d+(?:\+[A-Za-z][A-Za-z0-9]*)?$");
    }

    [Fact]
    public static void HarmonyId_Should_Be_Valid()
    {
        var harmonyId = typeof(UniLogTweaksMod)
            .GetProperty("HarmonyId", BindingFlags.NonPublic | BindingFlags.Static)
            ?.GetValue(null)
            ?.ToString();

        harmonyId.Should().NotBeNullOrEmpty();
        harmonyId.Should().Contain("com.nekometer.esnya");
        harmonyId.Should().Contain("UniLogTweaks");
    }

    [Fact]
    public static void Mod_Should_Implement_ResoniteMod()
    {
        var mod = new UniLogTweaksMod();
        mod.Should().BeAssignableTo<ResoniteModLoader.ResoniteMod>();
    }
}
