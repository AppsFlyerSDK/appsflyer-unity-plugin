---
title: Installation
category: 5f9705393c689a065c409b23
parentDoc: 6370c9e2441a4504d6bca3bd
order: 0
hnameden: false
---

# Adding appsflyer-unity-plugin to your project

## Adding the SDK to your project

In order to add the plugin to your project, you can either add the *unitypackage* **or** use *Unity Package Manager*. 
- [Installation adding the unitypackage](#using-unitypackage)
- [Installation using Unity Package Manager](#using-unity-package-manager)

**Note:**  The plugin is built with [The External Dependency Manager for Unity](https://github.com/googlesamples/unity-jar-resolver) (EDM4U) (formerly Play Services Resolver / Jar Resolver)
* The External Dependency Manager for Unity is distributed with the `appsflyer-unity-plugin` by default.
* This will ease the integration process, by resolving dependency conflicts between your plugin and other plugins in your project.
* Adding the `appsflyer-unity-plugin.v*.unitypackage` will automatically import all the assets required for both the AppsFlyer SDK and the External Dependency Manager for Unity.

## Using unitypackage
1. Clone / download the [plugin repository](https://github.com/AppsFlyerSDK/appsflyer-unity-plugin).
2. [Import](https://docs.unity3d.com/Manual/AssetPackages.html) the appsflyer-unity-plugin-*.unitypackage into your Unity project.
3. Go to Assets >> Import Package >> Custom Package.
4. Select the appsflyer-unity-plugin-*.unitypackage file.

**Note:** If you do not wish to include [The External Dependency Manager for Unity](https://github.com/googlesamples/unity-jar-resolver), refer to the steps of this [installation](#installation-with-unity-jar-resolver).

## Using Unity Package Manager

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
**Note:** If you do not wish to include [The External Dependency Manager for Unity](https://github.com/googlesamples/unity-jar-resolver), refer to steps 2 & 3 [here](#installation-without-unity-jar-resolver).


4. Add appsflyer-unity-plugin in the dependency :
Add this line for the regular mode
```
 "appsflyer-unity-plugin": "https://github.com/AppsFlyerSDK/appsflyer-unity-plugin.git#upm"
```
 Or this line for Strict mode :
```
 "appsflyer-unity-plugin": "https://github.com/AppsFlyerSDK/appsflyer-unity-plugin.git#Strict-upm"
```

---

# Installation without unity-jar-resolver
  
  * If you do not wish to include [The External Dependency Manager for Unity](https://github.com/googlesamples/unity-jar-resolver) then follow these steps:
  1. import `appsflyer-unity-plugin.v*.unitypackage` to your project but make sure to uncheck the `EDM4U` dependencies.
  <img src="https://user-images.githubusercontent.com/61788924/199495968-7aa911ed-27c4-4e5b-a496-3771d0405fd4.jpeg"  width="350">

  2. Download and add the required Android dependencies to the Assets/Plugins/Android folder:
      1. [AppsFlyer Android SDK](https://repo1.maven.org/maven2/com/appsflyer/af-android-sdk/6.4.3/af-android-sdk-6.4.3.aar)
      2. [AppsFlyer Unity Wrapper](https://repo1.maven.org/maven2/com/appsflyer/unity-wrapper/6.4.4/unity-wrapper-6.4.4.aar)
      3. [Google Installreferrer library](https://mvnrepository.com/artifact/com.android.installreferrer/installreferrer/2.1)
  3. Download and add the required iOS dependencies to the Assets/Plugins/iOS/AppsFlyer folder:
      1. [Download](https://s3.eu-west-1.amazonaws.com/download.appsflyer.com/ios/6.x.x/6.4.x/6.4.4/AF-iOS-SDK-v6.4.4.zip) the iOS SDK as a static library
      2. Unzip the file you downloaded
      3. Drag & drop all the files into the `Assets/Plugins/iOS/AppsFlyer` folder
