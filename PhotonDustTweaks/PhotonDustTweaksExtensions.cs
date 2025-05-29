using System.Collections.Generic;
using System.Linq;
using FrooxEngine;
using FrooxEngine.PhotonDust;
using FrooxEngine.UIX;
using ResoniteModLoader;

namespace EsnyaTweaks.PhotonDustTweaks;

internal static class PhotonDustTweaksExtensions
{
    private const string ADD_FROM_LABEL = "[Mod] Add modules from children";
    private const string REPLACE_FROM_LABEL = "[Mod] Replace modules from children";
    private const string DEDUPLICATE_LABEL = "[Mod] Deduplicate modules";
    private const string ADD_TO_LABEL = "[Mod] Add to parent ParticleStyle";
    private const string REMOVE_FROM_LABEL = "[Mod] Remove from parent ParticleStyle";

    public static void BuildInspectorUI(this IParticleSystemSubsystem module, UIBuilder ui)
    {
        ui.LocalButton(ADD_TO_LABEL, (button, _) => module.AddToStyle(button));
        ui.LocalButton(REMOVE_FROM_LABEL, (button, _) => module.RemoveFromStyle(button));
    }

    public static void LocalButton(this UIBuilder ui, in string label, ButtonEventHandler localAction)
    {
        var button = ui.Button(label);
        button.LocalPressed += localAction;
    }

    public static void AddToStyle(this IParticleSystemSubsystem module, IButton button)
    {
        if (module.Slot.GetComponentInParents<ParticleStyle>() is ParticleStyle style)
        {
            style.Modules.Add(module);
            button.LabelText = $"{ADD_TO_LABEL} (Added {module} to {style.Slot.Name}.{style.Name})";
            ResoniteMod.DebugFunc(() => $"Added {module} to {style}");
        }
    }

    public static void RemoveFromStyle(this IParticleSystemSubsystem module, IButton button)
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


    public static void BuildInspectorUI(this ParticleStyle style, UIBuilder ui)
    {
        ui.LocalButton(ADD_FROM_LABEL, (button, _) => style.AddFromStyle(button));
        ui.LocalButton(REPLACE_FROM_LABEL, (button, _) => style.ReplaceFromStyle(button));
        ui.LocalButton(DEDUPLICATE_LABEL, (button, _) => style.DeduplicateStyle(button));
    }

    public static List<IParticleSystemSubsystem> GetParticleModulesInChildren(this ParticleStyle style)
    {
        return style.Slot.GetComponentsInChildren<IParticleSystemSubsystem>(c => c != style && c is not ParticleEmitter);
    }

    public static void AddFromStyle(this ParticleStyle style, IButton button)
    {
        var modules = style.GetParticleModulesInChildren();

        var prevCount = style.Modules.Count;
        style.Modules.AddRangeUnique(modules);
        var count = style.Modules.Count - prevCount;

        button.LabelText = $"{ADD_FROM_LABEL} (Added {count} items to {style.Slot.Name}.{style.Name})";
        ResoniteMod.DebugFunc(() => $"Added {count} module(s) to {style}");
    }

    public static void ReplaceFromStyle(this ParticleStyle style, IButton button)
    {
        var modules = style.GetParticleModulesInChildren();
        style.Modules.Clear();
        style.Modules.AddRange(modules);
        var count = modules.Count;
        button.LabelText = $"{ADD_FROM_LABEL} (Replaced {count} items to {style.Slot.Name}.{style.Name})";
        ResoniteMod.DebugFunc(() => $"Replaced {count} module(s) to {style}");
    }

    public static void DeduplicateStyle(this ParticleStyle style, IButton button)
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
