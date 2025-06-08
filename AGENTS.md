# Agents Instructions

The CI workflow uses static checks that do not require Resonite assemblies.

## Coding Standards

- Stay DRY, but avoid premature abstraction.
- Prefer pure functions, immutability, and declarative style.
- Good naming & structure > comments; comment only non-obvious WHY
- Keep code self-explanatory.

## Checklist for Agents

- Formatting is enforced with `csharpier`.
- Before committing, run `dotnet csharpier check .` to verify formatting.
- Use `dotnet csharpier format .` to apply formatting fixes.
