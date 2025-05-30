using Elements.Core;
using HarmonyLib;
using ResoniteModLoader;
using System.Linq;
using System.Reflection;



#if DEBUG
using ResoniteHotReloadLib;
#endif

namespace EsnyaTweaks.FluxLoopTweaks;

/// <summary>
/// Tweaks Flux Loop execution.
/// </summary>
public partial class FluxLoopTweaksMod : ResoniteMod
{
    private static Assembly ModAssembly => typeof(FluxLoopTweaksMod).Assembly;

    /// <inheritdoc/>
    public override string Name => ModAssembly.GetCustomAttribute<AssemblyTitleAttribute>().Title;
    /// <inheritdoc/>
    public override string Author => ModAssembly.GetCustomAttribute<AssemblyCompanyAttribute>().Company;
    /// <inheritdoc/>
    public override string Version => ModAssembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
    /// <inheritdoc/>
    public override string Link => ModAssembly.GetCustomAttributes<AssemblyMetadataAttribute>().First(meta => meta.Key == "RepositoryUrl").Value;

    internal static string HarmonyId => $"com.nekometer.esnya.{ModAssembly.GetName()}";

    private static ModConfiguration? config;
    private static readonly Harmony harmony = new(HarmonyId);

    [AutoRegisterConfigKey]
    private static readonly ModConfigurationKey<int> timeoutKey = new("Timeout", "Timeout for in milliseconds.", computeDefault: () => 30_000);

    /// <summary>
    /// Maximum allowed loop execution time in milliseconds.
    /// </summary>
    public static int TimeoutMs => (config?.TryGetValue(timeoutKey, out var timeout)) == true ? timeout : 30_000;

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
    /// <summary>
    /// Unpatches Harmony before recompiling.
    /// </summary>
    public static void BeforeHotReload()
    {
        harmony.UnpatchAll(HarmonyId);
    }

    /// <summary>
    /// Re-initializes the mod after recompiling.
    /// </summary>
    public static void OnHotReload(ResoniteMod modInstance)
    {
        Init(modInstance);
    }
#endif

}
