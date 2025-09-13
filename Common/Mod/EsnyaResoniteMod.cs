using System;
using System.Reflection;
using System.Collections.Concurrent;
using System.Collections.Generic;
using HarmonyLib;
using ResoniteModLoader;
using EsnyaTweaks.Common.Reflection;
#if DEBUG
using ResoniteHotReloadLib;
#endif

namespace EsnyaTweaks.Common.Modding;

/// <summary>
/// Base mod that auto-populates metadata (Name/Author/Version/Link) from AssemblyMetadata.
/// </summary>
public abstract class EsnyaResoniteMod : ResoniteMod
{
    private const string DefaultHarmonyIdPrefix = "com.nekometer.esnya";

    // Auto-unregister registry keyed by HarmonyId
    private static readonly ConcurrentDictionary<string, List<Action>> AutoUnregisters = new();

    /// <inheritdoc/>
    public override string Name => AssemblyMetadata.Read(ModAssembly).Name;

    /// <inheritdoc/>
    public override string Author => AssemblyMetadata.Read(ModAssembly).Author;

    /// <inheritdoc/>
    public override string Version => AssemblyMetadata.Read(ModAssembly).Version;

    /// <inheritdoc/>
    public override string Link => AssemblyMetadata.Read(ModAssembly).Link;

    /// <summary>
    /// Gets the module assembly (derived type's assembly).
    /// </summary>
    protected Assembly ModAssembly => GetType().Assembly;

    /// <summary>
    /// Gets the full Harmony ID used for patching.
    /// Computed as "com.nekometer.esnya.{AssemblyName}" by default.
    /// </summary>
    protected virtual string HarmonyId => $"{DefaultHarmonyIdPrefix}.{ModAssembly.GetName()}";

    /// <summary>
    /// Helper to unpatch by HarmonyId in hot reload.
    /// </summary>
    /// <param name="harmonyId">Harmony ID.</param>
    public static void BeforeHotReload(string harmonyId)
    {
        RunAutoUnregisters(harmonyId);
        var harmony = new Harmony(harmonyId);
        harmony.UnpatchAll(harmonyId);
    }

    /// <summary>
    /// Helper to reapply patches and run init on hot reload.
    /// </summary>
    /// <param name="modInstance">Reloaded mod instance.</param>
    /// <param name="harmonyId">Harmony ID.</param>
    /// <param name="afterPatched">Optional callback executed after patching with the instance's configuration.</param>
    public static void OnHotReload(ResoniteMod modInstance, string harmonyId, Action<ModConfiguration>? afterPatched = null)
    {
        ArgumentNullException.ThrowIfNull(modInstance);

        // Safety: ensure any pending unregisters are executed even if BeforeHotReload wasn't called.
        RunAutoUnregisters(harmonyId);

        var harmony = new Harmony(harmonyId);
        harmony.PatchAll(modInstance.GetType().Assembly);

        ModConfiguration? config;
        try
        {
            // Acquire configuration if available; in test environments this may throw.
            config = modInstance.GetConfiguration();
        }
        catch
        {
            config = null;
        }

        if (modInstance is EsnyaResoniteMod baseMod)
        {
            baseMod.OnAfterHotReload(config);
        }
        if (config != null)
        {
            afterPatched?.Invoke(config);
        }
    }

    /// <inheritdoc/>
    public override void OnEngineInit()
    {
        var harmony = new Harmony(HarmonyId);
        ApplyPatches(harmony);

        var config = GetConfiguration();
        if (config != null)
        {
            OnInit(config);
        }

#if DEBUG
        HotReloader.RegisterForHotReload(this);
#endif
    }

    /// <summary>
    /// Performs Harmony patching for the given assembly. Override for custom patch scope.
    /// </summary>
    /// <param name="harmony">Harmony instance.</param>
    protected virtual void ApplyPatches(Harmony harmony)
    {
        ArgumentNullException.ThrowIfNull(harmony);
        harmony.PatchAll(ModAssembly);
    }

    /// <summary>
    /// Called after patches are applied. Use this to fetch configuration or do additional setup.
    /// </summary>
    /// <param name="config">Mod configuration instance.</param>
    protected virtual void OnInit(ModConfiguration config)
    {
    }

    /// <summary>
    /// Called after hot reload patching. Default behavior defers to <see cref="OnInit(ModConfiguration)"/>.
    /// </summary>
    /// <param name="config">Mod configuration instance.</param>
    protected virtual void OnAfterHotReload(ModConfiguration? config)
    {
        if (config != null)
        {
            OnInit(config);
        }
    }

    /// <summary>
    /// Register a DevCreateNew menu action and pair it with a hot-reload-safe auto-unregister
    /// that removes the menu option via HotReloadLib. The caller supplies the register action
    /// (typically DevCreateNewForm.AddAction) and this helper wires the corresponding removal.
    /// </summary>
    /// <param name="category">DevCreateNew category.</param>
    /// <param name="optionName">Option name.</param>
    /// <param name="register">Register action to execute immediately.</param>
    protected void RegisterDevCreateNew(string category, string optionName, Action register)
    {
        ArgumentException.ThrowIfNullOrEmpty(category);
        ArgumentException.ThrowIfNullOrEmpty(optionName);
        ArgumentNullException.ThrowIfNull(register);

        register();

        void Unregister()
        {
#if DEBUG
            try
            {
                HotReloader.RemoveMenuOption(category, optionName);
            }
            catch
            {
                // ignore in tests or where hot reload lib is missing
            }
#endif
        }

        // Suppress IDE0028 suggestion to keep compatibility across analyzers and language levels.
#pragma warning disable IDE0028
        AutoUnregisters.AddOrUpdate(
            HarmonyId,
            _ => new List<Action> { Unregister },
            (_, list) =>
            {
                list.Add(Unregister);
                return list;
            });
    }

    /// <summary>
    /// Registers the given action and ensures the provided <paramref name="unregister"/> is invoked
    /// automatically during hot reload (BeforeHotReload/OnHotReload).
    /// </summary>
    /// <param name="register">Action to execute immediately to register.</param>
    /// <param name="unregister">Action to execute automatically on hot reload.</param>
    protected void RegisterWithAutoUnregister(Action register, Action unregister)
    {
        ArgumentNullException.ThrowIfNull(register);
        ArgumentNullException.ThrowIfNull(unregister);

        register();

        AutoUnregisters.AddOrUpdate(
            HarmonyId,
            _ => new List<Action> { unregister },
            (_, list) =>
            {
                list.Add(unregister);
                return list;
            });
#pragma warning restore IDE0028
    }

    private static void RunAutoUnregisters(string harmonyId)
    {
        if (AutoUnregisters.TryRemove(harmonyId, out var actions))
        {
            foreach (var action in actions)
            {
                try
                {
                    action();
                }
                catch
                {
                    // swallow to ensure stability during reload
                }
            }
        }
    }
}
