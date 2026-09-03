# AppsFlyerRPC → Native SDK Method Mapping

**Audience:** engineers maintaining a cross-platform AppsFlyer plugin (Flutter, React Native, Unity, Cordova, Capacitor, Xamarin, …) who are replacing an existing per-platform native binding with `AppsFlyerRPC`.

**Purpose:** every RPC method is a thin wrapper over one specific `AppsFlyerLib` API (occasionally `AppsFlyerCrossPromotionHelper` / `AppsFlyerShareInviteHelper`). If your plugin's iOS native layer currently calls `AppsFlyerLib.shared().foo` directly, this document tells you exactly which RPC JSON call replaces that call site, so you can delete the native binding code entirely and go through the RPC bridge instead.

This is a companion to [`README.md`](../README.md), which has full JSON request/response examples, parameter tables, and the plugin integration guide. This document adds the one thing the README doesn't spell out: **which native SDK symbol each RPC method resolves to**, verified directly against the adapter source (`AppsFlyerSDKAdapter.swift`), not against documentation that can drift from code.

> **Verification note:** Every row below was traced through `AFRPCParser.swift` → `AFRPCHandlerRouter.swift` → the domain `Handler` → the domain `SDK` protocol → `AppsFlyerSDKAdapter.swift` (the single, literal pass-through to `AppsFlyerLib.shared()`). File references are given per section so you can re-verify after future changes.

---

## Migration model

```
Before (per-plugin native binding):
  Plugin native layer  →  AppsFlyerLib.shared().customerUserID = "user-123"

After (via RPC):
  Plugin native layer  →  AppsFlyerRPCBridge.executeJson(
                             {"method":"setCustomerUserId","params":{"customerId":"user-123"}}
                           )
                        →  AppsFlyerRPC internally calls AppsFlyerLib.shared().customerUserID = "user-123"
```

Your plugin's native bridge shrinks to two responsibilities: serialize a JSON request / deserialize the JSON response, and forward RPC events to your JS/Dart/C# event system. All SDK method names, property names, and validation logic move into `AppsFlyerRPC` — you no longer need per-platform native code that calls `AppsFlyerLib` directly.

---

## ⚠️ Known doc/schema drift (read before using `init`)

- **`init` is NOT a working alias for `initialize`.** `README.md` documents `init` as a "legacy alias," but the RPC dispatch table (`AFRPCParser.swift`, `methodParsers`) only registers `AFRPCInitRequest.methodName = "initialize"` (`AppsFlyerRPC/AppsFlyerRPC/Core/AFRPCTypedRequests.swift:35`). There is no `"init"` entry anywhere in that dictionary. Sending `{"method":"init", ...}` today throws `unknownMethod("init")` (404). **Use `initialize`.** This is a real bug in the current README that should be tracked and fixed separately — do not build plugin code against `init`.
- **`docs/schemas/rpc-request-envelope.schema.json` is stale** relative to the implementation: its `method` enum lists `"init"` (not `"initialize"`), still lists the removed `validateAndLogInAppPurchaseLegacy`, and is missing ~15 methods that exist in code today (`registerSessionReadyListener`, `unregisterSessionReadyListener`, `isSessionReady`, `handleLaunchOptions`, `setDisableIDFVCollection`, `setCurrentDeviceLanguage`, `logEvent`, and others). **Do not validate outgoing requests against this schema file as-is** — treat the Swift source (`AFRPCTypedRequests.swift` / `AFRPCParser.swift`) as the source of truth for supported methods until the schema is refreshed.

---

## Core — lifecycle & session (`AFRPCCoreSDK` → `AppsFlyerLib.shared()`)

Source: `Handler/Handlers/AFRPCCoreHandler.swift`, `Handler/Utilities/AppsFlyerSDKAdapter.swift` (Core extension).

