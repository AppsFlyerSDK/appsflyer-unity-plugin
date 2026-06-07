#!/usr/bin/env bash

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"

DEPLOY_PATH="$SCRIPT_DIR/outputs"
PACKAGE_NAME="appsflyer-unity-plugin-6.18.1-rc1.unitypackage"
UNITY_BIN="${UNITY_PATH:-/Applications/Unity/Unity.app/Contents/MacOS/Unity}"
EDM_PACKAGE="$SCRIPT_DIR/external-dependency-manager-1.2.183.unitypackage"
OUTPUT_DIR="$DEPLOY_PATH"
PRODUCTION=false

usage() {
  cat <<EOF
Usage: $(basename "$0") [OPTIONS]

Options:
  --version <version>       Plugin version for the package name.
  --output-dir <path>       Directory for the generated package.
  -p, --production          Preserve the legacy release output location.
  -h, --help                Show this help.

UNITY_PATH can override the Unity executable path.
EOF
}

while [[ $# -gt 0 ]]; do
  case "$1" in
    --version)
      PACKAGE_NAME="appsflyer-unity-plugin-6.18.1-rc1.unitypackage"
      shift 2
      ;;
    --output-dir)
      OUTPUT_DIR="$2"
      shift 2
      ;;
    -p|--production)
      PRODUCTION=true
      shift
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

if [[ ! -f "$EDM_PACKAGE" ]]; then
  echo "External Dependency Manager package not found: $EDM_PACKAGE" >&2
  exit 1
fi

if [[ ! -x "$UNITY_BIN" ]]; then
  echo "Unity executable not found or not executable: $UNITY_BIN" >&2
  exit 1
fi

mkdir -p "$OUTPUT_DIR"

TEMP_DIR="$(mktemp -d)"
TESTS_DIR="$REPO_ROOT/Assets/AppsFlyer/Tests"
TESTS_META="$REPO_ROOT/Assets/AppsFlyer/Tests.meta"
TESTS_BACKUP="$TEMP_DIR/Tests"
TESTS_META_BACKUP="$TEMP_DIR/Tests.meta"
TESTS_MOVED=false
TESTS_META_MOVED=false

cleanup() {
  if [[ "$TESTS_MOVED" == "true" && -d "$TESTS_BACKUP" ]]; then
    rm -rf "$TESTS_DIR"
    mv "$TESTS_BACKUP" "$TESTS_DIR"
  fi
  if [[ "$TESTS_META_MOVED" == "true" && -f "$TESTS_META_BACKUP" ]]; then
    rm -f "$TESTS_META"
    mv "$TESTS_META_BACKUP" "$TESTS_META"
  fi

  rm -rf "$REPO_ROOT/Assets/ExternalDependencyManager"
  rm -rf "$REPO_ROOT/Assets/PlayServicesResolver"
  rm -f "$REPO_ROOT/Assets/ExternalDependencyManager.meta"
  rm -f "$REPO_ROOT/Assets/PlayServicesResolver.meta"
  rm -rf "$REPO_ROOT/Library" "$REPO_ROOT/Logs" "$REPO_ROOT/Packages"
  rm -rf "$TEMP_DIR"
}
trap cleanup EXIT

echo "Start build for $PACKAGE_NAME"

if [[ -d "$TESTS_DIR" ]]; then
  echo "Temporarily moving Tests folder to avoid NUnit compilation errors in batch mode."
  mv "$TESTS_DIR" "$TESTS_BACKUP"
  TESTS_MOVED=true
fi

if [[ -f "$TESTS_META" ]]; then
  mv "$TESTS_META" "$TESTS_META_BACKUP"
  TESTS_META_MOVED=true
fi

"$UNITY_BIN" \
  -gvh_disable \
  -batchmode \
  -importPackage "$EDM_PACKAGE" \
  -nographics \
  -logFile "$SCRIPT_DIR/create_unity_core.log" \
  -projectPath "$REPO_ROOT" \
  -exportPackage \
  Assets/AppsFlyer \
  "$OUTPUT_DIR/$PACKAGE_NAME" \
  -quit

echo "Package exported successfully to $OUTPUT_DIR/$PACKAGE_NAME"

if [[ "$PRODUCTION" == "true" && "$OUTPUT_DIR" == "$DEPLOY_PATH" ]]; then
  mv "$OUTPUT_DIR/$PACKAGE_NAME" "$REPO_ROOT/$PACKAGE_NAME"
  rmdir "$OUTPUT_DIR" 2>/dev/null || true
  echo "Moved package to $REPO_ROOT/$PACKAGE_NAME"
fi
