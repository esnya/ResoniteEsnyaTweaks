# 0002 Redirection Map Deduplication

## Status
Accepted

## Context
`AssetOptimizationExtensions` built a redirection map using `Dictionary.Add`. Reprocessing the same components caused duplicate key exceptions.

## Decision
Use `Dictionary.TryAdd` and explicit checks for existing keys when building redirection maps and adding sync member redirections. This makes deduplication idempotent and avoids exceptions.

## Consequences
Duplicate entries are ignored; existing mappings take precedence and are preserved.
