# ADR 0002: Use `nameof` for accessible reflection targets

## Status
Accepted

## Context
`InventoryUITweaks` used string literals to specify method names in `AccessTools.Method` calls. These strings can silently drift when upstream APIs rename methods.

## Decision
Replace string literals with `nameof` where the referenced method is accessible from this project. Keep string literals only when the member is not accessible or Harmony requires a literal.

## Consequences
- Compile-time checking guards against typos and upstream renames for accessible members.
- Some strings remain for non-visible members and still require manual maintenance.
