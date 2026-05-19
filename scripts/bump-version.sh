#!/usr/bin/env bash
#
# bump-version.sh — Update all AppsFlyer Unity plugin version surfaces
#
# Usage:
#   ./scripts/bump-version.sh \
#     --plugin-version 6.18.0-rc1 \
#     --android-sdk-version 6.18.0 \
#     --ios-sdk-version 6.18.0
#
# The unity-wrapper version is always derived from --plugin-version.
# Run from the repo root.

set -euo pipefail

PLUGIN_VERSION=""
ANDROID_SDK_VERSION=""
IOS_SDK_VERSION=""

while [[ $# -gt 0 ]]; do
  case "$1" in
    --plugin-version)    PLUGIN_VERSION="$2";    shift 2 ;;
    --android-sdk-version) ANDROID_SDK_VERSION="$2"; shift 2 ;;
    --ios-sdk-version)   IOS_SDK_VERSION="$2";   shift 2 ;;
    *) echo "Unknown option: $1"; exit 1 ;;
  esac
done

[[ -z "$PLUGIN_VERSION" ]]       && { echo "Error: --plugin-version is required";       exit 1; }
[[ -z "$ANDROID_SDK_VERSION" ]]  && { echo "Error: --android-sdk-version is required";  exit 1; }
[[ -z "$IOS_SDK_VERSION" ]]      && { echo "Error: --ios-sdk-version is required";      exit 1; }

UNITY_WRAPPER_VERSION="$PLUGIN_VERSION"

echo "Bumping versions:"
echo "  plugin:         $PLUGIN_VERSION"
echo "  android-sdk:    $ANDROID_SDK_VERSION"
echo "  ios-sdk:        $IOS_SDK_VERSION"
echo "  unity-wrapper:  $UNITY_WRAPPER_VERSION"
echo ""

# ── 1. Assets/AppsFlyer/package.json ─────────────────────────────────────────
PKG_JSON="Assets/AppsFlyer/package.json"
echo "[1/8] $PKG_JSON"
sed -i.bak "s/\"version\": \"[^\"]*\"/\"version\": \"$PLUGIN_VERSION\"/" "$PKG_JSON"
rm -f "${PKG_JSON}.bak"

# ── 2. Assets/AppsFlyer/AppsFlyer.cs ─────────────────────────────────────────
AF_CS="Assets/AppsFlyer/AppsFlyer.cs"
echo "[2/8] $AF_CS"
sed -i.bak "s/kAppsFlyerPluginVersion = \"[^\"]*\"/kAppsFlyerPluginVersion = \"$PLUGIN_VERSION\"/" "$AF_CS"
rm -f "${AF_CS}.bak"

# ── 3-6. Assets/AppsFlyer/Editor/AppsFlyerDependencies.xml ───────────────────
DEPS_XML="Assets/AppsFlyer/Editor/AppsFlyerDependencies.xml"
echo "[3/8] $DEPS_XML — af-android-sdk"
sed -i.bak "s|af-android-sdk:[^\"]*|af-android-sdk:$ANDROID_SDK_VERSION|" "$DEPS_XML"

echo "[4/8] $DEPS_XML — unity-wrapper"
sed -i.bak "s|unity-wrapper:[^\"]*|unity-wrapper:$UNITY_WRAPPER_VERSION|" "$DEPS_XML"

echo "[5/8] $DEPS_XML — AppsFlyerFramework"
sed -i.bak "s|name=\"AppsFlyerFramework\" version=\"[^\"]*\"|name=\"AppsFlyerFramework\" version=\"$IOS_SDK_VERSION\"|" "$DEPS_XML"

echo "[6/8] $DEPS_XML — PurchaseConnector"
sed -i.bak "s|name=\"PurchaseConnector\" version=\"[^\"]*\"|name=\"PurchaseConnector\" version=\"$IOS_SDK_VERSION\"|" "$DEPS_XML"
rm -f "${DEPS_XML}.bak"

