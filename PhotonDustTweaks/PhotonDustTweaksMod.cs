using HarmonyLib;
using EsnyaTweaks.Common.Modding;
#if DEBUG
using ResoniteHotReloadLib;
#endif

namespace EsnyaTweaks.PhotonDustTweaks;

/// <summary>
/// Mod entry point for Photon Dust tweaks.
/// </summary>
public class PhotonDustTweaksMod : EsnyaResoniteMod
{
    internal static string HarmonyId =>
        $"com.nekometer.esnya.{typeof(PhotonDustTweaksMod).Assembly.GetName()}";

    internal static Harmony Harmony { get; } = new(HarmonyId);

#if DEBUG
    /// <summary>
    /// Unpatches before hot reload.
    /// </summary>
    public static void BeforeHotReload()
    {
        Harmony.UnpatchAll(HarmonyId);
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
    }
}
