using FrooxEngine;
using FrooxEngine.PhotonDust;
using FrooxEngine.UIX;
using HarmonyLib;
using System.ComponentModel;

namespace EsnyaTweaks.Patches;

[HarmonyPatchCategory("PhotonDust Inspector"), Description("Add add and remove to parent style buttons.")]

[HarmonyPatch(typeof(WorkerInspector), nameof(WorkerInspector.BuildInspectorUI))]
internal static class PhotonDust_WorkerInspector_BuildInspectorUI_Patch
{
    private const string ADD_LABEL = "[Mod] Add to parent ParticleStyle";
    private const string REMOVE_LABEL = "[Mod] Remove from parent ParticleStyle";

    static void Postfix(Worker worker, UIBuilder ui)
    {
        if (worker is IParticleSystemSubsystem module)
        {
            ResoniteModLoader.ResoniteMod.DebugFunc(() => $"ParticleSystem module {module} found. Building inspector UI...");
            BuildInspectorUI(module, ui);
        }
    }

    private static void BuildInspectorUI(IParticleSystemSubsystem module, UIBuilder ui)
    {

        Button(ui, ADD_LABEL, button => AddToStyle(button, module));
        Button(ui, REMOVE_LABEL, button => RemoveFromStyle(button, module));
    }

    private static void Button(UIBuilder ui, string text, System.Action<Button> onClick)
    {
        var button = ui.Button(text);
        button.IsPressed.OnValueChange += (value) =>
        {
            if (value) onClick(button);
        };
    }

    private static void AddToStyle(Button button, IParticleSystemSubsystem module)
    {
        if (module.Slot.GetComponentInParents<ParticleStyle>() is ParticleStyle style)
        {
            style.Modules.Add(module);
        }
    }

    private static void RemoveFromStyle(Button button, IParticleSystemSubsystem module)
    {
        if (module.Slot.GetComponentInParents<ParticleStyle>() is ParticleStyle style)
        {
            var before = style.Modules.Count;
            style.Modules.Remove(module);
            var removed = before - style.Modules.Count;

            if (removed > 0)
            {
                button.LabelText = $"{REMOVE_LABEL} (Removed {removed} items from {style.Slot.Name}.{style.Name})";
                ResoniteModLoader.ResoniteMod.DebugFunc(() => $"Removed {removed} module(s) from {style}");
            }
        }
    }
}
