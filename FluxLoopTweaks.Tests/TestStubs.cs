using System;
using System.Threading.Tasks;

namespace ProtoFlux.Core
{
    public interface IOperation { }

    public class DummyOperation : IOperation { }
}

namespace ProtoFlux.Runtimes.Execution
{
    public interface IExecutionRuntime { }

    public class ExecutionContext
    {
        public bool AbortExecution { get; set; }
    }

    public class ExecutionAbortedException : Exception
    {
        public ExecutionAbortedException(
            IExecutionRuntime? runtime,
            object node,
            ProtoFlux.Core.IOperation operation,
            bool isAsync
        ) { }
    }
}

namespace FrooxEngine.ProtoFlux
{
    public class Engine
    {
        public int UpdateTick { get; set; }
    }

    public class FrooxEngineContext : ProtoFlux.Runtimes.Execution.ExecutionContext
    {
        public Engine Engine { get; } = new();
    }
}

namespace ProtoFlux.Runtimes.Execution.Nodes
{
    using FrooxEngine.ProtoFlux;
    using ProtoFlux.Core;
    using ProtoFlux.Runtimes.Execution;

    public class ValueInput<T>
    {
        private readonly Func<ExecutionContext, T> _func;

        public ValueInput(Func<ExecutionContext, T> func) => _func = func;

        public T Evaluate(ExecutionContext context, bool defaultValue) => _func(context);
    }

    public class Call
    {
        public Action<ExecutionContext>? ExecuteAction { get; set; }
        public IOperation Target { get; set; } = new DummyOperation();

        public void Execute(ExecutionContext context) => ExecuteAction?.Invoke(context);
    }

    public class AsyncCall
    {
        public Func<FrooxEngineContext, Task>? ExecuteAsyncFunc { get; set; }
        public IOperation Target { get; set; } = new DummyOperation();

        public Task ExecuteAsync(FrooxEngineContext context) =>
            ExecuteAsyncFunc?.Invoke(context) ?? Task.CompletedTask;
    }

    public class Continuation
    {
        public IOperation Target { get; set; } = new DummyOperation();
    }

    public class While
    {
        public Call LoopStart { get; set; } = new();
        public ValueInput<bool> Condition { get; set; } = new(_ => false);
        public Call LoopIteration { get; set; } = new();
        public Continuation LoopEnd { get; set; } = new();
        public IExecutionRuntime Runtime { get; set; } = null!;
    }

    public class AsyncWhile
    {
        public ValueInput<bool> Condition { get; set; } = new(_ => false);
        public AsyncCall LoopStart { get; set; } = new();
        public AsyncCall LoopIteration { get; set; } = new();
        public Continuation LoopEnd { get; set; } = new();
        public IExecutionRuntime Runtime { get; set; } = null!;
    }
}

namespace ResoniteModLoader
{
    public class ResoniteMod
    {
        public virtual object? GetConfiguration() => null;

        public static void Warn(string message) { }
    }

    public interface ModConfiguration
    {
        T GetValue<T>(ModConfigurationKey<T> key);
    }

    public class ModConfigurationKey<T>
    {
        public ModConfigurationKey(string name, string description, Func<T> computeDefault) { }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class AutoRegisterConfigKeyAttribute : Attribute { }
}

namespace HarmonyLib
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class HarmonyPatch : Attribute
    {
        public HarmonyPatch(Type type, string methodName)
        {
            info = new PatchInfo { declaringType = type, methodName = methodName };
        }

        public PatchInfo info { get; }

        public class PatchInfo
        {
            public Type? declaringType;
            public string? methodName;
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class HarmonyPrefix : Attribute { }
}

namespace EsnyaTweaks.FluxLoopTweaks
{
    public partial class FluxLoopTweaksMod
    {
        public static int TimeoutMs { get; set; } = 1000;
    }
}
