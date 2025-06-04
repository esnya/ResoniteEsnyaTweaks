using System;
using System.Threading.Tasks;

namespace ProtoFlux.Core;

public interface IOperation { }

public class DummyOperation : IOperation { }

namespace ProtoFlux.Runtimes.Execution;

public interface IExecutionRuntime { }

public class ExecutionContext
{
    public bool AbortExecution { get; set; }
}

public class Engine
{
    public int UpdateTick { get; set; }
}

public class FrooxEngineContext : ExecutionContext
{
    public Engine Engine { get; } = new();
}

public class ExecutionAbortedException : Exception
{
    public ExecutionAbortedException(
        IExecutionRuntime? runtime,
        object? node,
        ProtoFlux.Core.IOperation operation,
        bool isAsync
    ) { }
}

namespace ProtoFlux.Runtimes.Execution.Nodes;

using ProtoFlux.Core;

public class AsyncCall
{
    public Func<FrooxEngineContext, Task>? Body { get; set; }
    public IOperation Target { get; set; } = new DummyOperation();

    public Task ExecuteAsync(FrooxEngineContext context) =>
        Body?.Invoke(context) ?? Task.CompletedTask;
}

public class Call
{
    public Action<ExecutionContext>? Body { get; set; }
    public IOperation Target { get; set; } = new DummyOperation();

    public void Execute(ExecutionContext context) => Body?.Invoke(context);
}

public class Continuation
{
    public IOperation Target { get; set; } = new DummyOperation();
}

public class ValueInput<T>
{
    public Func<FrooxEngineContext, T>? Evaluator { get; set; }

    public T Evaluate(FrooxEngineContext context, T defaultValue) =>
        Evaluator?.Invoke(context) ?? defaultValue;
}

public class AsyncWhile
{
    public ValueInput<bool> Condition { get; set; } = new();
    public AsyncCall LoopStart { get; set; } = new();
    public AsyncCall LoopIteration { get; set; } = new();
    public Continuation LoopEnd { get; set; } = new();
    public IExecutionRuntime? Runtime { get; set; }
}

public class While
{
    public ValueInput<bool> Condition { get; set; } = new();
    public Call LoopStart { get; set; } = new();
    public Call LoopIteration { get; set; } = new();
    public Continuation LoopEnd { get; set; } = new();
    public IExecutionRuntime? Runtime { get; set; }
}
