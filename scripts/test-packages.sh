#!/usr/bin/env bash
#
# Packs the solution into artifacts/packages (emptied first, so no package of an earlier version is left there)
# and tests the packed packages with tests/LocaleNames.Package.Test/run.sh, the same way the CI does.
#
# Usage: scripts/test-packages.sh [configuration]     (default: Release)
# Run from a terminal, it waits for Enter at the end, so the window stays open.
# Needs what run.sh needs: the .NET SDK and, for native AOT, the platform toolchain.

set -euo pipefail

CONFIGURATION=${1:-Release}
ROOT=$(cd "$(dirname "$0")/.." && pwd)
PACKAGES="$ROOT/artifacts/packages"

native() { local path="$1"; if command -v cygpath >/dev/null 2>&1; then cygpath -m "$path"; else echo "$path"; fi; }

# started from a terminal (e.g. by a double click), the window would close before the result can be read;
# waits for Enter however the script ends, keeps its exit code, and does not wait in the CI (no terminal)
pause() { if [[ -t 0 ]]; then echo; read -rp "Press Enter to close..." || true; fi; }
trap pause EXIT

# WSL on a checkout in the Windows file system (/mnt/c/...): bin and obj written by Windows builds are shared with
# the Linux ones and can break the restore ("Unable to find fallback package folder 'C:\Program Files (x86)\...'").
# Not an error, just where to look when the build below fails that way.
if [[ "$ROOT" == /mnt/* ]] && grep -qi microsoft /proc/version 2>/dev/null; then
  echo "WARNING: the repository is on the Windows file system ($ROOT)." >&2
  echo "  If the build fails in the restore, delete the bin and obj directories, or work in a copy in the Linux file system." >&2
  echo >&2
fi

rm -rf "$PACKAGES"

# build first, then pack without building, as the CI does: packing a clean solution in one step fails, because the
# pack of a project can run before the assemblies it packs are built (NU5026)
echo "== build ($CONFIGURATION)"
dotnet build "$(native "$ROOT/LocaleNames.slnx")" -c "$CONFIGURATION"

echo
echo "== pack into $PACKAGES"
dotnet pack "$(native "$ROOT/LocaleNames.slnx")" -c "$CONFIGURATION" --no-build -o "$(native "$PACKAGES")"

echo
bash "$ROOT/tests/LocaleNames.Package.Test/run.sh" "$PACKAGES"
