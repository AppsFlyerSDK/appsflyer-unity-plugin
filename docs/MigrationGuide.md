# Unity migration guide

1. [Remove the old Plugin](#removeOldPlugin)
2. [Init the new Plugin](#initnewplugin)
3. [Update code](#updateoldcode)


## <a id="removeOldPlugin"> Remove the old plugin 

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

2. Add the new .unitypackage, which can be found in the new plugin. 

## <a id="initnewplugin"> Init New Plugin
    
   There are two main options here: 
   1. Remove all old init code and use the new .prefab object. 
   2. Update your existing init code.
   
#### 1. remove all old init code 
To do this simpily remove the game object or all the appsflyer code in the game object where there sdk is being initalized.
Then follow the init guide for the new plugin.
    
#### 2. Update old init code with new code

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
        AppsFlyer.initSDK("devkey", "appID", this);
        AppsFlyer.startSDK();
    }
    
 // .....   
}
```
**Important**
If you are also implementing conversion data and/or deeplinking then you need to initalize the SDK with the `IAppsFlyerConversionData` interface.


## <a id="updateoldcode"> update other Code

Here is list of all the old API, and the new API.

API

- [setAppsFlyerKey](#initSDK)
- [trackAppLaunch](#initSDK)
- [setAppID](#initSDK)
- [getConversionData](#initSDK)
- [init](#initSDK)
- [loadConversionData](#initSDK)
- [setCurrencyCode](#sameapi)
- [setCustomerUserID](#sameapi)
- [setAdditionalData](#sameapi)
- [trackCrossPromoteImpression](#sameapi)
- [setMinTimeBetweenSessions](#sameapi)
- [setHost](#sameapi)
- [setUserEmails](#sameapi)
- [setResolveDeepLinkURLs](#sameapi)
- [setOneLinkCustomDomain](#sameapi)
- [trackRichEvent](#codeapi)
- [stopTracking](#codeapi)
- [setIsDebug](#sameapi)
- [getAppsFlyerId](#sameapi)
- [setDeviceTrackingDisabled](#codeapi)
- [setAppInviteOneLinkID](#sameapi)
- [generateUserInviteLink](#codeapi)
- [trackAndOpenStore](#codeapi)
- [setIsSandbox](#iosOnlyApi)
- [registerUninstall](#iosOnlyApi)
- [setCollectIMEI](#androidOnlyApi)
- [setCollectAndroidID](#androidOnlyApi)
- [setImeiData](#androidOnlyApi)
- [updateServerUninstallToken](#androidOnlyApi)
- [setAndroidIdData](#androidOnlyApi)
- [setPreinstallAttribution](#androidOnlyApi)
- [validateReceipt (ios)](#validateReceipt)
- [validateReceipt (android)](#validateReceipt)
- [createValidateInAppListener](#validateReceipt)
- [handlePushNotification](#deprecated)
- [enableUninstallTracking](#deprecated)
- [handleOpenUrl](#deprecated)
- [getHost](#deprecated)
- [loadConversionData](#deprecated)
- [setGCMProjectNumber](#deprecated)
- [setShouldCollectDeviceName](#deprecated)


#### <a id="initSDK"> Init SDK

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

#### <a id="sameapi"> API that stayed the same

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

#### <a id="codeapi"> Updated core API
    
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


#### <a id="iosOnlyApi"> iOS Only API
```c#
// old
AppsFlyer.setIsSandbox(bool isSandbox);
// new
#if UNITY_IOS && !UNITY_EDITOR
        AppsFlyeriOS.setUseReceiptValidationSandbox(true);
#endif

// old
AppsFlyer.registerUninstall(byte[] token);
// new
#if UNITY_IOS && !UNITY_EDITOR
        AppsFlyeriOS.registerUninstall(token);
#endif

```


#### <a id="androidOnlyApi"> Android Only API

```c#
// old
AppsFlyer.setCollectIMEI(bool shouldCollect);
// new 
#if UNITY_ANDROID && !UNITY_EDITOR
        AppsFlyerAndroid.setCollectIMEI(bool shouldCollect);
#endif

// old
AppsFlyer.setCollectAndroidID(bool shouldCollect);
//new
#if UNITY_ANDROID && !UNITY_EDITOR
        AppsFlyerAndroid.setCollectAndroidID(bool shouldCollect);
#endif

//old
AppsFlyer.setImeiData(string imeiData);
//new
#if UNITY_ANDROID && !UNITY_EDITOR
        AppsFlyerAndroid.setImeiData(string imeiData);
#endif

//old
AppsFlyer.updateServerUninstallToken(string token);
//new
#if UNITY_ANDROID && !UNITY_EDITOR
        AppsFlyerAndroid.updateServerUninstallToken(string token);
#endif

//old
AppsFlyer.setAndroidIdData(string androidIdData);
//new
#if UNITY_ANDROID && !UNITY_EDITOR
        AppsFlyerAndroid.setAndroidIdData("androidId");
#endif

//old
AppsFlyer.setPreinstallAttribution(string mediaSource, string campaign, string siteId);
//new
#if UNITY_ANDROID && !UNITY_EDITOR
        AppsFlyerAndroid.setPreinstallAttribution("mediaSource", "campaign", "siteId");
#endif

//old
AppsFlyer.handlePushNotification(Dictionary<string, string> payload);
//new
#if UNITY_ANDROID && !UNITY_EDITOR
        AppsFlyerAndroid.handlePushNotifications();
#endif


```


#### <a id="validateReceipt"> Validate Receipt

```c#
// android old api
AppsFlyer.validateReceipt(string publicKey, string purchaseData, string signature, string price, string currency, Dictionary<string, string> extraParams);

 // iOS old api
AppsFlyer.validateReceipt(string productIdentifier, string price, string currency, string transactionId, Dictionary<string, string> additionalParametes);
AppsFlyer.createValidateInAppListener(string aObject, string callbackMethod, string callbackFailedMethod);  

// android new
#if UNITY_ANDROID && !UNITY_EDITOR
        AppsFlyerAndroid.validateAndSendInAppPurchase(
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
        AppsFlyeriOS.validateAndSendInAppPurchase(
        "productIdentifier", 
        "price", 
        "currency", 
        "tranactionId", 
        null, 
        this);
#endif

```


#### <a id="deprecated"> Deprecated
    
```c#
//@Deprecated
AppsFlyer.enableUninstallTracking(string senderId);
AppsFlyer.handleOpenUrl(string url, string sourceApplication, string annotation);
AppsFlyer.getHost();
AppsFlyer.loadConversionData(string callbackObject, string callbackMethod, string callbackFailedMethod);
AppsFlyer.setGCMProjectNumber(string googleGCMNumber);
AppsFlyer.setShouldCollectDeviceName(bool shouldCollectDeviceName);
```


