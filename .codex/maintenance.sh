#!/bin/bash
set -euo pipefail

export PATH="$PATH:$HOME/.dotnet/tools"

# Refresh local tools
if command -v dotnet >/dev/null; then
  dotnet tool restore || true
  dotnet restore || true
fi
