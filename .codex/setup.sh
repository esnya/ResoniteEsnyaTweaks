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

# Fetch ResoniteModLoader release assemblies if missing
RES_DIR="$(dirname "$0")/../Resonite"
if [ ! -f "$RES_DIR/ResoniteModLoader.dll" ] || [ ! -f "$RES_DIR/0Harmony.dll" ]; then
  mkdir -p "$RES_DIR"
  release_json="$(curl -s https://api.github.com/repos/resonite-modding-group/ResoniteModLoader/releases/latest)"
  for asset in ResoniteModLoader.dll 0Harmony.dll; do
    url="$(echo "$release_json" | jq -r ".assets[] | select(.name==\"$asset\") | .browser_download_url")"
    curl -L "$url" -o "$RES_DIR/$asset"
  done
fi
