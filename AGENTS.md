# Agents Instructions

The CI workflow uses static checks that do not require Resonite assemblies.

## Coding Standards

- Stay DRY, but avoid premature abstraction.
- Prefer pure functions, immutability, and declarative style.
- Good naming & structure > comments; comment only non-obvious WHY
- Keep code self-explanatory.

## Checklist for Agents

- Formatting is enforced with `dotnet format`.
- Before committing, run `dotnet format --verify-no-changes` to verify formatting and code style.
- Use `dotnet format` (optionally with `style`/`whitespace` scopes) to apply formatting fixes.

## Required Workflow (Must)

- Format, build, and test are mandatory before task hand‑off.
  - Format: `dotnet format --verify-no-changes --no-restore`
  - Build (solution or project): `dotnet build -c Debug -v:minimal`
- Test: `dotnet test -c Debug -v:minimal`

- Resonite assemblies are not required by CI, but local builds for mods may need them. Do not change project files to hard‑code paths. Pass the property at invocation time:
  - Build with refs: `dotnet build -c Debug -p:ResonitePath="<ResoniteRoot>/"`
  - Test with refs: `dotnet test -c Debug -p:ResonitePath="<ResoniteRoot>/"`

- When adding Harmony patches or UI tweaks:
  - Prefer Postfix/Prefix over Transpiler for maintainability. Use Transpiler only when necessary.
  - Avoid altering public APIs; keep surface area minimal.

- Warnings policy:
  - Fix warnings introduced by your changes (style/analysis). If suppression is required for Harmony signatures, add targeted SuppressMessage with justification.
  - Do not attempt to refactor unrelated code solely to zero out legacy warnings.

- Commit hygiene:
  - Use Conventional Commit + gitmoji. One concise subject line per commit.
  - Group formatting-only changes separately when practical.

## Local Notes

- Create a local working directory `.local/` at the repo root for machine‑specific artifacts (references, symlinks, notes). This folder is ignored by VCS (see `.gitignore`).
- If you place documentation in `.local/` (e.g., setup notes, paths), Agents may read it, but nothing from `.local/` is ever committed.
- Keep project files agnostic; pass paths at invocation time (e.g., `-p:ResonitePath=...`).
- In this environment, `tmp/ResoniteReferences` has the required DLLs. You may symlink from `.local/Resonite/` to them for convenience.

## Testing Policy (Per Project)

- Every project MUST have tests. When adding features, include tests for newly added logic where feasible.
- Prefer extracting pure, side‑effect‑free helpers from:
  - Harmony patches
  - Code that depends on difficult‑to‑instantiate engine types
  - Mod entrypoints or static initializers
  into independent modules (no engine dependencies) so they can be unit‑tested.
- Keep patches thin: delegate to testable helpers; verify helpers with unit tests.
- Do not mock engine internals unnecessarily. If not feasible, limit tests to the pure parts.
