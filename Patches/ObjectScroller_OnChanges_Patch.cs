using System.Collections.Generic;
using System.Reflection.Emit;
using Elements.Core;
using EsnyaTweaks.Attributes;
using FrooxEngine;
using HarmonyLib;

namespace EsnyaTweaks.Patches;

[
    HarmonyPatchCategory("ObjectScroller"),
    TweakDescription("Fix Scaling of ObjectScroller (Only local).", defaultValue: false)
]
[HarmonyPatch(typeof(ObjectScroller), "OnChanges")]
internal static class ObjectScroller_OnChanges_Patch
{
    internal static IEnumerable<CodeInstruction> Transpiler(
        IEnumerable<CodeInstruction> instructions
    )
    {
        var mathxMin = typeof(MathX).Method(nameof(MathX.Min), new[] { typeof(float[]) });

        foreach (var instruction in instructions)
        {
            if (instruction.opcode == OpCodes.Ldc_I4_1)
            {
                yield return new CodeInstruction(OpCodes.Ldc_I4_2);
            }
            else if (instruction.operand?.Equals(mathxMin) ?? false)
            {
                yield return new CodeInstruction(OpCodes.Dup);
                yield return new CodeInstruction(OpCodes.Ldc_I4_1);
                yield return CodeInstruction.LoadLocal(12);
                yield return new CodeInstruction(OpCodes.Stelem_R4);
                yield return instruction;
            }
            else
            {
                yield return instruction;
            }
        }
    }
}
