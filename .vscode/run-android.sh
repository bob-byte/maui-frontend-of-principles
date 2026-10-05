#!/bin/zsh
# Build, install, and launch Principles on emulator-5554.
# Usage: run-android.sh [Debug|LocalDebug]
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
"$ROOT/.vscode/start-android-emulator.sh"

dotnet build "$ROOT/Principles/Principles.csproj" \
  -t:Build,Run \
  -f net10.0-android \
  -c "$CONFIG" \
  --nologo \
  -p:AdbTarget="-s emulator-5554"
