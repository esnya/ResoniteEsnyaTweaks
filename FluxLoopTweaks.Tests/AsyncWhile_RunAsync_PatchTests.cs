using System.Reflection;
using System.Threading.Tasks;
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

    [Fact]
    public static async Task Prefix_Should_Run_AsyncWhile_Loop()
    {
        var engine = new ProtoFlux.Runtimes.Execution.Engine();
        var context = new ProtoFlux.Runtimes.Execution.FrooxEngineContext();
        context.Engine.UpdateTick = 0;
        var iteration = 0;
        var asyncWhile = new ProtoFlux.Runtimes.Execution.Nodes.AsyncWhile
        {
            Condition = new ProtoFlux.Runtimes.Execution.Nodes.ValueInput<bool>
            {
                Evaluator = _ => iteration < 3,
            },
            LoopStart = new ProtoFlux.Runtimes.Execution.Nodes.AsyncCall(),
            LoopIteration = new ProtoFlux.Runtimes.Execution.Nodes.AsyncCall
            {
                Body = ctx =>
                {
                    iteration++;
                    ctx.Engine.UpdateTick++;
                    return Task.CompletedTask;
                },
            },
            LoopEnd = new ProtoFlux.Runtimes.Execution.Nodes.Continuation
            {
                Target = new ProtoFlux.Core.DummyOperation(),
            },
        };
        EsnyaTweaks.FluxLoopTweaks.FluxLoopTweaksMod.TimeoutMs = 1000;
        Task<ProtoFlux.Core.IOperation>? task = null;
        var ret = AsyncWhile_RunAsync_Patch.Prefix(asyncWhile, context, ref task!);
        ret.Should().BeFalse();
        var op = await task!;
        op.Should().BeSameAs(asyncWhile.LoopEnd.Target);
        context.AbortExecution.Should().BeFalse();
    }

    [Fact]
    public static async Task Prefix_Should_Abort_When_Timeout()
    {
        var context = new ProtoFlux.Runtimes.Execution.FrooxEngineContext();
        var asyncWhile = new ProtoFlux.Runtimes.Execution.Nodes.AsyncWhile
        {
            Condition = new ProtoFlux.Runtimes.Execution.Nodes.ValueInput<bool>
            {
                Evaluator = _ => true,
            },
            LoopStart = new ProtoFlux.Runtimes.Execution.Nodes.AsyncCall(),
            LoopIteration = new ProtoFlux.Runtimes.Execution.Nodes.AsyncCall(),
            LoopEnd = new ProtoFlux.Runtimes.Execution.Nodes.Continuation
            {
                Target = new ProtoFlux.Core.DummyOperation(),
            },
        };
        EsnyaTweaks.FluxLoopTweaks.FluxLoopTweaksMod.TimeoutMs = 0;
        Task<ProtoFlux.Core.IOperation>? task = null;
        AsyncWhile_RunAsync_Patch.Prefix(asyncWhile, context, ref task!);
        await Assert.ThrowsAsync<ProtoFlux.Runtimes.Execution.ExecutionAbortedException>(async () =>
            await task!
        );
        context.AbortExecution.Should().BeTrue();
        ResoniteModLoader.ResoniteMod.LastWarning.Should().NotBeNull();
    }

    [Fact]
    public static void Prefix_Should_Return_True_For_Non_FrooxEngine_Context()
    {
        var context = new ProtoFlux.Runtimes.Execution.ExecutionContext();
        var node = new ProtoFlux.Runtimes.Execution.Nodes.AsyncWhile();
        Task<ProtoFlux.Core.IOperation>? task = null;
        var result = AsyncWhile_RunAsync_Patch.Prefix(node, context, ref task!);
        result.Should().BeTrue();
    }
}
