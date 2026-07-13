# Plugin Bridge → SDK API Mapping

Reference for cross-platform plugin implementers (Unity, Flutter, React Native, Cordova,
Xamarin, Cocos2d-x, Capacitor, NativeScript, Expo, Unreal, Segment, mParticle, Adobe AIR/Mobile)
consuming the Android SDK through the `plugin_bridge` module.

Related docs: `docs/features/F-039-json-rpc-bridge.md`, `docs/features/F-010-plugin-type-registration.md`,
`docs/issue-cases/IC-174-plugin-bridge-customer-userid-rpc-leak.md`,
`docs/issue-cases/IC-170-anr-appsflyerlibcore-init-plugin-enum.md`.

---

## 1. Architecture

`plugin_bridge` is **not** a set of per-method public bindings. It is a single JSON-RPC-style
dispatcher. Plugin code never calls SDK classes directly — it sends a JSON string and gets a
response object back.

```
Plugin runtime (Flutter / RN / Unity / Cordova / Xamarin / Cocos2d-x / ...)
  → AppsFlyerRpcHandler.execute(jsonString: String): RpcResponse
      → JsonRpcRequestParser.parse(jsonString)
          - reads "method" (String) and "params" (JSONObject)
          - maps method name → typed RpcRequest subclass (~65 methods)
      → AppsFlyerRpcHandler.execute(request: RpcRequest): RpcResponse
          - each request maps to exactly one AppsFlyerLib call (or a share/* helper)
          - returns RpcResponse.Success<T> / RpcResponse.VoidSuccess / RpcResponse.Error(code, message)
```

**Request shape:**
```json
{ "method": "logEvent", "params": { "eventName": "af_purchase", "eventValues": {}, "awaitResponse": false } }
```

**Response shape** (`com.appsflyer.pluginbridge.model.RpcResponse`, a Kotlin sealed class — the
calling plugin layer is responsible for serializing it to JSON/whatever the target runtime needs):
- `Success<T>(result: T)`
- `VoidSuccess`
- `Error(code: Int, message: String)` — codes: `400` bad request, `404` method not found,
  `422` invalid parameters, `500` internal error, `503` SDK not initialized

**Async events** (conversion data, deep links, session ready) are delivered independently of the
request/response cycle, via the `RpcEventNotifier` callback (`(String) -> Unit`) supplied when
constructing `AppsFlyerRpcHandler`. Event JSON shape (`RpcEventFormatter.formatEvent`):
```json
{ "event": "onConversionDataSuccess", "data": { ... }, "timestamp": 1735600000000, "origin": "android" }
```
Event names: `onConversionDataSuccess`, `onConversionDataFail`, `onDeepLinking`, `onSessionReady`.

**Blocking calls:** `start`, `logEvent`, `generateInviteLink`, `validateAndLogInAppPurchase`
accept `"awaitResponse": true`. When set, the handler blocks the calling thread (`CountDownLatch`
+ timeout) until the SDK's async callback fires, and converts the result into the RPC response.
Timeouts: `logEvent` 5s, `start` 5s, `validateAndLogInAppPurchase` 5s, `generateInviteLink` 10s.
On timeout: `RpcResponse.Error(INTERNAL_ERROR, ...)`. **If the plugin calls this from a UI thread,
a stalled SDK callback stalls the UI for up to the timeout window — dispatch off the main thread.**

**Persistent listeners:** `registerConversionListener`, `subscribeForDeepLink`,
`registerSessionReadyListener` register strong-reference listeners that keep firing events into the
notifier until explicitly unregistered. Failing to unregister leaks the listener.

---

## 2. Method → SDK mapping

