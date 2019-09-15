
<img src="https://www.appsflyer.com/wp-content/uploads/2016/11/logo-1.svg"  width="450">

# unity_appsflyer_sdk 

AppsFlyer’s Unity SDK provides mobile app installation and event recording functionality for Android and iOS Unity projects. You can record installs, updates, and sessions and also record post-installs events (including in-app purchases, game levels, etc.) to evaluate ROI and user engagement levels.

Mobile apps, that are developed on the Unity platform, can integrate AppsFlyer's SDK once and attribution installs for both Android and iOS generated apps. The following guide details how to integrate AppsFlyer's SDK into your Unity code for your iOS and Android apps.

---

🛠 In order for us to provide optimal support, we would kindly ask you to submit any issues to support@appsflyer.com

*When submitting an issue please specify your AppsFlyer sign-up (account) email , your app ID , production steps, logs, code snippets and any additional relevant information.*


## Table of content

- [SDK versions](#plugin-build-for)
- [Installation](#installation)
- [Guides](#guides)
- [API](#api) 
- [Migration Doc](#migration) 
- [Demo](#demo)  


### <a id="plugin-build-for"> This plugin is built for

- iOS AppsFlyerSDK **v4.10.2**
- Android AppsFlyerSDK **v4.10.2** 


## <a id="installation"> 📲 Installation

1. Clone / download the repo.
2. Import the AppsFlyerUnityPlugin.unitypackage into your Unity project.
3. Go to Assets >> Import Package >> Custom Package
4. Select AppsFlyerUnityPlugin.unitypackage file.



## <a id="setup"> 🚀 Setup

### Using the AppsFlyerObject.prefab

1. Go to Assets > AppsFlyer and drag AppsFlyerObject.prefab to your scene.
<img src="https://firebasestorage.googleapis.com/v0/b/firstintegrationapp.appspot.com/o/Unity2_add_object.png?alt=media&token=526b87f4-d5aa-400b-805d-5efe3f38ac87"  >

2. Update the following fields:
- Dev Key (required) - AppsFlyer's Dev-Key, which is accessible from your AppsFlyer account under 'App Settings' in the dashboard.
- App ID (ios only) - Your iTunes Application ID. (If you app is not for iOS the leave field empty)
- Get Conversion Data - Set this to true if your app is using AppsFlyer for deeplinking
- Is Debug - set this to true to view the debug logs. (for development only!)

3. Update the code in Assets > AppsFlyer > AppsFlyerObjectScript.cs with any other [API](/docs/API.md) you wish to use.

### Manual intgration

Create a game object and add the following init code:

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

**Note** make sure not to call destroy on the game object. 

Check out the [API](/docs/API.md) and add it as you wish through your app.



 ## <a id="guides"> 📖 Guides

Great installation and setup guides can be viewed [here](/docs/Guides.md).

- [Init SDK](/docs/Guides.md#init-sdk)
    - [Init SDK without deeplinking](/docs/Guides.md#init-sdk-deeplink-basic)
    - [Init SDK with deeplinking callbacks](/docs/Guides.md#init-sdk-deeplink)
- [Deep Linking](/docs/Guides.md#deeplinking)
    - [Deferred Deep Linking (Get Conversion Data)](/docs/Guides.md#conversionData)
    - [Direct Deep Linking](/docs/Guides.md#handle-deeplinking)
    - [Android Deepling](/docs/Guides.md#android-deeplink)
        - [URI Scheme](/docs/Guides.md#uri-scheme)
        - [App Links](/docs/Guides.md#app-links)
    - [iOS Deeplink Setup](/docs/Guides.md#ios-deeplink)
- [Uninstall](/docs/Guides.md#track-app-uninstalls)
    - [iOS Uninstall Setup](/docs/Guides.md#track-app-uninstalls-ios)
    - [Android Uninstall Setup](/docs/Guides.md#track-app-uninstalls-android)
- [User invite attribution](/docs/Guides.md#UserInviteAttribution)
- [In-app purchase validation](/docs/Guides.md#InAppPurchaseValidation)


## <a id="api"> 📑 API
  
See the full [API](/docs/API.md) available for this plugin.

## <a id="migration"> ⏩ Migration 
  
Check the the migration docs from the previous unity plugin [here](/docs/MigrationGuide.md).

## <a id="demo"> 📱 Demo
  
Coming Soon ...
