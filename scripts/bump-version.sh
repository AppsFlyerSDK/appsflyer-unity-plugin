#!/usr/bin/env bash
#
# bump-version.sh — Update all AppsFlyer Unity plugin version surfaces
#
# Usage:
#   ./scripts/bump-version.sh \
#     --plugin-version 6.18.0-rc1 \
#     --android-sdk-version 6.18.0 \
#     --ios-sdk-version 6.18.0 \
#     [--ios-pc-version 6.18.0] \
#     [--android-pc-version 2.2.0]
#
# The unity-wrapper version defaults to the plugin base version (RC suffix
# stripped, e.g. 6.18.0-rc1 -> 6.18.0). Override with --unity-wrapper-version.
# iOS Purchase Connector defaults to --ios-sdk-version; override with --ios-pc-version.
# Android Purchase Connector version is independent; omit to leave it unchanged.
# Run from the repo root.

set -euo pipefail

PLUGIN_VERSION=""
ANDROID_SDK_VERSION=""
IOS_SDK_VERSION=""
UNITY_WRAPPER_VERSION=""
ANDROID_PC_VERSION=""
IOS_PC_VERSION=""
ANDROID_BILLING_VERSION=""

while [[ $# -gt 0 ]]; do
  case "$1" in
    --plugin-version) PLUGIN_VERSION="$2"; shift 2 ;;
    --android-sdk-version) ANDROID_SDK_VERSION="$2"; shift 2 ;;
    --ios-sdk-version) IOS_SDK_VERSION="$2"; shift 2 ;;
    --unity-wrapper-version) UNITY_WRAPPER_VERSION="$2"; shift 2 ;;
    --android-pc-version) ANDROID_PC_VERSION="$2"; shift 2 ;;
    --ios-pc-version) IOS_PC_VERSION="$2"; shift 2 ;;
    --android-billing-version) ANDROID_BILLING_VERSION="$2"; shift 2 ;;
    *) echo "Unknown option: $1"; exit 1 ;;
  esac
done

[[ -z "$PLUGIN_VERSION" ]]       && { echo "Error: --plugin-version is required";       exit 1; }
[[ -z "$ANDROID_SDK_VERSION" ]]  && { echo "Error: --android-sdk-version is required";  exit 1; }
[[ -z "$IOS_SDK_VERSION" ]]      && { echo "Error: --ios-sdk-version is required";      exit 1; }

BASE_VERSION="${PLUGIN_VERSION%%-rc*}"
UNITY_WRAPPER_VERSION="${UNITY_WRAPPER_VERSION:-$BASE_VERSION}"
IOS_PC_VERSION="${IOS_PC_VERSION:-$IOS_SDK_VERSION}"

echo "Bumping versions:"
echo "  plugin:         $PLUGIN_VERSION"
echo "  plugin-base:    $BASE_VERSION"
echo "  android-sdk:    $ANDROID_SDK_VERSION"
echo "  ios-sdk:        $IOS_SDK_VERSION"
echo "  unity-wrapper:  $UNITY_WRAPPER_VERSION"
echo "  ios-pc:         $IOS_PC_VERSION"
echo "  android-pc:     ${ANDROID_PC_VERSION:-"(unchanged)"}"
echo "  android-billing: ${ANDROID_BILLING_VERSION:-"(unchanged)"}"
echo ""

# ── 1. Assets/AppsFlyer/package.json ─────────────────────────────────────────
PKG_JSON="Assets/AppsFlyer/package.json"
echo "[1/14] $PKG_JSON"
sed -i.bak "s/\"version\": \"[^\"]*\"/\"version\": \"$PLUGIN_VERSION\"/" "$PKG_JSON"
rm -f "${PKG_JSON}.bak"

# ── 2. Assets/AppsFlyer/AppsFlyer.cs ─────────────────────────────────────────
AF_CS="Assets/AppsFlyer/AppsFlyer.cs"
echo "[2/14] $AF_CS"
sed -i.bak "s/kAppsFlyerPluginVersion = \"[^\"]*\"/kAppsFlyerPluginVersion = \"$PLUGIN_VERSION\"/" "$AF_CS"
rm -f "${AF_CS}.bak"

# ── 3-6. Assets/AppsFlyer/Editor/AppsFlyerDependencies.xml ───────────────────
DEPS_XML="Assets/AppsFlyer/Editor/AppsFlyerDependencies.xml"
echo "[3/14] $DEPS_XML — af-android-sdk"
sed -i.bak "s|af-android-sdk:[^\"]*|af-android-sdk:$ANDROID_SDK_VERSION|" "$DEPS_XML"