| RPC method | Native SDK API it wraps | Notes |
|---|---|---|
| `initialize` | `AppsFlyerLib.shared().initialize(devKey:appId:)` | Only way to set `appsFlyerDevKey`/`appleAppID` — both are read-only properties in SDK7. Direct native setters no longer work; this must be your plugin's entry point. |
| `start` | `AppsFlyerLib.shared().start(completionHandler:)` | With `awaitResponse:true`, RPC races the SDK completion against a timeout (`SDKTimeoutHelper`); without it, fire-and-forget. |
| `logEvent` | `AppsFlyerLib.shared().logEvent(name:values:completionHandler:)` (await) or `.logEvent(_:withValues:)` (fire-and-forget) | Choice of underlying overload depends on `awaitResponse`. |
| `setPluginInfo` | `AppsFlyerLib.shared().setPluginInfo(plugin:version:additionalParams:)` | `plugin` string is translated to the SDK's `Plugin` enum via a fixed mapping table (see [Supported plugin values](../README.md#setplugininfo) in the README) before being passed to the SDK. Call before `start`. |
| `registerSessionReadyListener` | `AppsFlyerLib.shared().registerSessionReadyListener(_:)` | SDK7 API. Fires the `onSessionReady` RPC event when the SDK calls the listener closure. |
| `unregisterSessionReadyListener` | `AppsFlyerLib.shared().unregisterSessionReadyListener()` | SDK7 API. |
| `isSessionReady` | `AppsFlyerLib.shared().isSessionReady()` | SDK7 getter. Returned as `result.data.isSessionReady`. |
| `handleLaunchOptions` | `AppsFlyerLib.shared().handleLaunchOptions(_:)` | Forward the host app's `didFinishLaunchingWithOptions` dictionary here instead of calling the SDK directly from your AppDelegate-equivalent. |

## Simple config — single-value property setters (`AFRPCSimpleConfigSDK`)

Source: `Handler/Handlers/AFRPCSimpleConfigHandler.swift`, `AppsFlyerSDKAdapter.swift` (Simple config extension). Every row here is a direct property get/set on `AppsFlyerLib.shared()` — no async, no completion handler.

| RPC method | Native SDK property |
|---|---|
| `isDebug` | `AppsFlyerLib.shared().isDebug` |
| `setCustomerUserId` | `AppsFlyerLib.shared().customerUserID` |
| `setAdditionalData` | `AppsFlyerLib.shared().customData` |
| `setCurrencyCode` | `AppsFlyerLib.shared().currencyCode` |
| `setDisableAdvertisingIdentifiers` | `AppsFlyerLib.shared().disableAdvertisingIdentifier` |
| `setDisableSKAdNetwork` | `AppsFlyerLib.shared().disableSKAdNetwork` |
| `setShouldCollectDeviceName` | `AppsFlyerLib.shared().shouldCollectDeviceName` |
| `setAppInviteOneLink` | `AppsFlyerLib.shared().appInviteOneLinkID` |
| `anonymizeUser` | `AppsFlyerLib.shared().anonymizeUser` |
| `setDisableCollectASA` | `AppsFlyerLib.shared().disableCollectASA` |
| `setDisableAppleAdsAttribution` | `AppsFlyerLib.shared().disableAppleAdsAttribution` |
| `setUseReceiptValidationSandbox` | `AppsFlyerLib.shared().useReceiptValidationSandbox` |
| `setUseUninstallSandbox` | `AppsFlyerLib.shared().useUninstallSandbox` |
| `setPhoneNumber` | `AppsFlyerLib.shared().phoneNumber` (setter) |
| `setDisableIDFVCollection` | `AppsFlyerLib.shared().disableIDFVCollection` |
| `setCurrentDeviceLanguage` | `AppsFlyerLib.shared().currentDeviceLanguage` |
| `stop` | `AppsFlyerLib.shared().isStopped` |

## Complex config — multi-param configuration (`AFRPCComplexConfigSDK`)

Source: `Handler/Handlers/AFRPCComplexConfigHandler.swift`, `AppsFlyerSDKAdapter.swift` (Complex config extension).

