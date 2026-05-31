---
name: sdk-version-bump
description: Safely bump the wrapped Android SDK or iOS SDK version in the AppsFlyer Unity plugin, update the plugin version if needed, and validate all related files including both billing library variants.
---

# SDK Version Bump

Use this skill when updating the native Android or iOS SDK version wrapped by the AppsFlyer Unity plugin.

## Goal

Perform a safe, consistent version bump across C#, Android, and iOS plugin layers, maintaining both billing library variants.

## Workflow

1. Identify which SDK is being bumped: Android SDK | iOS SDK | both
2. Locate current versions:
   - Version constant in `Assets/AppsFlyer/AppsFlyer.cs`
   - `android-unity-wrapper/build.gradle` (af-android-sdk, billing library v7/v8)
   - iOS podspec or dependency declarations
   - `CHANGELOG.md`
3. Update the native SDK version in all required places.
4. Check both billing library variants (v7 and v8) — both must be updated consistently.
5. Decide whether the Unity plugin version must also be bumped.
6. Check whether changelog must be updated.
7. Note that `deploy/` `.unitypackage` files must be regenerated on release.
8. Summarize: old version, new version, files changed, billing library impact, compatibility risk, validation steps.

## What to Check

### Android
- `android-unity-wrapper/build.gradle` — `af-android-sdk` dependency
- `android-unity-wrapper/gradle.properties` — `VERSION_NAME` and `VERSION_CODE` (incremented by `scripts/bump-version.sh` when the wrapper version changes)
- Billing library v7 and v8 build variants — both must be updated
- ProGuard rules if new classes are added
- `AppsFlyerAndroidWrapper.java` — version constants if any

### iOS
- iOS podspec or pod dependency declarations
- `AppsFlyerDependencies.xml` — `PurchaseConnector` iosPod version **must match** `AppsFlyerFramework` / `--ios-sdk-version`
- `README.md` and `docs/Introduction.md` — `iOS Purchase Connector` line must match iOS SDK version
- `AppsFlyeriOSWrapper.mm` — version constants if any

### Unity plugin
- `Assets/AppsFlyer/AppsFlyer.cs` — version constant
- `CHANGELOG.md`
- `deploy/` `.unitypackage` files (must regenerate after bump)

## Output Format

Return:
- Summary
- SDK bumped: Android | iOS | Both
- Previous version → New version
- Billing library variants impacted: v7 | v8 | both
- Files updated
- Plugin version impact
- Changelog impact
- `.unitypackage` regeneration required: Yes | No
- Validation steps
- Compatibility risk

## Rules

- **iOS Purchase Connector version = iOS AppsFlyer SDK version** (enforced by `scripts/bump-version.sh` and `rc-release.yml` verify step).
- Both billing library variants must be updated together.
- Do not update only one place if multiple version declarations exist.
- `.unitypackage` files are release artifacts — flag them as needing regeneration.
- Prefer a minimal and consistent change set.