echo "[4/14] $DEPS_XML — unity-wrapper"
sed -i.bak "s|unity-wrapper:[^\"]*|unity-wrapper:$UNITY_WRAPPER_VERSION|" "$DEPS_XML"

echo "[5/14] $DEPS_XML — AppsFlyerFramework"
sed -i.bak "s|name=\"AppsFlyerFramework\" version=\"[^\"]*\"|name=\"AppsFlyerFramework\" version=\"$IOS_SDK_VERSION\"|" "$DEPS_XML"

echo "[6/14] $DEPS_XML — PurchaseConnector (iOS) → $IOS_PC_VERSION"
sed -i.bak "s|name=\"PurchaseConnector\" version=\"[^\"]*\"|name=\"PurchaseConnector\" version=\"$IOS_PC_VERSION\"|" "$DEPS_XML"

if [[ -n "$ANDROID_PC_VERSION" ]]; then
  echo "[6b/14] $DEPS_XML — purchase-connector (Android)"
  sed -i.bak "s|purchase-connector:[^\"]*|purchase-connector:$ANDROID_PC_VERSION|" "$DEPS_XML"
fi
rm -f "${DEPS_XML}.bak"

# ── 7. Assets/AppsFlyer/Plugins/iOS/AppsFlyeriOSWrapper.mm ───────────────────
IOS_WRAPPER="Assets/AppsFlyer/Plugins/iOS/AppsFlyeriOSWrapper.mm"
echo "[7/14] $IOS_WRAPPER"
sed -i.bak "s|pluginVersion:@\"[^\"]*\"|pluginVersion:@\"$PLUGIN_VERSION\"|" "$IOS_WRAPPER"
rm -f "${IOS_WRAPPER}.bak"

# ── 8. android-unity-wrapper Java bridge ─────────────────────────────────────
ANDROID_WRAPPER_JAVA="android-unity-wrapper/unitywrapper/src/main/java/com/appsflyer/unity/AppsFlyerAndroidWrapper.java"
if [[ -f "$ANDROID_WRAPPER_JAVA" ]]; then
  echo "[8/14] $ANDROID_WRAPPER_JAVA — PluginInfo Unity plugin base version"
  sed -i.bak "s|PLUGIN_VERSION = \"[^\"]*\"|PLUGIN_VERSION = \"$BASE_VERSION\"|" "$ANDROID_WRAPPER_JAVA"
  rm -f "${ANDROID_WRAPPER_JAVA}.bak"
fi

# ── 9. android-unity-wrapper/gradle.properties ───────────────────────────────
ANDROID_WRAPPER_PROPS="android-unity-wrapper/gradle.properties"
if [[ -f "$ANDROID_WRAPPER_PROPS" ]]; then
  echo "[9/14] $ANDROID_WRAPPER_PROPS"
  current_version_code="$(grep '^VERSION_CODE=' "$ANDROID_WRAPPER_PROPS" | cut -d= -f2)"
  current_version_name="$(grep '^VERSION_NAME=' "$ANDROID_WRAPPER_PROPS" | cut -d= -f2)"
  if [[ "$current_version_name" != "$UNITY_WRAPPER_VERSION" ]]; then
    new_version_code=$((current_version_code + 1))
    sed -i.bak "s|^VERSION_CODE=.*|VERSION_CODE=$new_version_code|" "$ANDROID_WRAPPER_PROPS"
    echo "  VERSION_CODE=$new_version_code (was $current_version_code)"
  else
    echo "  VERSION_CODE=$current_version_code (unchanged; VERSION_NAME already $UNITY_WRAPPER_VERSION)"
  fi
  sed -i.bak "s|^VERSION_NAME=.*|VERSION_NAME=$UNITY_WRAPPER_VERSION|" "$ANDROID_WRAPPER_PROPS"
  if grep -q "^ANDROID_SDK_VERSION=" "$ANDROID_WRAPPER_PROPS"; then
    sed -i.bak "s|^ANDROID_SDK_VERSION=.*|ANDROID_SDK_VERSION=$ANDROID_SDK_VERSION|" "$ANDROID_WRAPPER_PROPS"
  else
    echo "ANDROID_SDK_VERSION=$ANDROID_SDK_VERSION" >> "$ANDROID_WRAPPER_PROPS"
  fi
  if [[ -n "$ANDROID_PC_VERSION" ]]; then
    if grep -q "^ANDROID_PC_VERSION=" "$ANDROID_WRAPPER_PROPS"; then
      sed -i.bak "s|^ANDROID_PC_VERSION=.*|ANDROID_PC_VERSION=$ANDROID_PC_VERSION|" "$ANDROID_WRAPPER_PROPS"
    else
      echo "ANDROID_PC_VERSION=$ANDROID_PC_VERSION" >> "$ANDROID_WRAPPER_PROPS"
    fi
  fi
  rm -f "${ANDROID_WRAPPER_PROPS}.bak"
