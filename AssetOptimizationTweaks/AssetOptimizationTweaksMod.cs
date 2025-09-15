using System.Runtime.CompilerServices;
using EsnyaTweaks.Common.Modding;
#if DEBUG
using ResoniteModLoader;
#endif

[assembly: InternalsVisibleTo("EsnyaTweaks.AssetOptimizationTweaks.Tests")]

namespace EsnyaTweaks.AssetOptimizationTweaks;

/// <summary>
/// Mod entry point for asset optimization tweaks.
/// </summary>
public class AssetOptimizationTweaksMod : EsnyaResoniteMod
{
    /// <inheritdoc/>
    protected override string HarmonyId => HarmonyIdValue;

    private static string HarmonyIdValue =>
        $"com.nekometer.esnya.{typeof(AssetOptimizationTweaksMod).Assembly.GetName().Name}";

#if DEBUG
    /// <summary>
    /// Unpatches before hot reload.
    /// </summary>
    public static void BeforeHotReload()
    {
        BeforeHotReload(HarmonyIdValue);
    }

    /// <summary>
    /// Reapplies patches after hot reload.
    /// </summary>
    /// <param name="mod">Reloaded mod instance.</param>
    public static void OnHotReload(ResoniteMod mod)
    {
        OnHotReload(mod, HarmonyIdValue);
    }
#endif
}