| RPC `method` | Params | Delegates to |
|---|---|---|
| `init` | `devKey` | `AppsFlyerLib.init(devKey, null, context)` |
| `start` | `awaitResponse` | `AppsFlyerLib.start()` / `start(AppsFlyerRequestListener)` |
| `logEvent` | `eventName, eventValues, awaitResponse` | `AppsFlyerLib.logEvent(context, eventName, eventValues[, listener])` |
| `isDebug` | `shouldEnable` | `AppsFlyerLib.setDebugLog(shouldEnable)` |
| `registerConversionListener` | — | `AppsFlyerLib.registerConversionListener(listener)` → events `onConversionDataSuccess` / `onConversionDataFail` |
| `unregisterConversionListener` | — | `AppsFlyerLib.unregisterConversionListener()` |
| `registerSessionReadyListener` | — | `AppsFlyerLib.registerSessionReadyListener(listener)` → event `onSessionReady` |
| `unregisterSessionReadyListener` | — | `AppsFlyerLib.unregisterSessionReadyListener()` |
| `isSessionReady` | — | `AppsFlyerLib.isSessionReady()` → Boolean |
| `setPluginInfo` | `plugin, pluginVersion` | Resolves `plugin` string → `Plugin` enum (case-insensitive), builds `PluginInfo`, `AppsFlyerLib.setPluginInfo(pluginInfo)` |
| `setCustomerUserId` | `customerId` | `AppsFlyerLib.setCustomerUserId(customerId)` |
| `setCurrencyCode` | `currencyCode` (exactly 3 chars) | `AppsFlyerLib.setCurrencyCode(currencyCode)` |
| `setAdditionalData` | `customData` | `AppsFlyerLib.setAdditionalData(customData)` |
| `setAppInviteOneLink` | `oneLinkId` | `AppsFlyerLib.setAppInviteOneLink(oneLinkId)` |
| `setMinTimeBetweenSessions` | `seconds` | `AppsFlyerLib.setMinTimeBetweenSessions(seconds)` |
| `setHost` | `hostPrefixName, hostName` | `AppsFlyerLib.setHost(hostPrefixName, hostName)` |
| `setOutOfStore` | `sourceName` | `AppsFlyerLib.setOutOfStore(sourceName)` |
| `setUserEmail` | `email` | `AppsFlyerLib.setUserEmail(email)` |
| `setUserPhone` | `countryCode, phoneNumber` | `AppsFlyerLib.setUserPhone(countryCode, phoneNumber)` |
| `setUserFirstName` | `firstName` | `AppsFlyerLib.setUserFirstName(firstName)` |
| `setUserLastName` | `lastName` | `AppsFlyerLib.setUserLastName(lastName)` |
| `setUserFbLoginId` | `fbLoginId: Long` | `AppsFlyerLib.setUserFbLoginId(fbLoginId)` |
| `clearUserPii` | — | `AppsFlyerLib.clearUserPii()` |
| `setPartnerData` | `partnerId, data` | `AppsFlyerLib.setPartnerData(partnerId, data)` |
| `setSharingFilterForPartners` | `partners: List<String>` | `AppsFlyerLib.setSharingFilterForPartners(*partners)` (`"all"` blocks everything) |
| `setPreinstallAttribution` | `mediaSource, campaign, siteId` | `AppsFlyerLib.setPreinstallAttribution(mediaSource, campaign, siteId)` |
| `setConsentData` | `isUserSubjectToGDPR, hasConsentForDataUsage, hasConsentForAdsPersonalization, hasConsentForAdStorage` | Builds `AppsFlyerConsent`, `AppsFlyerLib.setConsentData(consent)` |
| `setLogLevel` | `logLevel: String` | Resolves case-insensitively → `AFLogger.LogLevel`, `AppsFlyerLib.setLogLevel(level)`; unknown → `422` |
| `setIsUpdate` | `isUpdate` | `AppsFlyerLib.setIsUpdate(isUpdate)` |
| `setAppId` | `appId` | `AppsFlyerLib.setAppId(appId)` |
| `setInstallId` | `installId` | `AppsFlyerLib.setInstallId(installId)` |
| `anonymizeUser` | `shouldAnonymize` | `AppsFlyerLib.anonymizeUser(shouldAnonymize)` |
| `getAppsFlyerUID` | — | `AppsFlyerLib.getAppsFlyerUID(context)` → String |
| `getSdkVersion` | — | `AppsFlyerLib.sdkVersion` → String |
| `getHostName` | — | `AppsFlyerLib.hostName` |
| `getHostPrefix` | — | `AppsFlyerLib.hostPrefix` |
| `getOutOfStore` | — | `AppsFlyerLib.getOutOfStore(context)` |
| `getAttributionId` | — | `AppsFlyerLib.getAttributionId(context)` |
| `isStopped` | — | `AppsFlyerLib.isStopped` → Boolean |
| `isPreInstalledApp` | — | `AppsFlyerLib.isPreInstalledApp(context)` → Boolean |
| `setCollectAndroidID` | `isCollect` | `AppsFlyerLib.setCollectAndroidID(isCollect)` |
| `setDisableAdvertisingIdentifiers` | `isDisable` | `AppsFlyerLib.setDisableAdvertisingIdentifiers(isDisable)` |
| `setDisableNetworkData` | `isDisable` | `AppsFlyerLib.setDisableNetworkData(isDisable)` |
| `enableTCFDataCollection` | `shouldCollect` | `AppsFlyerLib.enableTCFDataCollection(shouldCollect)` |
| `disableAppSetId` | — | `AppsFlyerLib.disableAppSetId()` |
| `subscribeForDeepLink` | — | `AppsFlyerLib.subscribeForDeepLink(listener)` → event `onDeepLinking` `{status, error?, deepLink?}` |
| `unsubscribeForDeepLink` | — | **Soft unsubscribe only** — nulls the local ref; SDK has no public unsubscribe API, the original listener stays registered internally, later events are dropped (not cached), not truly removed |
| `performDeepLinking` | `url, shouldTriggerSession` | `AppsFlyerLib.performDeepLinking(url, shouldTriggerSession)` |
| `setDeepLinkTimeout` | `timeout: Long` | `AppsFlyerLib.setDeepLinkTimeout(timeout)` |
| `setResolveDeepLinkURLs` | `urls: List<String>` | `AppsFlyerLib.setResolveDeepLinkURLs(*urls)` |
| `setOneLinkCustomDomain` | `domains` | `AppsFlyerLib.setOneLinkCustomDomain(*domains)` |
| `appendParametersToDeepLinkingURL` | `contains, parameters` | `AppsFlyerLib.appendParametersToDeepLinkingURL(contains, parameters)` |
| `addPushNotificationDeepLinkPath` | `deepLinkPath: List<String>` | `AppsFlyerLib.addPushNotificationDeepLinkPath(*deepLinkPath)` |
| `enableFacebookDeferredApplinks` | `isEnabled` | `AppsFlyerLib.enableFacebookDeferredApplinks(isEnabled)` |
| `logCrossPromoteImpression` | `appId, campaign, userParams` | `CrossPromotionHelper.logCrossPromoteImpression(context, appId, campaign, userParams)` |
| `logAndOpenStore` | `promotedAppId, campaign, userParams` | `CrossPromotionHelper.logAndOpenStore(context, promotedAppId, campaign, userParams)` |
| `logInvite` | `channel, eventParameters` | `ShareInviteHelper.logInvite(context, channel, eventParameters)` |
| `generateInviteLink` | `channel, campaign, referrerName, referrerImageUrl, customerId, baseDeepLink, brandDomain, userParams, awaitResponse` | `ShareInviteHelper.generateInviteUrl(context)` → `LinkGenerator` with optional setters; `awaitResponse=false` → sync long link, `true` → async short link, falls back to long link on `onResponseError` (returned as `Success`, not `Error`) |
| `logAdRevenue` | `monetizationNetwork, mediationNetwork, currencyIso4217Code, revenue, additionalParameters` | Resolves `mediationNetwork` → `MediationNetwork` enum, builds `AFAdRevenueData`, `AppsFlyerLib.logAdRevenue(adRevenueData, additionalParameters)` |
| `logLocation` | `latitude (-90..90), longitude (-180..180)` | `AppsFlyerLib.logLocation(context, latitude, longitude)` |
| `logSession` | — | `AppsFlyerLib.logSession(context)` |
| `validateAndLogInAppPurchase` | `purchaseType, purchaseToken, productId, additionalParameters, awaitResponse (default true)` | Resolves `purchaseType` → `AFPurchaseType` (`subscription` / `one_time_purchase`), builds `AFPurchaseDetails`, `AppsFlyerLib.validateAndLogInAppPurchase(details, params[, callback])` |
| `sendPushNotificationData` | `campaign, pid, isRetargeting, additionalParameters` | Builds `AFPushData`, `AppsFlyerLib.sendPushNotificationData(pushData)` — **triggers a new Launch event even if one was already sent this session** |
| `updateServerUninstallToken` | `token` | `AppsFlyerLib.updateServerUninstallToken(context, token)` |
| `stop` | `shouldStop` | `AppsFlyerLib.stop(shouldStop, context)` |
| `onPause` | — | `AppsFlyerLib.onPause(context)` — primarily for Cocos2d-x, which has its own `applicationDidEnterBackground` event |

