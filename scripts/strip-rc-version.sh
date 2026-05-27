#!/usr/bin/env bash

set -euo pipefail

RC_VERSION=""
BASE_VERSION=""

usage() {
  cat <<EOF
Usage: $(basename "$0") --rc-version <version-rcN> [--base-version <version>]

Strips the RC suffix from every Unity plugin version surface managed by
scripts/bump-version.sh. Native SDK dependency versions are left unchanged.
EOF
}

while [[ $# -gt 0 ]]; do
  case "$1" in
    --rc-version)
      RC_VERSION="$2"
      shift 2
      ;;
    --base-version)
      BASE_VERSION="$2"
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

if [[ -z "$RC_VERSION" ]]; then
  echo "Error: --rc-version is required" >&2
  exit 1
fi

if [[ ! "$RC_VERSION" =~ ^[0-9]+\.[0-9]+\.[0-9]+-rc[0-9]+$ ]]; then
  echo "Error: --rc-version must match X.Y.Z-rcN" >&2
  exit 1
fi

if [[ -z "$BASE_VERSION" ]]; then
  BASE_VERSION="${RC_VERSION%-rc*}"
fi

FILES=(
  "Assets/AppsFlyer/package.json"
  "Assets/AppsFlyer/AppsFlyer.cs"
  "Assets/AppsFlyer/Editor/AppsFlyerDependencies.xml"
  "Assets/AppsFlyer/Plugins/iOS/AppsFlyeriOSWrapper.mm"
  "android-unity-wrapper/unitywrapper/src/main/java/com/appsflyer/unity/AppsFlyerAndroidWrapper.java"
  "android-unity-wrapper/gradle.properties"
  "deploy/build_unity_package.sh"
  "deploy/strict_mode_build_package.sh"
  "CHANGELOG.md"
  "README.md"
)

echo "Promoting $RC_VERSION to $BASE_VERSION"
SEARCH_VERSION="${RC_VERSION//./\\.}"

for file in "${FILES[@]}"; do
  if [[ -f "$file" ]]; then
    sed -i.bak "s|$SEARCH_VERSION|$BASE_VERSION|g" "$file"
    rm -f "$file.bak"
    echo "Updated $file"
  fi
done
