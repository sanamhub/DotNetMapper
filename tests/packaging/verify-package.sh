#!/usr/bin/env bash
# Consumes the packed DotNetMapper package the way a real user would: from a clean project with
# no reference to this repository's source, published under NativeAOT with warnings-as-errors.
# It also checks the package carries the analyzer and the buildTransitive props, because a
# package that installs but silently never intercepts is the failure this script exists for.
#
#   tests/packaging/verify-package.sh                       pack, then consume
#   tests/packaging/verify-package.sh --packages <dir>      use the .nupkg already in <dir>
set -euo pipefail

REPO="$(cd "$(dirname "$0")/../.." && pwd)"
PACKAGES=""

while [ $# -gt 0 ]; do
  case "$1" in
    --packages) PACKAGES="$2"; shift 2 ;;
    *) echo "unknown argument: $1" >&2; exit 2 ;;
  esac
done

# Under Git Bash, .NET reads an MSYS path like /tmp/x as C:\tmp\x. Anything handed to a .NET
# tool needs the native form, while the shell keeps using the MSYS one.
native() { if command -v cygpath >/dev/null 2>&1; then cygpath -w "$1"; else echo "$1"; fi; }

WORK="$(mktemp -d)"
trap 'rm -rf "$WORK"' EXIT

if [ -z "$PACKAGES" ]; then
  PACKAGES="$WORK/out"
  dotnet pack "$(native "$REPO/src/DotNetMapper/DotNetMapper.csproj")" -c Release -o "$(native "$PACKAGES")" \
    -p:Version="0.0.0-ci.${GITHUB_RUN_NUMBER:-local}" --verbosity quiet
fi
PACKAGES="$(cd "$PACKAGES" && pwd)"

# Glob and index, with no pipe for head to close early under pipefail.
FOUND=("$PACKAGES"/DotNetMapper.[0-9]*.nupkg)
[ -f "${FOUND[0]}" ] || { echo "no DotNetMapper package in $PACKAGES" >&2; exit 1; }
NUPKG="${FOUND[0]}"
VERSION="$(basename "$NUPKG" .nupkg)"
VERSION="${VERSION#DotNetMapper.}"
echo "consuming DotNetMapper $VERSION from $(native "$PACKAGES")"

# A missing analyzer or props means the consumer compiles but never gets interceptors.
for entry in "analyzers/dotnet/cs/DotNetMapper.Generator.dll" "buildTransitive/DotNetMapper.props" "lib/net10.0/DotNetMapper.dll" "README.md"; do
  if ! unzip -l "$NUPKG" | grep -q "$entry"; then
    echo "FAIL: package does not contain $entry" >&2
    exit 1
  fi
done

[ -f "$PACKAGES/DotNetMapper.$VERSION.snupkg" ] || { echo "FAIL: no .snupkg next to the package" >&2; exit 1; }

if unzip -p "$NUPKG" '*.nuspec' | grep -q '<dependency'; then
  echo "FAIL: the nuspec must have no dependencies" >&2
  exit 1
fi

# A fresh package cache, so a stale DotNetMapper from an earlier run can never be used.
export NUGET_PACKAGES="$(native "$WORK/nuget-cache")"

# The consumers are copied out of the repository so nothing from its root applies:
# Directory.Build.props, central versions, NuGet.Config, .editorconfig.
cp -R "$REPO/tests/packaging/consumers" "$WORK/consumers"

# DotNetMapper may only come from the folder under test; everything else only from nuget.org.
cat > "$WORK/consumers/NuGet.Config" <<XML
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="local" value="$(native "$PACKAGES")" />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" protocolVersion="3" />
  </packageSources>
  <packageSourceMapping>
    <packageSource key="local">
      <package pattern="DotNetMapper" />
    </packageSource>
    <packageSource key="nuget.org">
      <package pattern="*" />
    </packageSource>
  </packageSourceMapping>
</configuration>
XML

cd "$WORK/consumers"

project="$(native "$WORK/consumers/Aot/Aot.csproj")"
out="$WORK/out/Aot"
echo "publishing Aot consumer"
dotnet publish "$project" -c Release -o "$(native "$out")" -p:DotNetMapperVersion="$VERSION" --verbosity quiet
"$out/Aot"

# Interception must have happened: a generated file under the DotNetMapper.Generator folder.
if ! grep -rl "InterceptsLocationAttribute" "$WORK/consumers/Aot/obj" | grep -q "DotNetMapper.Generator"; then
  echo "FAIL: no intercepted generated file found under the DotNetMapper.Generator folder" >&2
  exit 1
fi

echo "package consumption OK"
