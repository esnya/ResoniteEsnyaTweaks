using System.Reflection;
using System.Runtime.CompilerServices;
using HarmonyLib;
using EsnyaTweaks.Common.Modding;
#if DEBUG
using ResoniteModLoader;
using ResoniteHotReloadLib;
#endif

[assembly: InternalsVisibleTo("EsnyaTweaks.InventoryUITweaks.Tests")]

namespace EsnyaTweaks.InventoryUITweaks;

/// <inheritdoc/>
public class InventoryUITweaksMod : EsnyaResoniteMod
{
    internal static string HarmonyId => $"com.nekometer.esnya.{ThisAssembly.GetName()}";

    private static Harmony Harmony { get; } = new(HarmonyId);

    private static Assembly ThisAssembly => typeof(InventoryUITweaksMod).Assembly;

#if DEBUG
    /// <summary>Unpatches Harmony before hot reload.</summary>
    public static void BeforeHotReload()
    {
        Harmony.UnpatchAll(HarmonyId);
    }

    /// <summary>Reapplies Harmony patches after hot reload.</summary>
    /// <param name="mod">Unused reference to the reloaded mod.</param>
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
