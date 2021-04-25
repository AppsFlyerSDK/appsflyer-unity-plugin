# Guides

<img src="https://massets.appsflyer.com/wp-content/uploads/2016/06/26122512/banner-img-ziv.png"  width="300">

## Table of content

- [Init SDK](#init-sdk)
    - [Init SDK without deeplinking](#init-sdk-deeplink-basic)
    - [Init SDK with deeplinking callbacks](#init-sdk-deeplink)
- [Deep Linking](#deeplinking)
    - [Deferred Deep Linking (Get Conversion Data)](#conversionData)
    - [Direct Deep Linking](#handle-deeplinking)
    - [Android Deepling](#android-deeplink)
        - [URI Scheme](#uri-scheme)
        - [App Links](#app-links)
    - [iOS Deeplink Setup](#ios-deeplink)
- [Uninstall](#track-app-uninstalls)
    - [iOS Uninstall Setup](#track-app-uninstalls-ios)
    - [Android Uninstall Setup](#track-app-uninstalls-android)
- [User invite attribution](#UserInviteAttribution)
- [In-app purchase validation](#InAppPurchaseValidation)
- [Collect IDFA with ATTrackingManager](#collect-idfa)
- [Request Listeners](#request-listeners)

## <a id="init-sdk"> Init SDK 

### <a id="init-sdk-deeplink-basic"> Init SDK without deeplinking

```c#
using AppsFlyerSDK;

public class AppsFlyerObjectScript : MonoBehaviour
{
  void Start()
  {
    /* AppsFlyer.setDebugLog(true); */
    AppsFlyer.initSDK("devkey", "appID");
    AppsFlyer.startSDK();
  }
}
```

### <a id="init-sdk-deeplink"> Init SDK with deeplinking callbacks

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

    public void onConversionDataSuccess(string conversionData)
    {
        AppsFlyer.AFLog("onConversionDataSuccess", conversionData);
        Dictionary<string, object> conversionDataDictionary = AppsFlyer.CallbackStringToDictionary(conversionData);
        // add deferred deeplink logic here
    }

    public void onConversionDataFail(string error)
    {
        AppsFlyer.AFLog("onConversionDataFail", error);
    }

    public void onAppOpenAttribution(string attributionData)
    {
        AppsFlyer.AFLog("onAppOpenAttribution", attributionData);
        Dictionary<string, object> attributionDataDictionary = AppsFlyer.CallbackStringToDictionary(attributionData);
        // add direct deeplink logic here
    }

    public void onAppOpenAttributionFailure(string error)
    {
        AppsFlyer.AFLog("onAppOpenAttributionFailure", error);
    }
}
```


##  <a id="deeplinking"> Deep Linking
    
![alt text](https://massets.appsflyer.com/wp-content/uploads/2018/03/21101417/app-installed-Recovered.png "")


#### The 2 Deep Linking Types:
Since users may or may not have the mobile app installed, there are 2 types of deep linking:

1. Deferred Deep Linking - Serving personalized content to new or former users, directly after the installation. 
2. Direct Deep Linking - Directly serving personalized content to existing users, which already have the mobile app installed.

For more info please check out the [OneLink™ Deep Linking Guide](https://support.appsflyer.com/hc/en-us/articles/208874366-OneLink-Deep-Linking-Guide#Intro).

###  <a id="deferred-deep-linking"> 1. Deferred Deep Linking (Get Conversion Data)

Check out the deferred deeplinkg guide from the AppFlyer knowledge base [here](https://support.appsflyer.com/hc/en-us/articles/207032096-Accessing-AppsFlyer-Attribution-Conversion-Data-from-the-SDK-Deferred-Deeplinking-#Introduction).

Code Sample to handle the conversion data:

```c#
public void onConversionDataSuccess(string conversionData)
{
    AppsFlyer.AFLog("onConversionDataSuccess", conversionData);
    Dictionary<string, object> conversionDataDictionary = AppsFlyer.CallbackStringToDictionary(conversionData);
    // add deferred deeplink logic here
}

public void onConversionDataFail(string error)
{
    AppsFlyer.AFLog("onConversionDataFail", error);
}
```

###  <a id="handle-deeplinking"> 2. Direct Deeplinking
    
When a deeplink is clicked on the device the AppsFlyer SDK will return the resolved link in the [onAppOpenAttribution](https://support.appsflyer.com/hc/en-us/articles/208874366-OneLink-Deep-Linking-Guide#deep-linking-data-the-onappopenattribution-method-) method.



```c#
public void onAppOpenAttribution(string attributionData)
{
    AppsFlyer.AFLog("onAppOpenAttribution", attributionData);
    Dictionary<string, object> attributionDataDictionary = AppsFlyer.CallbackStringToDictionary(attributionData);
    // add direct deeplink logic here
}

public void onAppOpenAttributionFailure(string error)
{
    AppsFlyer.AFLog("onAppOpenAttributionFailure", error);
}
```


###  <a id="android-deeplink"> Android Deeplink Setup
    
    
    
#### <a id="uri-scheme"> URI Scheme
In your app’s manifest add the following intent-filter to your relevant activity:
```xml 
<intent-filter>
    <action android:name="android.intent.action.VIEW" />
    <category android:name="android.intent.category.DEFAULT" />
    <category android:name="android.intent.category.BROWSABLE" />
    <data android:scheme="your unique scheme" />
</intent-filter>
```

#### <a id="app-links"> App Links
For more on App Links check out the guide [here](https://support.appsflyer.com/hc/en-us/articles/115005314223-Deep-Linking-Users-with-Android-App-Links#what-are-android-app-links).


###  <a id="ios-deeplink"> iOS Deeplink Setup
For more on Universal Links check out the guide [here](https://support.appsflyer.com/hc/en-us/articles/208874366-OneLink-Deep-Linking-Guide#setups-universal-links).
    
Essentially, the Universal Links method links between an iOS mobile app and an associate website/domain, such as AppsFlyer’s OneLink domain (xxx.onelink.me). To do so, it is required to:

1. Configure OneLink sub-domain and link to mobile app (by hosting the ‘apple-app-site-association’ file - AppsFlyer takes care of this part in the onelink setup on your dashboard)
2. Configure the mobile app to register approved domains:

```xml
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
    <dict>
        <key>com.apple.developer.associated-domains</key>
        <array>
            <string>applinks:test.onelink.me</string>
        </array>
    </dict>
</plist>
```



---



### <a id="track-app-uninstalls"> Measure uninstalls

#### <a id="track-app-uninstalls-ios"> iOS

AppsFlyer enables you to track app uninstalls. To handle notifications it requires  to modify your `AppDelegate.m`. Use [didRegisterForRemoteNotificationsWithDeviceToken](https://developer.apple.com/reference/uikit/uiapplicationdelegate) to register to the uninstall feature.

UnityEngine.iOS.NotificationServices is now deprecated. Please use the "Mobile Notifications" package instead. It is available in the Unity package manager. 

*Example:*

```c#
using AppsFlyerSDK;
using Unity.Notifications.iOS;

public class AppsFlyerObjectScript : MonoBehaviour, IAppsFlyerConversionData
{

    void Start()
    {
        AppsFlyer.initSDK("devKey", "appID", this);
        AppsFlyer.startSDK();
#if UNITY_IOS
  
        StartCoroutine(RequestAuthorization());
        Screen.orientation = ScreenOrientation.Portrait;

#endif

    }


#if UNITY_IOS
    IEnumerator RequestAuthorization()
    {
      
        using (var req = new AuthorizationRequest(AuthorizationOption.Alert | AuthorizationOption.Badge, true))
        {

            while (!req.IsFinished)
            {
                yield return null;
            }
             if (req.Granted && req.DeviceToken != "")
             {
                  AppsFlyeriOS.registerUninstall(Encoding.UTF8.GetBytes(req.DeviceToken));
      
             }
        }
    }
#endif
}
```

Read more about Uninstall register: [Appsflyer SDK support site](https://support.appsflyer.com/hc/en-us/articles/207032066-AppsFlyer-SDK-Integration-iOS)

#### <a id="track-app-uninstalls-android"> Android


1. Download the Unity Firebase SDK from: https://firebase.google.com/docs/unity/setup.
2. Import FirebaseMessaging.unitypackage into the project.
3. Import google-services.json into the project (obtained in the Firebase console)
    **Note** Manifest receivers should be automatically added by the Unity Firebase SDK.
4. In the Unity class handling the AppsFlyer code, add the following:
```c#
using Firebase.Messaging;
using Firebase.Unity;
```

5. Add to the `Start()` method:
```c#
Firebase.Messaging.FirebaseMessaging.TokenReceived += OnTokenReceived;
```
6. Add the following method:

```c#
    public void OnTokenReceived(object sender, Firebase.Messaging.TokenReceivedEventArgs token)
    {
#if UNITY_ANDROID
        AppsFlyerAndroid.updateServerUninstallToken(token.Token);
#endif
    }
```


Read more about Android  Uninstall Tracking: [Appsflyer SDK support site](https://support.appsflyer.com/hc/en-us/articles/208004986-Android-Uninstall-Tracking)

##  <a id="UserInviteAttribution"> User invite attribution

AppsFlyer allows you to attribute and record installs originating from user invites within your app. Allowing your existing users to invite their friends and contacts as new users to your app can be a key growth factor for your app.

Example:
```c#

public class AppsFlyerObjectScript : MonoBehaviour , IAppsFlyerConversionData, IAppsFlyerUserInvite {

...

    public void generateAppsFlyerLink()
    {
        Dictionary<string, string> parameters = new Dictionary<string, string>();
        parameters.Add("channel", "some_channel");
        parameters.Add("campaign", "some_campaign");
        parameters.Add("additional_param1", "some_param1");
        parameters.Add("additional_param2", "some_param2");
       
        // other params
        //parameters.Add("referrerName", "some_referrerName");
        //parameters.Add("referrerImageUrl", "some_referrerImageUrl");
        //parameters.Add("customerID", "some_customerID");
        //parameters.Add("baseDeepLink", "some_baseDeepLink");
        //parameters.Add("brandDomain", "some_brandDomain");
        

        AppsFlyer.generateUserInviteLink(parameters, this);
    }


    ... 

    public void onInviteLinkGenerated(string link)
    {
        AppsFlyer.AFLog("onInviteLinkGenerated", link);
    }

    public void onInviteLinkGeneratedFailure(string error)
    {
        AppsFlyer.AFLog("onInviteLinkGeneratedFailure", error);
    }

    public void onOpenStoreLinkGenerated(string link)
    {
        AppsFlyer.AFLog("onOpenStoreLinkGenerated", link);
    }
}
```


##  <a id="InAppPurchaseValidation"> In-app purchase validation

For In-App Purchase Receipt Validation, follow the instructions according to your operating system.

**Notes**
Calling validateReceipt automatically generates an af_purchase in-app event, so you don't need to send this event yourself.
The validate purchase response is triggered in the AppsFlyerTrackerCallbacks.cs class.

```c#
//To get the callbacks
//AppsFlyer.createValidateInAppListener ("AppsFlyerTrackerCallbacks", "onInAppBillingSuccess", "onInAppBillingFailure");
AppsFlyer.validateReceipt(string publicKey, string purchaseData, string signature, string price, string currency, Dictionary additionalParametes);
```

```c#
using UnityEngine.Purchasing;
using AppsFlyerSDK;

public class AppsFlyerObject : MonoBehaviour, IStoreListener, IAppsFlyerValidateReceipt
{

    public static string kProductIDConsumable = "com.test.cons";

    void Start()
    {
        AppsFlyer.initSDK("devKey", "devKey");
        AppsFlyer.startSDK();
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {
        string prodID = args.purchasedProduct.definition.id;
        string price = args.purchasedProduct.metadata.localizedPrice.ToString();
        string currency = args.purchasedProduct.metadata.isoCurrencyCode;

        string receipt = args.purchasedProduct.receipt;
        var recptToJSON = (Dictionary<string, object>)AFMiniJSON.Json.Deserialize(args.purchasedProduct.receipt);
        var transactionID = (string)recptToJSON["TransactionID"];

        if (String.Equals(args.purchasedProduct.definition.id, kProductIDConsumable, StringComparison.Ordinal))
        {
#if UNITY_IOS

            if(isSandbox)
            {
                AppsFlyeriOS.setUseReceiptValidationSandbox(true);
            }

            AppsFlyeriOS.validateAndSendInAppPurchase(prodID, price, currency, transactionID, null, this);
#elif UNITY_ANDROID
        var purchaseData = (string)recptToJSON["json"];
        var signature = (string)recptToJSON["signature"];
        AppsFlyerAndroid.validateAndSendInAppPurchase(
        "<google_public_key>", 
        signature, 
        purchaseData, 
        price, 
        currency, 
        null, 
        this);
#endif
        }

        return PurchaseProcessingResult.Complete;
    }

    public void didFinishValidateReceipt(string result)
    {
        AppsFlyer.AFLog("didFinishValidateReceipt", result);
    }

    public void didFinishValidateReceiptWithError(string error)
    {
        AppsFlyer.AFLog("didFinishValidateReceiptWithError", error);
    }

}

```

##  <a id="collect-idfa"> Collect IDFA with ATTrackingManager
    
⚠️ **By default, IDFA is collected. Consent dialog is optional.**
   
   
1. Add the `AppTrackingTransparency` framework to your xcode project. 
2. In the `Info.plist`:
    1. Add an entry to the list: Press +  next to `Information Property List`.
    2. Scroll down and select `Privacy - Tracking Usage Description`.
    3. Add as the value the wording you want to present to the user when asking for permission to collect the IDFA.
3. Call the `waitForATTUserAuthorizationWithTimeoutInterval` api before `startSDK()`
    
    ```c#
    #if UNITY_IOS && !UNITY_EDITOR
    AppsFlyeriOS.waitForATTUserAuthorizationWithTimeoutInterval(60);
    #endif
    ```
        
4. In the `AppsFlyerAppController` class, add:
    
    ```objectivec
    #import <AppTrackingTransparency/ATTrackingManager.h>
    
    ...
    
    - (void)didFinishLaunching:(NSNotification*)notification {
    
    if (@available(iOS 14, *)) {
          [ATTrackingManager requestTrackingAuthorizationWithCompletionHandler:^(ATTrackingManagerAuthorizationStatus status){
          }];
      }
    ...
    }
    
    ```
    


##  <a id="request-listeners"> Request Listeners
    
1. Attach the 'AppsFlyer.cs' script to the game object with the AppsFlyer init code. (AppsFlyerObject, ect)
2. Add the following code **before** startSDK()

Sessions response example:

```c#
    void Start()
    {
        AppsFlyer.OnRequestResponse += AppsFlyerOnRequestResponse;
        
        AppsFlyer.initSDK(devKey, appID, this);
        AppsFlyer.startSDK();
    }

    void AppsFlyerOnRequestResponse(object sender, EventArgs e)
    {
        var args = e as AppsFlyerRequestEventArgs;
        AppsFlyer.AFLog("AppsFlyerOnRequestResponse", " status code " + args.statusCode);
    }
```

In-App response example:

```c#
    void Start()
    {
        AppsFlyer.OnInAppResponse += (sender, args) =>
        {
            var af_args = args as AppsFlyerRequestEventArgs;
            AppsFlyer.AFLog("AppsFlyerOnRequestResponse", " status code " + af_args.statusCode);
        };
        
        AppsFlyer.initSDK(devKey, appID, this);
        AppsFlyer.startSDK();
    }


```

| statusCode      | errorDescription | 
| ----------- | ----------- | 
| 200      | null       | 
| 10   | "Event timeout. Check 'minTimeBetweenSessions' param"        | 
| 11   | "Skipping event because 'isStopTracking' enabled"        | 
| 40   | Network error: Error description comes from Android        | 
| 41   | "No dev key"        | 
| 50   | "Status code failure" + actual response code from the server        | 



