using System.Threading.Tasks;
using FluentAssertions;
using FrooxEngine.ProtoFlux;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;
using ProtoFlux.Runtimes.Execution.Nodes;
using Xunit;

namespace EsnyaTweaks.FluxLoopTweaks.Tests;

public class FluxLoopPatchExecutionTests
{
    [Fact]
    public void While_Prefix_Should_Not_Run_For_Unsupported_Context()
    {
        var loop = new While();
        var context = new ExecutionContext();
        IOperation? result = null;

        var executed = false;
        loop.LoopStart.ExecuteAction = _ => executed = true;

        var ret = While_Run_Patch.Prefix(loop, context, ref result);

        ret.Should().BeTrue();
        executed.Should().BeFalse();
        result.Should().BeNull();
    }

    [Fact]
    public void While_Prefix_Should_Run_Loop()
    {
        var iterations = 0;
        var loop = new While
        {
            LoopStart = new Call { ExecuteAction = _ => { } },
            Condition = new ValueInput<bool>(_ => iterations++ < 2),
            LoopIteration = new Call { ExecuteAction = _ => { } },
            LoopEnd = new Continuation { Target = new DummyOperation() },
            Runtime = new DummyRuntime(),
        };
        var context = new FrooxEngineContext();
        IOperation? result = null;

        var ret = While_Run_Patch.Prefix(loop, context, ref result);

        ret.Should().BeFalse();
        iterations.Should().Be(2);
        result.Should().Be(loop.LoopEnd.Target);
    }

    [Fact]
    public async Task AsyncWhile_Prefix_Should_Run_Loop()
    {
        var iterations = 0;
        var asyncWhile = new AsyncWhile
        {
            LoopStart = new AsyncCall { ExecuteAsyncFunc = _ => Task.CompletedTask },
            Condition = new ValueInput<bool>(_ => iterations++ < 2),
            LoopIteration = new AsyncCall { ExecuteAsyncFunc = _ => Task.CompletedTask },
            LoopEnd = new Continuation { Target = new DummyOperation() },
            Runtime = new DummyRuntime(),
        };
        var context = new FrooxEngineContext();
        Task<IOperation>? result = null;

        var ret = AsyncWhile_RunAsync_Patch.Prefix(asyncWhile, context, ref result);

        ret.Should().BeFalse();
        var op = await result!;
        iterations.Should().Be(2);
        op.Should().Be(asyncWhile.LoopEnd.Target);
    }

    private class DummyRuntime : IExecutionRuntime { }
}