---

## 3. Request examples

Full `{"method": ..., "params": {...}}` envelope for every RPC method, reflecting the exact field
names, types, and defaults in `RpcRequest.kt` / `JsonRpcRequestParser.kt`. Fields not present in
`params` fall back to the default shown in section 2 (e.g. omitting `awaitResponse` defaults to
`false`, except `validateAndLogInAppPurchase` and `generateInviteLink` which default to `true`).

#### `init`
```json
{ "method": "init", "params": { "devKey": "abcDEF123xyz" } }
```

#### `start`
```json
{ "method": "start", "params": { "awaitResponse": false } }
```

#### `logEvent`
```json
{
  "method": "logEvent",
  "params": {
    "eventName": "af_purchase",
    "eventValues": { "af_revenue": 9.99, "af_currency": "USD", "af_content_id": "1234" },
    "awaitResponse": false
  }
}
```

#### `isDebug`
```json
{ "method": "isDebug", "params": { "isDebug": true } }
```

#### `registerConversionListener`
```json
{ "method": "registerConversionListener", "params": {} }
```

#### `unregisterConversionListener`
```json
{ "method": "unregisterConversionListener", "params": {} }
```

#### `registerSessionReadyListener`
```json
{ "method": "registerSessionReadyListener", "params": {} }
```

