using Elements.Core;
using HarmonyLib;
using ResoniteModLoader;
using System.Linq;
using System.Reflection;



#if DEBUG
using ResoniteHotReloadLib;
#endif

namespace EsnyaTweaks.LODGroupTweaks;

/// <summary>
/// Tweaks LODGroup behavior.
/// </summary>
public partial class LODGroupTweaksMod : ResoniteMod
{
    private static Assembly ModAssembly => typeof(LODGroupTweaksMod).Assembly;

    /// <inheritdoc/>
    public override string Name => ModAssembly.GetCustomAttribute<AssemblyTitleAttribute>().Title;
    /// <inheritdoc/>
    public override string Author => ModAssembly.GetCustomAttribute<AssemblyCompanyAttribute>().Company;
    /// <inheritdoc/>
    public override string Version => ModAssembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
    /// <inheritdoc/>
    public override string Link => ModAssembly.GetCustomAttributes<AssemblyMetadataAttribute>().First(meta => meta.Key == "RepositoryUrl").Value;

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
    /// <summary>
    /// Unpatches Harmony before recompiling.
    /// </summary>
    public static void BeforeHotReload()
    {
        harmony.UnpatchAll(HarmonyId);
    }

    /// <summary>
    /// Re-initializes the mod after recompiling.
    /// </summary>
    public static void OnHotReload(ResoniteMod modInstance)
    {
        Init();
    }
#endif
}
