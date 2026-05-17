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

echo "[ios-pod-install] Writing Podfile to $IOS_BUILD_DIR"

cat > "$IOS_BUILD_DIR/Podfile" <<'PODFILE'
platform :ios, '15.0'

use_frameworks! :linkage => :static

target 'Unity-iPhone' do
  pod 'AppsFlyerFramework', '6.17.9'
  pod 'PurchaseConnector', '6.17.9'

  target 'Unity-iPhone Tests' do
    inherit! :search_paths
  end
end

target 'UnityFramework' do
  pod 'AppsFlyerFramework', '6.17.9'
  pod 'PurchaseConnector', '6.17.9'
end

post_install do |installer|
  # PurchaseConnector 6.17.9 ships a privacy bundle target that references a
  # file 'PurchaseConnector_Privacy' which does not exist on disk, causing
  # simulator builds to fail with "Build input file cannot be found".
  # Strip all build phases from the broken target so it becomes a no-op.
  installer.pods_project.targets.each do |target|
    next unless target.name == 'PurchaseConnector-PurchaseConnector_Privacy'
    target.build_phases.to_a.each(&:remove_from_project)
  end
end
PODFILE

echo "[ios-pod-install] Running pod install in $IOS_BUILD_DIR"
cd "$IOS_BUILD_DIR"
pod install

echo "[ios-pod-install] Patching project.pbxproj — removing hardcoded SDKROOT so -sdk flag wins"
# Unity hardcodes SDKROOT = iphoneos at the target level which overrides xcodebuild's -sdk flag.
# Must run after pod install since pod install rewrites parts of the project.
# Removing it lets the command-line -sdk iphonesimulator take effect for simulator builds.
sed -i '' 's/SDKROOT = iphoneos;//g' "Unity-iPhone.xcodeproj/project.pbxproj"

echo "[ios-pod-install] Done. Build with Unity-iPhone.xcworkspace"
