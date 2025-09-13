# Scene Auditor (Design Notes)

Purpose: Provide a generic, rule-driven auditor panel to find components in broken or inappropriate states across a scene. Rules are independent and composable, enabling future expansion.

Scope and boundaries:
- Editor-only panel, spawned from WorkerInspector.
- Read-only diagnostics. No automatic mutation or sorting.
- Current rules target LOD groups; rules are designed to be extendable to other types.

Public surface:
- Panel UI with inputs:
  - Root `Slot` reference selector.
  - Rule checkboxes.
  - Search button.
- Results list with (target reference, rule name, detail) and a button to open all WorkerInspectors.

Rules (initial):
- LOD: Non-descending order — detect `ScreenRelativeTransitionHeight` sequences with `h[i] <= h[i+1]`.
- LOD: Duplicate renderer owners — detect `MeshRenderer` assigned to 2+ `LODGroup`s.

Placement and dependencies:
- Project: `SceneAuditor` (mod + UI), independent from `LODGroupTweaks`.
- Pure helpers in `SceneAuditor/Rules/DetectionPrimitives.cs` for unit testing.

Testing strategy:
- Unit tests cover pure detection helpers (no engine dependencies).
- UI and patching remain thin and untested (engine-bound).

Migration:
- Sorting/auto-fix functionality removed from LOD tools.
- LOD inspector no longer includes scan/fix/open-violations buttons; use the Auditor panel instead.