fi

# ── 10. android-unity-wrapper/unitywrapper/build.gradle ──────────────────────
UNITYWRAPPER_BUILD="android-unity-wrapper/unitywrapper/build.gradle"
if [[ -f "$UNITYWRAPPER_BUILD" ]]; then
  echo "[10/14] $UNITYWRAPPER_BUILD — af-android-sdk uses ANDROID_SDK_VERSION"
  if grep -q "com.appsflyer:af-android-sdk:[^$]" "$UNITYWRAPPER_BUILD"; then
    sed -i.bak 's|com.appsflyer:af-android-sdk:[^"'"'"']*|com.appsflyer:af-android-sdk:$ANDROID_SDK_VERSION|' "$UNITYWRAPPER_BUILD"
    rm -f "${UNITYWRAPPER_BUILD}.bak"
  fi
  if [[ -n "$ANDROID_BILLING_VERSION" ]]; then
    echo "[10b/14] $UNITYWRAPPER_BUILD — billingclient:billing → $ANDROID_BILLING_VERSION"
    sed -i.bak "s|billingclient:billing:[^'\"]*|billingclient:billing:$ANDROID_BILLING_VERSION|" "$UNITYWRAPPER_BUILD"
    rm -f "${UNITYWRAPPER_BUILD}.bak"
  fi
fi

# ── 11. deploy/build_unity_package.sh ────────────────────────────────────────
BUILD_SH="deploy/build_unity_package.sh"
echo "[11/14] $BUILD_SH"
sed -i.bak "s|PACKAGE_NAME=\"appsflyer-unity-plugin-[^\"]*\.unitypackage\"|PACKAGE_NAME=\"appsflyer-unity-plugin-${PLUGIN_VERSION}.unitypackage\"|" "$BUILD_SH"
rm -f "${BUILD_SH}.bak"

# ── 12. deploy/strict_mode_build_package.sh ──────────────────────────────────
STRICT_SH="deploy/strict_mode_build_package.sh"
echo "[12/14] $STRICT_SH"
sed -i.bak "s|PACKAGE_NAME=\"appsflyer-unity-plugin-strict-mode-[^\"]*\.unitypackage\"|PACKAGE_NAME=\"appsflyer-unity-plugin-strict-mode-${PLUGIN_VERSION}.unitypackage\"|" "$STRICT_SH"
rm -f "${STRICT_SH}.bak"

# ── 9. test-app/Assets/Plugins/Android/mainTemplate.gradle ───────────────────
MAIN_GRADLE="test-app/Assets/Plugins/Android/mainTemplate.gradle"
if [[ -f "$MAIN_GRADLE" ]]; then
  echo "[13/14] $MAIN_GRADLE — af-android-sdk"
  sed -i.bak "s|com.appsflyer:af-android-sdk:[^']*|com.appsflyer:af-android-sdk:$ANDROID_SDK_VERSION|" "$MAIN_GRADLE"
  rm -f "${MAIN_GRADLE}.bak"
fi

# ── 10. scripts/ios-pod-install.sh ───────────────────────────────────────────
IOS_POD_SH="scripts/ios-pod-install.sh"
if [[ -f "$IOS_POD_SH" ]]; then
  echo "[14/14] $IOS_POD_SH — AppsFlyerFramework"
  sed -i.bak "s|pod 'AppsFlyerFramework', '[^']*'|pod 'AppsFlyerFramework', '$IOS_SDK_VERSION'|g" "$IOS_POD_SH"
  rm -f "${IOS_POD_SH}.bak"
fi

