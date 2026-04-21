using System.Collections.Generic;
using System.Linq;
using FrooxEngine;
using FrooxEngine.PhotonDust;
using FrooxEngine.UIX;
using ResoniteModLoader;
using EsnyaTweaks.Common.UIX;

namespace EsnyaTweaks.PhotonDustTweaks;

internal static class PhotonDustTweaksExtensions
{
    private const string ADDFROMLABEL = "[Mod] Add modules from children";
    private const string REPLACEFROMLABEL = "[Mod] Replace modules from children";
    private const string DEDUPLICATELABEL = "[Mod] Deduplicate modules";
    private const string ADDTOLABEL = "[Mod] Add to parent ParticleStyle";
    private const string REMOVEFROMLABEL = "[Mod] Remove from parent ParticleStyle";

    public static void BuildInspectorUI(this IParticleSystemSubsystem module, UIBuilder ui)
    {
        ui.LocalButton(ADDTOLABEL, (button, _) => module.AddToStyle(button));
        ui.LocalButton(REMOVEFROMLABEL, (button, _) => module.RemoveFromStyle(button));
    }

    public static void AddToStyle(this IParticleSystemSubsystem module, IButton button)
    {
        if (module.Slot.GetComponentInParents<ParticleStyle>() is ParticleStyle style)
        {
            style.Modules.Add(module);
            button.LabelText = $"{ADDTOLABEL} (Added {module} to {style.Slot.Name}.{style.Name})";
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
                button.LabelText =
                    $"{REMOVEFROMLABEL} (Removed {removed} items from {style.Slot.Name}.{style.Name})";
                ResoniteMod.DebugFunc(() => $"Removed {removed} module(s) from {style}");
            }
        }
    }

    public static void BuildInspectorUI(this ParticleStyle style, UIBuilder ui)
    {
        ui.LocalButton(ADDFROMLABEL, (button, _) => style.AddFromStyle(button));
        ui.LocalButton(REPLACEFROMLABEL, (button, _) => style.ReplaceFromStyle(button));
        ui.LocalButton(DEDUPLICATELABEL, (button, _) => style.DeduplicateStyle(button));
    }

    public static List<IParticleSystemSubsystem> GetParticleModulesInChildren(
        this ParticleStyle style)
    {
        return style.Slot.GetComponentsInChildren<IParticleSystemSubsystem>(c =>
            c != style && c is not ParticleEmitter);
    }

    public static void AddFromStyle(this ParticleStyle style, IButton button)
    {
        var modules = style.GetParticleModulesInChildren();

        var prevCount = style.Modules.Count;
        style.Modules.AddRangeUnique(modules);
        var count = style.Modules.Count - prevCount;

        button.LabelText =
            $"{ADDFROMLABEL} (Added {count} items to {style.Slot.Name}.{style.Name})";
        ResoniteMod.DebugFunc(() => $"Added {count} module(s) to {style}");
    }

    public static void ReplaceFromStyle(this ParticleStyle style, IButton button)
    {
        var modules = style.GetParticleModulesInChildren();
        style.Modules.Clear();
        style.Modules.AddRange(modules);
        var count = modules.Count;
        button.LabelText =
            $"{ADDFROMLABEL} (Replaced {count} items to {style.Slot.Name}.{style.Name})";
        ResoniteMod.DebugFunc(() => $"Replaced {count} module(s) to {style}");
    }

    public static void DeduplicateStyle(this ParticleStyle style, IButton button)
    {
        var prevCount = style.Modules.Count;
        var modules = (from r in style.Modules select r).ToList();
        style.Modules.Clear();
        style.Modules.AddRangeUnique(modules);
        var count = prevCount - style.Modules.Count;
        button.LabelText =
            $"{DEDUPLICATELABEL} (Removed {count} items from {style.Slot.Name}.{style.Name})";
        ResoniteMod.DebugFunc(() => $"Deduplicated {count} module(s) from {style}");
    }
}
