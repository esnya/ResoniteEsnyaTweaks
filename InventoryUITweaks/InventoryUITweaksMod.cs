using System.Runtime.CompilerServices;
using EsnyaTweaks.Common.Modding;
#if DEBUG
using ResoniteModLoader;
#endif

[assembly: InternalsVisibleTo("EsnyaTweaks.InventoryUITweaks.Tests")]

namespace EsnyaTweaks.InventoryUITweaks;

/// <inheritdoc/>
public class InventoryUITweaksMod : EsnyaResoniteMod
{
    /// <inheritdoc/>
    protected override string HarmonyId => $"com.nekometer.esnya.{typeof(InventoryUITweaksMod).Assembly.GetName()}";

#if DEBUG
    /// <summary>Unpatches Harmony before hot reload.</summary>
    public static void BeforeHotReload()
    {
        BeforeHotReload($"com.nekometer.esnya.{typeof(InventoryUITweaksMod).Assembly.GetName()}");
    }

    /// <summary>Reapplies Harmony patches after hot reload.</summary>
    /// <param name="mod">Reloaded mod instance.</param>
    public static void OnHotReload(ResoniteMod mod)
    {
        OnHotReload(mod, $"com.nekometer.esnya.{typeof(InventoryUITweaksMod).Assembly.GetName()}");
    }
#endif
}
