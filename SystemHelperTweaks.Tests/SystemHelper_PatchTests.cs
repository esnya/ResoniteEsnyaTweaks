using System.Reflection;
using FluentAssertions;
using HarmonyLib;
using SystemHelperClient;
using Xunit;

namespace EsnyaTweaks.SystemHelperTweaks.Tests;

public static class SystemHelper_PatchTests
{
    private static System.Type GetPatchType()
    {
        return typeof(SystemHelperTweaksMod).Assembly.GetType(
            "EsnyaTweaks.SystemHelperTweaks.SystemHelper_Patch",
            true
        )!;
    }

    [Fact]
    public static void Patch_Should_Have_HarmonyPatch_Attribute()
    {
        var patchType = GetPatchType();
        patchType.GetCustomAttribute<HarmonyPatch>().Should().NotBeNull();
    }

    [Fact]
    public static void Patch_Should_Target_SystemHelper_Class()
    {
        var patchType = GetPatchType();
        var attribute = patchType.GetCustomAttribute<HarmonyPatch>();
        attribute.Should().NotBeNull();
        attribute!.info.declaringType.Should().Be<SystemHelper>();
    }

    [Fact]
    public static void Patch_Should_Define_Ctor_Prefix()
    {
        var patchType = GetPatchType();
        var method = patchType.GetMethod("Ctor_Prefix", BindingFlags.Static | BindingFlags.Public);
        method.Should().NotBeNull();
        method!.IsStatic.Should().BeTrue();
    }

    [Fact]
    public static void Patch_Should_Define_GetClipboardData_Prefix()
    {
        var patchType = GetPatchType();
        var method = patchType.GetMethod(
            "GetClipboardData_Prefix",
            BindingFlags.Static | BindingFlags.Public
        );
        method.Should().NotBeNull();
        method!.IsStatic.Should().BeTrue();
    }
}
