using System.Runtime.CompilerServices;
using EsnyaTweaks.Common.Modding;
using ResoniteModLoader;

[assembly: InternalsVisibleTo("EsnyaTweaks.UniLogTweaks.Tests")]

namespace EsnyaTweaks.UniLogTweaks;

/// <summary>
/// Represents the UniLogTweaksMod which is a sealed class inheriting from ResoniteMod.
/// This class provides the necessary configurations and methods to initialize and manage the mod.
/// </summary>
public sealed class UniLogTweaksMod : EsnyaResoniteMod
{
    internal static bool AllowInfo =>
        Config?.TryGetValue(AllowInfoKey, out var value) == true && value;

    internal static bool AllowWarning =>
        Config?.TryGetValue(AllowWarningKey, out var value) == true && value;

    internal static bool AllowError =>
        Config?.TryGetValue(AllowErrorKey, out var value) != true || value;

    internal static bool AddIndent =>
        Config?.TryGetValue(AddIndentKey, out var value) != true || value;

    internal static new string HarmonyId => HarmonyIdValue;

    // Use base HarmonyId for runtime behavior; static property is for tests.
    private static string HarmonyIdValue =>
        $"com.nekometer.esnya.{typeof(UniLogTweaksMod).Assembly.GetName()}";

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
        BeforeHotReload(HarmonyIdValue);
    }

    /// <summary>
    /// Called after hot reload.
    /// </summary>
    /// <param name="modInstance">Reloaded mod instance.</param>
    public static void OnHotReload(ResoniteMod modInstance)
    {
        OnHotReload(
            modInstance,
            HarmonyIdValue);
    }
#endif

    /// <inheritdoc/>
    protected override void OnInit(ModConfiguration config)
    {
        Config = config;
    }
}
