# Agents Instructions

The CI workflow uses static checks that do not require Resonite assemblies.

## Environment Setup

- Target .NET SDK 9.0.
- `.codex/setup.sh` installs the SDK and restores tools and dependencies.
- `.codex/maintenance.sh` refreshes tools and dependencies after cache restoration.
- `Resonite.GameLibs` package and `ResoniteModLoader`, `0Harmony`, `ResoniteHotReloadLib`, and `ResoniteHotReloadLibCore` assemblies fetched from upstream releases allow builds without a local Resonite install. Hot reload libraries are provided by the `HotReloadLib.*.RML.zip` asset.
- These assemblies are downloaded only when no `FrooxEngine.dll` is found.
- `.codex/dotnet-env.sh` defines `DOTNET_ROOT` and `PATH` for both scripts.

## Coding Standards

- Stay DRY, but avoid premature abstraction.
- For UIBuilder buttons with local actions, use `UIX/UIBuilderExtensions.LocalButton` instead of defining per-project helpers.
- Prefer pure functions, immutability, and declarative style.
- Good naming & structure > comments; comment only non-obvious WHY
- Keep code self-explanatory.
- Order members from most to least accessible and ensure each file ends with exactly one trailing newline; place closing parentheses on the line of the last parameter.

## UIBuilder Guidelines (Must)

- Style-first: set `PreferredSize`, `MinSize`, and layout style before adding content.
- Containers: wrap scrollable content in `ScrollArea` and enable `FitContent` where appropriate for natural sizing.
- Fixed-width cells: prefer explicit widths for list/table-like rows to avoid layout thrash.
- Slot-attached elements: create UI under an explicit `Slot` when elements must persist or be addressed; avoid anonymous, floating elements.
- Local actions: use `UIX/UIBuilderExtensions.LocalButton` for local button handlers instead of per-project helpers.

## Shared Code Structure (Must)

- Use a shared project via `.projitems` (and/or `.shproj`) to include cross-project source files.
- Selection criteria: only include the minimum actually used by multiple projects (YAGNI/DRY). Do not preemptively add helpers.
- Tests can import the same `.projitems` directly to exercise pure helpers without depending on any mod project.
- Avoid hard-coded paths or project-specific references in shared code; keep it framework-agnostic.

## Checklist for Agents

- Formatting is enforced with `dotnet format`.
- Before committing, run `dotnet format --verify-no-changes` to verify formatting and code style.
- Use `dotnet format` (optionally with `style`/`whitespace` scopes) to apply formatting fixes.
 - MUST: `.local/AGENTS.md` が存在する場合は必ず参照し、指示（特にローカル環境・言語方針の上書き）に従う。

### Continuous Guideline Updates (Must)
- When we learn a repo-wide convention (e.g., UIBuilder style-first sizing, shared project (.projitems) usage), reflect it in this AGENTS.md in the same PR. Do not defer.
- Treat AGENTS.md as living documentation. If the change affects multiple repos, leave a TODO link to the canonical guideline.

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
  - Zero-warnings baseline（Must）: ビルド時の警告は許容しない。既存・新規とも修正し、抑制は最小限（やむを得ないシグネチャのみ）。
  - Prefer adjusting signatures or accessibility to eliminate warnings before using `SuppressMessage` or `#pragma`.
  - Harmony magic parameter names (e.g., `__instance`, `__result`) may use `[SuppressMessage("Style", "SA1313")]` with justification.

- Commit hygiene:
  - Use Conventional Commit + gitmoji. One concise subject line per commit.
  - Group formatting-only changes separately when practical.

## CI Simplification (Proposal)

- Build/test at solution level: `dotnet format --verify-no-changes --no-restore`, `dotnet build -c Release -v:minimal`, `dotnet test -c Release -v:minimal`.
- Avoid hardcoding project matrices; collect artifacts via glob: `**/bin/Release/net*/EsnyaTweaks.*.dll` excluding `**/*.Tests/**`.
- Keep Resonite references via shim approach already configured; do not bake paths into csproj.

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

## Rule Docs Index

- Root rules: AGENTS.md (this file)
- Local rules: `.local/AGENTS.md`（ローカル環境・言語ポリシー）
- UIX note (Local): `.local/ResoniteReferences/ui-builder-local-button.md`
- 追加のディレクトリ単位ルールは、各ディレクトリの `AGENTS.md` を参照（パターン: `**/AGENTS.md`）

## 言語ポリシー（本リポジトリ）

- MUST: レポート・チャット・PR 本文などの対話・報告は日本語で記述する（既定）。
- MAY: ユーザーから明示の指示がある場合のみ、別言語に切り替える。

## Naming Rules (Must)

- Avoid reserved-keyword segments in namespaces（例: `Shared`, `Mod` は不可）。
- 代替例: `Common`, `Modding`。

## Release Artifacts (Must)

- Publish only mod binaries matching `EsnyaTweaks.*.dll`.
- Exclude: test binaries（`*.Tests*`）、外部DLL（RML/Harmony 等）、`obj/`・`ref/` 生成物。
