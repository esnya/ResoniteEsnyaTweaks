#!/bin/bash
set -euo pipefail

source "$(dirname "$0")/dotnet-env.sh"

# Refresh local tools
if command -v dotnet >/dev/null; then
  dotnet tool restore || true
  dotnet restore || true
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
fi
