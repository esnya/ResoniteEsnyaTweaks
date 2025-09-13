using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using EsnyaTweaks.Common.Flux;
using FrooxEngine.ProtoFlux;
using HarmonyLib;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;
using ProtoFlux.Runtimes.Execution.Nodes;
using ResoniteModLoader;

namespace EsnyaTweaks.FluxLoopTweaks;

[HarmonyPatch(typeof(AsyncWhile), "RunAsync")]
internal static class AsyncWhile_RunAsync_Patch
{
    [SuppressMessage("Style", "SA1313", Justification = "Harmony magic parameters")]
    internal static bool Prefix(
        AsyncWhile __instance,
        ExecutionContext context,
        ref Task<IOperation> __result)
    {
        if (context is not FrooxEngineContext frooxEngineContext)
        {
            return true;
        }

        __result = RunAsync(
            __instance,
            frooxEngineContext,
            __instance.Condition,
            __instance.LoopStart,
            __instance.LoopIteration,
            __instance.LoopEnd,
            FluxLoopTweaksMod.TimeoutMs);
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static async Task<IOperation> RunAsync(
        AsyncWhile instance,
        FrooxEngineContext context,
        ValueInput<bool> condition,
        AsyncCall loopStart,
        AsyncCall loopIteration,
        Continuation loopEnd,
        int timeout)
    {
        await loopStart.ExecuteAsync(context).ConfigureAwait(true);

        var previousTick = context.Engine.UpdateTick;
        var stopwatch = Stopwatch.StartNew();

        while (condition.Evaluate(context, defaultValue: false))
        {
            var currentTick = context.Engine.UpdateTick;
            if (currentTick != previousTick)
            {
                stopwatch.Restart();
            }
            else if (LoopTimeoutPolicy.ShouldAbort(previousTick, currentTick, stopwatch.ElapsedMilliseconds, timeout, context.AbortExecution))
            {
                ResoniteMod.Warn($"AsyncWhile Timedout: {instance} ({stopwatch.ElapsedMilliseconds}ms)");
                context.AbortExecution = true;
                throw new ExecutionAbortedException(
                    instance.Runtime as IExecutionRuntime,
                    instance,
                    loopIteration.Target,
                    isAsync: true);
            }
            previousTick = currentTick;

            await loopIteration.ExecuteAsync(context).ConfigureAwait(true);
        }

        return loopEnd.Target;
    }
}
