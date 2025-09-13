using HarmonyLib;
using EsnyaTweaks.Common.Modding;
using EsnyaTweaks.SceneAuditor.Editor;
#if DEBUG
using ResoniteHotReloadLib;
#endif

namespace EsnyaTweaks.SceneAuditor;

/// <summary>
/// Mod entry point for scene auditing utilities.
/// </summary>
public sealed class SceneAuditorMod : EsnyaResoniteMod
{
    internal static string HarmonyId =>
        $"com.nekometer.esnya.{typeof(SceneAuditorMod).Assembly.GetName()}";

    private static Harmony Harmony { get; } = new(HarmonyId);

#if DEBUG
    /// <summary>
    /// Unpatches before hot reload.
    /// </summary>
    public static void BeforeHotReload()
    {
        Harmony.UnpatchAll(HarmonyId);
        CreateNewRegistration.Unregister();
    }

    /// <summary>
    /// Reapplies patches after hot reload.
    /// </summary>
    public static void OnHotReload()
    {
        Init();
    }
#endif

    /// <inheritdoc />
    public override void OnEngineInit()
    {
        Init();

#if DEBUG
        HotReloader.RegisterForHotReload(this);
#endif
    }

    private static void Init()
    {
        Harmony.PatchAll();
        CreateNewRegistration.Register();
    }
}