#### `unregisterSessionReadyListener`
```json
{ "method": "unregisterSessionReadyListener", "params": {} }
```

#### `isSessionReady`
```json
{ "method": "isSessionReady", "params": {} }
```

#### `setPluginInfo`
```json
{ "method": "setPluginInfo", "params": { "plugin": "flutter", "pluginVersion": "6.14.0" } }
```

#### `setCustomerUserId`
```json
{ "method": "setCustomerUserId", "params": { "customerId": "user-12345" } }
```

#### `setCurrencyCode`
```json
{ "method": "setCurrencyCode", "params": { "currencyCode": "USD" } }
```

#### `setAdditionalData`
```json
{ "method": "setAdditionalData", "params": { "customData": { "key1": "value1", "key2": 42 } } }
```

#### `setAppInviteOneLink`
```json
{ "method": "setAppInviteOneLink", "params": { "oneLinkId": "H5d2" } }
```

#### `setMinTimeBetweenSessions`
```json
{ "method": "setMinTimeBetweenSessions", "params": { "seconds": 30 } }
```

#### `setHost`
```json
{ "method": "setHost", "params": { "hostPrefixName": "custom", "hostName": "example.com" } }
```
`hostPrefixName` is nullable — omit it or pass `null` to clear it.

#### `setOutOfStore`
```json
{ "method": "setOutOfStore", "params": { "sourceName": "samsung" } }
```

#### `setUserEmail`
```json
{ "method": "setUserEmail", "params": { "email": "user@example.com" } }
```

#### `setUserPhone`
```json
{ "method": "setUserPhone", "params": { "countryCode": "+1", "phoneNumber": "5551234567" } }
```

#### `setUserFirstName`
```json
{ "method": "setUserFirstName", "params": { "firstName": "John" } }
```

#### `setUserLastName`
```json
{ "method": "setUserLastName", "params": { "lastName": "Doe" } }
```

#### `setUserFbLoginId`
```json
{ "method": "setUserFbLoginId", "params": { "fbLoginId": 123456789 } }
```

#### `clearUserPii`
```json
{ "method": "clearUserPii", "params": {} }
```

#### `setPartnerData`
```json
{ "method": "setPartnerData", "params": { "partnerId": "partner_abc", "data": { "key": "value" } } }
```

#### `setSharingFilterForPartners`
```json
{ "method": "setSharingFilterForPartners", "params": { "partners": ["facebook", "google_ads"] } }
```
Pass `["all"]` to block sharing with every partner.

#### `setPreinstallAttribution`
```json
{
  "method": "setPreinstallAttribution",
  "params": { "mediaSource": "samsung_preload", "campaign": "summer_promo", "siteId": "12345" }
}
```

#### `setConsentData`
```json
{
  "method": "setConsentData",
  "params": {
    "isUserSubjectToGDPR": true,
    "hasConsentForDataUsage": true,
    "hasConsentForAdsPersonalization": false,
    "hasConsentForAdStorage": true
  }
}
```
`hasConsentForDataUsage` / `hasConsentForAdsPersonalization` / `hasConsentForAdStorage` are nullable.

#### `setLogLevel`
```json
{ "method": "setLogLevel", "params": { "logLevel": "DEBUG" } }
```
Valid values: `NONE`, `ERROR`, `WARNING`, `INFO`, `DEBUG`, `VERBOSE` (case-insensitive).

