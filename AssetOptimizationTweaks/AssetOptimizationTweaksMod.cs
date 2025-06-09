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
    public override string Name =>
        ModAssembly.GetCustomAttribute<AssemblyTitleAttribute>()?.Title ?? "Unknown";

    /// <inheritdoc/>
    public override string Author =>
        ModAssembly.GetCustomAttribute<AssemblyCompanyAttribute>()?.Company ?? "Unknown";

    /// <inheritdoc/>
    public override string Version
    {
        get
        {
            var informationalVersion = ModAssembly
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                ?.InformationalVersion;
            if (informationalVersion != null)
            {
                // Remove git hash if present (e.g., "1.0.0+abc123" -> "1.0.0")
                var plusIndex = informationalVersion.IndexOf('+');
                return plusIndex >= 0
                    ? informationalVersion.Substring(0, plusIndex)
                    : informationalVersion;
            }
            return ModAssembly.GetCustomAttribute<AssemblyVersionAttribute>()?.Version ?? "0.0.0";
        }
    }

    /// <inheritdoc/>
    public override string Link =>
        ModAssembly
            .GetCustomAttributes<AssemblyMetadataAttribute>()
            .First(meta => meta.Key == "RepositoryUrl")
            .Value;

    internal static string HarmonyId => $"com.nekometer.esnya.{ModAssembly.GetName().Name}";

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
