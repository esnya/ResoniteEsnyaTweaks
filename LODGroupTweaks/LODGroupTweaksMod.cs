using EsnyaTweaks.Common.Modding;
#if DEBUG
using ResoniteModLoader;
#endif

namespace EsnyaTweaks.LODGroupTweaks;

/// <inheritdoc/>
public class LODGroupTweaksMod : EsnyaResoniteMod
{
    internal static new string HarmonyId => $"com.nekometer.esnya.{typeof(LODGroupTweaksMod).Assembly.GetName()}";
    // Use base HarmonyId for runtime behavior; static property is for tests.
#if DEBUG
    /// <summary>Unpatches all Harmony hooks before hot reload.</summary>
    public static void BeforeHotReload()
    {
        BeforeHotReload($"com.nekometer.esnya.{typeof(LODGroupTweaksMod).Assembly.GetName()}");
    }

    /// <summary>Reapplies Harmony hooks after hot reload.</summary>
    /// <param name="mod">Reloaded mod instance.</param>
    public static void OnHotReload(ResoniteMod mod)
    {
        OnHotReload(mod, $"com.nekometer.esnya.{typeof(LODGroupTweaksMod).Assembly.GetName()}");
    }
#endif
}
