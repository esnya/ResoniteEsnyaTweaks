using System.Reflection;
using HarmonyLib;
using EsnyaTweaks.Common.Modding;
using ResoniteModLoader;
using EsnyaTweaks.SceneAuditor.Editor;
#if DEBUG
using ResoniteHotReloadLib;
#endif

namespace EsnyaTweaks.SceneAuditor;

/// <inheritdoc/>
public sealed class SceneAuditorMod : EsnyaResoniteMod
{
    private static Assembly ThisAssembly => typeof(SceneAuditorMod).Assembly;

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
        CreateNewRegistration.Register();
    }

#if DEBUG
    /// <inheritdoc/>
    public static void BeforeHotReload()
    {
        harmony.UnpatchAll(HarmonyId);
        CreateNewRegistration.Unregister();
    }

    /// <inheritdoc/>
    public static void OnHotReload(ResoniteMod _)
    {
        Init();
    }
#endif
}
