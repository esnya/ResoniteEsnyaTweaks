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

# Fetch ResoniteModLoader and hot-reload assemblies if FrooxEngine is absent
RES_DIR="$(dirname "$0")/../Resonite"
if [ ! -f "$RES_DIR/FrooxEngine.dll" ]; then
  mkdir -p "$RES_DIR"
  if [ ! -f "$RES_DIR/ResoniteModLoader.dll" ] || [ ! -f "$RES_DIR/0Harmony.dll" ]; then
    release_json="$(curl -s https://api.github.com/repos/resonite-modding-group/ResoniteModLoader/releases/latest)"
    for asset in ResoniteModLoader.dll 0Harmony.dll; do
      url="$(echo "$release_json" | jq -r ".assets[] | select(.name==\"$asset\") | .browser_download_url")"
      curl -L "$url" -o "$RES_DIR/$asset"
    done
  fi
  if [ ! -f "$RES_DIR/ResoniteHotReloadLib.dll" ] || [ ! -f "$RES_DIR/ResoniteHotReloadLibCore.dll" ]; then
    release_json="$(curl -s https://api.github.com/repos/Nytra/ResoniteHotReloadLib/releases/latest)"
    url="$(echo "$release_json" | jq -r '.assets[] | select(.name | endswith("RML.zip")) | .browser_download_url')"
    tmp_zip="$RES_DIR/HotReloadLib.RML.zip"
    curl -L "$url" -o "$tmp_zip"
    unzip -j -o "$tmp_zip" ResoniteHotReloadLib.dll ResoniteHotReloadLibCore.dll -d "$RES_DIR"
    rm "$tmp_zip"
  fi
fi
