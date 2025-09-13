using System.Reflection;
using System.Runtime.CompilerServices;
using HarmonyLib;
using EsnyaTweaks.Common.Modding;
using ResoniteModLoader;
#if DEBUG
using ResoniteHotReloadLib;
#endif

[assembly: InternalsVisibleTo("EsnyaTweaks.InventoryUITweaks.Tests")]

namespace EsnyaTweaks.InventoryUITweaks;

/// <inheritdoc/>
public class InventoryUITweaksMod : EsnyaResoniteMod
{
    /// <inheritdoc/>
    public override void OnEngineInit()
    {
        harmony.PatchAll();

#if DEBUG
        HotReloader.RegisterForHotReload(this);
#endif
    }

#if DEBUG
    /// <inheritdoc/>
    public static void BeforeHotReload()
    {
        harmony.UnpatchAll(HarmonyId);
    }

    /// <inheritdoc/>
    public static void OnHotReload(ResoniteMod _)
    {
        harmony.PatchAll();
    }
#endif

    internal static string HarmonyId => $"com.nekometer.esnya.{ThisAssembly.GetName()}";

    private static Assembly ThisAssembly => typeof(InventoryUITweaksMod).Assembly;

    private static readonly Harmony harmony = new(HarmonyId);
}
