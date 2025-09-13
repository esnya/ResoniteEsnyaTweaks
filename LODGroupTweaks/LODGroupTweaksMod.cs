using System.Reflection;
using HarmonyLib;
using EsnyaTweaks.Common.Modding;
using ResoniteModLoader;
#if DEBUG
using ResoniteHotReloadLib;
#endif

namespace EsnyaTweaks.LODGroupTweaks;

/// <inheritdoc/>
public class LODGroupTweaksMod : EsnyaResoniteMod
{
    private static Assembly ThisAssembly => typeof(LODGroupTweaksMod).Assembly;

    internal static string HarmonyId => $"com.nekometer.esnya.{ThisAssembly.GetName()}";

    private static readonly Harmony harmony = new(HarmonyId);

    /// <inheritdoc/>
    public override void OnEngineInit()
    {
        Init();

#if DEBUG
        HotReloader.RegisterForHotReload(this);
#endif
    }

    private static void Init()
    {
        harmony.PatchAll();
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
}
