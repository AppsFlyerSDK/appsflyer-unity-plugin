# CLAUDE.md — AppsFlyer Unity Plugin

## Claude Agents

This repository includes custom agents under `.claude/agents/`.

Current agents:
- `unity-plugin-developer` — for Unity plugin development, C#/native bridge work, SDK version bumps, billing library variants, and release-related plugin changes
- `unity-e2e-tester` — for end-to-end validation on Android emulators and iOS simulators, including AppsFlyer event verification from logs

## Claude Skills

This repository includes reusable skills under `.claude/skills/`.

Plugin development skills:
- `sdk-version-bump` — bump wrapped Android/iOS SDK versions, plugin version metadata, and billing library variants
- `plugin-api-change` — add or modify Unity plugin APIs safely across C#, Android Java, and iOS Objective-C++
- `platform-channel-debug` — debug C# ↔ native bridge issues (P/Invoke, AndroidJavaClass)
- `plugin-release` — review plugin release readiness including `.unitypackage` artifacts

Quality / E2E skills:
- `appsflyer-event-validation` — verify expected AppsFlyer events and callbacks from logs and evidence
- `launch-log-analysis` — analyze app launch logs per session and compare expected vs actual AppsFlyer events
- `e2e-smoke-test` — run or review a basic emulator/simulator smoke test for plugin readiness

## Working Style

When a task matches one of the skills above, prefer using the relevant skill workflow.
When a task needs specialized reasoning, prefer the matching custom agent.
For plugin work, inspect C#, Android, and iOS layers together before changing code.
For E2E validation, rely on logs, callback payloads, and explicit evidence rather than assumptions.

---

## Architecture

C# API (`AppsFlyerSDK` namespace) → P/Invoke / AndroidJavaClass → Native Android (Java) / iOS (Objective-C++)

### Key files
- `Assets/AppsFlyer/AppsFlyer.cs` — main public C# API (source of truth)
- `Assets/AppsFlyer/AppsFlyerAndroid.cs` — Android platform delegation (`AndroidJavaClass`/`AndroidJavaObject`)
- `Assets/AppsFlyer/AppsFlyeriOS.cs` — iOS platform delegation (`[DllImport("__Internal")]`)
- `Assets/AppsFlyer/AppsFlyerPurchaseConnector.cs` — in-app purchase validation
- `Assets/AppsFlyer/Plugins/iOS/AppsFlyeriOSWrapper.mm` — Objective-C++ bridge
- `android-unity-wrapper/AppsFlyerAndroidWrapper.java` — Java bridge
- `Assets/AppsFlyer/Editor/` — Unity editor extensions
- `Assets/AppsFlyer/Tests/` — Unity playmode test suite
- `deploy/` — pre-built `.unitypackage` release artifacts (do not edit manually)

### Platform guards
- Use `#if UNITY_IOS` / `#if UNITY_ANDROID` in shared C# files for platform-specific code
- Namespace: `AppsFlyerSDK` — do not change
- Assembly definitions (`.asmdef`) enforce modularity — do not break boundaries

### Key version files
When bumping SDK or plugin versions, these files must stay in sync:
- Version constant in `Assets/AppsFlyer/AppsFlyer.cs`
- `android-unity-wrapper/build.gradle` — `af-android-sdk` + billing library v7 and v8
- iOS podspec or dependency declarations
- `CHANGELOG.md`
- `deploy/` `.unitypackage` files — must be regenerated on release

### SDK targets
- Android: af-android-sdk v6.17.6 (two billing library variants: v7 and v8)
- iOS: AppsFlyerFramework v6.17.9
- Unity: 2019.4+, Android API 16–34, Java/Kotlin 17

### Billing library variants
Two Gradle build variants exist for Google Play Billing migration (v7 → v8). Both must remain buildable and consistent after any Android wrapper change.

### CI
RC and production release gates run through `.github/workflows/rc-release.yml`, `rc-smoke.yml`, and `release_production_workflow.yml`. Pre-publish gates include Unity playmode unit tests (`.github/workflows/unity-playmode-tests.yml`, iOS/Android/Shared) in parallel with E2E, plus the tooling scenario runner (`.af-e2e/test-plan.json`). Post-publish smoke uses `.af-smoke/rc-test-plan.json`.
