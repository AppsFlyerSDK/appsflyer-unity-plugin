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

