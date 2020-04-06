# Adding appsflyer-unity-plugin to your project


- [Installation with unity-jar-resolver](#installation-with-unity-jar-resolver)
- [Installation without unity-jar-resolver](#installation-without-using-unity-jar-resolver)

## <a id="installation-with-unity-jar-resolver"> Installation with unity-jar-resolver
  
* The plugin is built with [The External Dependency Manager for Unity](https://github.com/googlesamples/unity-jar-resolver) (EDM4U) (formerly Play Services Resolver / Jar Resolver)
* The External Dependency Manager for Unity is distributed with the `appsflyer-unity-plugin` by default.
* This will ease the integration process, by resolving dependency conflicts between your plugin and other plugins in your project.
* Adding the `appsflyer-unity-plugin.v*.unitypackage` will automatically import all the assets required for both the AppsFlyer SDK and the External Dependency Manager for Unity.

<img src="https://firebasestorage.googleapis.com/v0/b/firstintegrationapp.appspot.com/o/Screen%20Shot%202020-04-02%20at%2014.38.02.png?alt=media&token=5044f527-d8ef-456c-a30c-7beb808ffaa5"  width="350">

## <a id="installation-without-using-unity-jar-resolver"> Installation without unity-jar-resolver
  
  * If you do not wish to include [The External Dependency Manager for Unity](https://github.com/googlesamples/unity-jar-resolver) then follow these steps:
  1. import `appsflyer-unity-plugin.v*.unitypackage` to your project but make sure to uncheck the `EDM4U` dependencies.
  <img src="https://firebasestorage.googleapis.com/v0/b/firstintegrationapp.appspot.com/o/Screen%20Shot%202020-04-02%20at%2014.38.30.png?alt=media&token=e556a324-b3b7-418c-8d2f-52ea9cf70f86"  width="350">

  2. Download and add the required Android dependencies to the Assets/Plugins/Android folder:
      1. [AppsFlyer Android SDK](https://repo1.maven.org/maven2/com/appsflyer/af-android-sdk/5.2.0/af-android-sdk-5.2.0.aar)
      2. [AppsFlyer Unity Wrapper](https://repo1.maven.org/maven2/com/appsflyer/unity-wrapper/5.2.0/unity-wrapper-5.2.0.aar)
      3. [Google Installreferrer library](https://mvnrepository.com/artifact/com.android.installreferrer/installreferrer/1.0)
  3. Download and add the required iOS dependencies to the Assets/Plugins/iOS/AppsFlyer folder:
      1. [Download](https://s3-eu-west-1.amazonaws.com/download.appsflyer.com/ios/AF-iOS-SDK.zip) the iOS SDK as a static library
      2. Unzip the file you downloaded
      3. Drag & drop all the files into the `Assets/Plugins/iOS/AppsFlyer` folder
