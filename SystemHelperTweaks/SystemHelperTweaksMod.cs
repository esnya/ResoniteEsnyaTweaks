using Elements.Core;
using HarmonyLib;
using ResoniteModLoader;
using System.Linq;
using System.Reflection;



#if DEBUG
using ResoniteHotReloadLib;
#endif

namespace EsnyaTweaks.SystemHelperTweaks;

public partial class SystemHelperTweaksMod : ResoniteMod
{
    private static Assembly ModAssembly => typeof(SystemHelperTweaksMod).Assembly;

    public override string Name => ModAssembly.GetCustomAttribute<AssemblyTitleAttribute>().Title;
    public override string Author => ModAssembly.GetCustomAttribute<AssemblyCompanyAttribute>().Company;
    public override string Version => ModAssembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
    public override string Link => ModAssembly.GetCustomAttributes<AssemblyMetadataAttribute>().First(meta => meta.Key == "RepositoryUrl").Value;

    internal static string HarmonyId => $"com.nekometer.esnya.{ModAssembly.GetName()}";


    private static readonly Harmony harmony = new(HarmonyId);

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
    public static void BeforeHotReload()
    {
        harmony.UnpatchAll(HarmonyId);
    }

    public static void OnHotReload(ResoniteMod modInstance)
    {
        Init();
    }
#endif
}
