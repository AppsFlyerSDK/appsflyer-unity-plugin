# Plan 01 — RPC Phase 1: C# Layer

**Branch:** `poc/rpc_unity`
**Spec:** `docs/RPC-Implementation-Plan.md`
**Goal:** Introduce a unified JSON-RPC dispatcher in C# that routes the 7 core SDK methods through a single `Execute()` entry point, while keeping the existing public `AppsFlyer.cs` API 100% unchanged.

---

## Phase 0: Documentation Discovery (COMPLETED)

### Allowed APIs — verified from source

**JSON serialization** (`Assets/AppsFlyer/AFMiniJSON.cs`, namespace `AFMiniJSON`):
```csharp
// Serialize Dictionary → JSON string
string json = AFMiniJSON.Json.Serialize(object obj);

// Deserialize JSON string → object (cast to Dictionary<string, object> or List<object>)
var dict = AFMiniJSON.Json.Deserialize(string json) as Dictionary<string, object>;
// Note: JSON numbers parse as long (not int) and double
```

**Existing callback handlers in `AppsFlyer.cs`** (they receive `UnitySendMessage` payloads):
- `inAppResponseReceived(string response)` — line 1031
- `requestResponseReceived(string response)` — line 1042
- `onDeepLinking(string response)` — line 1053

**Existing static events in `AppsFlyer.cs`**:
- `OnRequestResponse` (line 985) — `EventHandler`
- `OnInAppResponse` (line 1000) — `EventHandler`
- `OnDeepLinkReceived` (line 1015) — `EventHandler`

**Existing bridge dispatch pattern in `AppsFlyer.cs`**:
```csharp
// instance is IAppsFlyerNativeBridge, set at initSDK time
instance.startSDK(onRequestResponse != null, CallBackObjectName);  // line 99
instance.sendEvent(eventName, eventValues, ...);                   // line 122
```

**Platform dispatch guards used throughout the codebase**:
```csharp
#if UNITY_IOS && !UNITY_EDITOR
    // iOS-only
#elif UNITY_ANDROID && !UNITY_EDITOR
    // Android-only
#endif
```

**Test framework** (`Assets/AppsFlyer/Tests/Tests_Suite.cs`):
- NUnit (`NUnit.Framework`)
- NSubstitute (`NSubstitute`)
- Pattern: `Substitute.For<IAppsFlyerNativeBridge>()` → inject into `AppsFlyer.instance`
- Assert: `mock.Received().MethodName(expectedArgs)`

### Anti-patterns to avoid
- Do NOT use `int` when casting from `AFMiniJSON.Json.Deserialize()` — numbers come back as `long`
- Do NOT invent `Json.Parse()` or `Json.ToObject()` — only `Serialize()` and `Deserialize()` exist
- Do NOT change any public method signature in `AppsFlyer.cs` — zero breaking changes allowed
- Do NOT remove `IAppsFlyerNativeBridge` injection pattern — existing tests mock through it

---

## Phase 1: Create `AppsFlyerRPCClient.cs`

### What to implement

New file: `Assets/AppsFlyer/AppsFlyerRPCClient.cs`

This is a pure C# singleton with no Unity runtime dependencies (no MonoBehaviour), so it is fully unit-testable.