| RPC method | Native SDK API |
|---|---|
| `setResolveDeepLinkURLs` | `AppsFlyerLib.shared().resolveDeepLinkURLs` (property) |
| `setOneLinkCustomDomain` | `AppsFlyerLib.shared().oneLinkCustomDomains` (property) |
| `setHost` | `AppsFlyerLib.shared().setHost(_:hostName:)` — **SDK7 breaking change:** RPC params are `hostPrefixName`, `hostName` (renamed/reordered vs. the old `host`/`hostPrefix` naming). |
| `setMinTimeBetweenSessions` | `AppsFlyerLib.shared().minTimeBetweenSessions` (property, `UInt` seconds) |
| `setDeepLinkTimeout` | `AppsFlyerLib.shared().deepLinkTimeout` (property, `UInt` ms) |
| `setInstallId` | `AppsFlyerLib.shared().setInstallId(_:)` |
| `setSharingFilterForPartners` | `AppsFlyerLib.shared().setSharingFilterForPartners(_:)` |
| `setPartnerData` | `AppsFlyerLib.shared().setPartnerData(partnerId:data:)` |

## Privacy — consent (`AFRPCPrivacySDK`)

Source: `Handler/Handlers/AFRPCPrivacyHandler.swift`, `AppsFlyerSDKAdapter.swift` (Privacy extension).

| RPC method | Native SDK API |
|---|---|
| `setConsentData` | `AppsFlyerLib.shared().setConsentData(_:)`, constructing an `AppsFlyerConsent(isUserSubjectToGDPR:hasConsentForDataUsage:hasConsentForAdsPersonalization:hasConsentForAdStorage:)` from the RPC params (bridging `Bool?` → `NSNumber?`) |
| `enableTCFDataCollection` | `AppsFlyerLib.shared().enableTCFDataCollection(_:)` |

## Deep link (`AFRPCDeepLinkSDK`)

Source: `Handler/Handlers/AFRPCDeepLinkHandler.swift`, `AppsFlyerSDKAdapter.swift` (Deep link extension).

| RPC method | Native SDK API | Notes |
|---|---|---|
| `appendParametersToDeepLinkingURL` | `AppsFlyerLib.shared().appendParametersToDeepLinkingURL(contains:parameters:)` | |
| `addPushNotificationDeepLinkPath` | `AppsFlyerLib.shared().addPushNotificationDeepLinkPath(_:)` | |
| `enableFacebookDeferredApplinks` | `AppsFlyerLib.shared().enableFacebookDeferredApplinks(with:)` | RPC resolves the `FBSDKAppLinkUtility` class via `NSClassFromString` when `enable:true`, passes `nil` when `false`. |
| `setFacebookDeferredAppLink` | `AppsFlyerLib.shared().facebookDeferredAppLink` (property) | RPC validates the URL scheme (rejects dangerous schemes) before assigning. |
| `handleOpenURL` / `handleOpenUrl` | `AppsFlyerLib.shared().handleOpen(_:options:)` | Both casing variants map to the same SDK call, for plugin naming-convention compatibility. Dispatched on `MainActor`. |
| `continueUserActivity` | `AppsFlyerLib.shared().continue(_:restorationHandler:)` | RPC wraps the `url` param in an `NSUserActivity` (default type `NSUserActivityTypeBrowsingWeb` unless `activityType` given) before calling. Dispatched on `MainActor`. |
| `performOnAppAttributionWithURL` | `AppsFlyerLib.shared().performOnAppAttribution(with:)` | Dispatched on `MainActor`. |

## Monetization (`AFRPCMonetizationSDK`)

Source: `Handler/Handlers/AFRPCMonetizationHandler.swift`, `AppsFlyerSDKAdapter.swift` (Monetization extension). Note: two of these go through static helper classes, **not** `AppsFlyerLib.shared()` directly.

