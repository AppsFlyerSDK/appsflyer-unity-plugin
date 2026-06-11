#!/usr/bin/env bash
# ios-pod-install.sh — Write Podfile into the Unity-generated Xcode project and run pod install.
# Called after game-ci/unity-builder (or local Unity batchmode) generates the Xcode project.
#
# Usage:
#   ./scripts/ios-pod-install.sh <ios-build-dir>
#   ./scripts/ios-pod-install.sh test-app/Build/iOS

set -euo pipefail

IOS_BUILD_DIR="${1:-test-app/Build/iOS}"

if [ ! -d "$IOS_BUILD_DIR" ]; then
  echo "[ios-pod-install] ERROR: iOS build dir not found: $IOS_BUILD_DIR" >&2
  exit 1
fi

# Read versions from AppsFlyerDependencies.xml — single source of truth set by bump-version.sh
DEPS_XML="Assets/AppsFlyer/Editor/AppsFlyerDependencies.xml"
AF_VERSION=$(grep -Eo 'name="AppsFlyerFramework" version="[^"]*"' "$DEPS_XML" | grep -Eo '[0-9]+\.[0-9]+\.[0-9]+' | head -1)
PC_VERSION=$(grep -Eo 'name="PurchaseConnector" version="[^"]*"' "$DEPS_XML" | grep -Eo '[0-9]+\.[0-9]+\.[0-9]+' | head -1)

if [[ -z "$AF_VERSION" ]]; then
  echo "[ios-pod-install] ERROR: could not parse AppsFlyerFramework version from $DEPS_XML" >&2
  exit 1
fi

echo "[ios-pod-install] AppsFlyerFramework: $AF_VERSION"
echo "[ios-pod-install] PurchaseConnector:  ${PC_VERSION:-"(not found, skipping)"}"
echo "[ios-pod-install] Writing Podfile to $IOS_BUILD_DIR"

cat > "$IOS_BUILD_DIR/Podfile" <<PODFILE
platform :ios, '17.0'

use_frameworks! :linkage => :static

target 'Unity-iPhone' do
  pod 'AppsFlyerFramework', '$AF_VERSION'
$([ -n "$PC_VERSION" ] && echo "  pod 'PurchaseConnector', '$PC_VERSION'")
  target 'Unity-iPhone Tests' do
    inherit! :search_paths
  end
end

target 'UnityFramework' do
  pod 'AppsFlyerFramework', '$AF_VERSION'
$([ -n "$PC_VERSION" ] && echo "  pod 'PurchaseConnector', '$PC_VERSION'")
end

post_install do |installer|
  # All pod privacy bundle targets (names ending in _Privacy) reference a
  # compiled file that Xcode never produces on simulator builds, causing
  # "Build input file cannot be found". Remove every such target and all
  # dependency edges pointing to it.
  pods_project = installer.pods_project
  privacy_targets = pods_project.native_targets.select { |t| t.name.end_with?('_Privacy') }
  privacy_uuids   = privacy_targets.map(&:uuid).to_set

  pods_project.targets.each do |t|
    t.dependencies.select { |d|
      privacy_uuids.include?(d.target_proxy.remote_global_id_string) rescue false
    }.each(&:remove_from_project)
  end

  privacy_targets.each do |pt|
    pods_project.root_object.targets.delete(pt)
    pt.remove_from_project
  end
end
PODFILE

echo "[ios-pod-install] Running pod install in $IOS_BUILD_DIR"
cd "$IOS_BUILD_DIR"
pod install

echo "[ios-pod-install] Stubbing out _Privacy.bundle install_resource calls (simulator builds only)"
find . -name "*-resources.sh" -exec sed -i '' 's/install_resource.*_Privacy\.bundle.*/:/' {} \;

echo "[ios-pod-install] Patching project.pbxproj — removing hardcoded SDKROOT so -sdk flag wins"
# Unity hardcodes SDKROOT = iphoneos at the target level which overrides xcodebuild's -sdk flag.
# Must run after pod install since pod install rewrites parts of the project.
# Removing it lets the command-line -sdk iphonesimulator take effect for simulator builds.
sed -i '' 's/SDKROOT = iphoneos;//g' "Unity-iPhone.xcodeproj/project.pbxproj"

echo "[ios-pod-install] Done. Build with Unity-iPhone.xcworkspace"
