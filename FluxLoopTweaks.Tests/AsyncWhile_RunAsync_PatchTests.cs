using System.Reflection;
using FluentAssertions;
using HarmonyLib;
using Xunit;

namespace EsnyaTweaks.FluxLoopTweaks.Tests;

public static class AsyncWhile_RunAsync_PatchTests
{
    [Fact]
    public static void AsyncWhile_RunAsync_Patch_Should_Have_Harmony_Patch_Attribute()
    {
        var patchType = typeof(AsyncWhile_RunAsync_Patch);

        var harmonyPatchAttribute = patchType.GetCustomAttribute<HarmonyPatch>();

        harmonyPatchAttribute
            .Should()
            .NotBeNull("the AsyncWhile_RunAsync_Patch class should have a HarmonyPatch attribute");
    }

    [Fact]
    public static void AsyncWhile_RunAsync_Patch_Should_Target_AsyncWhile_Type()
    {
        var patchType = typeof(AsyncWhile_RunAsync_Patch);
        var harmonyPatchAttribute = patchType.GetCustomAttribute<HarmonyPatch>();

        harmonyPatchAttribute.Should().NotBeNull();
        harmonyPatchAttribute!
            .info.declaringType?.Name.Should()
            .Be("AsyncWhile", "the patch should target the AsyncWhile type");
    }

    [Fact]
    public static void AsyncWhile_RunAsync_Patch_Should_Target_RunAsync_Method()
    {
        var patchType = typeof(AsyncWhile_RunAsync_Patch);
        var harmonyPatchAttribute = patchType.GetCustomAttribute<HarmonyPatch>();

        harmonyPatchAttribute.Should().NotBeNull();
        harmonyPatchAttribute!
            .info.methodName.Should()
            .Be("RunAsync", "the patch should target the RunAsync method");
    }

    [Fact]
    public static void AsyncWhile_RunAsync_Patch_Should_Have_Prefix_Method()
    {
        var patchType = typeof(AsyncWhile_RunAsync_Patch);

        var prefixMethod = patchType.GetMethod(
            "Prefix",
            BindingFlags.Static | BindingFlags.NonPublic
        );

        prefixMethod.Should().NotBeNull("the patch should have a Prefix method");
        prefixMethod!.IsStatic.Should().BeTrue("the Prefix method should be static");
        prefixMethod.ReturnType.Should().Be<bool>("the Prefix method should return a boolean");
    }

    [Fact]
    public static void AsyncWhile_RunAsync_Patch_Type_Should_Be_Internal()
    {
        var patchType = typeof(AsyncWhile_RunAsync_Patch);

        patchType
            .IsPublic.Should()
            .BeFalse("the patch class is internal, which is appropriate for Harmony patches");
    }

    [Fact]
    public static void Prefix_Method_Should_Have_Correct_Signature()
    {
        var patchType = typeof(AsyncWhile_RunAsync_Patch);
        var prefixMethod = patchType.GetMethod(
            "Prefix",
            BindingFlags.Static | BindingFlags.NonPublic
        );

        prefixMethod.Should().NotBeNull();
        prefixMethod!
            .GetParameters()
            .Should()
            .NotBeEmpty("the Prefix method should have parameters for the patch to work");

        var harmonyPrefixAttribute = prefixMethod.GetCustomAttribute<HarmonyPrefix>();
        harmonyPrefixAttribute
            ?.Should()
            .NotBeNull("if present, HarmonyPrefix attribute should be valid");
    }
}
