using System.Reflection;
using Xunit;
using FluentAssertions;

namespace EsnyaTweaks.UniLogTweaks.Tests;

public static class UniLogTweaksModTests
{
    [Fact]
    public static void Mod_Should_Have_Valid_Name()
    {
        var mod = new UniLogTweaksMod();
        mod.Name.Should().Be("EsnyaTweaks.UniLogTweaks");
        mod.Name.Should().NotBe("Unknown");
    }

    [Fact]
    public static void Mod_Should_Have_Valid_Author()
    {
        var mod = new UniLogTweaksMod();
        mod.Author.Should().Be("esnya");
        mod.Author.Should().NotBe("Unknown");
    }

    [Fact]
    public static void Mod_Should_Have_Valid_Version()
    {
        var mod = new UniLogTweaksMod();
        mod.Version.Should().MatchRegex(@"^\d+\.\d+\.\d+(?:\+[A-Za-z][A-Za-z0-9]*)?$");
        mod.Version.Should().NotBe("Unknown");
        mod.Version.Should().NotBe("0.0.0");
    }

    // HarmonyId の検証は共通基底のテスト（Common.Tests）へ集約

    [Fact]
    public static void Mod_Should_Implement_ResoniteMod()
    {
        var mod = new UniLogTweaksMod();
        mod.Should().BeAssignableTo<ResoniteModLoader.ResoniteMod>();
    }
}
