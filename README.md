<img src="https://massets.appsflyer.com/wp-content/uploads/2018/06/20092440/static-ziv_1TP.png"  width="400" >

# appsflyer-unity-plugin

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://opensource.org/licenses/MIT)
[![GitHub tag](https://img.shields.io/github/v/release/AppsFlyerSDK/appsflyer-unity-plugin)](https://img.shields.io/github/v/release/AppsFlyerSDK/appsflyer-unity-plugin)
[![RC Release Pipeline](https://github.com/AppsFlyerSDK/appsflyer-unity-plugin/actions/workflows/rc-release.yml/badge.svg)](https://github.com/AppsFlyerSDK/appsflyer-unity-plugin/actions/workflows/rc-release.yml)
[![check packages](https://github.com/af-margot/appsflyer-unity-plugin-beta/actions/workflows/checksums_files.yml/badge.svg)](https://github.com/af-margot/appsflyer-unity-plugin-beta/actions/workflows/checksums_files.yml)


🛠 In order for us to provide optimal support, please contact AppsFlyer support through the Customer Assistant Chatbot for assistance with troubleshooting issues or product guidance. </br>
To do so, please follow [this article](https://support.appsflyer.com/hc/en-us/articles/23583984402193-Using-the-Customer-Assistant-Chatbot)


## 📖 The Unity documentation also be found [here](https://dev.appsflyer.com/hc/docs/unity-plugin)

### <a id="plugin-build-for"> This plugin is built for

- Android AppsFlyer SDK v6.18.1
- Android Purchase Connector 2.2.0
- iOS AppsFlyer SDK v6.18.1
- iOS Purchase Connector 6.18.2
---
## 📌 Google Play Billing Library 8 (6.18.0+)

From **v6.18.0** onward, the Unity plugin ships as a **single release line** with **Google Play Billing Library 8.0.0** on Android (Purchase Connector included in the main package). There is no separate v7 plugin variant for new 6.18.x releases.

- **Unity IAP:** Use **Unity IAP (`com.unity.purchasing`) 5.0.0 or newer** (recommended: latest 5.x). Unity IAP 4.x does **not** include Billing v8.
- **Migration:** Apps still on Billing v7 must migrate to Billing v8 APIs before upgrading to 6.18.0+.

### Still on Billing Library v7?

Use the last **6.17.x** dual-line release: **`v6.17.90`** (Billing v7) or **`v6.17.91`** (Billing v8). See [Installation](/docs/Installation.md) for Maven wrapper coordinates on 6.17.x.

---  
## <a id="new-in-6171">     🎉 New in 6.17.1 - Purchase Connector Integration 
- Starting from version 6.17.1, the **Purchase Connector is now integrated directly into the main AppsFlyer Unity plugin**. You no longer need to download, import, or maintain a separate Purchase Connector package.
- If you were previously using the standalone Purchase Connector from a separate repository, simply remove any references to `using AppsFlyerConnector;` from your codebase, as its functionality is now included in the main plugin under the `AppsFlyerSDK` namespace.
- The Purchase Connector now supports **StoreKit 2** for iOS 15+ alongside the existing StoreKit 1 support.
- For detailed migration instructions and new features, see our [Purchase Connector documentation](/docs/purchase-connector.md).

---
## <a id="breaking-changes-6175">     ❗❗ Breaking changes when updating to 6.17.5 ❗❗
- **In-App Purchase Validation API Changes**: The `validateAndSendInAppPurchase` method signatures have been updated for better type safety and cleaner code.
- **V2 Methods (Recommended)**: New overloads using structured data classes (`AFPurchaseDetailsAndroid`/`AFSDKPurchaseDetailsIOS`) are now the recommended approach.
- **Legacy Methods (Deprecated)**: The old string-based parameter methods are now deprecated but maintained for backward compatibility.
- **Migration Required**: If you're using the old `validateAndSendInAppPurchase` methods, consider migrating to the V2 versions for better maintainability.
- For detailed API documentation and migration examples, see our [API reference](/docs/API.md).

---  
## <a id="breaking-changes">     ❗❗ Breaking changes when updating to 6.12.20 ❗❗
- Starting from version 6.12.20, we have changed the way we distribute the plugin via UPM. The UPM branches will no longer hold a dependency for `com.google.external-dependency-manager` as it was proved to cause issues in different versions of Unity - to be clear, this dependency is still required to utilize our plugin, we just can't distribute the plugin with it in UPM form as the EDM4U dependency is [not available via UPM for quite a while already](https://github.com/googlesamples/unity-jar-resolver/issues/434#issuecomment-827028132) but is still available via `.unitypackage` or `.tgz` files, if you use UPM to fetch our plugin - [please download a suitable version of EDM4U](https://github.com/googlesamples/unity-jar-resolver) so you will be able to resolve the dependencies, or opt for [an installation without EDM4U](https://github.com/AppsFlyerSDK/appsflyer-unity-plugin/blob/master/docs/Installation.md#installation-without-unity-jar-resolver).
---  

## <a id="breaking-changes">     ❗❗ Breaking changes when updating to 6.6.0 ❗❗
- Starting version 6.6.0, there is no more need to differentiate between iOS and Android APIs. All APIs must be called with `AppsFlyer` class (even if the API is only iOS or Android).
- Please take into consideration that since version 6.6.0, most of the APIs require `initSDK` to be called prior to using them, and since version 6.10.10 only a handful of APIs will properly work when called prior to initialization: `setIsDebug`, `setCurrencyCode`, `setHost`, `disableSKAdNetwork`.

Example:

Before 6.6.0:
```c#
#if UNITY_IOS && !UNITY_EDITOR
    AppsFlyeriOS.waitForATTUserAuthorizationWithTimeoutInterval(60);
#endif
```
---

After 6.6.0:
```c#
#if UNITY_IOS && !UNITY_EDITOR
    AppsFlyer.waitForATTUserAuthorizationWithTimeoutInterval(60);
#endif
```
---

## <a id="strict-mode"> Strict Mode
The plugin supports a Strict Mode which completely removes the IDFA collection functionality and AdSupport framework dependencies.
Use the Strict Mode when developing apps for kids, for example.
More information about how to install the Strict Mode is available [here](/docs/Installation.md).


### <a id="init-sdk-deeplink"> AD_ID permission for Android

In v6.8.0 of the AppsFlyer SDK, we added the normal permission com.google.android.gms.permission.AD_ID to the SDK's AndroidManifest, to allow the SDK to collect the Android Advertising ID on apps targeting API 33. If your app is targeting children, you need to revoke this permission to comply with Google's Data policy. You can read more about it [here](https://dev.appsflyer.com/hc/docs/install-android-sdk#the-ad_id-permission).



 ---
## <a id="plugin-build-for"> 🚀 Getting Started
- [Installation](/docs/Installation.md)
- [Integration](/docs/BasicIntegration.md)
- [Test integration](/docs/Testing.md)
- [In-app events](/docs/InAppEvents.md)
- [Send Consent for DMA Compliance](/docs/DMAConsent.md)
- [Uninstall measurement](/docs/UninstallMeasurement.md)
## <a id="plugin-build-for"> 💰 Purchase Connector
- [Purchase Connector (ROI360)](/docs/purchase-connector.md)
## <a id="plugin-build-for"> 🔗 Deep Linking
- [Integration](/docs/DeepLinkIntegrate.md)
- [Unified Deep Link (UDL)](/docs/UnifiedDeepLink.md)
- [User invite](/docs/UserInvite.md)
## <a id="plugin-build-for"> 🧪 Sample App
- [ButterFlyer](https://github.com/AppsFlyerSDK/appsflyer-unity-sample-app)

----  
### [API reference](/docs/API.md)    
### [Troubleshooting](/docs/Troubleshooting.md)



