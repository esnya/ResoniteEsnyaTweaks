using EsnyaTweaks.Common.Modding;
using ResoniteModLoader;

namespace EsnyaTweaks.LODGroupTweaks;

/// <inheritdoc/>
public class LODGroupTweaksMod : EsnyaResoniteMod
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0051", Justification = "Accessed via reflection by tests")]
    private static new string HarmonyId => $"com.nekometer.esnya.{typeof(LODGroupTweaksMod).Assembly.GetName()}";
    // Use base HarmonyId for runtime behavior; static HarmonyId is for tests via reflection.
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
