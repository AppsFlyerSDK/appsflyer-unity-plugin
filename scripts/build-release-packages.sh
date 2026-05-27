#!/usr/bin/env bash

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"

VERSION=""
OUTPUT_DIR="$REPO_ROOT/dist/unitypackages"

usage() {
  cat <<EOF
Usage: $(basename "$0") --version <version> [--output-dir <path>]

Builds both Unity customer artifacts:
  - appsflyer-unity-plugin-<version>.unitypackage
  - strict-mode-sdk/appsflyer-unity-plugin-strict-mode-<version>.unitypackage
EOF
}

while [[ $# -gt 0 ]]; do
  case "$1" in
    --version)
      VERSION="$2"
      shift 2
      ;;
    --output-dir)
      OUTPUT_DIR="$2"
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

if [[ -z "$VERSION" ]]; then
  echo "Error: --version is required" >&2
  exit 1
fi

REGULAR_DIR="$OUTPUT_DIR"
STRICT_DIR="$OUTPUT_DIR/strict-mode-sdk"

mkdir -p "$REGULAR_DIR" "$STRICT_DIR"

"$REPO_ROOT/deploy/build_unity_package.sh" \
  --version "$VERSION" \
  --output-dir "$REGULAR_DIR"

"$REPO_ROOT/deploy/strict_mode_build_package.sh" \
  --version "$VERSION" \
  --output-dir "$STRICT_DIR"

echo "Built release packages:"
echo "  $REGULAR_DIR/appsflyer-unity-plugin-$VERSION.unitypackage"
echo "  $STRICT_DIR/appsflyer-unity-plugin-strict-mode-$VERSION.unitypackage"
