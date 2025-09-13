# EsnyaTweaks

A collection of small [ResoniteModLoader](https://github.com/resonite-modding-group/ResoniteModLoader) mods for [Resonite](https://resonite.com/). Each directory contains an independent project.

## Projects and Features

- **AssetOptimizationTweaks**: Advanced asset deduplication and memory optimization utilities. Provides powerful duplicate detection algorithms, hash-based asset comparison, and procedural asset optimization. Adds a "[Mod] Deduplicate Procedural Assets" button to the AssetOptimizationWizard for enhanced asset management.
- **FluxLoopTweaks**: Adds timeout functionality to `While` and `AsyncWhile` loops.
- **InventoryUITweaks**: Streamlines the inventory browser with single-click folder/back navigation and keeps toolbar buttons consistently ordered using disabled placeholders.
- **LODGroupTweaks**: Sets `LODGroup` update order to 1000 and provides inspector buttons to manage child LOD levels.
- **PhotonDustTweaks**: Adds inspector utilities for modules on parent `ParticleStyle` objects.
- **UniLogTweaks**: Provides options to control stack traces for UniLog messages.
- **SceneAuditor**: Editor-only diagnostics panel to identify sources of log spam and scene-state issues.

### SceneAuditor

- Purpose
  - Identify sources of Player log spam related to LOD configuration (e.g., non-descending LOD thresholds, duplicate renderer ownership). There is a suspicion that excessive LOD-related spam may correlate with Renderite crashes (unconfirmed).
  - Identify sources of normal Log spam related to PrimitiveMemberEditor misconfiguration (only flags missing TextEditor assignment; avoids false positives like missing IText).
- Usage
  - CreateNew: Editor / Scene Auditor (Mod)
  - Set Search Root (initially null). Search becomes enabled once a root is assigned.
  - Review results on the right pane; open target inspectors via the built‑in actions in RefEditor.
- Notes
  - UI is built with UIBuilder best practices: style-first sizing, ScrollArea + FitContent(Disabled, PreferredSize), fixed-width left cells (RefEditor) and single-line rows.

## Installation

1. Install the [ResoniteModLoader](https://github.com/resonite-modding-group/ResoniteModLoader).
2. Download the DLLs for the projects you want from the [releases page](https://github.com/esnya/ResoniteEsnyaTweaks/releases/latest).
3. Place them into your `rml_mods` folder. This folder should be located at `C:\Program Files (x86)\Steam\steamapps\common\Resonite\rml_mods` for a standard installation. You can create it if it's missing, or if you start the game once with the ResoniteModLoader installed it will create this folder for you.
4. Launch the game. Check your Resonite logs if you want to confirm that the mods are loaded.

## Development

For development, you will need the [ResoniteHotReloadLib](https://github.com/Nytra/ResoniteHotReloadLib) to be able to hot reload your mod with DEBUG build.

### Structure

- Each mod has its own test project (e.g., `*.Tests`).
- Shared code lives under `Common/`.
- Pure logic is extracted into `Common/Flux`, `Common/LOD`, `Common/Logging` and covered by `Common.PureTests` which does not reference FrooxEngine.

### Mod base and Hot Reload

All mods should inherit from `EsnyaTweaks.Common.Modding.EsnyaResoniteMod`. The base class:

- Applies Harmony patches on `OnEngineInit` and registers Hot Reload (DEBUG).
- Exposes `protected virtual void OnInit(ModConfiguration config)` and `protected virtual void OnAfterHotReload(ModConfiguration? config)`.
- Provides helpers for Hot Reload shims:
  - `EsnyaResoniteMod.BeforeHotReload(harmonyId)`
  - `EsnyaResoniteMod.OnHotReload(mod, harmonyId)`

### DevCreateNew registration (with auto-unregister)

Use the base helper to register a DevCreateNew menu entry and automatically unregister it on hot reload (DEBUG):

```
protected override void OnInit(ModConfiguration config)
{
    RegisterDevCreateNew(
        category: "Editor",
        optionName: "My Tool",
        register: () => FrooxEngine.DevCreateNewForm.AddAction(
            "Editor", "My Tool", slot => /* spawn UI */ ));
}

#if DEBUG
public static void BeforeHotReload() => BeforeHotReload("com.nekometer.esnya.<AssemblyName>");
public static void OnHotReload(ResoniteModLoader.ResoniteMod mod) => OnHotReload(mod, "com.nekometer.esnya.<AssemblyName>");
#endif
```

### Testing and Code Coverage

This project includes automated testing with code coverage reporting.

### Running Tests

```bash
# Run all tests
dotnet test

# Run tests with code coverage collection
dotnet test --collect:"XPlat Code Coverage"
```

### Generating Coverage Reports

After running tests with coverage collection, you can generate HTML coverage reports:

```bash
# Generate HTML coverage report
dotnet tool run reportgenerator "-reports:TestResults\**\coverage.cobertura.xml" "-targetdir:TestResults\html" "-reporttypes:Html;HtmlSummary;Badges;TextSummary"
```

The generated HTML report will be available at `TestResults\html\index.html`.

### Code Formatting

This project uses dotnet format for code formatting and style:

```bash
# Check formatting & style (fails if changes are needed)
dotnet format --verify-no-changes

# Apply formatting & style fixes
dotnet format
```
