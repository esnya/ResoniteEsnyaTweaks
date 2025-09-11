#!/bin/bash
set -euo pipefail

# Ensure .NET 9 SDK is available
if ! dotnet --list-sdks 2>/dev/null | grep -q '^9\.'; then
  curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --channel 9.0
fi

export DOTNET_ROOT="$HOME/.dotnet"
export PATH="$DOTNET_ROOT:$DOTNET_ROOT/tools:$PATH"

# Restore local tools
dotnet tool restore || true

# Restore the project dependencies
dotnet restore || true
