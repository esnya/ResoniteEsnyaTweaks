using System.Reflection;
using System.Runtime.CompilerServices;
using HarmonyLib;
using EsnyaTweaks.Common.Modding;
using ResoniteModLoader;
#if DEBUG
using ResoniteHotReloadLib;
#endif

[assembly: InternalsVisibleTo("EsnyaTweaks.AssetOptimizationTweaks.Tests")]

namespace EsnyaTweaks.AssetOptimizationTweaks;

/// <summary>
/// Asset optimization mod with advanced deduplication capabilities for Resonite.
/// Provides utilities to optimize assets by removing duplicates and improving memory usage.
/// </summary>
public class AssetOptimizationTweaksMod : EsnyaResoniteMod
{
    private static Assembly ThisAssembly => typeof(AssetOptimizationTweaksMod).Assembly;

    internal static string HarmonyId => $"com.nekometer.esnya.{ThisAssembly.GetName().Name}";

    private static readonly Harmony harmony = new(HarmonyId);

    /// <inheritdoc/>
    public override void OnEngineInit()
    {
        Init(this);

#if DEBUG
        HotReloader.RegisterForHotReload(this);
#endif
    }

    private static void Init(ResoniteMod _)
    {
        harmony.PatchAll();
    }

#if DEBUG
    /// <inheritdoc/>
    public static void BeforeHotReload()
    {
        harmony.UnpatchAll(HarmonyId);
    }

    /// <inheritdoc/>
    /// <param name="modInstance">The mod instance.</param>
    public static void OnHotReload(ResoniteMod modInstance)
    {
        Init(modInstance);
    }
#endif
}
