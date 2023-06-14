---
title: Uninstall Measurement
category: 5f9705393c689a065c409b23
parentDoc: 6370c9e2441a4504d6bca3bd
order: 4
hidden: false
---

- [iOS uninstall](#track-app-uninstalls-ios)
- [Android Uninstall](#track-app-uninstalls-android)

## iOS

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
                  AppsFlyer.registerUninstall(Encoding.UTF8.GetBytes(req.DeviceToken));
      
             }
        }
    }
#endif
}
```

Read more about Uninstall register: [Appsflyer SDK support site](https://support.appsflyer.com/hc/en-us/articles/207032066-AppsFlyer-SDK-Integration-iOS)

---

## Android


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
        AppsFlyer.updateServerUninstallToken(token.Token);
#endif
    }
```


Read more about Android  Uninstall Tracking: [Appsflyer SDK support site](https://support.appsflyer.com/hc/en-us/articles/208004986-Android-Uninstall-Tracking)


---
