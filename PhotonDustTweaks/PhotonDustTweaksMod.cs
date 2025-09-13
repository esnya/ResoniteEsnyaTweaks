using EsnyaTweaks.Common.Modding;
using ResoniteModLoader;

namespace EsnyaTweaks.PhotonDustTweaks;

/// <summary>
/// Mod entry point for Photon Dust tweaks.
/// </summary>
public class PhotonDustTweaksMod : EsnyaResoniteMod
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0051", Justification = "Accessed via reflection by tests")]
    private static new string HarmonyId => HarmonyIdValue;

    // Use base HarmonyId for runtime behavior; static HarmonyId is for tests via reflection.
    private static string HarmonyIdValue =>
        $"com.nekometer.esnya.{typeof(PhotonDustTweaksMod).Assembly.GetName()}";

#if DEBUG
    /// <summary>
    /// Unpatches before hot reload.
    /// </summary>
    public static void BeforeHotReload()
    {
        BeforeHotReload(HarmonyIdValue);
    }

    /// <summary>
    /// Reapplies patches after hot reload.
    /// </summary>
    /// <param name="mod">Reloaded mod instance.</param>
    public static void OnHotReload(ResoniteMod mod)
    {
        OnHotReload(mod, HarmonyIdValue);
    }
#endif
}
