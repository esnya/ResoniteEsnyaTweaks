using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Elements.Core;
using HarmonyLib;
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
public class AssetOptimizationTweaksMod : ResoniteMod
{
    private static Assembly ModAssembly => typeof(AssetOptimizationTweaksMod).Assembly;

    /// <inheritdoc/>
    public override string Name => ModAssembly.GetCustomAttribute<AssemblyTitleAttribute>().Title;

    /// <inheritdoc/>
    public override string Author =>
        ModAssembly.GetCustomAttribute<AssemblyCompanyAttribute>().Company;

    /// <inheritdoc/>
    public override string Version =>
        ModAssembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            .InformationalVersion;

    /// <inheritdoc/>
    public override string Link =>
        ModAssembly
            .GetCustomAttributes<AssemblyMetadataAttribute>()
            .First(meta => meta.Key == "RepositoryUrl")
            .Value;

    internal static string HarmonyId => $"com.nekometer.esnya.{ModAssembly.GetName()}";

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
