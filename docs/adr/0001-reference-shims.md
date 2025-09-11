# ADR 0001: Build without Resonite runtime

## Status
Accepted

## Context
Building the mods previously required copying assemblies from a local Resonite installation. This prevented CI builds and tests on machines without the game.

## Decision
Use the `Resonite.GameLibs` reference package and assemblies from the `ResoniteModLoader` release to satisfy game API references. `ResoniteModLoader.dll` and its bundled `0Harmony.dll` are downloaded during setup and referenced directly when the game is absent.

## Consequences
- Mods and tests compile in environments lacking the Resonite installation.
- Tests must avoid executing game APIs.
- When the game is installed, the build still prefers real assemblies.