```csharp
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using AFMiniJSON;

namespace AppsFlyerSDK
{
    public class AppsFlyerRPCException : Exception
    {
        public int Code { get; }
        public object Details { get; }

        public AppsFlyerRPCException(int code, string message, object details = null)
            : base(message)
        {
            Code = code;
            Details = details;
        }
    }

    public class AppsFlyerRPCClient
    {
        public static readonly AppsFlyerRPCClient instance = new AppsFlyerRPCClient();
        private AppsFlyerRPCClient() { }

        private long _requestCounter = 0;

        // Build a JSON-RPC request string
        public string BuildRequest(string method, Dictionary<string, object> parameters)
        {
            _requestCounter++;
            var request = new Dictionary<string, object>
            {
                { "id", method + "-" + _requestCounter },
                { "method", method },
                { "params", parameters ?? new Dictionary<string, object>() }
            };
            return Json.Serialize(request);
        }

        // Dispatch to native and return raw JSON response string
        public string Dispatch(string jsonRequest)
        {
#if UNITY_IOS && !UNITY_EDITOR
            return _afExecuteJson(jsonRequest);
#elif UNITY_ANDROID && !UNITY_EDITOR
            return _rpcBridge.CallStatic<string>("executeJson", jsonRequest);
#else
            // Editor / unsupported platform: return a no-op success response
            return "{\"id\":\"editor\",\"result\":{\"data\":null}}";
#endif
        }

        // Parse a JSON response. Returns result.data or throws AppsFlyerRPCException.
        public object ParseResponse(string jsonResponse)
        {
            if (string.IsNullOrEmpty(jsonResponse))
                throw new AppsFlyerRPCException(-1, "Empty response from native");

            var root = Json.Deserialize(jsonResponse) as Dictionary<string, object>;
            if (root == null)
                throw new AppsFlyerRPCException(-1, "Malformed response: " + jsonResponse);

            if (root.ContainsKey("error"))
            {
                var error = root["error"] as Dictionary<string, object>;
                int code = error != null && error.ContainsKey("code")
                    ? (int)(long)error["code"] : -1;
                string msg = error != null && error.ContainsKey("message")
                    ? (string)error["message"] : "Unknown RPC error";
                object details = error != null && error.ContainsKey("details")
                    ? error["details"] : null;
                throw new AppsFlyerRPCException(code, msg, details);
            }

            if (root.ContainsKey("result"))
            {
                var result = root["result"] as Dictionary<string, object>;
                return result != null && result.ContainsKey("data") ? result["data"] : null;
            }

            return null;
        }

        // Convenience: build + dispatch + parse in one call
        public object Execute(string method, Dictionary<string, object> parameters = null)
        {
            string request = BuildRequest(method, parameters);
            string response = Dispatch(request);
            return ParseResponse(response);
        }

#if UNITY_IOS && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern string _afExecuteJson(string jsonRequest);
#elif UNITY_ANDROID && !UNITY_EDITOR
        private static AndroidJavaClass _rpcBridge =
            new AndroidJavaClass("com.appsflyer.unity.AppsFlyerRPCBridge");
#endif
    }
}
```

### Documentation references
- JSON API: `Assets/AppsFlyer/AFMiniJSON.cs` — `Json.Serialize()` / `Json.Deserialize()`
- Platform dispatch pattern: `Assets/AppsFlyer/AppsFlyerAndroid.cs:13` (AndroidJavaClass), `Assets/AppsFlyer/AppsFlyeriOS.cs:662–1014` (DllImport)
- RPC protocol: `docs/RPC-Implementation-Plan.md` — "RPC Protocol" section

### Verification checklist
- [ ] `BuildRequest("init", new Dictionary<string,object>{{"devKey","abc"}})` produces `{"id":"init-1","method":"init","params":{"devKey":"abc"}}`
- [ ] `ParseResponse("{\"id\":\"x\",\"result\":{\"data\":null}}")` returns `null` without throwing
- [ ] `ParseResponse("{\"id\":\"x\",\"error\":{\"code\":422,\"message\":\"bad\"}}")` throws `AppsFlyerRPCException` with `Code == 422`
- [ ] `ParseResponse("")` throws `AppsFlyerRPCException` with `Code == -1`
- [ ] File compiles with zero warnings under `#if UNITY_EDITOR` path

---

## Phase 2: Add `onRPCEvent` Handler to `AppsFlyer.cs`

### What to implement

Add a new public method to the `AppsFlyer` class that receives unified JSON event envelopes from native via `UnitySendMessage`. This will be the single callback target for the RPC event stream.

Insert after `onDeepLinking` (line 1053) in `Assets/AppsFlyer/AppsFlyer.cs`:

```csharp
// Receives unified RPC event envelopes from native via UnitySendMessage.
// Format: {"event": "onConversionDataSuccess", "data": {...}}
public void onRPCEvent(string jsonEvent)
{
    try
    {
        var envelope = CallbackStringToDictionary(jsonEvent);
        if (envelope == null || !envelope.ContainsKey("event")) return;

        string eventType = envelope["event"] as string;
        var data = envelope.ContainsKey("data") ? envelope["data"] : null;
        string dataStr = data != null ? Json.Serialize(data) : jsonEvent;

        switch (eventType)
        {
            case "start":
            case "onRequestResponse":
                requestResponseReceived(dataStr);
                break;
            case "logEvent":
            case "onInAppResponse":
                inAppResponseReceived(dataStr);
                break;
            case "onDeepLinking":
            case "onDeepLinkReceived":
                onDeepLinking(dataStr);
                break;
            default:
                AFLog("onRPCEvent", "Unhandled event type: " + eventType);
                break;
        }
    }
    catch (Exception e)
    {
        AFLog("onRPCEvent", "Exception: " + e.Message);
    }
}
```

Also add `using AFMiniJSON;` to the using block at the top of `AppsFlyer.cs` (line 1–5) if not already present.

