---
title: Integration
category: 5f9705393c689a065c409b23
parentDoc: 6370c9e2441a4504d6bca3bd
order: 1
hidden: false
---

You can initialize the plugin by using the AppsFlyerObject prefab or manually.

- [Using the AppsFlyerObject.prefab](#using-the-appsflyerobjectprefab)
- [Manual integration](#manual-integration)
- [Collect IDFA with ATTrackingManager](#collect-idfa-with-attrackingmanager)
- [Sending SKAN postback to Appsflyer](#sending-skan-postback-to-appsflyer)
- [MacOS initialization](#macos-initialization)
- [Request Listeners (Optional)](#request-listeners-optional)

## Using the AppsFlyerObject.prefab

1. Go to Assets > AppsFlyer and drag AppsFlyerObject.prefab to your scene.
<img src="assets/unity_add_object.png" width="650">
<br/>
2. Update the following fields:

| Setting  | Description   |
| -------- | ------------- |
| **Dev Key**   |  AppsFlyer's [Dev Key](https://support.appsflyer.com/hc/en-us/articles/207032126-Android-SDK-integration-for-developers#integration-31-retrieving-your-dev-key), which is accessible from the AppsFlyer dashboard. |
| **App ID**      | Your iTunes Application ID. (If your app is not for iOS the leave field empty)  |
| **Get Conversion Data**    | Set this to true if your app is using AppsFlyer for deep linking.  |
| **Is Debug**    | Set this to true to view the debug logs. (for development only!)  |

3. Update the code in Assets > AppsFlyer > AppsFlyerObjectScript.cs with other available [API](/docs/api).

## Manual integration

Create a game object and add the following init code:

```c#
using AppsFlyerSDK;

public class AppsFlyerObjectScript : MonoBehaviour
{
  void Start()
  {
    AppsFlyer.initSDK("devkey", "appID");
    AppsFlyer.startSDK();
  }
}
```

> **Note:** 
> - Make sure not to call destroy on the game object. 
> - Use [`DontDestroyOnLoad`](https://docs.unity3d.com/ScriptReference/Object.DontDestroyOnLoad.html) to keep the object when loading a new scene.

---

## Set customer user ID

Set your own unique customer user ID (CUID) and cross-reference it with the unique AppsFlyer ID.

- Appear in AppsFlyer raw data CSV reports.
- Can be used in postback APIs to cross-reference with internal IDs.  
  To set the CUID, use:

```c#
AppsFlyer.setCustomerUserId("someId");
```

**Good practice!** Set the CUID early in the app flow—it is only associated with events reported after its setup.

- Recorded events will be associated with the CUID.
- Related data will appear in the raw data reports for installs and events..

### Associate the CUID with the install event

If it’s important for you to associate the install event with the CUID, call `setCustomerUserId` before calling `startSDK`.

## Collect IDFA with ATTrackingManager

1. Add the `AppTrackingTransparency` framework to your xcode project. 
2. In the `Info.plist`:
    1. Add an entry to the list: Press +  next to `Information Property List`.
    2. Scroll down and select `Privacy - Tracking Usage Description`.
    3. Add as the value the wording you want to present to the user when asking for permission to collect the IDFA.
3. Call the `waitForATTUserAuthorizationWithTimeoutInterval` api before `startSDK()`
    
    ```c#
    #if UNITY_IOS && !UNITY_EDITOR
    AppsFlyer.waitForATTUserAuthorizationWithTimeoutInterval(60);
    #endif
    ```
        
4. Reques the tracking authorization where you wish to display the prompt: <br/>
    You can use the following [package](https://github.com/Unity-Technologies/com.unity.ads.ios-support) or any other package that allows you to request the tracking authorization. 
    ```c#

    using Unity.Advertisement.IosSupport;

    /*  ... */
  
    if (ATTrackingStatusBinding.GetAuthorizationTrackingStatus() 
         == ATTrackingStatusBinding.AuthorizationTrackingStatus.NOT_DETERMINED)
        {
            ATTrackingStatusBinding.RequestAuthorizationTracking();
        }
     /*  ... */
  
  
---

## Send consent for DMA compliance

Unity SDK plugin offers two alternative methods for gathering consent data:

Through a Consent Management Platform (CMP): If the app uses a CMP that complies with the Transparency and Consent Framework (TCF) v2.2 protocol, the Unity SDK can automatically retrieve the consent details.

OR

Through a dedicated Unity SDK API: Developers can pass Google's required consent data directly to the Unity SDK using a specific API designed for this purpose.

### Use CMP to collect consent data

1. Initialize the SDK.
2. Call enableTCFDataCollection(true) api before startSDK() to instruct the SDK to collect the TCF data from the device.
3. Use the CMP to decide if you need the consent dialog in the current session to acquire the consent data. If you need the consent dialog move to step 4; otherwise move to step 5.
4. Get confirmation from the CMP that the user has made their consent decision and the data is available.
5. Call start().
    
    ```c#
        AppsFlyer.initSDK(devKey, appID, this);

        AppsFlyer.enableTCFDataCollection(true);
        
        //YOUR_CMP_FLOW()
        // if already has consent ready - you can start
            AppsFlyer.startSDK();
            
        //else Waiting for CMP completion and data ready and then start
        
            AppsFlyer.startSDK();
    ```

### Manually collect consent data

1. Initialize the SDK.
2. Determine whether the GDPR applies or not to the user.

### When GDPR applies to the user
1. Given that GDPR is applicable to the user, determine whether the consent data is already stored for this session.
    i.  If there is no consent data stored, show the consent dialog to capture the user consent decision.
    ii. If there is consent data stored continue to the next step.
    
2. To transfer the consent data to the SDK create an AppsFlyerConsent object with the following parameters:
    - hasConsentForDataUsage - Indicates whether the user has consented to use their data for advertising purposes.
    - hasConsentForAdsPersonalization - Indicates whether the user has consented to use their data for personalized advertising.
3. Call setConsentData()with the AppsFlyerConsent object.
5. Call start().
    
    ```c#
            
        // If the user is subject to GDPR - collect the consent data
        // or retrieve it from the storage
        ...
        // Set the consent data to the SDK:
        AppsFlyerConsent consent = AppsFlyerConsent.ForGDPRUser(true, true);
        AppsFlyer.setConsentData(consent);
            
        AppsFlyer.startSDK();
    ```

### When GDPR does not apply to the user
1. Create an AppsFlyerConsent object using the ForNonGDPRUser() initializer. This initializer doesn’t accept any parameters.
2. Pass the empty AppsFlyerConsent object to setConsentData().
2. Call start().
    
    ```c#
        // If the user is not subject to GDPR:
        AppsFlyerConsent consent = AppsFlyerConsent.ForNonGDPRUser();
        AppsFlyer.setConsentData(consent);
            
        AppsFlyer.startSDK();
    ```

 ## Verify consent data is sent
 To test whether your SDK sends DMA consent data with each event, perform the following steps:
 
 1. Enable the SDK debug mode.
 2. Search for consent_data in the log of the outgoing request.
 
 for more information visit [iOS](https://dev.appsflyer.com/hc/docs/ios-send-consent-for-dma-compliance)  
                            [Android](https://dev.appsflyer.com/hc/docs/android-send-consent-for-dma-compliance)  
---

## Sending SKAN postback to Appsflyer
  To register the AppsFlyer endpoint, you need to add the `NSAdvertisingAttributionReportEndpoint` key to your info.plist and set the value to `https://appsflyer-skadnetwork.com/`. 
More info on how to update the info.plist can be found [here](https://github.com/AppsFlyerSDK/appsflyer-unity-plugin/blob/master/docs/Troubleshooting.md#updating-the-infoplist). 

--- 

## MacOS initialization
1. Use the prefab `AppsFlyerObject`
2. Add your MacOS app id
3. Build for the platform `PC, Mac & Linux Standelone` and choose `MacOS` as the target platform.
  
---
## Request Listeners (Optional)
    
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

---