# ── CHANGELOG.md — update base version section (never RC-specific headers) ───
CHANGELOG="CHANGELOG.md"
CHANGELOG_HEADER="## v${BASE_VERSION}"
if [[ -f "$CHANGELOG" ]]; then
  CHANGELOG_BULLETS=(
    "* Update Android SDK version - $ANDROID_SDK_VERSION"
    "* Update iOS SDK version - $IOS_SDK_VERSION"
    "* Update iOS Purchase Connector version - $IOS_PC_VERSION"
    "* Update Android unity-wrapper version - $UNITY_WRAPPER_VERSION"
  )
  if [[ -n "$ANDROID_PC_VERSION" ]]; then
    CHANGELOG_BULLETS+=("* Update Android Purchase Connector version - $ANDROID_PC_VERSION")
  fi
  if [[ "$PLUGIN_VERSION" != "$BASE_VERSION" ]]; then
    CHANGELOG_BULLETS+=("* Unity plugin version - $PLUGIN_VERSION")
  fi

  BULLETS_FILE=$(mktemp)
  TMP=$(mktemp)
  printf '%s\n' "${CHANGELOG_BULLETS[@]}" > "$BULLETS_FILE"

  if grep -qF "$CHANGELOG_HEADER" "$CHANGELOG"; then
    awk -v header="$CHANGELOG_HEADER" -v bullets_file="$BULLETS_FILE" '
      BEGIN {
        while ((getline line < bullets_file) > 0) {
          bullets[++bullet_count] = line
        }
        close(bullets_file)
      }
      function section_has_bullet(section_text, bullet) {
        return index(section_text, bullet) > 0
      }
      function flush_section() {
        if (!in_section) {
          return
        }
        print header
        printf "%s", section_body
        for (i = 1; i <= bullet_count; i++) {
          if (!section_has_bullet(section_body, bullets[i])) {
            print bullets[i]
          }
        }
        in_section = 0
        section_body = ""
      }
      {
        if ($0 == header) {
          flush_section()
          in_section = 1
          next
        }
        if (in_section && /^## /) {
          flush_section()
          print $0
          next
        }
        if (in_section) {
          section_body = section_body $0 "\n"
          next
        }
        print $0
      }
      END {
        if (in_section) {
          flush_section()
        }
      }
    ' "$CHANGELOG" > "$TMP"
    mv "$TMP" "$CHANGELOG"
    echo "[+] CHANGELOG.md — appended version bumps to $CHANGELOG_HEADER"
  else
    awk -v header="$CHANGELOG_HEADER" -v bullets_file="$BULLETS_FILE" '
      BEGIN {
        while ((getline line < bullets_file) > 0) {
          bullets[++bullet_count] = line
        }
        close(bullets_file)
      }
      /^# Versions/ {
        print
        print ""
        print header
        for (i = 1; i <= bullet_count; i++) {
          print bullets[i]
        }
        print ""
        inserted = 1
        next
      }
      { print }
      END {
        if (!inserted) {
          print ""
          print header
          for (i = 1; i <= bullet_count; i++) {
            print bullets[i]
          }
          print ""
        }
      }
    ' "$CHANGELOG" > "$TMP"
    mv "$TMP" "$CHANGELOG"
    echo "[+] CHANGELOG.md — created $CHANGELOG_HEADER with version bumps"
  fi

  rm -f "$BULLETS_FILE"
fi

# ── README.md / docs — native SDK and Purchase Connector version surfaces ─────
update_doc_native_versions() {
  local file="$1"
  [[ -f "$file" ]] || return 0
  sed -i.bak "s|- Android AppsFlyer SDK v[0-9][0-9.]*|- Android AppsFlyer SDK v$ANDROID_SDK_VERSION|" "$file"
  sed -i.bak "s|- iOS AppsFlyer SDK v[0-9][0-9.]*|- iOS AppsFlyer SDK v$IOS_SDK_VERSION|" "$file"
  sed -i.bak "s|- iOS Purchase Connector [0-9][0-9.]*|- iOS Purchase Connector $IOS_PC_VERSION|" "$file"
  rm -f "${file}.bak"
}

README="README.md"
if [[ -f "$README" ]]; then
  update_doc_native_versions "$README"
  sed -i.bak "s|af-android-sdk:[0-9][0-9.]*|af-android-sdk:$ANDROID_SDK_VERSION|g" "$README"
  sed -i.bak "s|AppsFlyerFramework/[0-9][0-9.]*|AppsFlyerFramework/$IOS_SDK_VERSION|g" "$README"
  sed -i.bak "s|AppsFlyerFramework', '[0-9][0-9.]*|AppsFlyerFramework', '$IOS_SDK_VERSION|g" "$README"
  rm -f "${README}.bak"
  echo "[+] README.md — updated native SDK and Purchase Connector references"
fi

INTRO="docs/Introduction.md"
if [[ -f "$INTRO" ]]; then
  update_doc_native_versions "$INTRO"
  echo "[+] docs/Introduction.md — updated native SDK and Purchase Connector references"
fi

echo ""
echo "Done. Verify with: git diff"
