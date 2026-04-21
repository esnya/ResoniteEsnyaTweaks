using EsnyaTweaks.Common.Modding;
using EsnyaTweaks.SceneAuditor.Editor;
using ResoniteModLoader;

namespace EsnyaTweaks.SceneAuditor;

/// <summary>
/// Mod entry point for scene auditing utilities.
/// </summary>
public sealed class SceneAuditorMod : EsnyaResoniteMod
{
    /// <inheritdoc/>
    protected override string HarmonyId => $"com.nekometer.esnya.{typeof(SceneAuditorMod).Assembly.GetName()}";

#if DEBUG
    /// <summary>
    /// Unpatches before hot reload.
    /// </summary>
    public static void BeforeHotReload()
    {
        BeforeHotReload($"com.nekometer.esnya.{typeof(SceneAuditorMod).Assembly.GetName()}");
    }

    /// <summary>
    /// Reapplies patches and re-registers UI after hot reload.
    /// </summary>
    /// <param name="mod">Reloaded mod instance.</param>
    public static void OnHotReload(ResoniteMod mod)
    {
        OnHotReload(
            mod,
            $"com.nekometer.esnya.{typeof(SceneAuditorMod).Assembly.GetName()}");
    }
#endif

    /// <inheritdoc />
    protected override void OnInit(ModConfiguration config)
    {
        RegisterDevCreateNew(
            CreateNewRegistration.Category,
            CreateNewRegistration.OptionName,
            CreateNewRegistration.Register);
    }
}
