---
title: Unified Deep Linking (UDL)
category: 5f9705393c689a065c409b23
parentDoc: 6370c9e2441a4504d6bca3bd
order: 7
hidden: false
---

> 📘 **UDL privacy protection**
> 
> For new users, the UDL method only returns parameters relevant to deferred deep linking: `deep_link_value` and `deep_link_sub1-10`. Other parameters (`media_source`, `campaign`, `af_sub1-5`, etc.), return null.

# UDL flow

1. The SDK is triggered by:
   - **Deferred Deep Linking** - using a dedicated API
   - **Direct Deep Linking** - triggered by the OS via Android App Link, iOS Universal Links or URI scheme.
2. The SDK triggers the `OnDeepLink` method, and passes the deep link result object to the user.
3. The `OnDeepLink` method uses the deep link result object that includes the `deep_link_value` and other parameters to create the personalized experience for the users, which is the main goal of OneLink.

> Check out the Unified Deep Linking docs for [Android](https://dev.appsflyer.com/docs/android-unified-deep-linking) and [iOS](https://dev.appsflyer.com/docs/ios-unified-deep-linking).

# Considerations

* Requires AppsFlyer Android SDK V6.1.3 or later.
* Does not support SRN campaigns.
* For new users, the UDL method only returns parameters relevant to deferred deep linking: `deep_link_value` and `deep_link_sub1-10`. If you try to get any other parameters (media_source, campaign, af_sub1-5, etc.), they return `null`.
* `onAppOpenAttribution` will not be called. All code should migrate to `OnDeepLink`.
* `OnDeepLink` must be called **after** `initSDK`.
* `AppsFlyer.cs` **must** be attached to the game object.

# Implementation

1. Attach `AppsFlyer.cs` script to the game object with the AppsFlyer init code. (AppsFlyerObject)
2. Call initSDK with the `this` parameter in order for the `OnDeepLinkReceived` callback to be invoked:
    ```c#
    AppsFlyer.initSDK("devkey", "appID", this);
    ```    
3. Assign `OnDeepLink` to `AppsFlyer.OnDeepLinkReceived` in `Start()`
   ```c#
    AppsFlyer.OnDeepLinkReceived += OnDeepLink;
   ``` 
4. After `initSDK()` implement `OnDeepLink`.

## Example

```c#
using AppsFlyerSDK;

public class AppsFlyerObjectScript : MonoBehaviour
{
  void Start()
  {
    AppsFlyer.initSDK("devkey", "appID", this);
    AppsFlyer.OnDeepLinkReceived += OnDeepLink;
    AppsFlyer.startSDK();
  }
  
  void OnDeepLink(object sender, EventArgs args)
  {
      var deepLinkEventArgs = args as DeepLinkEventsArgs;

      switch (deepLinkEventArgs.status)
      {
          case DeepLinkStatus.FOUND:

              if (deepLinkEventArgs.isDeferred())
              {
                  AppsFlyer.AFLog("OnDeepLink", "This is a deferred deep link");
              }
              else
              {
                  AppsFlyer.AFLog("OnDeepLink", "This is a direct deep link");
              }
              
              // deepLinkParamsDictionary contains all the deep link parameters as keys
              Dictionary<string, object> deepLinkParamsDictionary = null;
      #if UNITY_IOS && !UNITY_EDITOR
              if (deepLinkEventArgs.deepLink.ContainsKey("click_event") && deepLinkEventArgs.deepLink["click_event"] != null)
              {
                  deepLinkParamsDictionary = deepLinkEventArgs.deepLink["click_event"] as Dictionary<string, object>;
              }
      #elif UNITY_ANDROID && !UNITY_EDITOR
                  deepLinkParamsDictionary = deepLinkEventArgs.deepLink;
      #endif

              break;
          case DeepLinkStatus.NOT_FOUND:
              AppsFlyer.AFLog("OnDeepLink", "Deep link not found");
              break;
          default:
              AppsFlyer.AFLog("OnDeepLink", "Deep link error");
              break;
      }
  }
}

```
