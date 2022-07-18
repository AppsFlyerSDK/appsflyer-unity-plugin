# Basic Integration

You can initialize the plugin by using the AppsFlyerObject prefab or manually.

- [Initialization using the prefab](#using-prefab)
- [Manual integration](#manual-integration)
- [Init with the deeplinking callbacks](#init-sdk-deeplink)
- [Collect IDFA with ATTrackingManager](#collect)
- [Sending SKAN postbacks to AppsFlyer](#skan)
- [Mac OS initialization beta](#macos)

### <a id="using-prefab"> Using the AppsFlyerObject.prefab

1. Go to Assets > AppsFlyer and drag AppsFlyerObject.prefab to your scene.
<img src="https://firebasestorage.googleapis.com/v0/b/firstintegrationapp.appspot.com/o/Unity2_add_object.png?alt=media&token=526b87f4-d5aa-400b-805d-5efe3f38ac87" width="650">
<br/>
2. Update the following fields:

| Setting  | Description   |
| -------- | ------------- |
| **Dev Key**   |  AppsFlyer's [Dev Key](https://support.appsflyer.com/hc/en-us/articles/207032126-Android-SDK-integration-for-developers#integration-31-retrieving-your-dev-key), which is accessible from the AppsFlyer dashboard. |
| **App ID**      | Your iTunes Application ID. (If your app is not for iOS the leave field empty)  |
| **Get Conversion Data**    | Set this to true if your app is using AppsFlyer for deep linking.  |
| **Is Debug**    | Set this to true to view the debug logs. (for development only!)  |

3. Update the code in Assets > AppsFlyer > AppsFlyerObjectScript.cs with other available [API](/docs/API.md).

### <a id="manual-integration"> Manual integration

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

---
# <a id="collect"> Collect IDFA with ATTrackingManager

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
# <a id="skan"> Sending SKAN postback to Appsflyer
  To register the AppsFlyer endpoint, you need to add the `NSAdvertisingAttributionReportEndpoint` key to your info.plist and set the value to `https://appsflyer-skadnetwork.com/`. 
More info on how to update the info.plist can be found [here](https://github.com/AppsFlyerSDK/appsflyer-unity-plugin/blob/master/docs/Troubleshooting.md#updating-the-infoplist). 

---
# <a id="macos"> MacOS initialization
1. Use the prefab `AppsFlyerObject`
2. Add your MacOS app id
3. Build for the platform `PC, Mac & Linux Standelone` and choose `MacOS` as the target platform.