#!/usr/bin/env bash

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"

DEPLOY_PATH="$SCRIPT_DIR/outputs"
PACKAGE_NAME="appsflyer-unity-plugin-strict-mode-7.0.2-rc3.unitypackage"
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
      PACKAGE_NAME="appsflyer-unity-plugin-strict-mode-7.0.2-rc3.unitypackage"
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

echo "Changing PurchaseConnector CocoaPod fallback to its strict-mode subspec."
# PurchaseConnector has no Strict SPM product, so keep the CocoaPods subspec swap for its
# iosPod fallback below, but drop its remoteSwiftPackage block entirely: EDM4U's
# SwiftPackageManager.AddPackagesToProject() adds every declared remoteSwiftPackage
# unconditionally, so leaving it in place would add the regular PurchaseConnector SPM
# package alongside the PurchaseConnector/Strict CocoaPod.
sed -i.bak 's|name="PurchaseConnector"|name="PurchaseConnector/Strict"|g' "$DEPS_XML"
sed -i.bak '/<remoteSwiftPackage url="https:\/\/github.com\/AppsFlyerSDK\/PurchaseConnector-Dynamic.git"/,/<\/remoteSwiftPackage>/d' "$DEPS_XML"

echo "Swapping AppsFlyerRPC and AppsFlyerFramework SPM packages to their strict-mode products."
# The iosPod names for AppsFlyerRPC/AppsFlyerFramework are left unchanged so they keep
# matching their remoteSwiftPackage's replacesPod value: EDM4U's podsToIgnore check
# (IOSResolver.GenPodfile) is an exact string compare against the iosPod's current name,
# so renaming the iosPod here (as PurchaseConnector's does) would break that match and
# cause both the CocoaPod and the SPM package to be added.
sed -i.bak 's|<swiftPackage name="AppsFlyerRPC" replacesPod="AppsFlyerRPC"/>|<swiftPackage name="AppsFlyerRPCStrict" replacesPod="AppsFlyerRPC"/>|' "$DEPS_XML"
sed -i.bak 's|url="https://github.com/AppsFlyerSDK/AppsFlyerFramework-Dynamic.git"|url="https://github.com/AppsFlyerSDK/AppsFlyerFramework-Strict.git"|' "$DEPS_XML"
sed -i.bak 's|<swiftPackage name="AppsFlyerLib-Dynamic" replacesPod="AppsFlyerFramework"/>|<swiftPackage name="AppsFlyerLib" replacesPod="AppsFlyerFramework"/>|' "$DEPS_XML"
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