| RPC method | Native SDK API | Notes |
|---|---|---|
| `validateAndLogInAppPurchase` | `AppsFlyerLib.shared().validateAndLogInAppPurchase(purchaseDetails:purchaseAdditionalDetails:completion:)` | Builds an `AFSDKPurchaseDetails` from `productId` + `transactionId` + `purchaseType` (`"subscription"` / `"oneTimePurchase"`). This is the SDK7 V2 path — the legacy 6-param `validateAndLogInAppPurchaseLegacy` API has no RPC exposure (see [What's new / removed](#whats-new--removed-vs-pre-sdk7-native-integrations) below). |
| `logAdRevenue` | `AppsFlyerLib.shared().logAdRevenue(_:additionalParameters:)` | Builds an `AFAdRevenueData` from `monetizationNetwork`/`mediationNetwork`/`currencyIso4217Code`/`revenue`. |
| `logCrossPromoteImpression` | `AppsFlyerCrossPromotionHelper.logCrossPromoteImpression(_:campaign:userParams:)` | Static helper class, not `AppsFlyerLib.shared()`. |
| `logAndOpenStore` | `AppsFlyerCrossPromotionHelper.logAndOpenStore(_:campaign:userParams:openStore:)` | Static helper. RPC surfaces the resulting URL as `result.data.clickURL`; the plugin is responsible for opening it. |
| `generateInviteLink` | `AppsFlyerShareInviteHelper.generateInviteLink(linkGenerator:completionHandler:)` | Static helper. RPC params map to `AppsFlyerLinkGenerator` setters (`setChannel`, `setCampaign`, `setReferrerName`, `setReferrerImageUrl`, `setReferrerCustomerId`, `setBaseDeepLink`, `.brandDomain`, `addUserParams`). SDK7 rename — was `generateInviteUrl`. |
| `logInvite` | `AppsFlyerShareInviteHelper.logInvite(_:eventParameters:)` | Static helper. |

## Notification (`AFRPCNotificationSDK`)

Source: `Handler/Handlers/AFRPCNotificationHandler.swift`, `AppsFlyerSDKAdapter.swift` (Notification extension).

| RPC method | Native SDK API |
|---|---|
| `handlePushNotification` | `AppsFlyerLib.shared().handlePushNotification(_:)` |
| `registerUninstall` | `AppsFlyerLib.shared().registerUninstall(_:)` (expects `Data`; RPC hex-decodes the JSON string param) |

## Data (`AFRPCDataSDK`)

Source: `Handler/Handlers/AFRPCDataHandler.swift`, `AppsFlyerSDKAdapter.swift` (Data extension).

| RPC method | Native SDK API |
|---|---|
| `getAppsFlyerUID` | `AppsFlyerLib.shared().getAppsFlyerUID()` |
| `getSdkVersion` | `AppsFlyerLib.shared().getSdkVersion()` |
| `setUserEmails` | `AppsFlyerLib.shared().setUserEmails(_:with:)` |
| `logLocation` | `AppsFlyerLib.shared().logLocation(longitude:latitude:)` |

## Listener (`AFRPCListenerSDK`) — delegate registration, not one-shot calls

Source: `Handler/Handlers/AFRPCListenerHandler.swift`, `Handler/Delegates/AFRPCRequestHandlerDelegates.swift`, `AppsFlyerSDKAdapter.swift` (Listener extension). These two RPC methods don't call an SDK method — they assign the RPC layer itself as the SDK delegate, then forward delegate callbacks back to the plugin as async RPC **events** (not request/response).

| RPC method | Native SDK API it wraps | What happens after registration |
|---|---|---|
| `registerConversionListener` | `AppsFlyerLib.shared().delegate = <self>` (sets `AppsFlyerLibDelegate`) | `AppsFlyerLibDelegate.onConversionDataSuccess(_:)` → RPC event `onConversionDataSuccess`; `.onConversionDataFail(_:)` → RPC event `onConversionDataFail`. |
| `registerDeeplinkListener` | `AppsFlyerLib.shared().deepLinkDelegate = <self>` (sets `AppsFlyerDeepLinkDelegate`) | `AppsFlyerDeepLinkDelegate.didResolveDeepLink(_:)` → RPC event `onDeepLinkReceived`, with `data.status` ∈ `"found" / "failure" / "notFound"`. |

`registerSessionReadyListener` / `unregisterSessionReadyListener` / `isSessionReady` are functionally in this same "listener" family but are implemented in the Core handler — see the Core table above.

---

## What's new / removed vs. pre-SDK7 native integrations

If your plugin's existing native code was written against a pre-SDK7 `AppsFlyerLib`, these are the RPC-relevant behavior changes to account for when you replace the direct binding with RPC calls — RPC does not paper over them:

| Change | Detail |
|---|---|
| **`appsFlyerDevKey` / `appleAppID` are read-only in SDK7** | You can no longer set them via direct property assignment. `initialize` is the only entry point exposed through RPC (and the only one in the underlying SDK). |
| **Session-ready gating is new** | `start` must be issued from inside the callback delivered by `registerSessionReadyListener` (SDK7 concept, has no pre-SDK7 equivalent). Do consent/ATT work in that callback before calling `start`. |
| **`setHost` params renamed/reordered** | Old: `host` (name) with prefix second. New (as exposed by RPC): `hostPrefixName` then `hostName`. |
| **`validateAndLogInAppPurchaseLegacy` removed** | The old 6-param IAP validation path (no `purchaseType`) has no RPC method and no SDK7 replacement path other than V2. Use `validateAndLogInAppPurchase`. |
| **`generateInviteUrl` renamed to `generateInviteLink`** | Same params; completion now also carries an `error` field (SDK7 callback signature changed to include `Error?`). |
| **`onAppOpenAttribution` / `onAppOpenAttributionFailure` retired** | These delegate callbacks (and their would-be RPC events) don't exist in SDK7. All retargeting/deep-link outcomes — including what used to be "on-app attribution" — now arrive as a single `onDeepLinkReceived` event; branch on `data.status`. |
| **`handleLaunchOptions` is new** | No pre-SDK7 direct equivalent exposed through RPC; forward the host app's launch options dictionary through this RPC call instead of any ad hoc native code you had. |

---

## Unit testing bridge calls

Examples below follow the exact conventions of the existing suite in
`AppsFlyerRPC/AppsFlyerRPCTests/`: XCTest, `@testable import AppsFlyerRPC`, one hand-written mock
per domain SDK protocol (`AFRPCConsolidatedHandlerTests.swift`), and shared assertion helpers
(`RPCTestHelpers.swift`). Every SDK call is mocked — nothing hits `AppsFlyerLib` or the network.
Use these as templates for testing your own plugin bridge's integration with `AppsFlyerRPC`, or
for extending the RPC module's own test suite.

### 1. Common setup — mock the domain SDK protocol, not `AppsFlyerLib`

Each domain handler (`AFRPCCoreHandler`, `AFRPCSimpleConfigHandler`, …) depends on a narrow
protocol (`AFRPCCoreSDK`, `AFRPCSimpleConfigSDK`, …), never on `AppsFlyerLib` directly. Tests
conform a small mock class to the protocol slice the handler actually needs — no 40-member stub:

```swift
import XCTest
@testable import AppsFlyerRPC

final class MockSimpleConfigSDK: AFRPCSimpleConfigSDK {
    var isDebug: Bool = false
    var customerUserID: String?
    var customData: [AnyHashable: Any]?
    var currencyCode: String?
    var disableAdvertisingIdentifier: Bool = false
    var disableSKAdNetwork: Bool = false
    var shouldCollectDeviceName: Bool = false
    var appInviteOneLinkID: String?
    var anonymizeUser: Bool = false
    var disableCollectASA: Bool = false
    var disableAppleAdsAttribution: Bool = false
    var useReceiptValidationSandbox: Bool = false
    var useUninstallSandbox: Bool = false
    func setPhoneNumber(_ phoneNumber: String) {}
    var disableIDFVCollection: Bool = false
    var currentDeviceLanguage: String?
    var isStopped: Bool = false
}
```

### 2. Handler-level unit test (sync property setter)

Construct the handler directly with the mock, build a typed request, call `handle(_:)`, and assert
against the mock's captured state — no JSON involved at this layer.

```swift
final class AFRPCSimpleConfigHandlerTests: XCTestCase {
    func testSetCustomerUserId_setsValue() async throws {
        // Given
        let mock = MockSimpleConfigSDK()
        let handler = AFRPCSimpleConfigHandler(sdk: mock)
        let req = try AFRPCSetCustomerUserIdRequest(from: ["customerId": "user-42"])

        // When
        let result = await handler.handle(.setCustomerUserId(req))

        // Then
        XCTAssertEqual(mock.customerUserID, "user-42")
        guard case .success = result else { XCTFail("Expected success"); return }
    }
}
```

### 3. End-to-end JSON round trip (the exact contract a plugin's native bridge exercises)

This is the layer your plugin's own Objective-C/Swift bridge code actually calls
(`AppsFlyerRPCBridge.executeJson` wraps `AFRPCClient.execute(jsonRequest:)`). Testing at this level
exercises real JSON parsing + validation + routing, not just one handler in isolation. Passing
`{ _ in }` as the event emitter is enough when the test doesn't care about async events.

```swift
final class MyBridgeUsageTests: XCTestCase {
    func testSetCustomerUserId_endToEndFromRawJSON() async {
        // Given
        let client = AFRPCClient { _ in }
        let json = #"{"id":"req-1","method":"setCustomerUserId","params":{"customerId":"user-42"}}"#

        // When
        let response = await client.execute(jsonRequest: json)

        // Then
        RPCTestHelpers.assertSuccessEnvelope(response, expectedId: "req-1")
    }
}
```

### 4. Blocking/awaited call — success path (`awaitResponse: true`)

`start`, `logEvent`, `validateAndLogInAppPurchase`, `logAndOpenStore`, and `generateInviteLink`
support `awaitResponse: true`, which routes through `SDKTimeoutHelper.withSDKCompletionTimeout` —
an `async` race between the SDK's completion handler and a timer, not a blocked thread + latch (the
Android bridge blocks a real thread for this; the iOS one is structured concurrency throughout).
Mocks that invoke their completion handler synchronously make this deterministic, no sleeping:

```swift
final class AFRPCCoreHandlerTests: XCTestCase {
    func testStart_awaitResponseTrue_returnsSuccessOnceSDKCompletes() async throws {
        // Given — MockCoreSDK.start(completionHandler:) invokes the handler immediately
        let mock = MockCoreSDK()
        let handler = AFRPCCoreHandler(sdk: mock, timeoutConfig: .test)
        let req = try AFRPCStartRequest(from: ["awaitResponse": true])

        // When
        let result = await handler.handle(.start(req))

        // Then
        guard case .success = result else { XCTFail("Expected success"); return }
        XCTAssertTrue(mock.startCalled)
    }
}
```

`timeoutConfig: .test` (defined in `SDKTimeoutHelper.swift`) shortens every timeout to 0.3s so
timeout-path tests stay fast; production code always uses `.default` (10–30s per operation).

### 5. Timeout path — SDK callback never fires

`SDKTimeoutHelperTests.swift` covers this directly at the helper level, since it's the single
choke point every awaited domain call routes through:

```swift
final class SDKTimeoutHelperTests: XCTestCase {
    func testTimeoutFiresWhenOperationNeverCompletes() async {
        // Given / When — the SDK-call closure never invokes `completion`
        let result = await SDKTimeoutHelper.withSDKCompletionTimeout(
            timeoutSeconds: 0.5,
            operationName: "neverComplete"
        ) { _ in }

        // Then
        guard case .failure(let failure) = result else { XCTFail("Expected failure"); return }
        XCTAssertEqual(failure.errorType, "timeout")
        XCTAssertTrue(failure.message.contains("timed out"))
    }
}
```

If your plugin bridge wraps this in its own async/await or Promise layer, mirror this shape: never
invoke the mock's completion handler and assert your wrapper surfaces a timeout, not a hang.

### 6. Malformed / unknown input

Two independent failure points: an unrecognized `method` string (404), and a recognized method
missing a required param (422).

```swift
func testUnknownMethod_throwsUnknownMethodError() {
    XCTAssertThrowsError(try AFRPCParser.validateMethod("notARealMethod")) { error in
        guard case AFRPCClientError.unknownMethod(let method) = error else {
            XCTFail("Expected unknownMethod, got \(error)"); return
        }
        XCTAssertEqual(method, "notARealMethod")
        XCTAssertEqual(AFRPCClientError.unknownMethod(method).rpcError.code, 404)
    }
}

func testInitialize_missingDevKey_throwsMissingParameter() {
    XCTAssertThrowsError(try AFRPCInitRequest(from: ["appId": "id123456789"])) { error in
        guard case AFRPCClientError.missingParameter(let param) = error else {
            XCTFail("Expected missingParameter, got \(error)"); return
        }
        XCTAssertEqual(param, "devKey")
        XCTAssertEqual(AFRPCClientError.missingParameter(param).rpcError.code, 422)
    }
}
```

Unlike the Android bridge (`optString`/`optBoolean` with silent defaults), the iOS `Request(from:)`
initializers throw on missing required params — there's no equivalent "loose validation" gotcha
here to defend against on the plugin side.

### 7. Persistent listener → event emission shape

`registerConversionListener` / `registerDeeplinkListener` don't return data directly — they wire
the RPC layer as the SDK delegate, which later emits async events through the same emitter your
plugin's `setEventHandler` callback receives. Simulate the SDK invoking its delegate method and
assert on the emitted `AFRPCEvent`, exactly as `AFRPCListenerLifecycleTests.swift` does:

```swift
final class AFRPCListenerLifecycleTests: XCTestCase {
    func testRegisterConversionListener_delegateCallback_emitsEvent() async throws {
        // Given
        var receivedEvents: [AFRPCEvent] = []
        let eventExpectation = expectation(description: "onConversionDataSuccess emitted")
        let handler = AFRPCRequestHandler { event in
            receivedEvents.append(event)
            if event.event == "onConversionDataSuccess" { eventExpectation.fulfill() }
        }
        let request = try AFRPCRegisterConversionListenerRequest(from: [:])
        _ = await handler.handle(request: .registerConversionListenerRequest(request), requestId: nil)

        // When — simulate the SDK firing its AppsFlyerLibDelegate callback
        handler.onConversionDataSuccess(["af_status": "Non-organic", "media_source": "facebook"])

        // Then
        await fulfillment(of: [eventExpectation], timeout: 2.0)
        XCTAssertEqual(receivedEvents.first?.event, "onConversionDataSuccess")
        XCTAssertEqual(receivedEvents.first?.origin, "ios")
    }
}
```

For the full existing suite (mock-per-domain handler tests, router dispatch tests, concurrency
tests, bridge hardening tests, characterization tests), see:
- `AppsFlyerRPC/AppsFlyerRPCTests/AFRPCConsolidatedHandlerTests.swift` — one mock + handler test per domain, plus router dispatch tests
- `AppsFlyerRPC/AppsFlyerRPCTests/AppsFlyerRPCiOSTests.swift` — handler/event/bridge/client integration tests
- `AppsFlyerRPC/AppsFlyerRPCTests/SDKTimeoutHelperTests.swift` — timeout helper unit tests
- `AppsFlyerRPC/AppsFlyerRPCTests/AppsFlyerRPCValidationTests.swift` — required-param validation errors, one per method
- `AppsFlyerRPC/AppsFlyerRPCTests/AppsFlyerRPCRoutingTests.swift` — unknown-method / error-code mapping
- `AppsFlyerRPC/AppsFlyerRPCTests/RPCTestHelpers.swift` — shared envelope assertion helpers used above

---

For full JSON payloads, parameter tables, and the plugin integration walkthrough (React Native / Flutter examples, step-by-step new-plugin guide, event schemas), see [`README.md`](../README.md).
