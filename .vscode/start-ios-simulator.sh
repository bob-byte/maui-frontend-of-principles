#!/bin/zsh
# Boot the first available iPhone simulator when none is running.
# Prints the booted simulator UDID on stdout.
set -euo pipefail

booted_udid() {
  xcrun simctl list devices booted | awk -F '[()]' '/iPhone/{print $2; exit}'
}

UDID="$(booted_udid || true)"
if [[ -z "$UDID" ]]; then
  UDID="$(xcrun simctl list devices available | awk -F '[()]' '/iPhone/{print $2; exit}')"
  if [[ -z "$UDID" ]]; then
    echo "No available iOS simulator found. Install one from Xcode > Settings > Platforms." >&2
    exit 1
  fi
  open -a Simulator
  xcrun simctl boot "$UDID" >/dev/null 2>&1 || true
  xcrun simctl bootstatus "$UDID" -b >/dev/null 2>&1 || true
  UDID="$(booted_udid || true)"
  if [[ -z "$UDID" ]]; then
    echo "iOS simulator did not finish booting." >&2
    exit 1
  fi
else
  open -a Simulator
fi

echo "$UDID"
