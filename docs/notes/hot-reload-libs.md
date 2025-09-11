# Design Note: Hot Reload Libraries without Resonite

## Purpose
Allow development environments lacking a Resonite installation to compile mods with hot reload hooks by downloading official hot reload libraries.

## Boundaries
- Applies only when `FrooxEngine.dll` is missing.
- Downloads `ResoniteHotReloadLib.dll` and `ResoniteHotReloadLibCore.dll` from the latest `Nytra/ResoniteHotReloadLib` release.
- Extraction occurs into `Resonite/` alongside other fetched assemblies.

## Public API
No new public APIs.

## Dependencies
- GitHub release assets from `resonite-modding-group/ResoniteModLoader`.
- GitHub release assets from `Nytra/ResoniteHotReloadLib`.

## Tests
Existing build and test commands ensure references resolve; no runtime tests executed without a full Resonite install.

## Migration
Remove the previous `ResoniteHotReloadLib` stub and reference the downloaded assemblies in debug builds.
