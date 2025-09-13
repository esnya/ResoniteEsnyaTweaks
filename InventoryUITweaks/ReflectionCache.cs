using System.Reflection;
using FrooxEngine;
using HarmonyLib;
using ResoniteModLoader;

namespace EsnyaTweaks.InventoryUITweaks;

internal static class ReflectionCache
{
    internal static readonly MethodInfo BrowserDialogGoUp = AccessTools.Method(typeof(BrowserDialog), "GoUp", [typeof(int)]);

    internal static readonly FieldInfo? InventoryItemUIDirectory = GetInventoryItemUIDirectoryField();

    private static FieldInfo? GetInventoryItemUIDirectoryField()
    {
        var field = AccessTools.Field(typeof(InventoryItemUI), "Directory");
        if (field == null)
        {
            ResoniteMod.Msg("[InventoryUITweaks] Error: Could not find field 'Directory' on InventoryItemUI. Reflection access failed.");
        }

        return field;
    }
}
