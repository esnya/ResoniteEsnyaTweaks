﻿using FrooxEngine.ProtoFlux;
using HarmonyLib;
using ProtoFlux.Core;
using ProtoFlux.Runtimes.Execution;
using ProtoFlux.Runtimes.Execution.Nodes;
using ResoniteModLoader;
using System.ComponentModel;

namespace EsnyaTweaks.Patches;


[HarmonyPatchCategory("While Timeout"), Description("Timeout While/AsyncWhile")]
[HarmonyPatch(typeof(While), "Run")]
internal static class While_Run_Patch
{
    internal static bool Prefix(While __instance, ExecutionContext context, ref IOperation __result)
    {
        if (context is not FrooxEngineContext frooxEngineContext) return false;
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        __instance.LoopStart.Execute(context);
        while (__instance.Condition.Evaluate(context, defaultValue: false))
        {
            if (stopwatch.ElapsedMilliseconds > EsnyaTweaksMod.timeoutMs || context.AbortExecution)
            {
                ResoniteMod.Warn($"While Timedout: {__instance} ({stopwatch.ElapsedMilliseconds}ms)");
                context.AbortExecution = true;
                throw new ExecutionAbortedException(__instance.Runtime as IExecutionRuntime, __instance, __instance.LoopIteration.Target, isAsync: false);
            }

            __instance.LoopIteration.Execute(context);
        }

        __result = __instance.LoopEnd.Target;

        return true;
    }
}