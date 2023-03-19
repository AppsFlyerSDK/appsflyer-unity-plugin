---
title: Test Integration
category: 5f9705393c689a065c409b23
parentDoc: 6370c9e2441a4504d6bca3bd
order: 2
hidden: false
---

You are can test your integration for the following OS:

- [Testing for iOS/Android](#testing-for-iosandroid)
- [Testing for Windows](#testing-for-windows)

## Testing for iOS/Android

In order to test the plugin, you need to build an iOS/Android app. Then you can follow these guides: 
- [Marketers](https://support.appsflyer.com/hc/en-us/articles/360001559405-Test-mobile-SDK-integration-with-the-app#introduction).
- [Android](https://dev.appsflyer.com/hc/docs/testing-android)
- [iOS](https://dev.appsflyer.com/hc/docs/testing-ios)

To enable the debug logs, set the following API to true:
```c#
AppsFlyer.setIsDebug(true);
```

---

## Testing for Windows

In order to test the plugin, you need to build your UWP app.
To enable the debug logs, please uncomment the following line in [AppsFlyerWindows.cs](https://github.com/AppsFlyerSDK/appsflyer-unity-plugin/blob/d0f1c05d17dc4e400609ca880f5079c31fdee73e/Assets/AppsFlyer/Windows/AppsFlyerWindows.cs#L1) file.

```c#
#define AFSDK_WIN_DEBUG
```

After running the app, you will be able to find the logs in `%USERPROFILE%\AppData\Local\Packages<productname>\TempState\UnityPlayer.log`