#### `setIsUpdate`
```json
{ "method": "setIsUpdate", "params": { "isUpdate": true } }
```

#### `setAppId`
```json
{ "method": "setAppId", "params": { "appId": "com.example.app" } }
```

#### `setInstallId`
```json
{ "method": "setInstallId", "params": { "installId": "custom-install-id-001" } }
```

#### `anonymizeUser`
```json
{ "method": "anonymizeUser", "params": { "shouldAnonymize": true } }
```

#### `getAppsFlyerUID`
```json
{ "method": "getAppsFlyerUID", "params": {} }
```

#### `getSdkVersion`
```json
{ "method": "getSdkVersion", "params": {} }
```

#### `getHostName`
```json
{ "method": "getHostName", "params": {} }
```

#### `getHostPrefix`
```json
{ "method": "getHostPrefix", "params": {} }
```

#### `getOutOfStore`
```json
{ "method": "getOutOfStore", "params": {} }
```

#### `getAttributionId`
```json
{ "method": "getAttributionId", "params": {} }
```

#### `isStopped`
```json
{ "method": "isStopped", "params": {} }
```

#### `isPreInstalledApp`
```json
{ "method": "isPreInstalledApp", "params": {} }
```

#### `setCollectAndroidID`
```json
{ "method": "setCollectAndroidID", "params": { "isCollect": false } }
```

#### `setDisableAdvertisingIdentifiers`
```json
{ "method": "setDisableAdvertisingIdentifiers", "params": { "isDisable": true } }
```

#### `setDisableNetworkData`
```json
{ "method": "setDisableNetworkData", "params": { "isDisable": true } }
```

#### `enableTCFDataCollection`
```json
{ "method": "enableTCFDataCollection", "params": { "shouldCollect": true } }
```

#### `disableAppSetId`
```json
{ "method": "disableAppSetId", "params": {} }
```

#### `subscribeForDeepLink`
```json
{ "method": "subscribeForDeepLink", "params": {} }
```

#### `unsubscribeForDeepLink`
```json
{ "method": "unsubscribeForDeepLink", "params": {} }
```

#### `performDeepLinking`
```json
{
  "method": "performDeepLinking",
  "params": { "url": "https://example.onelink.me/abc?deep_link_value=xyz", "shouldTriggerSession": true }
}
```

#### `setDeepLinkTimeout`
```json
{ "method": "setDeepLinkTimeout", "params": { "timeout": 3000 } }
```
`timeout` is milliseconds and must be positive.

#### `setResolveDeepLinkURLs`
```json
{ "method": "setResolveDeepLinkURLs", "params": { "urls": ["example.com", "www.example.com"] } }
```

#### `setOneLinkCustomDomain`
```json
{ "method": "setOneLinkCustomDomain", "params": { "domains": ["link.example.com"] } }
```

#### `appendParametersToDeepLinkingURL`
```json
{
  "method": "appendParametersToDeepLinkingURL",
  "params": { "contains": "af_dp", "parameters": { "utm_source": "newsletter" } }
}
```

#### `addPushNotificationDeepLinkPath`
```json
{ "method": "addPushNotificationDeepLinkPath", "params": { "deepLinkPath": ["push", "promotions"] } }
```

#### `enableFacebookDeferredApplinks`
```json
{ "method": "enableFacebookDeferredApplinks", "params": { "isEnabled": true } }
```

#### `logCrossPromoteImpression`
```json
{
  "method": "logCrossPromoteImpression",
  "params": {
    "appId": "com.example.otherapp",
    "campaign": "cross_promo_1",
    "userParams": { "utm_source": "in_app" }
  }
}
```

#### `logAndOpenStore`
```json
{
  "method": "logAndOpenStore",
  "params": {
    "promotedAppId": "com.example.otherapp",
    "campaign": "cross_promo_1",
    "userParams": { "utm_source": "in_app" }
  }
}
```

#### `logInvite`
```json
{
  "method": "logInvite",
  "params": { "channel": "sms", "eventParameters": { "invitee": "friend1" } }
}
```

#### `generateInviteLink`
```json
{
  "method": "generateInviteLink",
  "params": {
    "channel": "whatsapp",
    "campaign": "referral",
    "referrerName": "Jane",
    "referrerImageUrl": "https://example.com/avatar.png",
    "customerId": "user-12345",
    "baseDeepLink": "myapp://invite",
    "brandDomain": "invite.example.com",
    "userParams": { "promoCode": "JANE10" },
    "awaitResponse": true
  }
}
```
All fields except `awaitResponse` are nullable/optional — send only what you have.

