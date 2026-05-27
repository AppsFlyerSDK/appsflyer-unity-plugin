#!/usr/bin/env bash
# Ensure production .unitypackage files exist at repo-root paths expected by check_packages
# and release_to_production. Downloads from the RC GitHub prerelease when not on disk.

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"

VERSION=""
RELEASE_BRANCH=""

usage() {
  cat <<EOF
Usage: $(basename "$0") --version <X.Y.Z> --release-branch <releases/.../X.Y.Z-rcN>

Ensures these files exist:
  appsflyer-unity-plugin-<version>.unitypackage
  strict-mode-sdk/appsflyer-unity-plugin-strict-mode-<version>.unitypackage
EOF
}

while [[ $# -gt 0 ]]; do
  case "$1" in
    --version)
      VERSION="$2"
      shift 2
      ;;
    --release-branch)
      RELEASE_BRANCH="$2"
      shift 2
      ;;
    -h|--help)
      usage
      exit 0
      ;;
    *)
      echo "Unknown option: $1" >&2
      usage >&2
      exit 1
      ;;
  esac
done

if [[ -z "$VERSION" || -z "$RELEASE_BRANCH" ]]; then
  echo "Both --version and --release-branch are required." >&2
  usage >&2
  exit 1
fi

cd "$REPO_ROOT"

REGULAR="appsflyer-unity-plugin-${VERSION}.unitypackage"
STRICT="strict-mode-sdk/appsflyer-unity-plugin-strict-mode-${VERSION}.unitypackage"

if [[ -f "$REGULAR" && -f "$STRICT" ]]; then
  echo "Production packages already present:"
  echo "  $REGULAR"
  echo "  $STRICT"
  exit 0
fi

RC_VERSION="$(printf '%s\n' "$RELEASE_BRANCH" | grep -Eo '[0-9]+\.[0-9]+\.[0-9]+-rc[0-9]+' | tail -1)"
if [[ -z "$RC_VERSION" ]]; then
  echo "::error::Could not parse RC version from release branch: $RELEASE_BRANCH"
  exit 1
fi

if ! command -v gh >/dev/null 2>&1; then
  echo "::error::gh CLI is required to download RC prerelease assets."
  exit 1
fi

TAG="v${RC_VERSION}"
echo "Downloading RC packages from GitHub release $TAG ..."

WORKDIR="$REPO_ROOT/.af-artifacts/production-materialize"
rm -rf "$WORKDIR"
mkdir -p "$WORKDIR" "$REPO_ROOT/strict-mode-sdk"

gh release download "$TAG" \
  --pattern "appsflyer-unity-plugin-${RC_VERSION}.unitypackage" \
  --pattern "appsflyer-unity-plugin-strict-mode-${RC_VERSION}.unitypackage" \
  --dir "$WORKDIR"

RC_REGULAR="$WORKDIR/appsflyer-unity-plugin-${RC_VERSION}.unitypackage"
RC_STRICT="$WORKDIR/appsflyer-unity-plugin-strict-mode-${RC_VERSION}.unitypackage"

if [[ ! -f "$RC_REGULAR" ]]; then
  RC_REGULAR="$(find "$WORKDIR" -name "appsflyer-unity-plugin-${RC_VERSION}.unitypackage" -print -quit)"
fi
if [[ ! -f "$RC_STRICT" ]]; then
  RC_STRICT="$(find "$WORKDIR" -name "appsflyer-unity-plugin-strict-mode-${RC_VERSION}.unitypackage" -print -quit)"
fi

if [[ -z "$RC_REGULAR" || ! -f "$RC_REGULAR" ]]; then
  echo "::error::Missing regular package on RC release $TAG"
  exit 1
fi
if [[ -z "$RC_STRICT" || ! -f "$RC_STRICT" ]]; then
  echo "::error::Missing strict package on RC release $TAG"
  exit 1
fi

cp "$RC_REGULAR" "$REGULAR"
cp "$RC_STRICT" "$STRICT"

echo "Materialized production packages from $TAG:"
echo "  $REGULAR"
echo "  $STRICT"
