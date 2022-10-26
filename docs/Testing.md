---
title: Test Integration
category: 600892a5042c550044d58e1b
parentDoc: 6358e561b49b560010d89e2e
order: 3
hidden: false
---

# Testing 

- [Testing for iOS/Android](#iOSAndroid)
- [Testing for Windows](#uwp)

## <a id="iOSAndroid"> Testing for iOS/Android

In order to test the plugin, you need to build an iOS/Android app. And then you can follow this [guide](#https://support.appsflyer.com/hc/en-us/articles/360001559405-Test-mobile-SDK-integration-with-the-app#introduction)

To enable the debug logs, set the following API to true:
```c#
AppsFlyer.setIsDebug(true);
```


---

## <a id="uwp"> Testing for Windows

In order to test the plugin, you need to build your UWP app.
To enable the debug logs, please uncomment the following line in [AppsFlyerWindows.cs](Assets/AppsFlyer/Windows/AppsFlyerWindows.cs) file

```c#
#define AFSDK_WIN_DEBUG
```

After running the app, you will be able to find the logs in `%USERPROFILE%\AppData\Local\Packages<productname>\TempState\UnityPlayer.log`

