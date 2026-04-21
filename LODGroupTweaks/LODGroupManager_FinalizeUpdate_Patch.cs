using System.Collections.Generic;
using System.Reflection.Emit;
using FrooxEngine;
using HarmonyLib;
using ResoniteModLoader;

namespace EsnyaTweaks.LODGroupTweaks;

[HarmonyPatch(typeof(LODGroupManager), "FinalizeUpdate")]
internal static class LODGroupManager_FinalizeUpdate_Patch
{
    internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var lodGroupType = typeof(LODGroup);
        var lodsGetter = AccessTools.PropertyGetter(lodGroupType, "LODs");
        var lodsField = lodsGetter == null ? AccessTools.Field(lodGroupType, "LODs") : null;
        var validateMethod = AccessTools.Method(typeof(LODValidation), nameof(LODValidation.ValidateLODs));

        var injections = 0;
        foreach (var c in instructions)
        {
            yield return c;

            if (lodsGetter != null && validateMethod != null && c.opcode == OpCodes.Callvirt && Equals(c.operand, lodsGetter))
            {
                // After getter returns, duplicate list and call validator (leaving original on stack)
                yield return new CodeInstruction(OpCodes.Dup);
                yield return new CodeInstruction(OpCodes.Call, validateMethod);
                injections++;
            }
            else if (lodsField != null && validateMethod != null && c.opcode == OpCodes.Ldfld && Equals(c.operand, lodsField))
            {
                // After field load, duplicate list and call validator (leaving original on stack)
                yield return new CodeInstruction(OpCodes.Dup);
                yield return new CodeInstruction(OpCodes.Call, validateMethod);
                injections++;
            }
        }

        if (injections > 0)
        {
            ResoniteMod.DebugFunc(() => $"Injected LOD validation at {injections} point(s) in LODGroupManager.FinalizeUpdate");
        }
    }

    // Local validator removed in favor of shared LODValidation.ValidateLODs
}
