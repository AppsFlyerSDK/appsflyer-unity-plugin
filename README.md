
<img src="https://www.appsflyer.com/wp-content/uploads/2016/11/logo-1.svg"  width="450">

# unity_appsflyer_sdk 

AppsFlyer’s Unity SDK provides mobile app installation and event recording functionality for Android and iOS Unity projects. You can record installs, updates, and sessions and also record post-installs events (including in-app purchases, game levels, etc.) to evaluate ROI and user engagement levels.

Mobile apps that are developed on the Unity platform, can integrate AppsFlyer's SDK once and attribution installs for both Android and iOS generated apps. The following guide details how to integrate AppsFlyer's SDK into your Unity code for your iOS and Android apps.

---

🛠 In order for us to provide optimal support, we would kindly ask you to submit any issues to support@appsflyer.com

*When submitting an issue please specify your AppsFlyer sign-up (account) email , your app ID , production steps, logs, code snippets and any additional relevant information.*


## Table of content
- [Adding the SDK to your project](#add-sdk-to-project)
- [Initializing the SDK](#init-sdk)
- [Guides](#guides)
- [API](#api) 
- [Sample App](#sample-app)
- [Testing installs](#testing-installs)
- [Migration From Older Plugin Versions](#migration) 


## <a id="add-sdk-to-project"> 📲 Adding the SDK to your project

1. Clone / download this repository.
2. [Import](https://docs.unity3d.com/Manual/AssetPackages.html) the AppsFlyerUnityPlugin.unitypackage into your Unity project.
3. Go to Assets >> Import Package >> Custom Package
4. Select AppsFlyerUnityPlugin.unitypackage file.


## <a id="init-sdk"> 🚀 Initializing the SDK

### Using the AppsFlyerObject.prefab

1. Go to Assets > AppsFlyer and drag AppsFlyerObject.prefab to your scene.
<img src="https://firebasestorage.googleapis.com/v0/b/firstintegrationapp.appspot.com/o/Unity2_add_object.png?alt=media&token=526b87f4-d5aa-400b-805d-5efe3f38ac87"  >

2. Update the following fields:
- **Dev Key** (required) - AppsFlyer's [Dev-Key](https://support.appsflyer.com/hc/en-us/articles/211719806-Global-app-settings-#sdk-dev-key), which is accessible from your AppsFlyer account under 'App Settings' in the dashboard.
- **App ID** (ios only) - Your iTunes Application ID. (If you app is not for iOS the leave field empty)
- **Get Conversion Data** - Set this to true if your app is using AppsFlyer for deeplinking
- **Is Debug** - set this to true to view the debug logs. (for development only!)

3. Update the code in Assets > AppsFlyer > AppsFlyerObjectScript.cs with any other [API](/docs/API.md) you wish to use.

### Manual intgration

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

**Note:** Make sure not to call destroy on the game object. 


 ## <a id="guides"> 📖 Guides

Great installation and setup guides can be viewed [here](/docs/Guides.md).

- [Init SDK](/docs/Guides.md#init-sdk)
- [Deep Linking](/docs/Guides.md#deeplinking)
- [Uninstall](/docs/Guides.md#track-app-uninstalls)

## <a id="api"> 📑 API
  
See the full [API](/docs/API.md) available for this plugin.

## <a id="sample-app"> :pencil: Sample App
  
Check out the Unity Sample app [here](/docs/MigrationGuide.md).

## <a id="testing-installs"> 📱 Testing installs
  
  To test [non-organic](https://support.appsflyer.com/hc/en-us/articles/115005995229-Organic-vs-non-organic-installs) installs, follow these steps:
  1. Uninstall the app from your test device.
  2. Make sure your device is [whitelisted](https://support.appsflyer.com/hc/en-us/articles/207031996-Whitelisting-test-devices).
  3. Generate a AppsFlyer [Tracking Link](https://support.appsflyer.com/hc/en-us/articles/207033836-Custom-link-management#intro), send it to the test device and click on it.
  4. Reinstall the app on the device.
  
For more info on testing check out this [guide](https://support.appsflyer.com/hc/en-us/articles/360001559405-Testing-AppsFlyer-SDK-integration#introduction).

## <a id="migration"> ⏩ Migration 
  
Migrating from the old plugin? <br/>
Check the the migration docs [here](/docs/MigrationGuide.md).


