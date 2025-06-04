using System.Reflection;
using FluentAssertions;
using Xunit;

namespace EsnyaTweaks.PhotonDustTweaks.Tests;

internal static class PhotonDustTweaksModTests
{
    [Fact]
    public static void Mod_Should_Have_Valid_Name()
    {
        var mod = new PhotonDustTweaksMod();
        mod.Name.Should().Be("EsnyaTweaks.PhotonDustTweaks");
    }

    [Fact]
    public static void Mod_Should_Have_Valid_Author()
    {
        var mod = new PhotonDustTweaksMod();
        mod.Author.Should().Be("esnya");
    }

    [Fact]
    public static void Mod_Should_Have_Valid_Version()
    {
        var mod = new PhotonDustTweaksMod();
        mod.Version.Should().MatchRegex(@"^\d+\.\d+\.\d+(?:\+[A-Za-z][A-Za-z0-9]*)?$");
    }

    [Fact]
    public static void HarmonyId_Should_Be_Valid()
    {
        var harmonyId = typeof(PhotonDustTweaksMod)
            .GetProperty("HarmonyId", BindingFlags.NonPublic | BindingFlags.Static)
            ?.GetValue(null)
            ?.ToString();

        harmonyId.Should().NotBeNullOrEmpty();
        harmonyId.Should().Contain("com.nekometer.esnya");
        harmonyId.Should().Contain("PhotonDustTweaks");
    }

    [Fact]
    public static void Mod_Should_Implement_ResoniteMod()
    {
        var mod = new PhotonDustTweaksMod();
        mod.Should().BeAssignableTo<ResoniteModLoader.ResoniteMod>();
    }
}
