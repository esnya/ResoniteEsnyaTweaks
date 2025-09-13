using System.Reflection;
using System.Runtime.CompilerServices;
using HarmonyLib;
using EsnyaTweaks.Common.Modding;
using ResoniteModLoader;
#if DEBUG
using ResoniteHotReloadLib;
#endif

[assembly: InternalsVisibleTo("EsnyaTweaks.FluxLoopTweaks.Tests")]

namespace EsnyaTweaks.FluxLoopTweaks;

/// <inheritdoc/>
public class FluxLoopTweaksMod : EsnyaResoniteMod
{
    private static readonly Harmony Harmony = new(HarmonyId);

    [AutoRegisterConfigKey]
    private static readonly ModConfigurationKey<int> TimeoutKey = new(
        "Timeout",
        "Timeout in milliseconds.",
        computeDefault: () => 30_000);

    private static ModConfiguration? config;

    /// <summary>
    /// Gets the timeout value in milliseconds for loop operations.
    /// </summary>
    public static int TimeoutMs => config?.GetValue(TimeoutKey) ?? 30_000;

    internal static string HarmonyId => $"com.nekometer.esnya.{ThisAssembly.GetName()}";

    private static Assembly ThisAssembly => typeof(FluxLoopTweaksMod).Assembly;

#if DEBUG
    /// <summary>
    /// Unpatches Harmony patches before hot reload.
    /// </summary>
    public static void BeforeHotReload()
    {
        Harmony.UnpatchAll(HarmonyId);
    }

    /// <summary>
    /// Reapplies Harmony patches after hot reload.
    /// </summary>
    /// <param name="modInstance">The mod instance.</param>
    public static void OnHotReload(ResoniteMod modInstance)
    {
        Harmony.PatchAll();
        config = modInstance?.GetConfiguration();
    }
#endif

    /// <inheritdoc/>
    public override void OnEngineInit()
    {
        Harmony.PatchAll();
        config = GetConfiguration();

#if DEBUG
        HotReloader.RegisterForHotReload(this);
#endif
    }
}
