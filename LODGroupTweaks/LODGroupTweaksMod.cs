using System.Reflection;
using HarmonyLib;
using EsnyaTweaks.Common.Modding;
using ResoniteModLoader;
#if DEBUG
using ResoniteHotReloadLib;
#endif

namespace EsnyaTweaks.LODGroupTweaks;

/// <summary>Mod entry point for LODGroup tweaks.</summary>
public class LODGroupTweaksMod : EsnyaResoniteMod
{
    internal static string HarmonyId => $"com.nekometer.esnya.{ThisAssembly.GetName()}";

    private static Assembly ThisAssembly => typeof(LODGroupTweaksMod).Assembly;

    private static Harmony Harmony { get; } = new(HarmonyId);

#if DEBUG
    /// <summary>Unpatches all Harmony hooks before hot reload.</summary>
    public static void BeforeHotReload()
    {
        Harmony.UnpatchAll(HarmonyId);
    }

    /// <summary>Reapplies Harmony hooks after hot reload.</summary>
    /// <param name="mod">Unused mod instance.</param>
    public static void OnHotReload(ResoniteMod mod)
    {
        _ = mod;
        Harmony.PatchAll();
    }
#endif

    /// <inheritdoc/>
    public override void OnEngineInit()
    {
        Harmony.PatchAll();

#if DEBUG
        HotReloader.RegisterForHotReload(this);
#endif
    }
}
