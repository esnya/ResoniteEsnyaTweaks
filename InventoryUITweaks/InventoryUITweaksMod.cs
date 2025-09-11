using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using HarmonyLib;
using ResoniteModLoader;
#if DEBUG
using ResoniteHotReloadLib;
#endif

[assembly: InternalsVisibleTo("EsnyaTweaks.InventoryUITweaks.Tests")]

namespace EsnyaTweaks.InventoryUITweaks;

/// <inheritdoc/>
public class InventoryUITweaksMod : ResoniteMod
{
    private static Assembly ModAssembly => typeof(InventoryUITweaksMod).Assembly;

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
            if (!string.IsNullOrEmpty(informationalVersion))
            {
                var plusIndex = informationalVersion.IndexOf('+', System.StringComparison.Ordinal);
                return plusIndex >= 0
                    ? informationalVersion[..plusIndex]
                    : informationalVersion;
            }
            return ModAssembly.GetCustomAttribute<AssemblyVersionAttribute>()?.Version ?? "0.0.0";
        }
    }

    /// <inheritdoc/>
    public override string Link =>
        ModAssembly
            .GetCustomAttributes<AssemblyMetadataAttribute>()
            .FirstOrDefault(meta => meta.Key == "RepositoryUrl")?.Value
        ?? "";

    internal static string HarmonyId => $"com.nekometer.esnya.{ModAssembly.GetName()}";

    private static readonly Harmony harmony = new(HarmonyId);

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
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Hot reload signature")]
    public static void OnHotReload(ResoniteMod modInstance)
    {
        harmony.PatchAll();
    }
#endif

}
