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
    private static Assembly ThisAssembly => typeof(PhotonDustTweaksMod).Assembly;

    internal static string HarmonyId => $"com.nekometer.esnya.{ThisAssembly.GetName()}";

    //private static ModConfiguration? config;
    private static readonly Harmony harmony = new(HarmonyId);

    /// <inheritdoc/>
    public override void OnEngineInit()
    {
        Init(this);

#if DEBUG
        HotReloader.RegisterForHotReload(this);
#endif
    }
#pragma warning disable IDE0060 // Remove unused parameter
    private static void Init(ResoniteMod modInstance)
    {
        harmony.PatchAll();
        //config = modInstance?.GetConfiguration();
    }

#if DEBUG
    /// <inheritdoc/>
    public static void BeforeHotReload()
    {
        harmony.UnpatchAll(HarmonyId);
    }

    /// <inheritdoc/>
    public static void OnHotReload(ResoniteMod modInstance)
    {
        Init(modInstance);
    }
#endif
}
