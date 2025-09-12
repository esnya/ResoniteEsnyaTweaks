# ADR 0002: Guard UniLog patch against null messages

## Status
Accepted

## Context
The UniLog patch indents multi-line messages by replacing newline characters. When the incoming message is null, calling `Replace` throws a null reference exception.

## Decision
Add a null check before performing the replacement. If the message is null and indentation is enabled, set it to an empty string.

## Consequences
- Logging hooks no longer throw when given null messages.
- Tests cover the null message scenario to prevent regression.
