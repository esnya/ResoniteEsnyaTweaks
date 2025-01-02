using EsnyaTweaks.Attributes;
using FrooxEngine.ProtoFlux;
using HarmonyLib;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;
using ProtoFlux.Runtimes.Execution.Nodes;
using ResoniteModLoader;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace EsnyaTweaks.Patches;

[HarmonyPatchCategory("While Timeout"), TweakDescription("Timeout While/AsyncWhile")]
[HarmonyPatch(typeof(AsyncWhile), "RunAsync")]
internal static class AsyncWhile_RunAsync_Patch
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static async Task<IOperation> RunAsync(AsyncWhile instance, FrooxEngineContext context, ValueInput<bool> Condition, AsyncCall LoopStart, AsyncCall LoopIteration, Continuation LoopEnd, int timeout)
    {
        await LoopStart.ExecuteAsync(context).ConfigureAwait(true);

        var previousTick = context.Engine.UpdateTick;
        var stopwatch = Stopwatch.StartNew();

        while (Condition.Evaluate(context, defaultValue: false))
        {
            var currentTick = context.Engine.UpdateTick;
            if (currentTick != previousTick)
            {
                stopwatch.Restart();
            }
            else if (stopwatch.ElapsedMilliseconds > timeout || context.AbortExecution)
            {
                ResoniteMod.Warn($"AsyncWhile Timedout: {instance} ({stopwatch.ElapsedMilliseconds}ms)");
                context.AbortExecution = true;
                throw new ExecutionAbortedException(instance.Runtime as IExecutionRuntime, instance, LoopIteration.Target, isAsync: true);
            }
            previousTick = currentTick;

            await LoopIteration.ExecuteAsync(context).ConfigureAwait(true);
        }
        return LoopEnd.Target;
    }

    internal static bool Prefix(AsyncWhile __instance, ExecutionContext context, ref Task<IOperation> __result)
    {
        if (context is FrooxEngineContext frooxEngineContext)
        {
            __result = RunAsync(__instance, frooxEngineContext, __instance.Condition, __instance.LoopStart, __instance.LoopIteration, __instance.LoopEnd, EsnyaTweaksMod.timeoutMs);
            return true;
        }
        return false;
    }
}
