#!/usr/bin/env bash
#
# Builds an application from the PACKED LocaleNames packages in several configurations and runs it:
#
#   all-jit       LocaleNames, regular publish        data in the shared assembly, which is deployed
#   all-aot       LocaleNames, native AOT             the shared assembly is kept for AOT
#   select-jit    LocaleNames.Data + LocaleNamesLocales   selected data in the app, the shared assembly is left out
#   select-aot    the same with native AOT            smaller than all-aot
#   nobuild-aot   LocaleNames.Data without its build assets, native AOT
#                 must FAIL, loudly, with the way to fix it (not return empty translations)
#
# Usage: run.sh <directory with the .nupkg files> [package version]
# Needs the .NET SDK and, for native AOT, the platform toolchain (Linux: clang and zlib1g-dev;
# Windows: Visual Studio C++ tools, vswhere.exe must be on PATH).

set -uo pipefail

if [[ $# -lt 1 ]]; then
  echo "usage: $0 <directory with the .nupkg files> [package version]" >&2
  exit 2
fi

# Git Bash on Windows rewrites arguments that look like paths (even inside -p:Name=value), which breaks URLs.
export MSYS2_ARG_CONV_EXCL="*"

native() { if command -v cygpath >/dev/null 2>&1; then cygpath -m "$1"; else echo "$1"; fi; }

FEED=$(cd "$1" && pwd)
HERE=$(cd "$(dirname "$0")" && pwd)
PROJECT="$HERE/LocaleNames.Package.Test.csproj"

VERSION=${2:-}
if [[ -z "$VERSION" ]]; then
  newest=$(ls -t "$FEED"/LocaleNames.Core.*.nupkg 2>/dev/null | head -1)
  if [[ -z "$newest" ]]; then
    echo "no LocaleNames.Core.*.nupkg in $FEED" >&2
    exit 2
  fi
  VERSION=$(basename "$newest" .nupkg)
  VERSION=${VERSION#LocaleNames.Core.}
fi

EXE_SUFFIX=""
if [[ "${OS:-}" == "Windows_NT" ]]; then
  EXE_SUFFIX=".exe"
  vswhere_dir="/c/Program Files (x86)/Microsoft Visual Studio/Installer"
  [[ -d "$vswhere_dir" ]] && export PATH="$PATH:$vswhere_dir"
fi

# Native AOT on Linux links with a C toolchain and needs the zlib headers. Without them three of the scenarios
# fail in the linker, so say it at once and say what to install.
if [[ "$(uname -s)" == "Linux" ]]; then
  if ! command -v clang >/dev/null 2>&1 && ! command -v gcc >/dev/null 2>&1; then
    echo "The native AOT scenarios need a C compiler (clang or gcc), which is not installed." >&2
    echo "  Debian/Ubuntu: sudo apt-get install -y clang zlib1g-dev" >&2
    exit 2
  fi
  if [[ ! -f /usr/include/zlib.h ]]; then
    echo "The native AOT scenarios need the zlib headers, which are not installed." >&2
    echo "  Debian/Ubuntu: sudo apt-get install -y zlib1g-dev" >&2
    exit 2
  fi
fi

WORK=$(mktemp -d)
FAILED=0
trap '[[ $FAILED -eq 0 ]] && rm -rf "$WORK" || echo "work directory kept: $WORK"' EXIT

# an isolated package cache, so a package of the same version from an earlier run is never reused
PACKAGES=$(native "$WORK/packages")
NUGET_CONFIG="$WORK/nuget.config"
cat >"$NUGET_CONFIG" <<EOF
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="packed" value="$(native "$FEED")" />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
  </packageSources>
</configuration>
EOF

echo "packages: $FEED, version: $VERSION"

fail() { echo "  FAIL: $*"; FAILED=1; }

publish() {
  local name=$1 package=$2 aot=$3 select=$4
  if ! dotnet publish "$(native "$PROJECT")" -c Release \
    -o "$(native "$WORK/out/$name")" \
    --artifacts-path "$(native "$WORK/artifacts/$name")" \
    -p:LnPackage="$package" -p:LnAot="$aot" -p:LnSelect="$select" \
    -p:LocaleNamesVersion="$VERSION" \
    -p:RestorePackagesPath="$PACKAGES" -p:RestoreConfigFile="$(native "$NUGET_CONFIG")" \
    >"$WORK/$name.publish.log" 2>&1; then
    fail "$name: publish failed"
    tail -25 "$WORK/$name.publish.log"
    return 1
  fi
}

# runs the published app, output in $WORK/<name>.run.log, returns its exit code
run_app() {
  local name=$1
  "$WORK/out/$name/LocaleNames.Package.Test$EXE_SUFFIX" >"$WORK/$name.run.log" 2>&1
}

expect_works() {
  local name=$1
  if run_app "$name"; then
    echo "  ok: runs, returns the right translations"
  else
    fail "$name: the app failed"
    tail -15 "$WORK/$name.run.log"
  fi
}

size_of() { wc -c <"$WORK/out/$1/LocaleNames.Package.Test$EXE_SUFFIX" | tr -d ' '; }

echo "== all-jit"
if publish all-jit LocaleNames false false; then
  expect_works all-jit
  [[ -f "$WORK/out/all-jit/LocaleNames.Data.dll" ]] && echo "  ok: LocaleNames.Data.dll is deployed" || fail "all-jit: LocaleNames.Data.dll is missing"
fi

echo "== all-aot"
ALL_AOT_SIZE=0
if publish all-aot LocaleNames true false; then
  expect_works all-aot
  ALL_AOT_SIZE=$(size_of all-aot)
  echo "  size: $ALL_AOT_SIZE bytes"
fi

echo "== select-jit"
if publish select-jit LocaleNames.Data false true; then
  expect_works select-jit
  [[ -f "$WORK/out/select-jit/LocaleNames.Data.dll" ]] && fail "select-jit: LocaleNames.Data.dll must be left out of the output" || echo "  ok: LocaleNames.Data.dll is left out"
  [[ -f "$WORK/out/select-jit/LocaleNames.Core.dll" ]] && echo "  ok: LocaleNames.Core.dll comes with LocaleNames.Data" || fail "select-jit: LocaleNames.Core.dll is missing"
fi

echo "== select-aot"
if publish select-aot LocaleNames.Data true true; then
  expect_works select-aot
  size=$(size_of select-aot)
  echo "  size: $size bytes"
  if [[ "$ALL_AOT_SIZE" -gt 0 && "$size" -ge "$ALL_AOT_SIZE" ]]; then
    fail "select-aot: $size bytes is not smaller than all-aot ($ALL_AOT_SIZE bytes)"
  fi
fi

echo "== nobuild-aot (must fail loudly)"
if publish nobuild-aot LocaleNames.Data.NoBuild true false; then
  if run_app nobuild-aot; then
    fail "nobuild-aot: the app returned data although the data assembly was trimmed away"
  else
    for expected in "LocaleNames found no locale data" "How to fix it" "TrimmerRootAssembly"; do
      grep -q "$expected" "$WORK/nobuild-aot.run.log" \
        && echo "  ok: the error says '$expected'" \
        || { fail "nobuild-aot: the error does not say '$expected'"; tail -15 "$WORK/nobuild-aot.run.log"; }
    done
  fi
fi

echo
if [[ $FAILED -eq 0 ]]; then
  echo "All package scenarios passed."
else
  echo "Some package scenarios FAILED."
fi

exit $FAILED
