using FrooxEngine;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace EsnyaTweaks.Performance;


/// <summary>
/// Fixes TrackedDevicePositioner not flagging setting as registered.
/// 
/// <see href="https://github.com/Yellow-Dog-Man/Resonite-Issues/issues/3351">#3351</see>
/// </summary>

[HarmonyPatchCategory(nameof(PatchCategory.Performance))]
[HarmonyPatch(typeof(TrackedDevicePositioner), nameof(TrackedDevicePositioner.BeforeInputUpdate))]
[HarmonyPriority(-100)]
internal static class TrackedDevicePositioner_BeforeInputUpdate_Patch
{
    internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        foreach (var instruction in instructions)
        {
            yield return instruction;
            if (instruction.opcode == OpCodes.Call && instruction.operand is MethodInfo methodInfo && methodInfo.Name == "RegisterValueChanges")
            {
                EsnyaTweaksMod.DebugFunc(() => $"{methodInfo} found. Patching...");
                yield return new CodeInstruction(OpCodes.Ldarg_0);
                yield return new CodeInstruction(OpCodes.Ldc_I4_1);
                yield return new CodeInstruction(OpCodes.Stfld, AccessTools.Field(typeof(TrackedDevicePositioner), "_settingRegistered"));
            }
        }
    }
}