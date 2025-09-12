# Design Note: Shared LocalButton UIBuilder extension

## Purpose
Provide a reusable extension method to create UI buttons that fire `LocalPressed` handlers without repeating boilerplate across mods.

## Boundaries
- Applies to UIBuilder-based inspectors needing local action wiring.
- Covers only the `LocalButton` helper; additional UI helpers may be added later if necessary.

## Public API
- `Button UIBuilderExtensions.LocalButton(this UIBuilder ui, string label, ButtonEventHandler localAction)`

## Dependencies
- FrooxEngine.UIX types (`UIBuilder`, `Button`, `ButtonEventHandler`).

## Tests
- Exercised indirectly by existing build and test commands.

## Migration
- Replace per-project `LocalButton` implementations with the shared extension.
