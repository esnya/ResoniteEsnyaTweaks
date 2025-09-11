#!/bin/bash
set -euo pipefail

source "$(dirname "$0")/dotnet-env.sh"

# Refresh local tools
if command -v dotnet >/dev/null; then
  dotnet tool restore || true
  dotnet restore || true
fi
