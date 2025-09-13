using System.Reflection;
using System.Runtime.CompilerServices;
using HarmonyLib;
using EsnyaTweaks.Common.Modding;
using ResoniteModLoader;
#if DEBUG
using ResoniteHotReloadLib;
#endif

[assembly: InternalsVisibleTo("EsnyaTweaks.InventoryUITweaks.Tests")]

namespace EsnyaTweaks.InventoryUITweaks;

/// <inheritdoc/>
public class InventoryUITweaksMod : EsnyaResoniteMod
{
    private static Assembly ThisAssembly => typeof(InventoryUITweaksMod).Assembly;

    internal static string HarmonyId => $"com.nekometer.esnya.{ThisAssembly.GetName()}";

    private static readonly Harmony harmony = new(HarmonyId);

    /// <inheritdoc/>
    public override void OnEngineInit()
    {
        harmony.PatchAll();

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
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Hot reload signature")]
    public static void OnHotReload(ResoniteMod modInstance)
    {
        harmony.PatchAll();
    }
#endif

}
