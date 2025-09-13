using System.Reflection;
using HarmonyLib;
using EsnyaTweaks.Common.Modding;
using ResoniteModLoader;
#if DEBUG
using ResoniteHotReloadLib;
#endif

namespace EsnyaTweaks.UniLogTweaks;

/// <summary>
/// Represents the UniLogTweaksMod which is a sealed class inheriting from ResoniteMod.
/// This class provides the necessary configurations and methods to initialize and manage the mod.
/// </summary>
public sealed class UniLogTweaksMod : EsnyaResoniteMod
{
    private static Assembly ThisAssembly => typeof(UniLogTweaksMod).Assembly;

    internal static string HarmonyId => $"com.nekometer.esnya.{ThisAssembly.GetName()}";

    private static ModConfiguration? config;
    private static readonly Harmony harmony = new(HarmonyId);
    private static readonly ModConfigurationKey<bool> allowInfo = new(
        "AllowInfo",
        "Allow stack trace for Info log.",
        computeDefault: () => false
    );
    private static readonly ModConfigurationKey<bool> allowWarning = new(
        "AllowWarning",
        "Allow stack trace for Warning log.",
        computeDefault: () => false
    );
    private static readonly ModConfigurationKey<bool> allowError = new(
        "AllowError",
        "Allow stack trace for Error log.",
        computeDefault: () => true
    );

    private static readonly ModConfigurationKey<bool> addIndent = new(
        "AddIndent",
        "Add indent to multi-line logs.",
        computeDefault: () => true
    );

    internal static bool AllowInfo =>
        config?.TryGetValue(allowInfo, out var value) == true && value;
    internal static bool AllowWarning =>
        config?.TryGetValue(allowWarning, out var value) == true && value;
    internal static bool AllowError =>
        (config?.TryGetValue(allowError, out var value)) != true || value;

    internal static bool AddIndent =>
        config?.TryGetValue(addIndent, out var value) != true || value;

    /// <summary>
    /// Initializes the mod.
    /// </summary>
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
    /// Called before hot reload.
    /// </summary>
    public static void BeforeHotReload()
    {
        harmony.UnpatchAll(HarmonyId);
    }

    /// <summary>
    /// Called after hot reload.
    /// </summary>
    /// <param name="modInstance"></param>
    public static void OnHotReload(ResoniteMod modInstance)
    {
        Init(modInstance);
    }
#endif
}
