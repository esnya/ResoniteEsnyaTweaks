using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Elements.Core;
using HarmonyLib;
using ResoniteModLoader;
#if DEBUG
using ResoniteHotReloadLib;
#endif

[assembly: InternalsVisibleTo("EsnyaTweaks.FluxLoopTweaks.Tests")]

namespace EsnyaTweaks.FluxLoopTweaks;

/// <inheritdoc/>
public partial class FluxLoopTweaksMod : ResoniteMod
{
    private static Assembly ModAssembly => typeof(FluxLoopTweaksMod).Assembly;

    /// <inheritdoc/>
    public override string Name => ModAssembly.GetCustomAttribute<AssemblyTitleAttribute>().Title;

    /// <inheritdoc/>
    public override string Author =>
        ModAssembly.GetCustomAttribute<AssemblyCompanyAttribute>().Company;

    /// <inheritdoc/>
    public override string Version =>
        ModAssembly.GetCustomAttribute<AssemblyVersionAttribute>().Version;

    /// <inheritdoc/>
    public override string Link =>
        ModAssembly
            .GetCustomAttributes<AssemblyMetadataAttribute>()
            .First(meta => meta.Key == "RepositoryUrl")
            .Value;

    internal static string HarmonyId => $"com.nekometer.esnya.{ModAssembly.GetName()}";

    private static ModConfiguration? config;
    private static readonly Harmony harmony = new(HarmonyId);

    [AutoRegisterConfigKey]
    private static readonly ModConfigurationKey<int> timeoutKey = new(
        "Timeout",
        "Timeout for in milliseconds.",
        computeDefault: () => 30_000
    );

    /// <summary>
    /// Gets the timeout value in milliseconds for loop operations.
    /// </summary>
    public static int TimeoutMs => config?.GetValue(timeoutKey) ?? 30_000;

    /// <inheritdoc/>
    public override void OnEngineInit()
    {
        Init(this);

#if DEBUG
        HotReloader.RegisterForHotReload(this);
#endif
    }

    private static void Init(ResoniteMod modInstance)
    {
        harmony.PatchAll();
        config = modInstance?.GetConfiguration();
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
