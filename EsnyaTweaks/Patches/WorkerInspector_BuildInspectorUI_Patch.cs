using FrooxEngine;
using FrooxEngine.UIX;
using HarmonyLib;
using System.ComponentModel;

namespace EsnyaTweaks.Patches;

[HarmonyPatchCategory("LODGroup Inspector"), Description("Add useful buttons to LODGroup inspector.")]

[HarmonyPatch(typeof(WorkerInspector), nameof(WorkerInspector.BuildInspectorUI))]
internal static class WorkerInspector_BuildInspectorUI_Patch
{
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

        Button(ui, "[Mod] Add LOD Level from children", () => SetupFromChildren(lodGroup));
        Button(ui, "[Mod] Remove LODGroups from children", () => RemoveFromChildren(lodGroup));
    }

    private static void Button(UIBuilder ui, string text, System.Action onClick)
    {
        var button = ui.Button(text);
        button.IsPressed.OnValueChange += (value) =>
        {
            if (value) onClick();
        };
    }

    private static void SetupFromChildren(LODGroup lodGroup)
    {
        lodGroup.AddLOD(0.01f, lodGroup.Slot);
    }

    private static void RemoveFromChildren(LODGroup lodGroup)
    {
        lodGroup.Slot.GetComponentsInChildren<LODGroup>(c => c != lodGroup).ForEach(c => c.Destroy());
    }
}

