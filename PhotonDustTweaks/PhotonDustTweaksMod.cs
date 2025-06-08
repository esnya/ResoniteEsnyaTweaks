using System.Linq;
using System.Reflection;
using Elements.Core;
using HarmonyLib;
using ResoniteModLoader;
#if DEBUG
using ResoniteHotReloadLib;
#endif

namespace EsnyaTweaks.PhotonDustTweaks;

/// <inheritdoc/>
public class PhotonDustTweaksMod : ResoniteMod
{
    private static Assembly ModAssembly => typeof(PhotonDustTweaksMod).Assembly;

    /// <inheritdoc/>
    public override string Name =>
        ModAssembly.GetCustomAttribute<AssemblyTitleAttribute>()?.Title ?? "Unknown";

    /// <inheritdoc/>
    public override string Author =>
        ModAssembly.GetCustomAttribute<AssemblyCompanyAttribute>()?.Company ?? "Unknown";

    /// <inheritdoc/>
    public override string Version
    {
        get
        {
            var informationalVersion = ModAssembly
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                ?.InformationalVersion;
            if (informationalVersion != null)
            {
                // Remove git hash if present (e.g., "1.0.0+abc123" -> "1.0.0")
                var plusIndex = informationalVersion.IndexOf('+');
                return plusIndex >= 0
                    ? informationalVersion.Substring(0, plusIndex)
                    : informationalVersion;
            }
            return ModAssembly.GetCustomAttribute<AssemblyVersionAttribute>()?.Version ?? "0.0.0";
        }
    }

    /// <inheritdoc/>
    public override string Link =>
        ModAssembly
            .GetCustomAttributes<AssemblyMetadataAttribute>()
            .First(meta => meta.Key == "RepositoryUrl")
            .Value;

    internal static string HarmonyId => $"com.nekometer.esnya.{ModAssembly.GetName()}";

    //private static ModConfiguration? config;
    private static readonly Harmony harmony = new(HarmonyId);

    /// <inheritdoc/>
    public override void OnEngineInit()
    {
        Init(this);

#if DEBUG
        HotReloader.RegisterForHotReload(this);
#endif
    }
#pragma warning disable IDE0060 // Remove unused parameter
    private static void Init(ResoniteMod modInstance)
    {
        harmony.PatchAll();
        //config = modInstance?.GetConfiguration();
    }

#if DEBUG
    /// <inheritdoc/>
    public static void BeforeHotReload()
    {
        harmony.UnpatchAll(HarmonyId);
    }

    /// <inheritdoc/>
    public static void OnHotReload(ResoniteMod modInstance)
    {
        Init(modInstance);
    }
#endif
}
