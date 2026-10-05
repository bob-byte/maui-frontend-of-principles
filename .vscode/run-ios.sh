#!/bin/zsh
# Build, install, and launch Principles on the booted iPhone simulator.
# mlaunch waits until the app exits, so this script stays running while the app is open.
# Usage: run-ios.sh [Debug|LocalDebug]
set -euo pipefail

CONFIG="${1:-Debug}"
case "$CONFIG" in
  Debug|LocalDebug) ;;
  *)
    echo "Configuration must be Debug or LocalDebug." >&2
    exit 1
    ;;
esac

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
# shellcheck disable=SC1091
source "$ROOT/.vscode/ensure-dotnet.sh"
UDID="$("$ROOT/.vscode/start-ios-simulator.sh")"

if [[ "$(uname -m)" == "arm64" ]]; then
  RID="iossimulator-arm64"
else
  RID="iossimulator-x64"
fi

# iOS Run does not depend on Build; -t:Run alone fails with
# "The app must be built before the arguments to launch...".
dotnet build "$ROOT/Principles/Principles.csproj" \
  -t:Build,Run \
  -f net10.0-ios \
  -c "$CONFIG" \
  --nologo \
  -p:RuntimeIdentifier="$RID" \
  "-p:_DeviceName=:v2:udid=${UDID}"
