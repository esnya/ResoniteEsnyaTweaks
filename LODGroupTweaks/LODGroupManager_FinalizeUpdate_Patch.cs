using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using FrooxEngine;
using HarmonyLib;
using ResoniteModLoader;

namespace EsnyaTweaks.LODGroupTweaks;

[HarmonyPatch]
internal static class LODGroupManager_FinalizeUpdate_Patch
{

    [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by Harmony via reflection")]
    [SuppressMessage("Performance", "CA1859", Justification = "Harmony requires MethodBase return type")]
    private static MethodBase TargetMethod()
    {
        return typeof(LODGroupManager).Method("FinalizeUpdate");
    }

    [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by Harmony via reflection")]
    [SuppressMessage("Performance", "CA1859", Justification = "Harmony requires IEnumerable signature for transpilers")]
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var codes = new List<CodeInstruction>(instructions);
        try
        {
            var lodGroupType = typeof(LODGroup);
            var lodsProp = lodGroupType.GetProperty("LODs", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var lodsGetter = lodsProp?.GetGetMethod(true);
            var lodsField = lodsProp == null
                ? lodGroupType.GetField("LODs", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                : null;
            var validateMethod = typeof(LODGroupManager_FinalizeUpdate_Patch)
                .GetMethod(nameof(ValidateLODs), BindingFlags.Static | BindingFlags.NonPublic);

            var injections = 0;
            for (var i = 0; i < codes.Count; i++)
            {
                var c = codes[i];
                if (lodsGetter != null && validateMethod != null && c.opcode == OpCodes.Callvirt && Equals(c.operand, lodsGetter))
                {
                    // After getter returns, duplicate list and call validator (leaving original on stack)
                    codes.Insert(++i, new CodeInstruction(OpCodes.Dup));
                    codes.Insert(++i, new CodeInstruction(OpCodes.Call, validateMethod));
                    injections++;
                }
                else if (lodsField != null && validateMethod != null && c.opcode == OpCodes.Ldfld && Equals(c.operand, lodsField))
                {
                    // After field load, duplicate list and call validator (leaving original on stack)
                    codes.Insert(++i, new CodeInstruction(OpCodes.Dup));
                    codes.Insert(++i, new CodeInstruction(OpCodes.Call, validateMethod));
                    injections++;
                }
            }

            if (injections > 0)
            {
                ResoniteMod.DebugFunc(() => $"Injected LOD validation at {injections} point(s) in LODGroupManager.FinalizeUpdate");
            }
        }
        catch (Exception ex)
        {
            ResoniteMod.Warn($"FinalizeUpdate transpiler failed: {ex}");
        }

        return codes;
    }

    // Non-intrusive validator: logs issues in Debug, does not modify the collection or stack value in the caller
    private static void ValidateLODs(IEnumerable<LODGroup.LOD> src)
    {
        if (!ResoniteMod.IsDebugEnabled())
        {
            return;
        }

        var hasNull = false;
        var nonDescendingFound = false;
        var prev = float.PositiveInfinity;

        var heights = Elements.Core.Pool.BorrowList<float>();
        try
        {
            foreach (var l in src)
            {
                if (l == null)
                {
                    hasNull = true;
                    continue;
                }
                var h = GetHeight(l);
                heights.Add(h);
                if (h >= prev)
                {
                    nonDescendingFound = true;
                }
                prev = h;
            }

            if ((hasNull || nonDescendingFound) && TryMarkLogged(src))
            {
                ResoniteMod.DebugFunc(() =>
                    $"Detected LOD violations: " +
                    (hasNull ? "contains nulls; " : string.Empty) +
                    (nonDescendingFound ? "order not descending; " : string.Empty) +
                    $"heights=[{string.Join(", ", heights)}]");
            }
        }
        finally
        {
            Elements.Core.Pool.Return(ref heights);
        }
    }

    // Deduplicate logs per LODs collection instance during runtime (no group context available here)
    [SuppressMessage("Style", "IDE0028")]
    [SuppressMessage("Style", "IDE0090")]
    private static ConditionalWeakTable<object, object> LoggedSources { get; } = new();

    private static bool TryMarkLogged(object src)
    {
        if (src == null)
        {
            return false;
        }
        lock (LoggedSources)
        {
            if (LoggedSources.TryGetValue(src, out _))
            {
                return false;
            }
            LoggedSources.Add(src, new object());
            return true;
        }
    }


    private static float GetHeight(LODGroup.LOD l)
    {
        try
        {
            return l?.ScreenRelativeTransitionHeight.Value ?? 0f;
        }
        catch
        {
            return 0f;
        }
    }
}
