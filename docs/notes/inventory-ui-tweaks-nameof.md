# Design Note: nameof for Inventory UI reflection targets

## Purpose
Use `nameof` for `AccessTools.Method` calls where the target method is accessible to improve refactoring safety and reduce string literals.

## Boundaries
- Applies to `InventoryUITweaks` patches only.
- Retains string literals for members that are not visible or when Harmony requires a literal name.

## Public API
No public API changes.

## Dependencies
- Harmony `AccessTools`
- C# `nameof` operator

## Tests
Existing build and test suites cover the changes; no new tests required.

## Migration
No migration steps.
