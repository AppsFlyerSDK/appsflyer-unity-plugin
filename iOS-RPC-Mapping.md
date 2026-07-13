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

For full JSON payloads, parameter tables, and the plugin integration walkthrough (React Native / Flutter examples, step-by-step new-plugin guide, event schemas), see [`README.md`](../README.md).
