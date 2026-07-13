# RPC Implementation Coverage

Tracks every SDK method routed through the JSON-RPC layer in the Unity plugin.
Source of truth: `Assets/AppsFlyer/AppsFlyer.cs`.

- **Cross-platform** — RPC call fires on both iOS and Android.
- **iOS only** — method is iOS-specific (ATT, SKAdNetwork, IDFV, receipt sandbox, etc.).
- **Android only** — method is implemented in `plugin_bridge` but not yet wired in C# (gap list).

---

## Cross-Platform Methods

Implemented in both the iOS RPC handler and the Android `plugin_bridge`.

| C# method (AppsFlyer.cs) | RPC method name | Notes |
|---|---|---|
| `initSDK` | `init` | Fires after legacy native `initSDK` call |
| `startSDK` | `start` | |
| `sendEvent` | `logEvent` | |
| `stopTracking` | `setStopped` | |
| `setIsDebug` | `isDebug` | |
| `setCustomerUserId` | `setCustomerUserId` | |
| `setAppInviteOneLinkID` | `setAppInviteOneLinkID` | |
| `setDeepLinkTimeout` | `setDeepLinkTimeout` | |
| `setAdditionalData` | `setAdditionalData` | |
| `setResolveDeepLinkURLs` | `setResolveDeepLinkURLs` | |
| `setOneLinkCustomDomain` | `setOneLinkCustomDomains` | |
| `setCurrencyCode` | `setCurrencyCode` | |
| `setConsentData` | `setConsentData` | |
| `logAdRevenue` | `logAdRevenue` | |
| `logLocation` | `logLocation` | |
| `anonymizeUser` | `setAnonymizeUser` | |
| `enableTCFDataCollection` | `enableTCFDataCollection` | |
| `getAppsFlyerUID` | `getAppsFlyerUID` | Synchronous `Execute` (returns UID) |
| `setMinTimeBetweenSessions` | `setMinTimeBetweenSessions` | |
| `setHost` | `setHost` | |
| `setUserEmails` | `setUserEmails` | |
| `setPhoneNumber` | `setPhoneNumber` | |
| `setSharingFilterForAllPartners` | `setSharingFilterForPartners` | Maps to same RPC method |
| `setSharingFilter` | `setSharingFilterForPartners` | Maps to same RPC method |
| `setSharingFilterForPartners` | `setSharingFilterForPartners` | |
| `getConversionData` | `registerConversionListener` | |
| `setShouldCollectDeviceName` | `setShouldCollectDeviceName` | |
| `attributeAndOpenStore` | `logAndOpenStore` | |
| `recordCrossPromoteImpression` | `logCrossPromoteImpression` | |
| `validateAndSendInAppPurchase` (V2) | `validateAndLogInAppPurchaseV2` | Core SDK validation — not PurchaseConnector |
| `setCurrentDeviceLanguage` | `setCurrentDeviceLanguage` | |
| `generateInviteLink` | `generateInviteLink` | |
| `addPushNotificationDeepLinkPath` | `addPushNotificationDeepLinkPath` | |
| `setDisableAdvertisingIdentifiers` | `setDisableAdvertisingIdentifier` | |
| `subscribeForDeepLink` | `registerDeeplinkListener` | |
| `setPartnerData` | `setPartnerData` | |
| `registerSessionReadyListener` | `registerSessionReadyListener` | Android fires fire-and-forget; iOS fires with `callbackObjectName` param |
| `isSessionReady` (internal check) | `isSessionReady` | Synchronous `Execute`; used in Android session-ready race fix |
| `unregisterSessionReadyListener` | `unregisterSessionReadyListener` | |

---

## iOS-Only Methods

These RPC calls only make sense on iOS. The Android `plugin_bridge` does not implement them.

| C# method (AppsFlyer.cs) | RPC method name | iOS feature |
|---|---|---|
| `setDisableCollectASA` | `setDisableCollectASA` | Apple Search Ads attribution |
| `setDisableAppleAdsAttribution` | `setDisableAppleAdsAttribution` | Apple Ads attribution |
| `setUseReceiptValidationSandbox` | `setUseReceiptValidationSandbox` | StoreKit receipt sandbox |
| `setUseUninstallSandbox` | `setUseUninstallSandbox` | Uninstall tracking sandbox (APNS) |
| `handleOpenUrl` | `handleOpenUrl` | URL scheme / Universal Link handoff |
| `registerUninstallToken` | `registerUninstall` | APNS device token for uninstall tracking |
| `waitForATTUserAuthorizationWithTimeoutInterval` | `waitForATT` | App Tracking Transparency |
| `setDisableSKAdNetwork` | `setDisableSKAdNetwork` | SKAdNetwork |
| `setDisableIDFVCollection` | `setDisableIDFVCollection` | IDFV (Identifier for Vendor) |