### Documentation references
- Existing callback handlers to route to: `AppsFlyer.cs:1031` (`inAppResponseReceived`), `AppsFlyer.cs:1042` (`requestResponseReceived`), `AppsFlyer.cs:1053` (`onDeepLinking`)
- Existing events fired by those handlers: `AppsFlyer.cs:985` (`OnRequestResponse`), `AppsFlyer.cs:1000` (`OnInAppResponse`), `AppsFlyer.cs:1015` (`OnDeepLinkReceived`)
- JSON parsing helper already in file: `AppsFlyer.cs:1089` (`CallbackStringToDictionary`)

### Verification checklist
- [ ] `onRPCEvent("{\"event\":\"start\",\"data\":{\"statusCode\":200}}")` fires `OnRequestResponse`
- [ ] `onRPCEvent("{\"event\":\"logEvent\",\"data\":{\"statusCode\":200}}")` fires `OnInAppResponse`
- [ ] `onRPCEvent("{\"event\":\"onDeepLinking\",\"data\":{}}")` fires `OnDeepLinkReceived`
- [ ] `onRPCEvent("{\"event\":\"unknown\",\"data\":{}}")` logs via `AFLog` and does not throw
- [ ] `onRPCEvent("")` / `onRPCEvent(null)` does not throw (handled by try/catch)

---

## Phase 3: Route Core Methods in `AppsFlyer.cs` Through RPC

### What to implement

Route the 7 Phase 1 RPC methods through `AppsFlyerRPCClient.instance.Execute()`. All other methods continue delegating to `instance` (IAppsFlyerNativeBridge) unchanged.

**Target methods and their new implementations:**

#### `initSDK` (lines 30 and 48)

The overloads keep their existing signatures. Internally, after calling the existing platform init (to preserve non-RPC setup), also register the RPC callback object name:

```csharp
// In the existing initSDK(string devKey, string appID, MonoBehaviour gameObject) overload
// AFTER the existing platform-specific bridge init calls, add:
AppsFlyerRPCClient.instance.Execute("init", new Dictionary<string, object>
{
    { "devKey", devKey },
    { "appId", appID }
});
```

> **Note:** For Phase 1 (POC), the RPC `init` call is made in addition to the existing init, not instead of it. Full replacement happens in Phase 2/3 when native bridges are ready.

#### `startSDK` (line 91)

```csharp
// Add alongside existing instance.startSDK() call:
try {
    AppsFlyerRPCClient.instance.Execute("start");
} catch (AppsFlyerRPCException e) {
    AFLog("startSDK", "RPC error: " + e.Message);
}
```

#### `setIsDebug` (line 174)

```csharp
AppsFlyerRPCClient.instance.Execute("isDebug", new Dictionary<string, object>
{
    { "enabled", shouldEnable }
});
```

#### `sendEvent` (line 115)

```csharp
AppsFlyerRPCClient.instance.Execute("logEvent", new Dictionary<string, object>
{
    { "eventName", eventName },
    { "eventValues", eventValues }
});
```

#### `waitForATTUserAuthorizationWithTimeoutInterval` (line 841) — iOS only

```csharp
#if UNITY_IOS && !UNITY_EDITOR
AppsFlyerRPCClient.instance.Execute("waitForATT", new Dictionary<string, object>
{
    { "timeout", timeoutInterval }
});
#endif
```

#### `getConversionData` (line 606) — maps to `registerConversionListener`

```csharp
AppsFlyerRPCClient.instance.Execute("registerConversionListener", new Dictionary<string, object>
{
    { "register", true },
    { "callbackObjectName", objectName }
});
```

#### `subscribeForDeepLink` (line 930) — maps to `registerDeeplinkListener`

```csharp
AppsFlyerRPCClient.instance.Execute("registerDeeplinkListener", new Dictionary<string, object>
{
    { "register", true },
    { "callbackObjectName", CallBackObjectName }
});
```

### Anti-pattern guards
- Do NOT remove the existing `instance.methodName()` bridge calls in this phase — run them in parallel with RPC (POC mode)
- Do NOT change any public method signature
- Do NOT change `CallBackObjectName` static field (used by UnitySendMessage routing)

### Documentation references
- Existing bridge calls to keep alongside: `AppsFlyer.cs:99` (startSDK), `AppsFlyer.cs:122` (sendEvent), `AppsFlyer.cs:178` (setIsDebug)
- RPC method names: `docs/RPC-Implementation-Plan.md` — "RPC Method Registry" table