#### `logAdRevenue`
```json
{
  "method": "logAdRevenue",
  "params": {
    "monetizationNetwork": "admob",
    "mediationNetwork": "ADMOB",
    "currencyIso4217Code": "USD",
    "revenue": 0.0125,
    "additionalParameters": { "ad_unit": "rewarded_video" }
  }
}
```

#### `logLocation`
```json
{ "method": "logLocation", "params": { "latitude": 32.0853, "longitude": 34.7818 } }
```

#### `logSession`
```json
{ "method": "logSession", "params": {} }
```

#### `validateAndLogInAppPurchase`
```json
{
  "method": "validateAndLogInAppPurchase",
  "params": {
    "purchaseType": "subscription",
    "purchaseToken": "token_abc123",
    "productId": "premium_monthly",
    "additionalParameters": { "af_currency": "USD" },
    "awaitResponse": true
  }
}
```
`purchaseType` must be `subscription` or `one_time_purchase`. `awaitResponse` defaults to `true`
for this method (unlike most others, which default to `false`).

#### `sendPushNotificationData`
```json
{
  "method": "sendPushNotificationData",
  "params": {
    "campaign": "push_campaign_1",
    "pid": "push_provider",
    "isRetargeting": true,
    "additionalParameters": { "af_sub1": "value" }
  }
}
```

#### `updateServerUninstallToken`
```json
{ "method": "updateServerUninstallToken", "params": { "token": "fcm-token-xyz" } }
```

#### `stop`
```json
{ "method": "stop", "params": { "shouldStop": true } }
```
`shouldStop` defaults to `true` if omitted.

#### `onPause`
```json
{ "method": "onPause", "params": {} }
```

---

## 4. Plugin identification (`setPluginInfo`)

A plugin must call `setPluginInfo` immediately after creating the SDK instance, **before** `init`.
The string is resolved case-insensitively against the `Plugin` enum by matching the enum constant
name, the full `pluginName`, or `pluginName` with the `android_` prefix stripped (e.g. `"REACT_NATIVE"`,
`"android_react_native"`, and `"react_native"` all resolve to the same value). Unknown values
return `RpcResponse.Error(422, ...)` listing the valid entries.

```
NATIVE("android_native")          FLUTTER("android_flutter")        CORDOVA("android_cordova")
UNITY("android_unity")            REACT_NATIVE("android_react_native")  XAMARIN("android_xamarin")
ADOBE_AIR("android_adobe_air")    ADOBE_MOBILE("android_adobe_mobile")  MPARTICLE("android_mparticle")
COCOS_2DX("android_cocos2dx")     NATIVE_SCRIPT("android_native_script") EXPO("android_expo")
UNREAL("android_unreal")          CAPACITOR("android_capacitor")     SEGMENT("android_segment")
```

If `setPluginInfo` is never called, the SDK defaults to `Plugin.NATIVE` with the SDK's own version
(`PluginDetailsProviderImpl.androidNative`, lazily initialized — see gotcha below). Every event
payload is tagged via `preparePluginDetails()` → `{"platform": pluginName, "version": ..., "extras": ...}`.
The `Plugin` enum is hardcoded in the SDK — adding a new platform requires an SDK release, not a
bridge-side change. Plugin info cannot be cleared within a process lifetime once set.

---

## 5. Unit testing bridge calls

These examples follow the same conventions as the existing suite
(`plugin_bridge/src/test/java/com/appsflyer/pluginbridge/`): JUnit 5, MockK, Given-When-Then.
`AppsFlyerLib` and `Context` are mocked; nothing hits the network or real SDK state. Note this
module mocks with plain `mockk(relaxed = true)` rather than the `com.appsflyer.testshelpers.relaxedMockk`
wrapper used in `sdk_main` — `plugin_bridge` doesn't depend on `sdk_main`'s test fixtures.

### 5.1 Common setup

