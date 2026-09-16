#!/usr/bin/env bash

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"

DEPLOY_PATH="$SCRIPT_DIR/outputs"
PACKAGE_NAME="appsflyer-unity-plugin-strict-mode-6.17.900.unitypackage"
UNITY_BIN="${UNITY_PATH:-/Applications/Unity/Unity.app/Contents/MacOS/Unity}"
EDM_PACKAGE="$REPO_ROOT/Assets/ExternalDependencyManager/Editor/external-dependency-manager-1.2.187.unitypackage"
OUTPUT_DIR="$DEPLOY_PATH"
PRODUCTION=false

usage() {
  cat <<EOF
Usage: $(basename "$0") [OPTIONS]

Options:
  --version <version>       Plugin version for the package name.
  --output-dir <path>       Directory for the generated package.
  -p, --production          Preserve the legacy strict-mode output location.
  -h, --help                Show this help.

UNITY_PATH can override the Unity executable path.
EOF
}

while [[ $# -gt 0 ]]; do
  case "$1" in
    --version)
      PACKAGE_NAME="appsflyer-unity-plugin-strict-mode-6.17.900.unitypackage"
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
DEPS_XML="$REPO_ROOT/Assets/AppsFlyer/Editor/AppsFlyerDependencies.xml"
TESTS_DIR="$REPO_ROOT/Assets/AppsFlyer/Tests"
TESTS_META="$REPO_ROOT/Assets/AppsFlyer/Tests.meta"
TESTS_BACKUP="$TEMP_DIR/Tests"
TESTS_META_BACKUP="$TEMP_DIR/Tests.meta"
TESTS_MOVED=false
TESTS_META_MOVED=false

cleanup() {
  if [[ -f "$TEMP_DIR/AppsFlyerDependencies.xml" ]]; then
    cp "$TEMP_DIR/AppsFlyerDependencies.xml" "$DEPS_XML"
  fi
  if [[ "$TESTS_MOVED" == "true" && -d "$TESTS_BACKUP" ]]; then
    rm -rf "$TESTS_DIR"
    mv "$TESTS_BACKUP" "$TESTS_DIR"
  fi
  if [[ "$TESTS_META_MOVED" == "true" && -f "$TESTS_META_BACKUP" ]]; then
    rm -f "$TESTS_META"
    mv "$TESTS_META_BACKUP" "$TESTS_META"
  fi

  rm -rf "$REPO_ROOT/Library" "$REPO_ROOT/Logs" "$REPO_ROOT/Packages"
  rm -rf "$TEMP_DIR"
}
trap cleanup EXIT

echo "Start build for $PACKAGE_NAME"

cp "$DEPS_XML" "$TEMP_DIR/AppsFlyerDependencies.xml"

echo "Removing all remoteSwiftPackage (SPM) blocks so strict mode resolves exclusively via CocoaPods."
# Strict-mode pods (AppsFlyerFramework/Strict, PurchaseConnector/Strict) have no SPM
# equivalents, so any remoteSwiftPackage block left in place would still be added by
# EDM4U's SwiftPackageManager.AddPackagesToProject(), which applies every declared
# remoteSwiftPackage unconditionally regardless of the iosPods block. That would pull
# in the regular (non-strict) SPM packages alongside the strict CocoaPods below.
sed -i.bak '/<remoteSwiftPackage /,/<\/remoteSwiftPackage>/d' "$DEPS_XML"
# Drop the now-stale comment (from the non-strict XML) that documents the SPM/iosPod
# fallback behavior, since strict mode no longer declares any remoteSwiftPackage.
sed -i.bak '/<!-- iOS dependencies via Swift Package Manager/,/disabled it falls back to the iosPods entries unchanged, so the Podfile path still works. -->/d' "$DEPS_XML"

echo "Swapping AppsFlyerFramework, AppsFlyerRPC, and PurchaseConnector iosPods to their strict-mode subspecs."
# AppsFlyerRPC's default ("Main") subspec depends on the plain AppsFlyerFramework pod, not
# AppsFlyerFramework/Strict, so it must also be pinned to its own Strict subspec here —
# otherwise CocoaPods pulls in both AppsFlyerFramework subspecs for the same target, each
# vendoring an xcframework product named AppsFlyerLib.xcframework, causing pod install to
# fail with "conflicting names: appsflyerlib.xcframework".
sed -i.bak 's|name="AppsFlyerFramework"|name="AppsFlyerFramework/Strict"|' "$DEPS_XML"
sed -i.bak 's|name="AppsFlyerRPC"|name="AppsFlyerRPC/Strict"|' "$DEPS_XML"
sed -i.bak 's|name="PurchaseConnector"|name="PurchaseConnector/Strict"|g' "$DEPS_XML"
rm -f "$DEPS_XML.bak"

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
  -logFile "$SCRIPT_DIR/create_unity_strict.log" \
  -projectPath "$REPO_ROOT" \
  -exportPackage \
  Assets/AppsFlyer \
  "$OUTPUT_DIR/$PACKAGE_NAME" \
  -quit

echo "Package exported successfully to $OUTPUT_DIR/$PACKAGE_NAME"

if [[ "$PRODUCTION" == "true" && "$OUTPUT_DIR" == "$DEPLOY_PATH" ]]; then
  mkdir -p "$REPO_ROOT/strict-mode-sdk"
  mv "$OUTPUT_DIR/$PACKAGE_NAME" "$REPO_ROOT/strict-mode-sdk/$PACKAGE_NAME"
  rmdir "$OUTPUT_DIR" 2>/dev/null || true
  echo "Moved strict package to $REPO_ROOT/strict-mode-sdk/$PACKAGE_NAME"
fi
