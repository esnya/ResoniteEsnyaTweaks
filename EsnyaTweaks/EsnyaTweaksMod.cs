using Elements.Core;
using HarmonyLib;
using ResoniteModLoader;
using System.Linq;
using System.Reflection;
using System;




#if DEBUG
using ResoniteHotReloadLib;
#endif

namespace EsnyaTweaks;

/// <summary>
/// Mod entry point.
/// </summary>
public partial class EsnyaTweaksMod : ResoniteMod
{
    private static Assembly ModAssembly => typeof(EsnyaTweaksMod).Assembly;

    /// <summary>
    /// Mod name.
    /// </summary>
    public override string Name => ModAssembly.GetCustomAttribute<AssemblyTitleAttribute>().Title;

    /// <summary>
    /// Mod author.
    /// </summary>
    public override string Author => ModAssembly.GetCustomAttribute<AssemblyCompanyAttribute>().Company;

    /// <summary>
    /// Mod version.
    /// </summary>
    public override string Version => ModAssembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;

    /// <summary>
    /// Mod description.
    /// </summary>
    public override string Link => ModAssembly.GetCustomAttributes<AssemblyMetadataAttribute>().First(meta => meta.Key == "RepositoryUrl").Value;

    internal static string HarmonyId => $"com.nekometer.esnya.{ModAssembly.GetName()}";


    private static ModConfiguration? config;
    private static readonly Harmony harmony = new(HarmonyId);

    /// <summary>
    /// Called when the engine is initialized.
    /// </summary>
    public override void OnEngineInit()
    {
        Init(this);

#if DEBUG
        HotReloader.RegisterForHotReload(this);
#endif
    }

    private static void Init(ResoniteMod modInstance)
    {

        foreach (var category in Enum.GetNames(typeof(PatchCategory)))
        {
            harmony.PatchCategory(category);
        }

        config = modInstance?.GetConfiguration();
    }

#if DEBUG
    /// <summary>
    /// Called before hot reload.
    /// </summary>
    public static void BeforeHotReload()
    {
        try
        {
            foreach (var category in Enum.GetNames(typeof(PatchCategory)))
            {
                harmony.UnpatchCategory(category);
            }
        }
        catch (System.Exception e)
        {
            Error(e);
        }
    }

    /// <summary>
    /// Called after hot reload.
    /// </summary>
    /// <param name="modInstance"></param>
    public static void OnHotReload(ResoniteMod modInstance)
    {
        Init(modInstance);
    }
#endif
}
