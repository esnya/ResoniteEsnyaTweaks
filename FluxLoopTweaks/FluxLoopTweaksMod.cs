using System.Runtime.CompilerServices;
using EsnyaTweaks.Common.Modding;
using ResoniteModLoader;

[assembly: InternalsVisibleTo("EsnyaTweaks.FluxLoopTweaks.Tests")]

namespace EsnyaTweaks.FluxLoopTweaks;

/// <inheritdoc/>
public class FluxLoopTweaksMod : EsnyaResoniteMod
{
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

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0051", Justification = "Accessed via reflection by tests")]
    private static new string HarmonyId => $"com.nekometer.esnya.{typeof(FluxLoopTweaksMod).Assembly.GetName()}";

#pragma warning disable SA1512 // Single-line comments should not be followed by blank line
    // Use base HarmonyId for runtime behavior; static HarmonyId is for tests via reflection.
#pragma warning restore SA1512 // Single-line comments should not be followed by blank line
#if DEBUG
    /// <summary>
    /// Unpatches Harmony patches before hot reload.
    /// </summary>
    public static void BeforeHotReload()
    {
        BeforeHotReload($"com.nekometer.esnya.{typeof(FluxLoopTweaksMod).Assembly.GetName()}");
    }

    /// <summary>
    /// Reapplies Harmony patches after hot reload.
    /// </summary>
    /// <param name="modInstance">The mod instance.</param>
    public static void OnHotReload(ResoniteMod modInstance)
    {
        OnHotReload(
            modInstance,
            $"com.nekometer.esnya.{typeof(FluxLoopTweaksMod).Assembly.GetName()}");
    }
#endif

    /// <inheritdoc/>
    protected override void OnInit(ModConfiguration config)
    {
        FluxLoopTweaksMod.config = config;
    }
}
