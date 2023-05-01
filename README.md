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

- Android AppsFlyer SDK **v6.10.3** 
- iOS AppsFlyer SDK **v6.10.1**

---
### <a id="init-sdk-deeplink"> AD_ID permission for Android

In v6.8.0 of the AppsFlyer SDK, we added the normal permission com.google.android.gms.permission.AD_ID to the SDK's AndroidManifest, to allow the SDK to collect the Android Advertising ID on apps targeting API 33. If your app is targeting children, you need to revoke this permission to comply with Google's Data policy. You can read more about it [here](https://dev.appsflyer.com/hc/docs/install-android-sdk#the-ad_id-permission).

---  
## <a id="breaking-changes">     ❗❗ Breaking changes when updating to 6.6.0 ❗❗
- Starting version 6.6.0, there is no more need to differentiate between iOS and Android APIs. All APIs must be called with `AppsFlyer` class (even if the API is only iOS or Android).
- Please take into consideration that since version 6.6.0, most of the APIs require `initSDK` to be called prior to using them, only a handful of APIs will properly work when called prior to initialization: `setIsDebug`, `setCurrencyCode`, `setHost`, `disableSKAdNetwork`.

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

## <a id="breaking-changes">     ❗❗ Breaking changes when updating to 6.3.0 ❗❗

- 6.3.0 supports Universal Windows Platform. As part of this update, the AppsFlyerObjectScript changes to include the app_id for your UWP app. If you made changes to this file, please merge them with the new AppsFlyerObjectScript.
Please also note that you can leave the uwp app id field empty. 

- From version `6.3.0`, we use `xcframework` for iOS platform, then you need to use cocoapods version >= 1.10

## <a id="migration"> ⏩ Migration 
  
Migrating from the old plugin? (version V4) <br/>
View the migration docs [here](/docs/MigrationGuide.md).

⚠️ There are **breaking** changes when migrating to `Unity v5`. This includes new API, different class/package names, and the removal of `com.appsflyer.GetDeepLinkingActivity`.

 ---
## <a id="plugin-build-for"> 🚀 Getting Started
- [Installation](/docs/Installation.md)
- [Integration](/docs/BasicIntegration.md)
- [Test integration](/docs/Testing.md)
- [In-app events](/docs/InAppEvents.md)
- [Uninstall measurement](/docs/UninstallMeasurement.md)
## <a id="plugin-build-for"> 🌟 Deep Linking
- [Integration](/docs/DeepLinkIntegrate.md)
- [Unified Deep Link (UDL)](/docs/UnifiedDeepLink.md)
- [User invite](/docs/UserInvite.md)

----  
### [API refrence](/docs/API.md)    
### [Troubleshooting](/docs/Troubleshooting.md)