```kotlin
package com.appsflyer.pluginbridge.handler

import android.content.Context
import com.appsflyer.AppsFlyerLib
import com.appsflyer.pluginbridge.model.RpcResponse
import com.appsflyer.pluginbridge.parser.JsonRpcRequestParser
import io.mockk.mockk
import org.junit.jupiter.api.Assertions.assertEquals
import org.junit.jupiter.api.Assertions.assertTrue
import org.junit.jupiter.api.BeforeEach
import org.junit.jupiter.api.Test

class MyPluginBridgeUsageTest {

    private lateinit var mockContext: Context
    private lateinit var mockAppsFlyerLib: AppsFlyerLib
    private lateinit var capturedEvents: MutableList<String>
    private lateinit var handler: AppsFlyerRpcHandler
    private lateinit var parser: JsonRpcRequestParser

    @BeforeEach
    fun setup() {
        mockContext = mockk(relaxed = true)
        mockAppsFlyerLib = mockk(relaxed = true)
        capturedEvents = mutableListOf()
        parser = JsonRpcRequestParser()

        handler = AppsFlyerRpcHandler(
            context = mockContext,
            pluginNotifier = { eventJson -> capturedEvents.add(eventJson) },
            appsFlyerLib = mockAppsFlyerLib
        )
    }
}
```

### 5.2 End-to-end JSON round trip (the exact contract a plugin's own bindings should exercise)

```kotlin
@Test
fun `setCustomerUserId end-to-end from raw JSON string`() {
    // Given
    val requestJson = """{"method":"setCustomerUserId","params":{"customerId":"user_12345"}}"""

    // When
    val response = handler.execute(requestJson)

    // Then
    assertTrue(response is RpcResponse.VoidSuccess)
    verify { mockAppsFlyerLib.setCustomerUserId("user_12345") }
}
```

### 5.3 Synchronous setter — verifying the SDK call and captured argument

```kotlin
@Test
fun `setConsentData builds AppsFlyerConsent and forwards it to the SDK`() {
    // Given
    val consentSlot = slot<AppsFlyerConsent>()
    every { mockAppsFlyerLib.setConsentData(capture(consentSlot)) } just Runs
    val request = SetConsentDataRequest(
        isUserSubjectToGDPR = true,
        hasConsentForDataUsage = true,
        hasConsentForAdsPersonalization = false,
        hasConsentForAdStorage = true
    )

    // When
    val response = handler.execute(request)

    // Then
    assertTrue(response is RpcResponse.VoidSuccess)
    assertTrue(consentSlot.captured.isUserSubjectToGDPR)
}
```

### 5.4 Blocking call (`awaitResponse: true`) — success path, no `Thread.sleep`

Capture the callback with a MockK `slot` and invoke it synchronously via `answers` so the
`CountDownLatch` inside `awaitCallback` counts down immediately — deterministic, no real threading.

```kotlin
@Test
fun `start with awaitResponse true returns success once SDK callback fires`() {
    // Given
    val listenerSlot = slot<AppsFlyerRequestListener>()
    every { mockAppsFlyerLib.start(capture(listenerSlot)) } answers {
        listenerSlot.captured.onSuccess()
    }
    val request = StartRequest(awaitResponse = true)

    // When
    val response = handler.execute(request)

    // Then
    assertTrue(response is RpcResponse.VoidSuccess)
}
```

### 5.5 Blocking call — timeout path

If the SDK never invokes the callback, the handler must surface a timeout error rather than
hang the plugin's calling thread indefinitely.

```kotlin
@Test
fun `logEvent returns timeout error when SDK callback never fires`() {
    // Given — mockAppsFlyerLib.logEvent(...) is relaxed and never invokes the listener
    val request = LogEventRequest(eventName = "af_purchase", eventValues = null, awaitResponse = true)

    // When
    val response = handler.execute(request)

    // Then
    assertTrue(response is RpcResponse.Error)
    val error = response as RpcResponse.Error
    assertEquals(RpcErrorCodes.INTERNAL_ERROR, error.code)
}
```
This test is slow by design (it waits out the real `LOG_EVENT_TIMEOUT_MILLIS` = 5s) — keep it in a
suite that's fine paying that cost, or reduce the timeout via a constructor seam if one gets added.

### 5.6 Malformed / unknown input

```kotlin
@Test
fun `unknown method name throws IllegalArgumentException from the parser`() {
    // Given
    val requestJson = """{"method":"notARealMethod","params":{}}"""

    // Then
    assertThrows<IllegalArgumentException> {
        parser.parse(requestJson)
    }
}

@Test
fun `setLogLevel with an unknown level returns a 422 error listing valid values`() {
    // Given
    val request = SetLogLevelRequest(logLevel = "SUPER_VERBOSE")

    // When
    val response = handler.execute(request)

    // Then
    assertTrue(response is RpcResponse.Error)
    assertEquals(RpcErrorCodes.INVALID_PARAMETERS, (response as RpcResponse.Error).code)
}
```

