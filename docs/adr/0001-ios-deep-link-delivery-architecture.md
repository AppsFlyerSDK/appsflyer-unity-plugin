# ADR 0001: iOS native deep-link delivery — NotificationCenter observers + a narrow UnityAppController swizzle, with no delivery path under Unity's Swift Xcode project type

## Status

Accepted (known gap, not yet closed).

## Context

Before the RPC migration, iOS deep-link delivery worked by subclassing/category-swizzling
`UnityAppController` directly: `application:openURL:...`, `application:didFinishLaunchingWithOptions:`,
`application:continueUserActivity:restorationHandler:`, and remote-notification handling were all
intercepted this way and forwarded into `AppsFlyerAttribution` (which itself forwards straight into the
native `AppsFlyerLib` SDK, independent of Unity/C# — see `AppsFlyerAttribution.m`'s `isBridgeReady`
queuing).

`UnityAppController` subclassing/swizzling only exists when Unity generates a "Classic" (Objective-C
`AppDelegate`-based) Xcode project. Unity also supports a Swift-based Xcode project type
(gated in our code by `#if __has_include("UnityAppController.h")`), where that header — and the whole
`UnityAppController` class — doesn't exist. Any code that swizzles it simply doesn't compile in under
that export type; there is no equivalent class to hook.

Unity does post `NSNotificationCenter` notifications for some of these events under both project
types (`kUnityOnOpenURL`, `kUnityDidReceiveRemoteNotification`, declared in `AppDelegateListener.h`) —
covering classic `application:openURL:...`, Scene-lifecycle `scene:openURLContexts:`, and remote
notifications. So we migrated `AppsFlyerAppController.mm` to observe those notifications directly
instead of swizzling, which works under both project types.

Two cases have no notification equivalent:

1. **Cold-start URL-scheme launch** (`UIApplicationLaunchOptionsURLKey` in
   `application:didFinishLaunchingWithOptions:`) — no notification is posted for this either. Fixed
   in this change by adding a `didFinishLaunchingWithOptions:` swizzle alongside the existing
   `continueUserActivity` one in `AppsFlyer+AppController.m`, under the same
   `#if __has_include("UnityAppController.h")` guard.
2. **Universal Links** (`application:continueUserActivity:restorationHandler:` /
   `scene:continueUserActivity:`) — classic `UnityAppController` posts no notification for this case
   either, which is why the narrow swizzle in `AppsFlyer+AppController.m` still exists for it. But
   because that swizzle needs `UnityAppController.h`, it compiles out entirely under Unity's Swift
   Xcode project type, and Unity's Swift template has no `NSNotificationCenter`-based equivalent for
   this call to observe instead.

## Decision

Accept a mixed architecture: NSNotificationCenter observers (`AppsFlyerAppController.mm`) for the
notification-backed cases (works under both project types), plus a narrow `UnityAppController`
category swizzle (`AppsFlyer+AppController.m`, guarded on `__has_include("UnityAppController.h")`)
for the two cases with no notification equivalent — cold-start URL-scheme launch and Universal Links.

## Consequences

- Cold-start URL-scheme deep links and Universal Links only deliver automatically under Unity's
  Classic (Objective-C `AppDelegate`) Xcode project type.
- **Under Unity's Swift Xcode project type, Universal Links have no automatic delivery path at all.**
  There is no compile error or runtime warning — the swizzle file's contents simply compile out via
  `#if __has_include`, silently. Integrators on that export type must call
  `AppsFlyer.continueUserActivity(url, activityType)` (`Assets/AppsFlyer/AppsFlyer.cs`) manually from
  their own Swift `AppDelegate`/scene-delegate `continueUserActivity` handler.
- `test-app`'s E2E suite uses `xcodeProjectType: 0` (Classic), so this gap is not exercised by CI.
  Regressions here would not be caught automatically.
- If Unity ever ships a Swift-side `NSNotificationCenter` notification for `continueUserActivity` (or
  an equivalent lifecycle hook), the fix is to add an observer for it in `AppsFlyerAppController.mm`,
  matching the existing `kUnityOnOpenURL`/`kUnityDidReceiveRemoteNotification` pattern, and this ADR
  can be closed.
