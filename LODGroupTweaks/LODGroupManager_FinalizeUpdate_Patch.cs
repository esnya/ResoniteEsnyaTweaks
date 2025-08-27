using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Reflection.Emit;
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
        return AccessTools.Method(typeof(LODGroupManager), "FinalizeUpdate");
    }

    [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by Harmony via reflection")]
    [SuppressMessage("Performance", "CA1859", Justification = "Harmony requires IEnumerable signature for transpilers")]
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var codes = new List<CodeInstruction>(instructions);
        try
        {
            var lodGroupType = typeof(LODGroup);
            var lodsGetter = AccessTools.PropertyGetter(lodGroupType, "LODs");
            var lodsField = lodsGetter == null ? AccessTools.Field(lodGroupType, "LODs") : null;
            var validateMethod = AccessTools.Method(typeof(LODValidation), nameof(LODValidation.ValidateLODs));

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

    // Local validator removed in favor of shared LODValidation.ValidateLODs
}
