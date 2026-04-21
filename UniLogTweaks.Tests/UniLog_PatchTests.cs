using System.Reflection;
using Elements.Core;
using HarmonyLib;
using Xunit;
using FluentAssertions;

namespace EsnyaTweaks.UniLogTweaks.Tests;

public static class UniLog_PatchTests
{
    private static System.Type GetPatchType()
    {
        return typeof(UniLogTweaksMod).Assembly.GetType(
            "EsnyaTweaks.UniLogTweaks.UniLog_Patch",
            true)!;
    }

    [Fact]
    public static void Patch_Should_Have_HarmonyPatch_Attribute()
    {
        GetPatchType().GetCustomAttribute<HarmonyPatch>().Should().NotBeNull();
    }

    [Fact]
    public static void Patch_Should_Target_UniLog_Class()
    {
        var attribute = GetPatchType().GetCustomAttribute<HarmonyPatch>();
        attribute.Should().NotBeNull();
        attribute!.info.declaringType.Should().Be(typeof(UniLog));
    }

    [Fact]
    public static void Patch_Should_Define_Log_Prefix()
    {
        var method = GetPatchType()
            .GetMethod("Log_Prefix", BindingFlags.Static | BindingFlags.NonPublic);
        method.Should().NotBeNull();
        method!.IsStatic.Should().BeTrue();
    }

    [Fact]
    public static void Patch_Should_Define_Warning_Prefix()
    {
        var method = GetPatchType()
            .GetMethod("Warning_Prefix", BindingFlags.Static | BindingFlags.NonPublic);
        method.Should().NotBeNull();
        method!.IsStatic.Should().BeTrue();
    }

    [Fact]
    public static void Patch_Should_Define_Error_Prefix()
    {
        var method = GetPatchType()
            .GetMethod("Error_Prefix", BindingFlags.Static | BindingFlags.NonPublic);
        method.Should().NotBeNull();
        method!.IsStatic.Should().BeTrue();
    }
}