# ── 7. deploy/build_unity_package.sh ─────────────────────────────────────────
BUILD_SH="deploy/build_unity_package.sh"
echo "[7/8] $BUILD_SH"
sed -i.bak "s|PACKAGE_NAME=\"appsflyer-unity-plugin-[^\"]*\.unitypackage\"|PACKAGE_NAME=\"appsflyer-unity-plugin-${PLUGIN_VERSION}.unitypackage\"|" "$BUILD_SH"
rm -f "${BUILD_SH}.bak"

# ── 8. deploy/strict_mode_build_package.sh ───────────────────────────────────
STRICT_SH="deploy/strict_mode_build_package.sh"
echo "[8/8] $STRICT_SH"
sed -i.bak "s|PACKAGE_NAME=\"appsflyer-unity-plugin-strict-mode-[^\"]*\.unitypackage\"|PACKAGE_NAME=\"appsflyer-unity-plugin-strict-mode-${PLUGIN_VERSION}.unitypackage\"|" "$STRICT_SH"
rm -f "${STRICT_SH}.bak"

# ── 9. test-app/Assets/Plugins/Android/mainTemplate.gradle ───────────────────
MAIN_GRADLE="test-app/Assets/Plugins/Android/mainTemplate.gradle"
if [[ -f "$MAIN_GRADLE" ]]; then
  echo "[9/10] $MAIN_GRADLE — af-android-sdk"
  sed -i.bak "s|com.appsflyer:af-android-sdk:[^']*|com.appsflyer:af-android-sdk:$ANDROID_SDK_VERSION|" "$MAIN_GRADLE"
  rm -f "${MAIN_GRADLE}.bak"
fi

# ── 10. scripts/ios-pod-install.sh ───────────────────────────────────────────
IOS_POD_SH="scripts/ios-pod-install.sh"
if [[ -f "$IOS_POD_SH" ]]; then
  echo "[10/10] $IOS_POD_SH — AppsFlyerFramework"
  sed -i.bak "s|pod 'AppsFlyerFramework', '[^']*'|pod 'AppsFlyerFramework', '$IOS_SDK_VERSION'|g" "$IOS_POD_SH"
  rm -f "${IOS_POD_SH}.bak"
fi

# ── CHANGELOG.md — prepend new version header if not already present ──────────
CHANGELOG="CHANGELOG.md"
if [[ -f "$CHANGELOG" ]]; then
  if ! grep -q "^## $PLUGIN_VERSION" "$CHANGELOG"; then
    TODAY=$(date +%Y-%m-%d)
    TMP=$(mktemp)
    # Prepend the new section after any top-level title (# ...) or at file start
    awk -v ver="$PLUGIN_VERSION" -v date="$TODAY" '
      NR==1 && /^# / { print; print ""; print "## " ver " (" date ")"; print ""; next }
      NR==1           { print "## " ver " (" date ")"; print ""; print; next }
      { print }
    ' "$CHANGELOG" > "$TMP"
    mv "$TMP" "$CHANGELOG"
    echo "[+] CHANGELOG.md — prepended ## $PLUGIN_VERSION"
  else
    echo "[~] CHANGELOG.md — ## $PLUGIN_VERSION already present, skipped"
  fi
fi

# ── README.md — update inline native SDK version references ──────────────────
README="README.md"
if [[ -f "$README" ]]; then
  # Replace versioned maven artifact refs and CocoaPods pod versions
  # Pattern: af-android-sdk:<old_version> → af-android-sdk:<new_version>
  sed -i.bak "s|af-android-sdk:[0-9][0-9.]*|af-android-sdk:$ANDROID_SDK_VERSION|g" "$README"
  # Pattern: AppsFlyerFramework/<old_version> or AppsFlyerFramework ~> <old_version>
  sed -i.bak "s|AppsFlyerFramework/[0-9][0-9.]*|AppsFlyerFramework/$IOS_SDK_VERSION|g" "$README"
  sed -i.bak "s|AppsFlyerFramework', '[0-9][0-9.]*|AppsFlyerFramework', '$IOS_SDK_VERSION|g" "$README"
  rm -f "${README}.bak"
  echo "[+] README.md — updated native SDK version references"
fi

echo ""
echo "Done. Verify with: git diff"
