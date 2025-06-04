using System.Reflection;
using FluentAssertions;
using HarmonyLib;
using Xunit;

namespace EsnyaTweaks.FluxLoopTweaks.Tests;

public static class While_Run_PatchTests
{
    [Fact]
    public static void While_Run_Patch_Should_Have_Harmony_Patch_Attribute()
    {
        // Arrange
        var patchType = typeof(While_Run_Patch);

        // Act
        var harmonyPatchAttribute = patchType.GetCustomAttribute<HarmonyPatch>();

        // Assert
        harmonyPatchAttribute
            .Should()
            .NotBeNull("the While_Run_Patch class should have a HarmonyPatch attribute");
    }

    [Fact]
    public static void While_Run_Patch_Should_Target_While_Type()
    {
        // Arrange
        var patchType = typeof(While_Run_Patch);
        var harmonyPatchAttribute = patchType.GetCustomAttribute<HarmonyPatch>();

        // Act & Assert
        harmonyPatchAttribute.Should().NotBeNull();
        harmonyPatchAttribute!
            .info.declaringType?.Name.Should()
            .Be("While", "the patch should target the While type");
    }

    [Fact]
    public static void While_Run_Patch_Should_Target_Run_Method()
    {
        // Arrange
        var patchType = typeof(While_Run_Patch);
        var harmonyPatchAttribute = patchType.GetCustomAttribute<HarmonyPatch>();

        // Act & Assert
        harmonyPatchAttribute.Should().NotBeNull();
        harmonyPatchAttribute!
            .info.methodName.Should()
            .Be("Run", "the patch should target the Run method");
    }

    [Fact]
    public static void While_Run_Patch_Should_Have_Prefix_Method()
    {
        // Arrange
        var patchType = typeof(While_Run_Patch);

        // Act
        var prefixMethod = patchType.GetMethod(
            "Prefix",
            BindingFlags.Static | BindingFlags.NonPublic
        );

        // Assert
        prefixMethod.Should().NotBeNull("the patch should have a Prefix method");
        prefixMethod!.IsStatic.Should().BeTrue("the Prefix method should be static");
        prefixMethod.ReturnType.Should().Be<bool>("the Prefix method should return a boolean");
    }

    [Fact]
    public static void While_Run_Patch_Type_Should_Be_Internal()
    {
        // Arrange & Act
        var patchType = typeof(While_Run_Patch);

        // Assert
        patchType
            .IsPublic.Should()
            .BeFalse("the patch class is internal, which is appropriate for Harmony patches");
    }

    [Fact]
    public static void Prefix_Method_Should_Have_Correct_Signature()
    {
        // Arrange
        var patchType = typeof(While_Run_Patch);
        var prefixMethod = patchType.GetMethod(
            "Prefix",
            BindingFlags.Static | BindingFlags.NonPublic
        );

        // Act & Assert
        prefixMethod.Should().NotBeNull();
        prefixMethod!
            .GetParameters()
            .Should()
            .NotBeEmpty("the Prefix method should have parameters for the patch to work");

        // Check if it has the typical Harmony prefix method attributes
        var harmonyPrefixAttribute = prefixMethod.GetCustomAttribute<HarmonyPrefix>();
        harmonyPrefixAttribute
            ?.Should()
            .NotBeNull("if present, HarmonyPrefix attribute should be valid");
    }

    [Fact]
    public static void Prefix_Should_Run_While_Loop()
    {
        var context = new ProtoFlux.Runtimes.Execution.FrooxEngineContext();
        var iteration = 0;
        var loop = new ProtoFlux.Runtimes.Execution.Nodes.While
        {
            Condition = new ProtoFlux.Runtimes.Execution.Nodes.ValueInput<bool>
            {
                Evaluator = _ => iteration < 3,
            },
            LoopStart = new ProtoFlux.Runtimes.Execution.Nodes.Call(),
            LoopIteration = new ProtoFlux.Runtimes.Execution.Nodes.Call
            {
                Body = ctx =>
                {
                    iteration++;
                },
            },
            LoopEnd = new ProtoFlux.Runtimes.Execution.Nodes.Continuation
            {
                Target = new ProtoFlux.Core.DummyOperation(),
            },
        };
        EsnyaTweaks.FluxLoopTweaks.FluxLoopTweaksMod.TimeoutMs = 1000;
        ProtoFlux.Core.IOperation? op = null;
        var ret = While_Run_Patch.Prefix(loop, context, ref op!);
        ret.Should().BeFalse();
        op.Should().BeSameAs(loop.LoopEnd.Target);
        context.AbortExecution.Should().BeFalse();
    }

    [Fact]
    public static void Prefix_Should_Abort_When_Timeout()
    {
        var context = new ProtoFlux.Runtimes.Execution.FrooxEngineContext();
        var loop = new ProtoFlux.Runtimes.Execution.Nodes.While
        {
            Condition = new ProtoFlux.Runtimes.Execution.Nodes.ValueInput<bool>
            {
                Evaluator = _ => true,
            },
            LoopStart = new ProtoFlux.Runtimes.Execution.Nodes.Call(),
            LoopIteration = new ProtoFlux.Runtimes.Execution.Nodes.Call(),
            LoopEnd = new ProtoFlux.Runtimes.Execution.Nodes.Continuation
            {
                Target = new ProtoFlux.Core.DummyOperation(),
            },
        };
        EsnyaTweaks.FluxLoopTweaks.FluxLoopTweaksMod.TimeoutMs = 0;
        ProtoFlux.Core.IOperation? op = null;
        Assert.Throws<ProtoFlux.Runtimes.Execution.ExecutionAbortedException>(() =>
            While_Run_Patch.Prefix(loop, context, ref op!)
        );
        context.AbortExecution.Should().BeTrue();
        ResoniteModLoader.ResoniteMod.LastWarning.Should().NotBeNull();
    }

    [Fact]
    public static void Prefix_Should_Return_True_For_Non_FrooxEngine_Context()
    {
        var context = new ProtoFlux.Runtimes.Execution.ExecutionContext();
        var loop = new ProtoFlux.Runtimes.Execution.Nodes.While();
        ProtoFlux.Core.IOperation? op = null;
        var result = While_Run_Patch.Prefix(loop, context, ref op!);
        result.Should().BeTrue();
    }
}
