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
    internal static string HarmonyId =>
        $"com.nekometer.esnya.{typeof(UniLogTweaksMod).Assembly.GetName()}";

    internal static bool AllowInfo =>
        Config?.TryGetValue(AllowInfoKey, out var value) == true && value;

    internal static bool AllowWarning =>
        Config?.TryGetValue(AllowWarningKey, out var value) == true && value;

    internal static bool AllowError =>
        Config?.TryGetValue(AllowErrorKey, out var value) != true || value;

    internal static bool AddIndent =>
        Config?.TryGetValue(AddIndentKey, out var value) != true || value;

    private static Harmony Harmony { get; } = new(HarmonyId);

    private static ModConfigurationKey<bool> AllowInfoKey { get; } = new(
        "AllowInfo",
        "Allow stack trace for Info log.",
        computeDefault: () => false);

    private static ModConfigurationKey<bool> AllowWarningKey { get; } = new(
        "AllowWarning",
        "Allow stack trace for Warning log.",
        computeDefault: () => false);

    private static ModConfigurationKey<bool> AllowErrorKey { get; } = new(
        "AllowError",
        "Allow stack trace for Error log.",
        computeDefault: () => true);

    private static ModConfigurationKey<bool> AddIndentKey { get; } = new(
        "AddIndent",
        "Add indent to multi-line logs.",
        computeDefault: () => true);

    private static ModConfiguration? Config { get; set; }

#if DEBUG
    /// <summary>
    /// Called before hot reload.
    /// </summary>
    public static void BeforeHotReload()
    {
        Harmony.UnpatchAll(HarmonyId);
    }

    /// <summary>
    /// Called after hot reload.
    /// </summary>
    /// <param name="modInstance">Reloaded mod instance.</param>
    public static void OnHotReload(ResoniteMod modInstance)
    {
        Init(modInstance);
    }
#endif

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
        Harmony.PatchAll();
        Config = modInstance?.GetConfiguration() ?? Config;
    }
}
