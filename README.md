<img src="https://massets.appsflyer.com/wp-content/uploads/2018/06/20092440/static-ziv_1TP.png"  width="400" >

# appsflyer-unity-plugin

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://opensource.org/licenses/MIT)
[![GitHub tag](https://img.shields.io/github/v/release/AppsFlyerSDK/appsflyer-unity-plugin)](https://img.shields.io/github/v/release/AppsFlyerSDK/appsflyer-unity-plugin)
[![Unit tests](https://github.com/AppsFlyerSDK/appsflyer-unity-plugin/actions/workflows/main.yml/badge.svg)](https://github.com/AppsFlyerSDK/appsflyer-unity-plugin/actions/workflows/main.yml)
[![check packages](https://github.com/af-margot/appsflyer-unity-plugin-beta/actions/workflows/checksums_files.yml/badge.svg)](https://github.com/af-margot/appsflyer-unity-plugin-beta/actions/workflows/checksums_files.yml)


🛠 In order for us to provide optimal support, we would kindly ask you to submit any issues to support@appsflyer.com

> *When submitting an issue please specify your AppsFlyer sign-up (account) email , your app ID , production steps, logs, code snippets and any additional relevant information.*

## 📖 The Unity documentation also be found [here](https://dev.appsflyer.com/hc/docs/unity-plugin)

### <a id="plugin-build-for"> This plugin is built for

- Android AppsFlyer SDK v6.17.0
- Android Purchase Connector 2.1.0
- iOS AppsFlyer SDK v6.17.1
- iOS Purchase Connector 6.17.1

---  
## <a id="new-in-6171">     🎉 New in 6.17.1 - Purchase Connector Integration 
- Starting from version 6.17.1, the **Purchase Connector is now integrated directly into the main AppsFlyer Unity plugin**. You no longer need to download, import, or maintain a separate Purchase Connector package.
- If you were previously using the standalone Purchase Connector from a separate repository, simply remove any references to `using AppsFlyerConnector;` from your codebase, as its functionality is now included in the main plugin under the `AppsFlyerSDK` namespace.
- The Purchase Connector now supports **StoreKit 2** for iOS 15+ alongside the existing StoreKit 1 support.
- For detailed migration instructions and new features, see our [Purchase Connector documentation](/docs/purchase-connector.md).
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



