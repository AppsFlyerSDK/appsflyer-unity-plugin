---
title: Migration guide from v4
category: 600892a5042c550044d58e1b
parentDoc: 6358e561b49b560010d89e2e
order: 11
hidden: false
---

1. [Remove the old Plugin](#remove-the-old-plugin)
2. [Init the new Plugin](#init-the-new-plugin)
3. [Update deeplink logic](#update-deeplink-logic)
4. [Update code](#update-other-code)

:warning: There are breaking changes when migrating to Unity v5. This includes:
* New class names
* New android package name
* `com.appsflyer.GetDeepLinkingActivity` does not exist in Unity v5. This is no longer required for deeplinking
* unity-jar-resolver is used to import assets

# Remove the old plugin 

1. Remove all the items contained in `AppsFlyerUnityPlugin_v4.x.x.unitypackage`

Here is a list of all the filed included:

```
Assets/Plugins/AppsFlyer.cs
Assets/Plugins/AFInAppEvents.cs
Assets/Plugins/AppsFlyerTrackerCallbacks.cs
---
Assets/Plugins/Android/AppsFlyerAndroidPlugin.jar 
Assets/Plugins/Android/AF-Android-SDK.jar 
Assets/Plugins/Android/installreferrer-1.0.aar
---
Assets/Plugins/iOS/AppsFlyerAppController.mm
Assets/Plugins/iOS/AppsFlyerCrossPromotionHelper.h
Assets/Plugins/iOS/AppsFlyerDelegate.h
Assets/Plugins/iOS/AppsFlyerDelegate.mm
Assets/Plugins/iOS/AppsFlyerLinkGenerator.h
Assets/Plugins/iOS/AppsFlyerShareInviteHelper.h
Assets/Plugins/iOS/AppsFlyerTracker.h
Assets/Plugins/iOS/AppsFlyerWrapper.h
Assets/Plugins/iOS/AppsFlyerWrapper.mm
Assets/Plugins/iOS/libAppsFlyerLib.a

```

# Init the new plugin
    
1. Add the new .unitypackage, which can be found in the new plugin. 
    
2. There are two main options of initialization: 
   1. Remove all old init code and use the new .prefab object. 
   2. Update your existing init code.
   
## 1. remove all old init code 
To do this simpily remove the game object or all the appsflyer code in the game object where there sdk is being initalized.
Then follow the init guide for the new plugin.
    
## 2. Update old init code with new code

Replace old init code:

```c#
void Start () {
    AppsFlyer.setAppsFlyerKey("K2***********99");
    /* AppsFlyer.setIsDebug(true); */
#if UNITY_IOS
  AppsFlyer.setAppID("41******85");
  AppsFlyer.trackAppLaunch();
  AppsFlyer.getConversionData();
#elif UNITY_ANDROID
  AppsFlyer.setAppID ("com.appsflyer.test");
  AppsFlyer.init("K2**********99","AppsFlyerTrackerCallbacks");
#endif
}
```

With new init code:


```c#
using AppsFlyerSDK;

public class AppsFlyerObjectScript : MonoBehaviour , IAppsFlyerConversionData
{
    void Start()
    {
        /* AppsFlyer.setDebugLog(true); */
        AppsFlyer.init-sdk("devkey", "appID", this);
        AppsFlyer.startSDK();
    }
    
 // .....   
}
```
**Important**
If you are also implementing conversion data and/or deeplinking then you need to initialize the SDK with the `IAppsFlyerConversionData` interface.

## Update deeplink logic 
    
Unity v5 does not include `com.appsflyer.GetDeepLinkingActivity`. <br>This was used in Unity v4 as a workaround for deeplinking.<br>
If you are using this class for deeplinking, then make sure to remove the GetDeepLinkingActivity from the AndroidManifest.xml file. 

## Update other Code

Here is a list of all the old API, and the new API.

**API**

- [setAppsFlyerKey](#init-sdk)
- [trackAppLaunch](#init-sdk)
- [setAppID](#init-sdk)
- [getConversionData](#init-sdk)
- [init](#init-sdk)
- [loadConversionData](#init-sdk)
- [setCurrencyCode](#api-that-did-not-change)
- [setCustomerUserID](#api-that-did-not-change)
- [setAdditionalData](#api-that-did-not-change)
- [trackCrossPromoteImpression](#api-that-did-not-change)
- [setMinTimeBetweenSessions](#api-that-did-not-change)
- [setHost](#api-that-did-not-change)
- [setUserEmails](#api-that-did-not-change)
- [setResolveDeepLinkURLs](#api-that-did-not-change)
- [setOneLinkCustomDomain](#api-that-did-not-change)
- [trackRichEvent](#updated-core-api)
- [stopTracking](#updated-core-api)
- [setIsDebug](#api-that-did-not-change)
- [getAppsFlyerId](#api-that-did-not-change)
- [setDeviceTrackingDisabled](#updated-core-api)
- [setAppInviteOneLinkID](#api-that-did-not-change)
- [generateUserInviteLink](#updated-core-api)
- [trackAndOpenStore](#updated-core-api)
- [setIsSandbox](#ios-only-api)
- [registerUninstall](#ios-only-api)
- [setCollectIMEI](#android-only-api)
- [setCollectAndroidID](#android-only-api)
- [setImeiData](#android-only-api)
- [updateServerUninstallToken](#android-only-api)
- [setAndroidIdData](#android-only-api)
- [setPreinstallAttribution](#android-only-api)
- [validate-receipt (ios)](#validate-receipt)
- [validate-receipt (android)](#validate-receipt)
- [createValidateInAppListener](#validate-receipt)
- [handlePushNotification](#deprecated)
- [enableUninstallTracking](#deprecated)
- [handleOpenUrl](#deprecated)
- [getHost](#deprecated)
- [loadConversionData](#deprecated)
- [setGCMProjectNumber](#deprecated)
- [setShouldCollectDeviceName](#deprecated)


## Init SDK
```c#
// Old API's
AppsFlyer.setAppsFlyerKey(string key);
AppsFlyer.trackAppLaunch();
AppsFlyer.setAppID(string appleAppId);
AppsFlyer.getConversionData ();
AppsFlyer.init(string devKey);
AppsFlyer.init(string devKey, string callbackObject);
AppsFlyer.loadConversionData(string callbackObject);

// New API's
AppsFlyer.initSDK(string key, string app_id); // without deeplinking/conversion data
AppsFlyer.initSDK(string key, string app_id, MonoBehaviour gameObject); // with deeplinking/conversion data
AppsFlyer.startSDK();
```

## API that did not change
```c#
AppsFlyer.setCurrencyCode(string currencyCode);
AppsFlyer.setCustomerUserID(string customerUserID);
AppsFlyer.setAdditionalData(Dictionary<string, string> extraData);
AppsFlyer.trackCrossPromoteImpression(string appId, string campaign);
AppsFlyer.setMinTimeBetweenSessions(int seconds);
AppsFlyer.setHost(string hostPrefixName, string hostName);
AppsFlyer.setUserEmails(EmailCryptType cryptType, params string[] userEmails);
AppsFlyer.setResolveDeepLinkURLs(params string[] userEmails);
AppsFlyer.setOneLinkCustomDomain(params string[] domains);
AppsFlyer.setIsDebug(bool isDebug);
AppsFlyer.getAppsFlyerId();
AppsFlyer.setAppInviteOneLinkID(string oneLinkID);
```

## Updated core API
```c#
// old
AppsFlyer.trackRichEvent(string eventName, Dictionary<string, string> eventValues);
// new
AppsFlyer.sendEvent(string eventName, Dictionary<string, string> eventValues);

// old
AppsFlyer.stopTracking(bool isStopTracking);
// new
AppsFlyer.stopSDK(bool isStopTracking);
   
// old   
AppsFlyer.setDeviceTrackingDisabled(bool state);
// new   
AppsFlyer.anonymizeUser(true);

// old 
AppsFlyer.generateUserInviteLink(Dictionary<string,string> parameters, string callbackObject,string callbackMethod, string callbackFailedMethod);
// new
AppsFlyer.generateUserInviteLink(Dictionary<string, string> parameters, MonoBehaviour gameObject);
    
// old 
AppsFlyer.trackAndOpenStore(string promotedAppId, string campaign, Dictionary<string,string> customParams);
// new 
AppsFlyer.trackAndOpenStore(string appID, string campaign, Dictionary<string, string> userParams, MonoBehaviour gameObject);
```

## iOS Only API
```c#
// old
AppsFlyer.setIsSandbox(bool isSandbox);
// new
#if UNITY_IOS && !UNITY_EDITOR
        AppsFlyer.setUseReceiptValidationSandbox(true);
#endif

// old
AppsFlyer.registerUninstall(byte[] token);
// new
#if UNITY_IOS && !UNITY_EDITOR
        AppsFlyer.registerUninstall(token);
#endif

// old
AppsFlyer.handleOpenUrl(string url, string sourceApplication, string annotation);
// new
#if UNITY_IOS && !UNITY_EDITOR
        AppsFlyer.handleOpenUrl(string url, string sourceApplication, string annotation);
#endif
```


## Android Only API

```c#
// old
AppsFlyer.setCollectIMEI(bool shouldCollect);
// new 
#if UNITY_ANDROID && !UNITY_EDITOR
        AppsFlyer.setCollectIMEI(bool shouldCollect);
#endif

// old
AppsFlyer.setCollectAndroidID(bool shouldCollect);
//new
#if UNITY_ANDROID && !UNITY_EDITOR
        AppsFlyer.setCollectAndroidID(bool shouldCollect);
#endif

//old
AppsFlyer.setImeiData(string imeiData);
//new
#if UNITY_ANDROID && !UNITY_EDITOR
        AppsFlyer.setImeiData(string imeiData);
#endif

//old
AppsFlyer.updateServerUninstallToken(string token);
//new
#if UNITY_ANDROID && !UNITY_EDITOR
        AppsFlyer.updateServerUninstallToken(string token);
#endif

//old
AppsFlyer.setAndroidIdData(string androidIdData);
//new
#if UNITY_ANDROID && !UNITY_EDITOR
        AppAppsFlyersFlyerAndroid.setAndroidIdData("androidId");
#endif

//old
AppsFlyer.setPreinstallAttribution(string mediaSource, string campaign, string siteId);
//new
#if UNITY_ANDROID && !UNITY_EDITOR
        AppsFlyer.setPreinstallAttribution("mediaSource", "campaign", "siteId");
#endif

//old
AppsFlyer.handlePushNotification(Dictionary<string, string> payload);
//new
#if UNITY_ANDROID && !UNITY_EDITOR
        AppsFlyer.handlePushNotifications();
#endif
```

## Validate Receipt

```c#
// android old api
AppsFlyer.validate-receipt(string publicKey, string purchaseData, string signature, string price, string currency, Dictionary<string, string> extraParams);

 // iOS old api
AppsFlyer.validate-receipt(string productIdentifier, string price, string currency, string transactionId, Dictionary<string, string> additionalParametes);
AppsFlyer.createValidateInAppListener(string aObject, string callbackMethod, string callbackFailedMethod);  

// android new
#if UNITY_ANDROID && !UNITY_EDITOR
        AppsFlyer.validateAndSendInAppPurchase(
        "publicKey", 
        "signature", 
        "purchaseData", 
        "price", 
        "currency", 
        null, 
        this);
#endif

// ios new 
#if UNITY_IOS && !UNITY_EDITOR
        AppsFlyer.validateAndSendInAppPurchase(
        "productIdentifier", 
        "price", 
        "currency", 
        "tranactionId", 
        null, 
        this);
#endif
```

## Deprecated
    
```c#
//@Deprecated
AppsFlyer.enableUninstallTracking(string senderId);
AppsFlyer.getHost();
AppsFlyer.loadConversionData(string callbackObject, string callbackMethod, string callbackFailedMethod);
AppsFlyer.setGCMProjectNumber(string googleGCMNumber);
AppsFlyer.setShouldCollectDeviceName(bool shouldCollectDeviceName);
```


