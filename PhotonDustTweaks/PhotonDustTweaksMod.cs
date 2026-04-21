using System.Runtime.CompilerServices;
using EsnyaTweaks.Common.Modding;
#if DEBUG
using ResoniteModLoader;
#endif

[assembly: InternalsVisibleTo("EsnyaTweaks.PhotonDustTweaks.Tests")]

namespace EsnyaTweaks.PhotonDustTweaks;

/// <summary>
/// Mod entry point for Photon Dust tweaks.
/// </summary>
public class PhotonDustTweaksMod : EsnyaResoniteMod
{
    internal static new string HarmonyId => HarmonyIdValue;

    // Use base HarmonyId for runtime behavior; static property is for tests.
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
