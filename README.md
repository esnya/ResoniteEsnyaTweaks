# EsnyaTweaks

A collection of small [ResoniteModLoader](https://github.com/resonite-modding-group/ResoniteModLoader) mods for [Resonite](https://resonite.com/). Each directory contains an independent project.

## Projects and Features

- **EsnyaTweaks**: Miscellaneous patches including an ObjectScroller scaling fix.
- **FluxLoopTweaks**: Adds timeout functionality to `While` and `AsyncWhile` loops.
- **LODGroupTweaks**: Sets `LODGroup` update order to 1000 and provides inspector buttons to manage child LOD levels.
- **PhotonDustTweaks**: Adds inspector utilities for modules on parent `ParticleStyle` objects.
- **SystemHelperTweaks**: Adds timeout and restart functionality to `SystemHelper` initialization.
- **UniLogTweaks**: Provides options to control stack traces for UniLog messages.

## Installation

1. Install the [ResoniteModLoader](https://github.com/resonite-modding-group/ResoniteModLoader).
1. Download the DLLs for the projects you want from the [releases page](https://github.com/esnya/ResoniteEsnyaTweaks/releases/latest).
1. Place them into your `rml_mods` folder. This folder should be located at `C:\Program Files (x86)\Steam\steamapps\common\Resonite\rml_mods` for a standard installation. You can create it if it's missing, or if you start the game once with the ResoniteModLoader installed it will create this folder for you.
1. Launch the game. Check your Resonite logs if you want to confirm that the mods are loaded.


## Development Requirements

For development, you will need the [ResoniteHotReloadLib](https://github.com/Nytra/ResoniteHotReloadLib) to be able to hot reload your mod with DEBUG build.

