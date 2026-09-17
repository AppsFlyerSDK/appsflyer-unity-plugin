# AppsFlyer RPC Bridge — Unity Plugin Implementation Plan

## Background & Goal

The [`AppsFlyerSDK/appsflyer_flutter`](https://github.com/AppsFlyerSDK/appsflyer_flutter) repo proves an RPC-based pattern where a single JSON-over-channel contract replaces dozens of individual native function bindings. The goal here is to port that same pattern to Unity — replacing the current P/Invoke (iOS) and `AndroidJavaClass` (Android) bridges with a unified JSON-RPC dispatcher on both platforms.

---

## Architecture: Before vs After

**Current Unity bridge:**
```
C# (per-method calls)
  → iOS: [DllImport] → extern "C" functions → AppsFlyerLib directly
  → Android: AndroidJavaClass.CallStatic() → static Java methods → AppsFlyerLib
```

**After RPC:**
```
C# (single _executeRPC(jsonRequest) + event stream)
  → iOS: [DllImport "_afExecuteJson"] → Swift wrapper → AppsFlyerRPCBridge → AppsFlyerLib
  → Android: AndroidJavaClass.Call("executeJson") → Java RPC dispatcher → AppsFlyerLib
  ← Callbacks: UnitySendMessage (same as today, payload becomes JSON event envelope)
```

---

## RPC Protocol (shared across platforms)

**Request:**
```json
{ "id": "init-1751200000", "method": "init", "params": { "devKey": "...", "appId": "..." } }
```

**Success response:**
```json
{ "id": "init-1751200000", "result": { "data": null } }
```

**Error response:**
```json
{ "id": "init-1751200000", "error": { "code": 422, "message": "Invalid devKey", "details": {} } }
```

**Inbound event (SDK callback → C#):**
```json
{ "event": "onConversionDataSuccess", "data": { ... } }
```

---

## RPC Method Registry (v1 scope)

Mirrors the 7 methods already proven in the Flutter RPC plugin:

| RPC method | Current Unity C# method | Platform |
|---|---|---|
| `init` | `initSDK()` | iOS + Android |
| `start` | `startSDK()` | iOS + Android |
| `isDebug` | `setIsDebug()` | iOS + Android |
| `logEvent` | `sendEvent()` | iOS + Android |
| `waitForATT` | `waitForATTUserAuthorizationWithTimeoutInterval()` | iOS only |
| `registerConversionListener` | callback registration | iOS + Android |
| `registerDeeplinkListener` | `subscribeForDeepLink()` | iOS + Android |

Additional methods for full Unity API parity are deferred to Phase 2+.

---

## Phase 0 — Prerequisites & Research

### Step 0.1 — Confirm AppsFlyerRPC iOS module distribution

The Flutter plugin depends on `pod 'AppsFlyerRPC'` from a local path. Before Unity work begins, confirm:

- Is the module available as a pre-built `.xcframework` that can be dropped into `Assets/Plugins/iOS/`?
- Or will it be published to a CocoaPods spec repo (requires Unity post-build podspec)?
- Unity does not use CocoaPods by default — distribution method determines the iOS integration path.

**Decision required:** XCFramework drop-in vs Unity Package Manager podspec.

### Step 0.2 — Confirm AppsFlyerRPC Android module availability

The Flutter repo's `dev/android-side-impl` branch shows an Android stub that is not yet complete. Confirm:

- Is there a published `AppsFlyerRPC` Android AAR or Maven artifact?
- Or does the Android RPC dispatcher need to be written from scratch inside `android-unity-wrapper`?

**Decision required:** External AAR dependency vs self-contained dispatcher written in-wrapper.

### Step 0.3 — Decide async response model

`AppsFlyerRPCBridge.executeJson` on iOS is async (callback-based). The Unity P/Invoke boundary can handle this two ways:

- **Option A — Blocking semaphore:** Block the calling thread until the native callback fires, return synchronously. Simpler, acceptable since Unity main-thread calls are already frame-blocking.
- **Option B — Async via UnitySendMessage:** Return immediately with a correlation ID; native delivers the response via `UnitySendMessage("onRPCResponse", jsonResponse)`. Cleaner, matches the inbound event model.

**Decision required:** Blocking vs async response model.

---

## Phase 1 — C# RPC Layer

### Step 1.1 — Create `AppsFlyerRPCClient.cs`

New singleton C# class responsible for all RPC I/O:

```csharp
public class AppsFlyerRPCClient {
    public static readonly AppsFlyerRPCClient instance = new AppsFlyerRPCClient();

    // Build and dispatch a JSON-RPC request, return parsed result or throw
    public object Execute(string method, Dictionary<string, object> parameters);

    // Parse a raw JSON response string
    private object ParseResponse(string jsonResponse);
}

public class AppsFlyerRPCException : Exception {
    public int Code;
    public string Message;
    public object Details;
}
```

Key design points:
- All outbound calls are `string → string` (request JSON in, response JSON out)
- Auto-incrementing or timestamp-based request IDs
- All inbound events arrive on `AppsFlyerObjectScript` via a single unified `onRPCEvent` handler

### Step 1.2 — Platform dispatch inside `AppsFlyerRPCClient`

```csharp
#if UNITY_IOS && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern string _afExecuteJson(string jsonRequest);

    private string Dispatch(string jsonRequest) => _afExecuteJson(jsonRequest);

#elif UNITY_ANDROID && !UNITY_EDITOR
    private static AndroidJavaClass _rpcBridge =
        new AndroidJavaClass("com.appsflyer.unity.AppsFlyerRPCBridge");

    private string Dispatch(string jsonRequest) =>
        _rpcBridge.CallStatic<string>("executeJson", jsonRequest);
#endif
```

### Step 1.3 — Refactor `AppsFlyer.cs` to delegate to RPC client

The existing public API surface stays 100% unchanged (no breaking change for customers). Internally each method builds an RPC request and calls `AppsFlyerRPCClient.instance.Execute(...)` instead of calling `AppsFlyeriOS` / `AppsFlyerAndroid` directly.

Existing interfaces (`IAppsFlyerNativeBridge`, `IAppsFlyerIOSBridge`, `IAppsFlyerAndroidBridge`) are marked `[Obsolete]` but kept for one release cycle.

### Step 1.4 — Inbound event routing

Native callbacks still arrive via `UnitySendMessage`. Add a single handler to `AppsFlyerObjectScript`:

```csharp
public void onRPCEvent(string jsonEvent) {
    // Parse { "event": "onConversionDataSuccess", "data": {...} }
    // Route to existing C# events (OnRequestResponse, OnDeepLinkReceived, etc.)
}
```

Existing customer callback wiring does not change.

### Step 1.5 — Unit tests

In `Assets/AppsFlyer/Tests/Tests_Suite.cs`:
- JSON request builder produces correct `id/method/params` shape
- `ParseResponse` extracts `result.data` correctly
- `ParseResponse` throws `AppsFlyerRPCException` on error response
- `onRPCEvent` routes event types to correct C# events

---

## Phase 2 — iOS RPC Implementation

### Step 2.1 — Integrate AppsFlyerRPC framework

Depending on the decision from Step 0.1:

- **XCFramework:** Place `AppsFlyerRPC.xcframework` in `Assets/Plugins/iOS/`. Add `Editor/PostBuildProcessor.cs` to link it in the generated Xcode project.
- **Podspec:** Add `appsflyer-unity-plugin.podspec` declaring `s.dependency 'AppsFlyerRPC'` and `s.ios.deployment_target = '13.0'`.

### Step 2.2 — Add `AppsFlyerRPCWrapper.mm` (new file)

Single `extern "C"` entry point bridging C# → Swift → AppsFlyerRPCBridge:

```objc
// AppsFlyerRPCWrapper.mm
extern "C" {
    // Synchronous variant (Option A):
    const char* _afExecuteJson(const char* jsonRequest) {
        __block NSString* response = nil;
        dispatch_semaphore_t sem = dispatch_semaphore_create(0);
        [AppsFlyerRPCBridge.shared executeJson:[NSString stringWithUTF8String:jsonRequest]
                                    completion:^(NSString* jsonResponse) {
            response = jsonResponse;
            dispatch_semaphore_signal(sem);
        }];
        dispatch_semaphore_wait(sem, DISPATCH_TIME_FOREVER);
        return [response UTF8String];
    }
}
```

If async (Option B) is chosen, return the request `id` immediately and deliver the response via `UnitySendMessage`.

### Step 2.3 — Wire event streaming

Replace per-callback `UnitySendMessage` calls in `AppsFlyeriOSWrapper.mm` with a single handler on `AppsFlyerRPCBridge`:

```swift
AppsFlyerRPCBridge.shared.setEventHandler { jsonEvent in
    UnitySendMessage(objectName, "onRPCEvent", (jsonEvent as NSString).utf8String)
}
```

### Step 2.4 — Deprecate legacy extern "C" functions

Mark individual `extern "C"` functions in `AppsFlyeriOSWrapper.mm` with deprecation comments. Do not remove them in this phase — remove in the following minor version after the RPC path is validated.

### Step 2.5 — Build config

- Minimum iOS deployment target: `13.0` (required by AppsFlyerRPC module)
- Ensure Swift interop bridging header is generated correctly by Unity's Xcode post-processor

---

## Phase 3 — Android RPC Implementation

### Step 3.1 — Create `AppsFlyerRPCBridge.java`

New file in `android-unity-wrapper/unitywrapper/src/main/java/com/appsflyer/unity/`:

```java
public class AppsFlyerRPCBridge {

    public static String executeJson(String jsonRequest) {
        try {
            JSONObject req = new JSONObject(jsonRequest);
            String id     = req.getString("id");
            String method = req.getString("method");
            JSONObject params = req.optJSONObject("params");
            return route(id, method, params);
        } catch (JSONException e) {
            return errorResponse("unknown", -32700, "Parse error: " + e.getMessage());
        }
    }

    private static String route(String id, String method, JSONObject params) {
        switch (method) {
            case "init":    return handleInit(id, params);
            case "start":   return handleStart(id, params);
            case "logEvent": return handleLogEvent(id, params);
            case "isDebug": return handleIsDebug(id, params);
            // ... etc.
            default:        return errorResponse(id, 404, "Unknown method: " + method);
        }
    }
}
```

### Step 3.2 — Implement method handlers

Each handler calls `AppsFlyerLib.getInstance()` and returns a JSON response string:

```java
private static String handleInit(String id, JSONObject params) {
    String devKey = params.optString("devKey");
    AppsFlyerLib.getInstance().init(devKey, conversionListener, getContext());
    return successResponse(id, null);
}

private static String handleLogEvent(String id, JSONObject params) {
    String eventName = params.optString("eventName");
    Map<String, Object> values = toMap(params.optJSONObject("eventValues"));
    AppsFlyerLib.getInstance().logEvent(getContext(), eventName, values);
    return successResponse(id, null);
}
```

### Step 3.3 — Inbound event routing (Android → C#)

Register a single `AppsFlyerConversionListener` and `DeepLinkListener` in the bridge. All callbacks produce a JSON event envelope and call:

```java
UnityPlayer.UnitySendMessage(objectName, "onRPCEvent", jsonEvent);
```

This replaces per-callback `UnitySendMessage` calls currently in `AppsFlyerAndroidWrapper.java`.

### Step 3.4 — Rebuild the AAR

```bash
cd android-unity-wrapper
./gradlew :unitywrapper:assembleRelease
# Output: unitywrapper/build/outputs/aar/unitywrapper-release.aar
# Copy to: Assets/Plugins/Android/appsflyer-unity-wrapper-X.X.X.aar
```

### Step 3.5 — Deprecate legacy static methods in `AppsFlyerAndroidWrapper.java`

Mark individual static methods with `@Deprecated`. Keep for one release cycle.

---

## Phase 4 — Integration Testing

### Step 4.1 — iOS smoke test

- Build iOS Simulator app with new RPC bridge
- Validate: init → start → logEvent → conversion callback → deep link callback
- Compare event JSON payloads to pre-RPC baseline

### Step 4.2 — Android smoke test

- Build Android APK with new AAR
- Run on emulator
- Validate same event set as iOS

### Step 4.3 — Regression check

- Confirm `AppsFlyer.cs` public API is 100% backwards compatible
- Run existing `Tests_Suite.cs` suite with no changes required from the customer side

---

## Phase 5 — Cleanup & Release Prep

### Step 5.1 — Mark legacy bridge files obsolete

```csharp
[Obsolete("Direct platform bridge — use AppsFlyerRPCClient internally")]
public class AppsFlyerAndroid : IAppsFlyerAndroidBridge { ... }

[Obsolete("Direct platform bridge — use AppsFlyerRPCClient internally")]
public class AppsFlyeriOS : IAppsFlyerIOSBridge { ... }
```

### Step 5.2 — Version bump

Update all 4 version-bearing files:
1. `Assets/AppsFlyer/AppsFlyer.cs` → `kAppsFlyerPluginVersion`
2. `Assets/AppsFlyer/Plugins/iOS/AppsFlyeriOSWrapper.mm` → `pluginVersion:`
3. `android-unity-wrapper/.../AppsFlyerAndroidWrapper.java` → `PLUGIN_VERSION`
4. `Assets/AppsFlyer/package.json` → `"version"`

### Step 5.3 — RC pipeline

Run the full RC pipeline via the `plugin-release` skill.

---

## Open Decisions

| # | Question | Options |
|---|---|---|
| 1 | How is AppsFlyerRPC iOS module distributed to Unity? | XCFramework drop-in vs CocoaPods podspec |
| 2 | Does an Android AppsFlyerRPC module exist as AAR/Maven? | If yes, add as dependency; if no, write dispatcher in-wrapper |
| 3 | Should the RPC async response use a blocking semaphore or async UnitySendMessage? | Semaphore (simpler) vs async (cleaner) |
| 4 | Should legacy P/Invoke functions be kept in parallel or removed immediately? | Keep for one release (safe) vs remove now (clean) |
| 5 | Which Unity version is the minimum target? | Affects Swift interop bridging header requirements |

---

## File Change Summary

| File | Change |
|---|---|
| `Assets/AppsFlyer/AppsFlyerRPCClient.cs` | **New** — unified JSON-RPC dispatcher |
| `Assets/AppsFlyer/AppsFlyer.cs` | Internals delegate to RPC client; public API unchanged |
| `Assets/AppsFlyer/AppsFlyerObjectScript.cs` | Add `onRPCEvent()` unified event handler |
| `Assets/AppsFlyer/AppsFlyerAndroid.cs` | Mark `[Obsolete]` |
| `Assets/AppsFlyer/AppsFlyeriOS.cs` | Mark `[Obsolete]` |
| `Assets/AppsFlyer/Plugins/iOS/AppsFlyerRPCWrapper.mm` | **New** — single `_afExecuteJson` extern "C" entry |
| `Assets/AppsFlyer/Plugins/iOS/AppsFlyeriOSWrapper.mm` | Deprecate individual extern "C" functions |
| `Assets/AppsFlyer/Plugins/iOS/AppsFlyerRPC.xcframework` | **New** — iOS RPC framework (if XCFramework approach) |
| `android-unity-wrapper/.../AppsFlyerRPCBridge.java` | **New** — Android JSON-RPC dispatcher |
| `android-unity-wrapper/.../AppsFlyerAndroidWrapper.java` | Mark `@Deprecated` on individual methods |
| `Assets/AppsFlyer/Plugins/Android/*.aar` | Rebuilt with new RPC bridge class |
| `Assets/AppsFlyer/Tests/Tests_Suite.cs` | Add RPC unit tests |

---

## Reference

- Flutter RPC plugin: [AppsFlyerSDK/appsflyer_flutter](https://github.com/AppsFlyerSDK/appsflyer_flutter)
- Flutter architecture doc: `docs/ARCHITECTURE.md` in the Flutter repo
- Flutter iOS bridge: `ios/Classes/AppsflyerFlutterPlugin.swift`
- Flutter Android stub: `android/src/main/kotlin/com/appsflyer/AppsflyerFlutterPlugin.kt`
