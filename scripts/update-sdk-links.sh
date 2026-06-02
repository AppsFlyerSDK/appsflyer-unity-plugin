#!/usr/bin/env bash

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"

DEPS_XML="$REPO_ROOT/Assets/AppsFlyer/Editor/AppsFlyerDependencies.xml"
INSTALLATION_MD="$REPO_ROOT/docs/Installation.md"

read_android_package_version() {
  local artifact="$1"
  grep -Eo "spec=\"com\.appsflyer:${artifact}:[^\"]+\"" "$DEPS_XML" |
    sed -E "s/.*:${artifact}:([^\"]+)\"/\1/" |
    head -1
}

read_ios_pod_version() {
  local pod="$1"
  grep -Eo "name=\"${pod}\" version=\"[^\"]+\"" "$DEPS_XML" |
    sed -E 's/.* version="([^"]+)"/\1/' |
    head -1
}

require_version() {
  local name="$1"
  local value="$2"

  if [[ -z "$value" ]]; then
    echo "Error: could not resolve $name from $DEPS_XML" >&2
    exit 1
  fi
}

ANDROID_SDK_VERSION="$(read_android_package_version "af-android-sdk")"
UNITY_WRAPPER_VERSION="$(read_android_package_version "unity-wrapper")"
IOS_SDK_VERSION="$(read_ios_pod_version "AppsFlyerFramework")"

require_version "Android SDK version" "$ANDROID_SDK_VERSION"
require_version "Unity wrapper version" "$UNITY_WRAPPER_VERSION"
require_version "iOS SDK version" "$IOS_SDK_VERSION"

IOS_MAJOR_VERSION="${IOS_SDK_VERSION%%.*}"
IOS_MINOR_VERSION="${IOS_SDK_VERSION%.*}"

export ANDROID_SDK_VERSION
export UNITY_WRAPPER_VERSION
export IOS_SDK_VERSION
export IOS_MAJOR_VERSION
export IOS_MINOR_VERSION

perl -0pi -e '
  my $version = $ENV{"ANDROID_SDK_VERSION"};
  s#https://repo1\.maven\.org/maven2/com/appsflyer/af-android-sdk/[0-9]+(?:\.[0-9]+)+/af-android-sdk-[0-9]+(?:\.[0-9]+)+\.aar#https://repo1.maven.org/maven2/com/appsflyer/af-android-sdk/$version/af-android-sdk-$version.aar#g;
' "$INSTALLATION_MD"

perl -0pi -e '
  my $version = $ENV{"UNITY_WRAPPER_VERSION"};
  s#^      2\. \[AppsFlyer Unity Wrapper\]\([^\n]+\).*$#      2. [AppsFlyer Unity Wrapper](https://repo1.maven.org/maven2/com/appsflyer/unity-wrapper/$version/unity-wrapper-$version.aar) - Billing Library 8#mg;
' "$INSTALLATION_MD"

perl -0pi -e '
  my $version = $ENV{"IOS_SDK_VERSION"};
  my $major = $ENV{"IOS_MAJOR_VERSION"};
  my $minor = $ENV{"IOS_MINOR_VERSION"};
  s#https://github\.com/AppsFlyerSDK/AppsFlyerFramework/releases/tag/[0-9]+(?:\.[0-9]+)+#https://github.com/AppsFlyerSDK/AppsFlyerFramework/releases/tag/$version#g;
  s#appsflyer\.com/ios/[0-9]+\.x\.x/[0-9]+\.[0-9]+\.x/[0-9]+(?:\.[0-9]+)+/AF-iOS-SDK-v[0-9]+(?:\.[0-9]+)+#appsflyer.com/ios/$major.x.x/$minor.x/$version/AF-iOS-SDK-v$version#g;
' "$INSTALLATION_MD"

echo "Updated installation links:"
echo "  Android SDK: $ANDROID_SDK_VERSION"
echo "  Unity wrapper: $UNITY_WRAPPER_VERSION"
echo "  iOS SDK: $IOS_SDK_VERSION"
