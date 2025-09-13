# StyleCop Cleanup

## Purpose
Reduce StyleCop warnings by aligning common utilities with analyzer expectations.

## Changes
- Ensure files end with a single newline.
- Order members from most to least accessible.
- Place closing parentheses on the line of the last parameter.
- Enable SA1111 to enforce closing-parenthesis placement.
- Replace broad analyzer suppressions with signature or accessibility adjustments.
- Suppress SA1313 for Harmony magic parameters (`__instance`, `__result`) where renaming is not viable.

## Scope
Entire solution.

## Tests
Existing unit tests cover affected helpers.

