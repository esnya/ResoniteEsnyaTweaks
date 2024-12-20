# EsnyaTweaks

A [ResoniteModLoader](https://github.com/resonite-modding-group/ResoniteModLoader) mod for [Resonite](https://resonite.com/).

## Features

- **While Timeout**: Adds timeout functionality to `While` and `AsyncWhile` loops to prevent infinite loops.
- **LODGroup UpdateOrder**: Sets `LODGroup` update order to 1000 to prevent rendering issues.
- **LODGroup Inspector Enhancements**: Adds buttons to the LODGroup inspector for adding/removing LOD levels from children.
- **PhotonDust Inspector Enhancements**: Adds buttons to the PhotonDust inspector for adding/removing modules to/from parent ParticleStyle.
- **System Helper Timeout**: Adds timeout and restart functionality to `SystemHelper` initialization.

## Installation

1. Install the [ResoniteModLoader](https://github.com/resonite-modding-group/ResoniteModLoader).
1. Place the [EsnyaTweaks.dll](https://github.com/esnya/ResoniteEsnyaTweaks/releases/latest/download/EsnyaTweaks.dll) into your `rml_mods` folder. This folder should be located at `C:\Program Files (x86)\Steam\steamapps\common\Resonite\rml_mods` for a standard installation. You can create it if it's missing, or if you start the game once with the ResoniteModLoader installed it will create this folder for you.
1. Launch the game. If you want to check that the mod is working you can check your Resonite logs.


## Development Requirements

For development, you will need the [ResoniteHotReloadLib](https://github.com/Nytra/ResoniteHotReloadLib) to be able to hot reload your mod with DEBUG build.
