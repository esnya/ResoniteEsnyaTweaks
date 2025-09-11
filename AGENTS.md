# Agents Instructions

The CI workflow uses static checks that do not require Resonite assemblies.

## Environment Setup

- Target .NET SDK 9.0.
- `.codex/setup.sh` installs the SDK and restores tools and dependencies.
- `.codex/maintenance.sh` refreshes tools and dependencies after cache restoration.
- `.codex/dotnet-env.sh` defines `DOTNET_ROOT` and `PATH` for both scripts.

## Coding Standards

- Stay DRY, but avoid premature abstraction.
- Prefer pure functions, immutability, and declarative style.
- Good naming & structure > comments; comment only non-obvious WHY
- Keep code self-explanatory.

## Checklist for Agents

- Formatting is enforced with `dotnet format`.
- Before committing, run `dotnet format --verify-no-changes` to verify formatting and code style.
- Use `dotnet format` (optionally with `style`/`whitespace` scopes) to apply formatting fixes.
