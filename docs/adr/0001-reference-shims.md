# ADR 0001: Build without Resonite runtime

## Status
Accepted

## Context
Building the mods previously required copying assemblies from a local Resonite installation. This prevented CI builds and tests on machines without the game.

## Decision
Use the `Resonite.GameLibs` reference package and assemblies from the `ResoniteModLoader` and `ResoniteHotReloadLib` releases to satisfy game API references. `ResoniteModLoader.dll`, `0Harmony.dll`, `ResoniteHotReloadLib.dll`, and `ResoniteHotReloadLibCore.dll` are downloaded during setup only when no `FrooxEngine.dll` is present and referenced directly when the game is absent.

## Consequences
- Mods and tests compile in environments lacking the Resonite installation.
- Tests must avoid executing game APIs.
- When the game is installed, the build still prefers real assemblies and skips downloading the mod loader.
