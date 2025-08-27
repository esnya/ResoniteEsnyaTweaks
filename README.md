# EsnyaTweaks

A collection of small [ResoniteModLoader](https://github.com/resonite-modding-group/ResoniteModLoader) mods for [Resonite](https://resonite.com/). Each directory contains an independent project.

## Projects and Features

- **AssetOptimizationTweaks**: Advanced asset deduplication and memory optimization utilities. Provides powerful duplicate detection algorithms, hash-based asset comparison, and procedural asset optimization. Adds a "[Mod] Deduplicate Procedural Assets" button to the AssetOptimizationWizard for enhanced asset management.
- **FluxLoopTweaks**: Adds timeout functionality to `While` and `AsyncWhile` loops.
- **LODGroupTweaks**: Sets `LODGroup` update order to 1000 and provides inspector buttons to manage child LOD levels.
- **PhotonDustTweaks**: Adds inspector utilities for modules on parent `ParticleStyle` objects.
- **UniLogTweaks**: Provides options to control stack traces for UniLog messages.

## Installation

1. Install the [ResoniteModLoader](https://github.com/resonite-modding-group/ResoniteModLoader).
2. Download the DLLs for the projects you want from the [releases page](https://github.com/esnya/ResoniteEsnyaTweaks/releases/latest).
3. Place them into your `rml_mods` folder. This folder should be located at `C:\Program Files (x86)\Steam\steamapps\common\Resonite\rml_mods` for a standard installation. You can create it if it's missing, or if you start the game once with the ResoniteModLoader installed it will create this folder for you.
4. Launch the game. Check your Resonite logs if you want to confirm that the mods are loaded.

## Development

For development, you will need the [ResoniteHotReloadLib](https://github.com/Nytra/ResoniteHotReloadLib) to be able to hot reload your mod with DEBUG build.

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
