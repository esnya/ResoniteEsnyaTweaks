using Elements.Core;
using HarmonyLib;
using ResoniteModLoader;
using System.Linq;
using System.Reflection;
using System;
using System.Collections.Generic;
using System.ComponentModel;


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

    internal static int timeoutMs;

    private static ModConfiguration? config;
    private static readonly Harmony harmony = new(HarmonyId);

    private static readonly Dictionary<string, ModConfigurationKey<bool>> patchCategoryKeys = new();

    [AutoRegisterConfigKey]
    private static readonly ModConfigurationKey<int> timeoutKey = new("Timeout", "Timeout for in milliseconds.", computeDefault: () => 30_000);

    static EsnyaTweaksMod()
    {
        DebugFunc(() => $"Static Initializing {nameof(EsnyaTweaksMod)}...");

        var keys = from t in AccessTools.GetTypesFromAssembly(ModAssembly)
                   select new KeyValuePair<string?, string?>(t.GetCustomAttribute<HarmonyPatchCategory>()?.info?.category, t.GetCustomAttribute<DescriptionAttribute>()?.Description) into pair
                   where pair.Key is not null && pair.Value is not null
                   select new ModConfigurationKey<bool>(pair.Key!, pair.Value!, computeDefault: () => true);

        foreach (var key in keys)
        {
            DebugFunc(() => $"Registering patch category {key.Name}...");
            patchCategoryKeys[key.Name] = key;
        }
    }

    /// <summary>
    /// Called when the mod is initialized to custom define configuration.
    /// </summary>
    /// <param name="builder"></param>
    /// <exception cref="ArgumentNullException">Raised when builder is null.</exception>"
    public override void DefineConfiguration(ModConfigurationDefinitionBuilder builder)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder), "builder is null.");
        }

        foreach (var key in patchCategoryKeys.Values)
        {
            DebugFunc(() => $"Adding configuration key for {key.Name}...");
            builder.Key(key);
        }
    }

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
        config = modInstance.GetConfiguration();

        if (config is null)
        {
            Warn("Configuration is null. Enabling all patches...");

            foreach (var pair in patchCategoryKeys)
            {
                UpdatePatch(pair.Key, true);
            }
        }
        else
        {
            config.OnThisConfigurationChanged += OnConfigChanged;

            timeoutMs = config.GetValue(timeoutKey);

            foreach (var pair in patchCategoryKeys)
            {
                if (config.TryGetValue(pair.Value, out var value))
                {
                    UpdatePatch(pair.Key, value);
                }
                else if (config.TryGetValue(pair.Value, out value))
                {
                    UpdatePatch(pair.Key, value);
                }
                else
                {
                    Warn($"Configuration for {pair.Key} is not found. Force enabling...");
                    UpdatePatch(pair.Key, true);
                }
            }
        }
    }

    private static void UpdatePatch(string category, bool state)
    {
        if (state)
        {
            DebugFunc(() => $"Patching {category}...");
            harmony.PatchCategory(category.ToString());
        }
        else
        {
            DebugFunc(() => $"Unpatching {category}...");
            harmony.UnpatchAll(HarmonyId);
        }
    }

    private static void OnConfigChanged(ConfigurationChangedEvent change)
    {
        if (change.Key is ModConfigurationKey<bool> key)
        {
            UpdatePatch(key.Name, change.Config.GetValue(key));
        }
        else if (change.Key == timeoutKey)
        {
            timeoutMs = change.Config.GetValue(timeoutKey);
        }
    }

#if DEBUG
    /// <summary>
    /// Called before hot reload.
    /// </summary>
    public static void BeforeHotReload()
    {
        try
        {
            if (config is not null)
            {
                config.OnThisConfigurationChanged -= OnConfigChanged;
            }

            foreach (var category in patchCategoryKeys.Keys)
            {
                UpdatePatch(category, false);
            }
        }
        catch (Exception e)
        {
            Error(e);
        }
    }

    /// <summary>Called after hot reload.</summary>
    /// <param name="modInstance"></param>
    /// <exception cref="ArgumentNullException">Raised when modInstance is null.</exception>
    public static void OnHotReload(ResoniteMod modInstance)
    {
        if (modInstance is null)
        {
            throw new ArgumentNullException(nameof(modInstance), "modInstance is null for hot reload. Hot reload failed.");
        }

        Init(modInstance);
    }
#endif
}
