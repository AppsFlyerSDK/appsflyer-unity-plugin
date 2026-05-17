---
name: plugin-api-change
description: Safely implement or modify a Unity plugin API in the AppsFlyer Unity plugin, including C# API, Android Java bridge, and iOS Objective-C bridge changes.
---

# Plugin API Change

Use this skill when adding, removing, or changing a Unity plugin API.

## Goal

Make a safe API change across all plugin layers while preserving backward compatibility where possible.

## Workflow

1. Identify the public C# API being changed in `Assets/AppsFlyer/AppsFlyer.cs`.
2. Trace the full path: C# API → `AppsFlyerAndroid.cs` (Android) | `AppsFlyeriOS.cs` (iOS) → Native bridge
3. Verify whether the API already exists in some form.
4. Check whether the change affects: method signatures, argument mapping, callback behavior, event payload shape, Android/iOS parity.
5. Implement changes in all required layers.
6. Check whether tests or example code must be updated.
7. Summarize compatibility risk and missing validation.

## What to Check

### C# layer
- `AppsFlyer.cs` — public API entry point
- `AppsFlyerAndroid.cs` — Android delegation (uses `AndroidJavaClass`/`AndroidJavaObject`)
- `AppsFlyeriOS.cs` — iOS delegation (uses `[DllImport("__Internal")]`)
- `#if UNITY_IOS` / `#if UNITY_ANDROID` guards
- Assembly definition (`.asmdef`) boundaries — do not break
- Namespace `AppsFlyerSDK` — do not change

### Android bridge
- `AppsFlyerAndroidWrapper.java` — Java bridge method
- Mapping to AppsFlyer Android SDK
- Threading safety

### iOS bridge
- `AppsFlyeriOSWrapper.mm` — Objective-C++ bridge function
- Mapping to AppsFlyer iOS SDK
- Delegate/callback wiring

## Output Format

Return:
- Summary
- C# API impact
- Android impact
- iOS impact
- Assembly definition impact
- Compatibility risk
- Tests to add or update
- Validation steps

## Rules

- Keep Android and iOS behavior aligned unless a platform difference is intentional.
- Do not silently break the public C# API.
- Both billing library variants must remain buildable after the change.
- Do not stop at C#-only changes if native bridge work is required.