---

## Android-Only Methods (plugin_bridge implemented, not yet wired in C#)

The Android `plugin_bridge` (`JsonRpcRequestParser.kt`) handles these methods but no C# call in `AppsFlyer.cs` routes to them via RPC yet. These are gaps for future wiring.

| RPC method name | Notes |
|---|---|
| `appendParametersToDeepLinkingURL` | Append query params to deep link URL |
| `clearUserPii` | Clear all PII fields |
| `disableAppSetId` | Disable Android App Set ID collection |
| `enableFacebookDeferredApplinks` | Facebook DDL integration |
| `getAttributionId` | Synchronous — returns Meta attribution ID |
| `getHostName` / `getHostPrefix` | Synchronous getters |
| `getOutOfStore` / `setOutOfStore` | Out-of-store attribution source |
| `getSdkVersion` | Synchronous — returns SDK version string |
| `isPreInstalledApp` | Synchronous — preinstall detection |
| `isStopped` | Synchronous — SDK stopped state |
| `logInvite` | Log invite event |
| `logSession` | Manual session logging |
| `onPause` | Lifecycle signal |
| `performDeepLinking` | Manual deep link re-trigger |
| `sendPushNotificationData` | Forward push payload to SDK |
| `setAppId` | Override app ID |
| `setCollectAndroidID` | Android ID collection toggle |
| `setDisableNetworkData` | Disable network data collection |
| `setInstallId` | Set custom install ID |
| `setIsUpdate` | Mark app as update vs fresh install |
| `setLogLevel` | SDK log verbosity |
| `setPluginInfo` | Plugin metadata (set automatically by wrapper) |
| `setPreinstallAttribution` | Preinstall attribution params |
| `setUserFbLoginId` / `setUserFirstName` / `setUserLastName` | Additional user identifiers |
| `subscribeForDeepLink` / `unsubscribeForDeepLink` | Deep link listener toggle |
| `unregisterConversionListener` | Remove conversion data listener |
| `updateServerUninstallToken` | Update FCM token for uninstall tracking |
| `validateAndLogInAppPurchase` | Android in-app purchase validation (via `plugin_bridge`, no PurchaseConnector) |

---

## Not in RPC (PurchaseConnector — intentionally excluded)

These methods go through their own native path and are **never** routed via RPC.

| C# method | Platform | Native path |
|---|---|---|
| `initPurchaseConnector` | iOS | P/Invoke `_initPurchaseConnector` |
| `startObservingTransactions` | iOS | P/Invoke `_startObservingTransactions` |
| `stopObservingTransactions` | iOS | P/Invoke `_stopObservingTransactions` |
| `setIsSandbox` | iOS | P/Invoke `_setIsSandbox` |
| `setAutoLogPurchaseRevenue` | iOS | P/Invoke `_setAutoLogPurchaseRevenue` |
| `setPurchaseRevenueDelegate` | iOS | P/Invoke `_setPurchaseRevenueDelegate` |
| `setPurchaseRevenueDataSource` | iOS | P/Invoke `_setPurchaseRevenueDataSource` |
| `setStoreKitVersion` | iOS | P/Invoke `_setStoreKitVersion` |
| `logConsumableTransaction` | iOS | P/Invoke `_logConsumableTransaction` |
| `initPurchaseConnector` | Android | `AppsFlyerAndroidWrapper` (stubbed in dev SDK) |
| `build` | Android | `AppsFlyerAndroidWrapper` (stubbed in dev SDK) |
| `setIsSandbox` | Android | `AppsFlyerAndroidWrapper` (stubbed in dev SDK) |
| `setAutoLogSubscriptions` | Android | `AppsFlyerAndroidWrapper` (stubbed in dev SDK) |
| `setAutoLogInApps` | Android | `AppsFlyerAndroidWrapper` (stubbed in dev SDK) |
| `setPurchaseRevenueValidationListeners` | Android | `AppsFlyerAndroidWrapper` (stubbed in dev SDK) |
| `startObservingTransactions` | Android | `AppsFlyerAndroidWrapper` (stubbed in dev SDK) |
| `stopObservingTransactions` | Android | `AppsFlyerAndroidWrapper` (stubbed in dev SDK) |

