# Plan: Unity Scene Delegate (UIScene) Support — Research + Implementation

**Goal:** Determine exactly what breaks in the AppsFlyer Unity plugin's iOS native bridge once Unity ships mandatory `UIWindowSceneDelegate` support, and implement the fix before Apple's hard-requirement deadline lands.

**Context:** Unity is adding native `UIScene` support across all actively serviced editor versions (6000.5.0a3+, 6000.4.0b8+, 6000.3.8f1+, 6000.0.68f1+, 2022.3.72f1+) because Apple is making `UIScene` mandatory in the major iOS release after iOS 26 ("not opt-in" — Unity's own words). Source: [Unity forum announcement](https://discussions.unity.com/t/info-apple-update-your-editor-to-receive-uiscene-lifecycle-support/1709065). Unity's changes touch `UnityAppController.h/mm`, `DisplayManager.mm`, `Info.plist`, and add two new files to the Xcode `UnityFramework` target: `UnityScene.h`/`UnityScene.mm`. The announcement states older AppDelegate-related code is **removed**, not left in place alongside the new scene path.

This supersedes the open-ended "plugin-by-plugin compatibility matrix, deferred to a later pass" note in DELIVERY-115275 with a dated forcing function. It was raised via the `appsflyer.sdk.ios` repo's own iOS 27 scene-lifecycle impact assessment — see `specs/003-scene-lifecycle-impact-assessment/research.md` R8 and `impact-assessment-report.md` §8 in that repo for the origin analysis and why this is filed as a separate Unity-plugin ticket rather than folded into that assessment's `AppsFlyerLib`-integrator docs fix (TICKET-01).

**Process note:** This changes the deeplink-forwarding code path inside a shipped native bridge — treat it as a feature change requiring `/af-ship`-equivalent review in this repo's own workflow (PRD/tech-design/tests), not a docs-only or maintenance-bypass change. Do not skip straight to editing `.mm` files.

---

## Phase 0: Discovery (seeded — verify, don't re-derive from scratch)

The two files that own iOS deeplink forwarding today:

| File | Mechanism | What it does |
|---|---|---|
| `Assets/AppsFlyer/Plugins/iOS/AppsFlyer+AppController.m` | Method swizzling on `UnityAppController` via `+load`, guarded by `#if __has_include("UnityAppController.h")` | Swizzles `application:continueUserActivity:restorationHandler:` (Universal Link warm/cold) and `application:didFinishLaunchingWithOptions:` (custom-URL-scheme **cold launch** only — reads `UIApplicationLaunchOptionsURLKey`) |
| `Assets/AppsFlyer/Plugins/iOS/AppsFlyerAppController.mm` | `NSNotificationCenter` observer on `kUnityOnOpenURL` / `kUnityDidReceiveRemoteNotification`, guarded by `#if __has_include("AppDelegateListener.h")` | Handles warm-launch custom-URL-scheme opens and push notifications. Its own top comment already asserts Unity posts these notifications "for both classic AppDelegate methods and Scene-lifecycle equivalents (`scene:openURLContexts:`, `scene:continueUserActivity:`) — see `UnityAppController.mm`/`UnityScene.mm`" — **this claim was written before `UnityScene.mm` shipped in any editor and has not been verified against a real one.** |

Both bridge into `AppsFlyerAttribution.h`'s `+shared` singleton (`continueUserActivity:restorationHandler:`, `handleOpenUrl:options:`, `handleOpenUrl:sourceApplication:annotation:`) — the C-level API surface does not need to change; only what calls it does.

**The specific risk**, precisely stated: `AppsFlyer+AppController.m`'s entire swizzle compiles out silently — no warning, no error — if `UnityAppController.h` stops existing in a scene-only export mode. Separately, even if the header still exists, the swizzled methods (`continueUserActivity:`, `didFinishLaunchingWithOptions:`) may simply never be called by the runtime once `UnityScene.mm` owns the scene connection path instead — a silent-at-runtime failure that's worse than a compile-time one, because there's no build signal at all. Either failure mode reproduces the same class of bug as `appsflyer.sdk.ios`'s R2 finding: **cold-launch Universal Link data dropped**, but this time inside AppsFlyer's own plugin code, not an integrator's undocumented app.

## Phase 1: Investigate (execute in a fresh session; read every cited file before concluding anything)

### 1.1 — Get a real `UnityScene.mm` to read against

1. Install the earliest editor version from the forum announcement's list that's actually released (`6000.3.8f1` or `2022.3.72f1`, whichever is available first).
2. Export a minimal iOS build from that editor (an empty scene is enough) and open the generated Xcode project.
3. Locate the generated `UnityScene.h`/`UnityScene.mm` inside the `UnityFramework` target and read them in full. Confirm:
   - Does `UnityAppController.h` still exist in the export, and does `UnityAppController` still receive `application:continueUserActivity:restorationHandler:` / `application:didFinishLaunchingWithOptions:` calls, or does `UnityScene.mm` intercept the scene-based equivalents (`scene:continueUserActivity:`, `scene:willConnectToSession:options:`) exclusively?
   - Does `UnityScene.mm` post `kUnityOnOpenURL` / `kUnityDidReceiveRemoteNotification` (or equivalents) the same way the classic `UnityAppController.mm` does — verify the claim already asserted (unverified) in `AppsFlyerAppController.mm`'s header comment?
   - Is a cold-launch Universal Link's `NSUserActivity` delivered via `scene:willConnectToSession:options:`'s `connectionOptions.userActivities`, matching `appsflyer.sdk.ios` research.md R2's finding for non-Unity scene apps?
4. Record file:line citations for every claim above — do not describe behavior you didn't actually read in the generated source.

### 1.2 — Confirm or refute the compile-out risk

1. With the same export, check whether `AppsFlyer+AppController.m`'s `#if __has_include("UnityAppController.h")` guard evaluates true or false.
2. If true (header still exists): add temporary logging inside the swizzled `__swizzled_continueUserActivity`/`__swizzled_didFinishLaunchingWithOptions` functions, trigger a cold-launch Universal Link and a cold-launch custom-URL-scheme open against the real device/simulator, and confirm whether the logs fire.
3. If false (header gone): this confirms the silent compile-out risk as real, not hypothetical.
4. Repeat the same live-launch test for `AppsFlyerAppController.mm`'s notification observer path (`kUnityOnOpenURL`, warm-launch custom URL scheme; push notification receipt).

**Verification for Phase 1:** a clear, evidenced verdict — not a guess — on which of the two bridge files (or both) actually breaks under a real Unity scene-only export, and exactly which deeplink/launch scenario is affected (cold Universal Link / cold custom-scheme / warm custom-scheme / push).

## Phase 2: Implement (only after Phase 1 has a concrete, evidenced verdict)

1. For whatever gap Phase 1 confirms, add the equivalent forwarding call into `UnityScene.mm`'s generated scene-connection callback — mirroring the fix pattern `appsflyer.sdk.ios`'s TICKET-01 already validated for non-Unity integrators (call `AppsFlyerAttribution`'s `continueUserActivity:restorationHandler:` / `handleOpenUrl:options:` from inside the scene-lifecycle callback, before the equivalent of `UIApplicationDidBecomeActiveNotification` fires).
2. Because `UnityScene.mm` is Unity-generated (not checked into this plugin's repo the way `UnityAppController.mm` historically was addressable via category/swizzle), determine the correct injection mechanism — likely a new `AppsFlyerUnityScene+AppsFlyer` category/extension file added to the plugin's `Assets/AppsFlyer/Plugins/iOS/` directory that Unity's build post-processor (`Assets/AppsFlyer/Editor/`) wires into the exported Xcode project, analogous to how `AppsFlyer+AppController.m` is added today. Do not assume `UnityScene.mm` itself is editable/patchable at export time without a post-process step — verify how the existing Editor post-processor already injects `AppsFlyer+AppController.m`/`AppsFlyerAppController.mm` and replicate that mechanism.
3. Keep both the legacy (`UnityAppController`-swizzle) and new (`UnityScene`-hook) code paths guarded by their own `__has_include` checks, matching the existing pattern, so the plugin keeps working for host apps still on older, non-scene Unity exports.
4. Add/extend playmode or native integration tests analogous to `test-app/Assets/iOS/UnityAppControllerDeepLink.mm` covering the scene-based cold-launch Universal Link and custom-URL-scheme cases specifically.
5. Update `CHANGELOG.md` and the plugin version per this repo's version-management convention (see `CLAUDE.md` — Key version files) if this ships as a release.

## Output Contract

Report, in order:
- **Phase 1 verdict**: exact scenario(s) broken, with file:line citations from the real generated `UnityScene.mm`/`UnityAppController.mm` you read — not from this plan's predictions.
- **Fix applied**: files changed, injection mechanism used, and why.
- **Test coverage**: what was added, what passed.
- **Compatibility risk**: effect on host apps still building with pre-scene Unity editor versions.
- **Cross-reference**: confirmation that this closes the gap named in `appsflyer.sdk.ios`'s `specs/003-scene-lifecycle-impact-assessment/impact-assessment-report.md` §8 and supersedes DELIVERY-115275's open item.
