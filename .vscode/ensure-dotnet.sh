# Resolve dotnet when a task shell does not load the user profile.
if ! command -v dotnet >/dev/null 2>&1; then
  if [[ -x /usr/local/share/dotnet/dotnet ]]; then
    export PATH="/usr/local/share/dotnet:${PATH}"
  elif [[ -x "${HOME}/.dotnet/dotnet" ]]; then
    export PATH="${HOME}/.dotnet:${PATH}"
  else
    echo "dotnet was not found. Install the .NET 10 SDK." >&2
    exit 1
  fi
fi
