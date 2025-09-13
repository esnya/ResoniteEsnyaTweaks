# StyleCop Cleanup

## Purpose
Reduce StyleCop warnings by aligning common utilities with analyzer expectations.

## Changes
- Ensure files end with a single newline.
- Order members from most to least accessible.
- Keep static members before instance members and limit each file to a single type.
- Place closing parentheses on the line of the last parameter.
- Enable SA1111 to enforce closing-parenthesis placement.
- Replace broad analyzer suppressions with signature or accessibility adjustments.
- Suppress SA1313 for Harmony magic parameters (`__instance`, `__result`) where renaming is not viable.
- Avoid underscores in field or parameter names except for Harmony magic parameters.
- Match file names with their first type to satisfy SA1649.
- Apply `<inheritdoc/>` only when overriding or implementing base members.
- Disable CA1303 (literal strings require localization) repository-wide and remove per-call suppressions.
- Wrap `using` directives needed only in debug builds in `#if DEBUG` blocks to silence Release warnings.

## Scope
Entire solution.

## Tests
Existing unit tests cover affected helpers.

