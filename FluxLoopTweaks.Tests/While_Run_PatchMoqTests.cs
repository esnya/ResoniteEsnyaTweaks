using System.Reflection;
using FluentAssertions;
using HarmonyLib;
using ProtoFlux.Runtimes.Execution.Nodes; // for While type
using Xunit;

namespace EsnyaTweaks.FluxLoopTweaks.Tests;

public static class While_Run_PatchTests
{
    [Fact]
    public static void Patch_Should_Have_HarmonyPatch_Attribute()
    {
        var patchType = typeof(While_Run_Patch);
        patchType
            .GetCustomAttribute<HarmonyPatch>()
            .Should()
            .NotBeNull("While_Run_Patch class should have HarmonyPatch attribute");
    }

    [Fact]
    public static void Patch_Should_Target_While_Type()
    {
        var patchType = typeof(While_Run_Patch);
        var attribute = patchType.GetCustomAttribute<HarmonyPatch>();
        attribute.Should().NotBeNull();
        attribute!.info.declaringType.Should().Be<While>("the patch should target the While type");
    }

    [Fact]
    public static void Patch_Should_Target_Run_Method()
    {
        var patchType = typeof(While_Run_Patch);
        var attribute = patchType.GetCustomAttribute<HarmonyPatch>();
        attribute.Should().NotBeNull();
        attribute!.info.methodName.Should().Be("Run");
    }

    [Fact]
    public static void Patch_Should_Have_Prefix_Method()
    {
        var method = typeof(While_Run_Patch).GetMethod(
            "Prefix",
            BindingFlags.Static | BindingFlags.NonPublic
        );
        method.Should().NotBeNull();
        method!.ReturnType.Should().Be<bool>("Prefix method should return bool");
    }

    [Fact]
    public static void Patch_Should_Be_Internal_Class()
    {
        var patchType = typeof(While_Run_Patch);
        patchType.IsPublic.Should().BeFalse("patch class should be internal");
    }
}
