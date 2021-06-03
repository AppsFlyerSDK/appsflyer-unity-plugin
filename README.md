<img src="https://www.appsflyer.com/wp-content/uploads/2016/11/logo-1.svg"  width="450">

# appsflyer-unity-plugin

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://opensource.org/licenses/MIT)
[![GitHub tag](https://img.shields.io/github/v/release/AppsFlyerSDK/appsflyer-unity-plugin)](https://img.shields.io/github/v/release/AppsFlyerSDK/appsflyer-unity-plugin)


🛠 In order for us to provide optimal support, we would kindly ask you to submit any issues to support@appsflyer.com

> *When submitting an issue please specify your AppsFlyer sign-up (account) email , your app ID , production steps, logs, code snippets and any additional relevant information.*

## Table of content
- [Breaking changes 6.3.0](#breaking-changes) 
- [Migration from older plugin versions](#migration) 
- [Adding the SDK to your project](#add-sdk-to-project)
- [Initializing the SDK](#init-sdk)
    - [Using the AppsFlyerObject.prefab](#using-prefab)
    - [Manual integration](#manual-integration)
- [Guides](#guides)
- [API](#api) 

<hr/>


### <a id="plugin-build-for"> This plugin is built for

- Android AppsFlyer SDK **v6.3.0** 
- iOS AppsFlyer SDK **v6.3.1**

## <a id="breaking-changes"> 	❗❗ Breaking changes when updating to 6.3.0 ❗❗

- 6.3.0 supports Universal Windows Platform. As part of this update, the AppsFlyerObjectScript changes to include the app_id for your UWP app. If you made changes to this file, please merge them with the new AppsFlyerObjectScript.
Please also note that you can leave the uwp app id field empty. 

- From version `6.3.0`, we use `xcframework` for iOS platform, then you need to use cocoapods version >= 1.10

## <a id="add-sdk-to-project"> 📲 Adding the SDK to your project

**Using unitypackage:**
1. Clone / download this repository.
2. [Import](https://docs.unity3d.com/Manual/AssetPackages.html) the appsflyer-unity-plugin-*.unitypackage into your Unity project.
3. Go to Assets >> Import Package >> Custom Package.
4. Select the appsflyer-unity-plugin-*.unitypackage file.

> **Note:** The plugin uses the [The External Dependency Manager for Unity](https://github.com/googlesamples/unity-jar-resolver) (EDM4U) (formerly Play Services Resolver / Jar Resolver). If you do not want to use EDM4U see the [Installation guide](/docs/Installation.md) for more details.

**Using Unity Package Manager:**
1. Go to your packages folder, and open `manifest.json` 
2. Add Google game package registery fpr the external dependency Manager. 
```
  "scopedRegistries": [
    {
      "name": "Game Package Registry by Google",
      "url": "https://unityregistry-pa.googleapis.com",
      "scopes": [
        "com.google"
      ]
    }
  ]
```

4. Add appsflyer-unity-plugin in the dependency :
Add this line for the regular mode
```
 "appsflyer-unity-plugin": "https://github.com/AppsFlyerSDK/appsflyer-unity-plugin.git#upm"
```
 Or this line for Strict mode :
```
 "appsflyer-unity-plugin": "https://github.com/AppsFlyerSDK/appsflyer-unity-plugin.git#Strict-upm"
```


## <a id="init-sdk"> 🚀 Initializing the SDK

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

> **Note:** Make sure not to call destroy on the game object. 


 ## <a id="guides"> 📖 Guides


- [Installation](/docs/Installation.md)
- [Init SDK](/docs/Guides.md#init-sdk)
- [Deep Linking](/docs/Guides.md#deeplinking)
- [Uninstall](/docs/Guides.md#track-app-uninstalls)
- [User Invite](/docs/Guides.md#-user-invite-attribution)


## <a id="api"> 📑 API
  
See the full [API](/docs/API.md) available for this plugin.

## <a id="migration"> ⏩ Migration 
  
Migrating from the old plugin? (version V4) <br/>
View the migration docs [here](/docs/MigrationGuide.md).

⚠️ There are **breaking** changes when migrating to `Unity v5`. This includes new API, different class/package names, and the removal of `com.appsflyer.GetDeepLinkingActivity`.



