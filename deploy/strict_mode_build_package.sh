#!/usr/bin/env bash

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"

DEPLOY_PATH="$SCRIPT_DIR/outputs"
PACKAGE_NAME="appsflyer-unity-plugin-strict-mode-6.18.0-rc2.unitypackage"
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
  -p, --production          Preserve the legacy strict-mode output location.
  -h, --help                Show this help.

UNITY_PATH can override the Unity executable path.
EOF
}

while [[ $# -gt 0 ]]; do
  case "$1" in
    --version)
      PACKAGE_NAME="appsflyer-unity-plugin-strict-mode-6.18.0-rc2.unitypackage"
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
IOS_WRAPPER="$REPO_ROOT/Assets/AppsFlyer/Plugins/iOS/AppsFlyeriOSWrapper.mm"
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
  if [[ -f "$TEMP_DIR/AppsFlyeriOSWrapper.mm" ]]; then
    cp "$TEMP_DIR/AppsFlyeriOSWrapper.mm" "$IOS_WRAPPER"
  fi
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

cp "$DEPS_XML" "$TEMP_DIR/AppsFlyerDependencies.xml"
cp "$IOS_WRAPPER" "$TEMP_DIR/AppsFlyeriOSWrapper.mm"

echo "Changing iOS pods to strict-mode variants."
sed -i.bak 's|name="AppsFlyerFramework"|name="AppsFlyerFramework/Strict"|g' "$DEPS_XML"
sed -i.bak 's|name="PurchaseConnector"|name="PurchaseConnector/Strict"|g' "$DEPS_XML"
rm -f "$DEPS_XML.bak"

echo "Disabling IDFA/ATT calls for strict mode."
sed -i.bak 's|^\([[:space:]]*\)\(\[AppsFlyerLib shared\]\.disableAdvertisingIdentifier\)|\1//\2|g' "$IOS_WRAPPER"
sed -i.bak 's|^\([[:space:]]*\)\(\[\[AppsFlyerLib shared\] waitForATTUserAuthorizationWithTimeoutInterval:timeoutInterval\];\)|\1//\2|g' "$IOS_WRAPPER"
rm -f "$IOS_WRAPPER.bak"

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
