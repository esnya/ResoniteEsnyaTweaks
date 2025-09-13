using System.Linq;
using System.Reflection;
using HarmonyLib;
using ResoniteModLoader;
using EsnyaTweaks.SceneAuditor.Editor;
#if DEBUG
using ResoniteHotReloadLib;
#endif

namespace EsnyaTweaks.SceneAuditor;

/// <inheritdoc/>
public sealed class SceneAuditorMod : ResoniteMod
{
    private static Assembly ModAssembly => typeof(SceneAuditorMod).Assembly;

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
            if (!string.IsNullOrEmpty(informationalVersion))
            {
                var plusIndex = informationalVersion.IndexOf('+', System.StringComparison.Ordinal);
                return plusIndex >= 0
                    ? informationalVersion![..plusIndex]
                    : informationalVersion!;
            }
            return ModAssembly.GetCustomAttribute<AssemblyVersionAttribute>()?.Version ?? "0.0.0";
        }
    }

    /// <inheritdoc/>
    public override string Link =>
        ModAssembly
            .GetCustomAttributes<AssemblyMetadataAttribute>()
            .FirstOrDefault(meta => meta.Key == "RepositoryUrl")?.Value
        ?? "";

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
