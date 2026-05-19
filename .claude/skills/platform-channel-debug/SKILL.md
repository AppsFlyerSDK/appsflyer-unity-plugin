---
name: platform-channel-debug
description: Debug communication issues between C# code and native Android/iOS implementations in the AppsFlyer Unity plugin.
---

# Platform Channel Debug

Use this skill when a C#-to-native or native-to-C# flow is not working correctly.

## Goal

Find where the Unity native bridge is broken between C#, the platform-specific wrappers, and native SDK integration.

## Workflow

1. Identify the failing flow: C# → native method call | native → C# callback | initialization/registration issue.
2. Trace the path end-to-end:
   - C# `AppsFlyer.cs` → `AppsFlyerAndroid.cs` (AndroidJavaClass) | `AppsFlyeriOS.cs` ([DllImport])
   - → `AppsFlyerAndroidWrapper.java` | `AppsFlyeriOSWrapper.mm`
   - → Native AppsFlyer SDK callback
3. Verify argument and payload mapping.
4. Check for mismatches: method names, argument types, platform guards (`#if UNITY_IOS` / `#if UNITY_ANDROID`), null handling, callback thread assumptions.
5. Identify the first broken point in the chain.
6. Recommend the smallest safe fix.

## What to Check

- `AppsFlyer.cs` — public API call and platform delegation
- `AppsFlyerAndroid.cs` — AndroidJavaClass/AndroidJavaObject call
- `AppsFlyeriOS.cs` — DllImport extern function signature
- `AppsFlyerAndroidWrapper.java` — Java bridge method signature
- `AppsFlyeriOSWrapper.mm` — Objective-C++ bridge function signature
- Callback propagation back to C# (Unity main thread safety)
- Assembly definition boundaries

## Output Format

Return:
- Failing scenario
- Expected flow
- Actual break point
- Evidence
- Suspected root cause
- Recommended fix
- Validation steps

## Rules

- Do not say the issue is in native SDK behavior unless the bridge has been verified first.
- Unity requires callbacks to be dispatched to the main thread — verify threading.
- Prefer tracing the real execution path over guessing.
- Separate confirmed breakage from hypotheses.
- If evidence is incomplete, say `needs verification`.
