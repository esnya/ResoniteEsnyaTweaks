#!/bin/bash
set -euo pipefail

# Ensure .NET 9 SDK is available
if ! (command -v dotnet >/dev/null && dotnet --list-sdks 2>/dev/null | grep -q '^9\.') ; then
  curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --channel 9.0
fi

env_file="$(dirname "$0")/dotnet-env.sh"
source "$env_file"

if [ -n "${GITHUB_PATH:-}" ]; then
  {
    echo "$DOTNET_ROOT"
    echo "$DOTNET_ROOT/tools"
  } >> "$GITHUB_PATH"
else
  profile="$HOME/.bashrc"
  if ! grep -Fq '.dotnet' "$profile" 2>/dev/null; then
    cat "$env_file" >> "$profile"
  fi
fi

# Restore local tools
dotnet tool restore || true

# Restore the project dependencies
dotnet restore || true
