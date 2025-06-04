using System.Linq;
using System.Reflection;
using Elements.Core;
using HarmonyLib;
using ResoniteModLoader;
#if DEBUG
using ResoniteHotReloadLib;
#endif

namespace EsnyaTweaks.SystemHelperTweaks;

/// <inheritdoc/>
public partial class SystemHelperTweaksMod : ResoniteMod
{
    private static Assembly ModAssembly => typeof(SystemHelperTweaksMod).Assembly;

    /// <inheritdoc/>
    public override string Name => ModAssembly.GetCustomAttribute<AssemblyTitleAttribute>().Title;

    /// <inheritdoc/>
    public override string Author =>
        ModAssembly.GetCustomAttribute<AssemblyCompanyAttribute>().Company;

    /// <inheritdoc/>
    public override string Version =>
        ModAssembly.GetCustomAttribute<AssemblyVersionAttribute>().Version;

    /// <inheritdoc/>
    public override string Link =>
        ModAssembly
            .GetCustomAttributes<AssemblyMetadataAttribute>()
            .First(meta => meta.Key == "RepositoryUrl")
            .Value;

    internal static string HarmonyId => $"com.nekometer.esnya.{ModAssembly.GetName()}";

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
