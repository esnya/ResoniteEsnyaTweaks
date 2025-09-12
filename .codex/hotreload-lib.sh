# shellcheck shell=bash

ensure_hot_reload_libs() {
  local res_dir="$1"
  if [ ! -f "$res_dir/ResoniteHotReloadLib.dll" ] || [ ! -f "$res_dir/ResoniteHotReloadLibCore.dll" ]; then
    local release_json="$(curl -s https://api.github.com/repos/Nytra/ResoniteHotReloadLib/releases/latest)"
    local url="$(echo "$release_json" | jq -r '.assets[] | select(.name | test("^HotReloadLib.*\\.RML\\.zip$")) | .browser_download_url')"
    if [ -z "$url" ] || [ "$url" = "null" ]; then
      echo "Error: Could not find HotReloadLib RML zip asset in the latest release." >&2
      return 1
    fi
    local tmp_zip="$res_dir/HotReloadLib.RML.zip"
    curl -L "$url" -o "$tmp_zip"
    unzip -j -o "$tmp_zip" ResoniteHotReloadLib.dll ResoniteHotReloadLibCore.dll -d "$res_dir"
    rm "$tmp_zip"
  fi
}
