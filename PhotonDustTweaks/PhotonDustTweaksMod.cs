using System.Reflection;
using HarmonyLib;
using EsnyaTweaks.Common.Modding;
using ResoniteModLoader;
#if DEBUG
using ResoniteHotReloadLib;
#endif

namespace EsnyaTweaks.PhotonDustTweaks;

/// <inheritdoc/>
public class PhotonDustTweaksMod : EsnyaResoniteMod
{
    /// <inheritdoc/>
    public override void OnEngineInit()
    {
        Init();

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
    public static void OnHotReload(ResoniteMod _)
    {
        Init();
    }
#endif

    internal static string HarmonyId => $"com.nekometer.esnya.{ThisAssembly.GetName()}";

    private static Assembly ThisAssembly => typeof(PhotonDustTweaksMod).Assembly;

    private static readonly Harmony harmony = new(HarmonyId);

    private static void Init()
    {
        harmony.PatchAll();
    }
}
