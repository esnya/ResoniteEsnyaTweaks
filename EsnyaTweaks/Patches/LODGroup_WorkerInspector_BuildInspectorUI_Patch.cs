using FrooxEngine;
using FrooxEngine.UIX;
using HarmonyLib;
using System.ComponentModel;

namespace EsnyaTweaks.Patches;

[HarmonyPatchCategory("LODGroup Inspector"), Description("Add useful buttons to LODGroup inspector.")]

[HarmonyPatch(typeof(WorkerInspector), nameof(WorkerInspector.BuildInspectorUI))]
internal static class WorkerInspector_BuildInspectorUI_Patch
{
    private const string ADD_LABEL = "[Mod] Add LOD Level from children";
    private const string REMOVE_LABEL = "[Mod] Remove LODGroups from children";

    static void Postfix(Worker worker, UIBuilder ui)
    {
        if (worker is LODGroup lodGroup)
        {
            ResoniteModLoader.ResoniteMod.DebugFunc(() => $"LODGroup {lodGroup} found. Building inspector UI...");
            BuildInspectorUI(lodGroup, ui);
        }
    }

    private static void BuildInspectorUI(LODGroup lodGroup, UIBuilder ui)
    {
        Button(ui, ADD_LABEL, button => SetupFromChildren(button, lodGroup));
        Button(ui, REMOVE_LABEL, button => RemoveFromChildren(button, lodGroup));
    }

    private static void Button(UIBuilder ui, string text, System.Action<Button> onClick)
    {
        var button = ui.Button(text);
        button.IsPressed.OnValueChange += (value) =>
        {
            if (value) onClick(button);
        };
    }

    private static void SetupFromChildren(Button button, LODGroup lodGroup)
    {
        lodGroup.AddLOD(0.01f, lodGroup.Slot);
    }

    private static void RemoveFromChildren(Button button, LODGroup lodGroup)
    {
        var groups = lodGroup.Slot.GetComponentsInChildren<LODGroup>(c => c != lodGroup);
        int count = 0;
        foreach (var group in groups)
        {
            group.Destroy();
            count++;
        }

        button.LabelText = $"{REMOVE_LABEL} (Removed {count} groups)";
    }
}
