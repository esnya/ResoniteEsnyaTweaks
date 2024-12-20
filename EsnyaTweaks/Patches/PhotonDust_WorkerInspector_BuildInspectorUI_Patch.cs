using FrooxEngine;
using FrooxEngine.PhotonDust;
using FrooxEngine.UIX;
using HarmonyLib;
using ResoniteModLoader;
using System.ComponentModel;
using System.Linq;

namespace EsnyaTweaks.Patches;

[HarmonyPatchCategory("PhotonDust Inspector"), Description("Add add and remove to parent style buttons.")]

[HarmonyPatch(typeof(WorkerInspector), nameof(WorkerInspector.BuildInspectorUI))]
internal static class PhotonDust_WorkerInspector_BuildInspectorUI_Patch
{
    private const string ADD_FROM_LABEL = "[Mod] Add modules from children";
    private const string REPLACE_FROM_LABEL = "[Mod] Replace modules from children";
    private const string DEDUPLICATE_LABEL = "[Mod] Deduplicate modules";
    private const string ADD_TO_LABEL = "[Mod] Add to parent ParticleStyle";
    private const string REMOVE_FROM_LABEL = "[Mod] Remove from parent ParticleStyle";

    static void Postfix(Worker worker, UIBuilder ui)
    {
        if (worker is IParticleSystemSubsystem module)
        {
            ResoniteMod.DebugFunc(() => $"ParticleSystem module {module} found. Building inspector UI...");
            BuildInspectorUI(module, ui);
        }
        if (worker is ParticleStyle style)
        {
            ResoniteMod.DebugFunc(() => $"ParticleStyle {style} found. Building inspector UI...");
            BuildInspectorUI(style, ui);
        }
    }

    private static void BuildInspectorUI(IParticleSystemSubsystem module, UIBuilder ui)
    {

        Button(ui, ADD_TO_LABEL, button => AddToStyle(button, module));
        Button(ui, REMOVE_FROM_LABEL, button => RemoveFromStyle(button, module));
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
            button.LabelText = $"{ADD_TO_LABEL} (Added {module} to {style.Slot.Name}.{style.Name})";
            ResoniteMod.DebugFunc(() => $"Added {module} to {style}");
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
                button.LabelText = $"{REMOVE_FROM_LABEL} (Removed {removed} items from {style.Slot.Name}.{style.Name})";
                ResoniteMod.DebugFunc(() => $"Removed {removed} module(s) from {style}");
            }
        }
    }


    private static void BuildInspectorUI(ParticleStyle style, UIBuilder ui)
    {
        Button(ui, ADD_FROM_LABEL, button => AddFromStyle(button, style));
        Button(ui, REPLACE_FROM_LABEL, button => ReplaceFromStyle(button, style));
        Button(ui, DEDUPLICATE_LABEL, button => DeduplicateStyle(button, style));
    }


    private static void AddFromStyle(Button button, ParticleStyle style)
    {
        var modules = style.Slot.GetComponentsInChildren<IParticleSystemSubsystem>(c => c != style);

        var prevCount = style.Modules.Count;
        style.Modules.AddRangeUnique(modules);
        var count = style.Modules.Count - prevCount;

        button.LabelText = $"{ADD_FROM_LABEL} (Added {count} items to {style.Slot.Name}.{style.Name})";
        ResoniteMod.DebugFunc(() => $"Added {count} module(s) to {style}");
    }

    private static void ReplaceFromStyle(Button button, ParticleStyle style)
    {
        var modules = style.Slot.GetComponentsInChildren<IParticleSystemSubsystem>(c => c != style);
        style.Modules.Clear();
        style.Modules.AddRange(modules);
        var count = modules.Count;
        button.LabelText = $"{ADD_FROM_LABEL} (Replaced {count} items to {style.Slot.Name}.{style.Name})";
        ResoniteMod.DebugFunc(() => $"Replaced {count} module(s) to {style}");
    }

    private static void DeduplicateStyle(Button button, ParticleStyle style)
    {
        var prevCount = style.Modules.Count;
        var modules = (from r in style.Modules select r).ToList();
        style.Modules.Clear();
        style.Modules.AddRangeUnique(modules);
        var count = prevCount - style.Modules.Count;
        button.LabelText = $"{DEDUPLICATE_LABEL} (Removed {count} items from {style.Slot.Name}.{style.Name})";
        ResoniteMod.DebugFunc(() => $"Deduplicated {count} module(s) from {style}");
    }
}