### Verification checklist
- [ ] `AppsFlyer.startSDK()` still calls `instance.startSDK()` (existing tests still pass)
- [ ] `AppsFlyer.startSDK()` also calls `AppsFlyerRPCClient.instance.Execute("start")`
- [ ] `AppsFlyer.sendEvent("test", null)` calls `AppsFlyerRPCClient.instance.Execute("logEvent", ...)`
- [ ] All 71 existing tests in `Tests_Suite.cs` pass without modification

---

## Phase 4: Add Unit Tests in `Tests_Suite.cs`

### What to implement

Add a new test class `AppsFlyerRPCClientTests` inside `Assets/AppsFlyer/Tests/Tests_Suite.cs`, after the existing `AppsFlyerSDKTests` class.

Copy the test class setup pattern from `AppsFlyerSDKTests:8–17`:
```csharp
// Existing pattern to follow:
[SetUp]
public void SetUp()
{
    mock = Substitute.For<IAppsFlyerNativeBridge>();
    AppsFlyer.instance = mock;
}
```

New test class:
```csharp
namespace AppsFlyerSDK.Tests
{
    [TestFixture]
    public class AppsFlyerRPCClientTests
    {
        private AppsFlyerRPCClient rpc;

        [SetUp]
        public void SetUp()
        {
            rpc = AppsFlyerRPCClient.instance;
        }

        // --- BuildRequest tests ---

        [Test]
        public void BuildRequest_ProducesCorrectMethod()
        {
            string json = rpc.BuildRequest("init", null);
            var dict = AFMiniJSON.Json.Deserialize(json) as Dictionary<string, object>;
            Assert.AreEqual("init", dict["method"]);
        }

        [Test]
        public void BuildRequest_IdContainsMethodName()
        {
            string json = rpc.BuildRequest("start", null);
            var dict = AFMiniJSON.Json.Deserialize(json) as Dictionary<string, object>;
            StringAssert.Contains("start", (string)dict["id"]);
        }

        [Test]
        public void BuildRequest_WithParams_IncludesParams()
        {
            var parameters = new Dictionary<string, object> { { "devKey", "abc123" } };
            string json = rpc.BuildRequest("init", parameters);
            var dict = AFMiniJSON.Json.Deserialize(json) as Dictionary<string, object>;
            var paramsDict = dict["params"] as Dictionary<string, object>;
            Assert.AreEqual("abc123", paramsDict["devKey"]);
        }

        [Test]
        public void BuildRequest_NullParams_ProducesEmptyParamsObject()
        {
            string json = rpc.BuildRequest("start", null);
            var dict = AFMiniJSON.Json.Deserialize(json) as Dictionary<string, object>;
            var paramsDict = dict["params"] as Dictionary<string, object>;
            Assert.IsNotNull(paramsDict);
            Assert.AreEqual(0, paramsDict.Count);
        }

        [Test]
        public void BuildRequest_IdIsUniqueAcrossCalls()
        {
            string json1 = rpc.BuildRequest("start", null);
            string json2 = rpc.BuildRequest("start", null);
            var id1 = (AFMiniJSON.Json.Deserialize(json1) as Dictionary<string, object>)["id"];
            var id2 = (AFMiniJSON.Json.Deserialize(json2) as Dictionary<string, object>)["id"];
            Assert.AreNotEqual(id1, id2);
        }

        // --- ParseResponse tests ---

        [Test]
        public void ParseResponse_Success_ReturnsData()
        {
            string response = "{\"id\":\"x\",\"result\":{\"data\":null}}";
            Assert.IsNull(rpc.ParseResponse(response));
        }

        [Test]
        public void ParseResponse_SuccessWithData_ReturnsData()
        {
            string response = "{\"id\":\"x\",\"result\":{\"data\":{\"uid\":\"abc\"}}}";
            var data = rpc.ParseResponse(response) as Dictionary<string, object>;
            Assert.AreEqual("abc", data["uid"]);
        }

        [Test]
        public void ParseResponse_ErrorResponse_ThrowsRPCException()
        {
            string response = "{\"id\":\"x\",\"error\":{\"code\":422,\"message\":\"bad devKey\"}}";
            var ex = Assert.Throws<AppsFlyerRPCException>(() => rpc.ParseResponse(response));
            Assert.AreEqual(422, ex.Code);
            StringAssert.Contains("bad devKey", ex.Message);
        }

        [Test]
        public void ParseResponse_EmptyString_ThrowsRPCException()
        {
            Assert.Throws<AppsFlyerRPCException>(() => rpc.ParseResponse(""));
        }

        [Test]
        public void ParseResponse_NullString_ThrowsRPCException()
        {
            Assert.Throws<AppsFlyerRPCException>(() => rpc.ParseResponse(null));
        }

        [Test]
        public void ParseResponse_MalformedJson_ThrowsRPCException()
        {
            Assert.Throws<AppsFlyerRPCException>(() => rpc.ParseResponse("{not valid json"));
        }

        [Test]
        public void ParseResponse_ErrorCode_IsLongSafe()
        {
            // AFMiniJSON parses numbers as long — verify cast doesn't throw
            string response = "{\"id\":\"x\",\"error\":{\"code\":500,\"message\":\"server error\"}}";
            var ex = Assert.Throws<AppsFlyerRPCException>(() => rpc.ParseResponse(response));
            Assert.AreEqual(500, ex.Code);
        }

        // --- onRPCEvent routing tests ---
        // These test the AppsFlyer.onRPCEvent dispatcher added in Phase 2.
        // Use a fresh AppsFlyer MonoBehaviour instance or test via event subscription.

        [Test]
        public void OnRPCEvent_StartEvent_FiresOnRequestResponse()
        {
            bool fired = false;
            AppsFlyer.OnRequestResponse += (s, e) => fired = true;
            var af = new GameObject().AddComponent<AppsFlyer>();
            af.onRPCEvent("{\"event\":\"start\",\"data\":{\"statusCode\":200,\"errorDescription\":\"\"}}");
            Assert.IsTrue(fired);
            AppsFlyer.OnRequestResponse = null;
        }

        [Test]
        public void OnRPCEvent_UnknownEvent_DoesNotThrow()
        {
            var af = new GameObject().AddComponent<AppsFlyer>();
            Assert.DoesNotThrow(() =>
                af.onRPCEvent("{\"event\":\"unknownEvent\",\"data\":{}}"));
        }

        [Test]
        public void OnRPCEvent_EmptyString_DoesNotThrow()
        {
            var af = new GameObject().AddComponent<AppsFlyer>();
            Assert.DoesNotThrow(() => af.onRPCEvent(""));
        }
    }
}
```