### 5.7 Persistent listener → event notifier shape

Verifies the exact JSON shape (`event`, `data`, `timestamp`, `origin`) a plugin's event channel
receives — this is what a plugin's own JS/Dart callback dispatch should be tested against.

```kotlin
@Test
fun `registerConversionListener forwards onConversionDataSuccess as a JSON event`() {
    // Given
    val listenerSlot = slot<AppsFlyerConversionListener>()
    every { mockAppsFlyerLib.registerConversionListener(capture(listenerSlot)) } just Runs
    handler.execute(RegisterConversionListenerRequest)

    // When
    listenerSlot.captured.onConversionDataSuccess(
        mapOf("media_source" to "facebook", "campaign" to "summer_sale")
    )

    // Then
    assertEquals(1, capturedEvents.size)
    val eventJson = JSONObject(capturedEvents[0])
    assertEquals("onConversionDataSuccess", eventJson.getString("event"))
    assertEquals("android", eventJson.getString("origin"))
    assertEquals("facebook", eventJson.getJSONObject("data").getString("media_source"))
}
```

For the full existing suite (571–1857 lines per file, covering every method above plus every
`require()` validation branch), see:
- `plugin_bridge/src/test/java/com/appsflyer/pluginbridge/handler/AppsFlyerRpcHandlerTest.kt`
- `plugin_bridge/src/test/java/com/appsflyer/pluginbridge/parser/JsonRpcRequestParserTest.kt`
- `plugin_bridge/src/test/java/com/appsflyer/pluginbridge/model/RpcRequestValidationTest.kt`

Note: there are also older, smaller duplicate test files directly under
`.../pluginbridge/` (`AppsFlyerRpcHandlerTest.kt`, `JsonRpcRequestParserTest.kt`,
`RpcIntegrationTest.kt`) covering the same classes — likely refactor leftovers, not an
intentionally maintained parallel suite. Prefer the versions under `handler/`, `parser/`, and
`model/` as the source of truth until that duplication is cleaned up.

---

## 6. Gotchas for implementers

- **Never expose PII getters through the bridge.** IC-174: a `getCustomerUserId` method was once
  added that cast the handler's `appsFlyerLib` field to the *internal* `AppsFlyerLibCore` class to
  read the raw CUID — bypassing the public API and leaking PII to any caller. It was removed.
  Any new bridge method must go through the public `AppsFlyerLib` surface only, and any request
  that would read back PII (CUID, device identifiers, etc.) should be rejected in review.
- **Don't eagerly construct objects that load DexGuard-protected classes.** IC-170: a `@JvmOverloads`
  default parameter that eagerly instantiated a collaborator triggered main-thread class loading of
  the DexGuard-protected `Plugin` enum, causing ANRs in `init()`. Fixed by making the plugin-info
  fallback a `by lazy { ... }` property instead of eager field init. Follow the same pattern for any
  new bridge-adjacent code that touches enums/classes guarded by DexGuard.
- **`unsubscribeForDeepLink` is a soft unsubscribe.** The SDK has no public unsubscribe API; the
  handler just drops its own reference. The original listener remains registered internally —
  don't assume no more deep link work happens SDK-side after calling this.
- **Blocking calls can stall the caller thread** for up to 5–10s if the underlying SDK callback
  never fires (`start`, `logEvent`, `validateAndLogInAppPurchase`, `generateInviteLink` with
  `awaitResponse: true`). Always invoke these off the plugin's main/UI thread.
- **Loose parameter validation.** `JsonRpcRequestParser` uses `optString`/`optBoolean` with
  defaults — malformed or missing params silently become defaults rather than raising an error.
  Validate params on the plugin side before sending.
- **No method versioning.** A breaking parameter change to an existing method name would silently
  apply new defaults to older plugin code calling the same method name. Coordinate SDK-side
  breaking changes with all plugin maintainers before shipping.
- **Persistent listeners leak if not unregistered.** `registerConversionListener`,
  `subscribeForDeepLink`, `registerSessionReadyListener` hold strong references and keep emitting
  events until the corresponding `unregister*` call is made (or, for deep link, until the local ref
  is dropped — see soft-unsubscribe note above).

