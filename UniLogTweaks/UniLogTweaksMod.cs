using Elements.Core;
using HarmonyLib;
using ResoniteModLoader;
using System.Linq;
using System.Reflection;



#if DEBUG
using ResoniteHotReloadLib;
#endif

namespace EsnyaTweaks.UniLogTweaks;

/// <summary>
/// Tweaks UniLog behavior.
/// </summary>
public sealed class UniLogTweaksMod : ResoniteMod
{
    private static Assembly ModAssembly => typeof(UniLogTweaksMod).Assembly;

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
    private static readonly ModConfigurationKey<bool> allowInfo = new("AllowInfo", "Allow stack trace for Info log.", computeDefault: () => false);
    private static readonly ModConfigurationKey<bool> allowWarning = new("AllowWarning", "Allow stack trace for Warning log.", computeDefault: () => false);
    private static readonly ModConfigurationKey<bool> allowError = new("AllowError", "Allow stack trace for Error log.", computeDefault: () => true);

    private static readonly ModConfigurationKey<bool> addIndent = new("AddIndent", "Add indent to multi-line logs.", computeDefault: () => true);

    internal static bool AllowInfo => config?.TryGetValue(allowInfo, out var value) == true && value;
    internal static bool AllowWarning => config?.TryGetValue(allowWarning, out var value) == true && value;
    internal static bool AllowError => (config?.TryGetValue(allowError, out var value)) != true || value;

    internal static bool AddIndent => config?.TryGetValue(addIndent, out var value) != true || value;

    /// <inheritdoc/>
    public override void OnEngineInit()
    {
#if DEBUG
        HotReloader.RegisterForHotReload(this);
#endif

        Init(this);
    }

    private static void Init(ResoniteMod modInstance)
    {
        harmony.PatchAll();
        config = modInstance?.GetConfiguration() ?? config;
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
