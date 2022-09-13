# Unified Deep Linking(UDL)

    
![alt text](https://dev.appsflyer.com/hc/docs/unified-deep-linking-udl "")


#### The 3 Deep Linking Types:
1. Deferred Deep Linking - Serving personalized content to new or former users, directly after the installation. 
2. Direct Deep Linking - Directly serving personalized content to existing users, which already have the mobile app installed.
3. Unified deep linking (UDL) - enables you to send new and existing users to a specific in-app activity as soon as the app is opened.

For more info please check out the [OneLink™ Deep Linking Guide](https://support.appsflyer.com/hc/en-us/articles/208874366-OneLink-Deep-Linking-Guide#Intro).

---

The flow works as follows:

1. User clicks the OneLink short URL.
2. The iOS Universal Links/ Android App Links (for deep linking) or the deferred deep link, trigger the SDK.
3. The SDK triggers the didResolveDeepLink method, and passes the deep link result object to the user.
4. The OnDeepLinkReceived method uses the deep link result object that includes the deep_link_value and other parameters to create the personalized experience for the users, which is the main goal of OneLink.

> Check out the Unified Deep Linking docs for [Android](https://dev.appsflyer.com/docs/android-unified-deep-linking) and [iOS](https://dev.appsflyer.com/docs/ios-unified-deep-linking).

Considerations:

* Requires AppsFlyer Android SDK V6.1.3 or later.
* Does not support SRN campaigns.
* Does not provide af_dp in the API response.
* onAppOpenAttribution will not be called. All code should migrate to `OnDeepLinkReceived`.
* OnDeepLinkReceived must be called **after** `initSDK`.
* AppsFlyer.cs **must** be attached to the game object.

Implementation:

1. Attach AppsFlyer.cs script to the game object with the AppsFlyer init code. (AppsFlyerObject)
2. After `initSDK()` implement `OnDeepLinkReceived`.

Example:

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
      
      // deeplink logic here
  }
}

```

Parsing deeplink object example:

```c#
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

                break;
            case DeepLinkStatus.NOT_FOUND:
                AppsFlyer.AFLog("OnDeepLink", "Deep link not found");
                break;
            default:
                AppsFlyer.AFLog("OnDeepLink", "Deep link error");
                break;
        }
    }
```