### Documentation references
- Test pattern to copy from: `Tests_Suite.cs:8–17` (SetUp), `Tests_Suite.cs:19–47` (assertion style)
- NSubstitute not required for this class (no mock needed — testing pure C# logic)
- `AddComponent<AppsFlyer>()` pattern for MonoBehaviour tests: Unity Test Framework PlayMode requirement

### Verification checklist
- [ ] All new tests are in namespace `AppsFlyerSDK.Tests`
- [ ] `AppsFlyerRPCClientTests` does not use `Substitute.For` (no interface to mock — tests pure logic)
- [ ] All 11 `AppsFlyerRPCClientTests` tests pass in Editor play mode
- [ ] All 71 pre-existing tests continue to pass

---

## Final Verification

Run after all phases are complete:

```bash
# Grep: confirm no public method signatures changed
grep -n "public static void initSDK\|public static void startSDK\|public static void sendEvent" \
    Assets/AppsFlyer/AppsFlyer.cs

# Grep: confirm RPC client exists and has correct class name
grep -n "class AppsFlyerRPCClient\|class AppsFlyerRPCException" \
    Assets/AppsFlyer/AppsFlyerRPCClient.cs

# Grep: confirm onRPCEvent handler exists
grep -n "public void onRPCEvent" Assets/AppsFlyer/AppsFlyer.cs

# Grep: confirm no invented APIs
grep -rn "Json.Parse\|Json.ToObject\|Json.FromString" Assets/AppsFlyer/

# Grep: confirm int cast not used on Deserialize results (should cast to long)
grep -n "(int).*Deserialize\|(int).*\[\"" Assets/AppsFlyer/AppsFlyerRPCClient.cs
# Expected: 0 matches (we cast long → int explicitly: (int)(long)...)
```

Unity Test Runner: Run `AppsFlyerRPCClientTests` + `AppsFlyerSDKTests` — all must pass.

---

## File Change Summary

| File | Change |
|---|---|
| `Assets/AppsFlyer/AppsFlyerRPCClient.cs` | **New** — RPC client singleton, exception class, platform dispatch |
| `Assets/AppsFlyer/AppsFlyer.cs` | Add `onRPCEvent()` handler; add RPC calls alongside existing bridge calls for 7 methods |
| `Assets/AppsFlyer/Tests/Tests_Suite.cs` | Add `AppsFlyerRPCClientTests` class with 13 tests |

**Files NOT changed in this phase:**
- `AppsFlyerAndroid.cs` — untouched
- `AppsFlyeriOS.cs` — untouched
- `AppsFlyerObjectScript.cs` — untouched
- `IAppsFlyerNativeBridge.cs` — untouched
- Any native `.mm` / `.java` files — untouched (Phase 2 & 3)
