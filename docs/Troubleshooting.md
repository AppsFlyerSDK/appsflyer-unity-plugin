---
title: Troubleshooting
category: 5f9705393c689a065c409b23
parentDoc: 6370c9e2441a4504d6bca3bd
order: 10
hidden: false
---

# iOS Swizzling 

* AppsFlyer Unity Plugin uses the [iOS life cycle](https://developer.apple.com/documentation/uikit/app_and_environment/managing_your_app_s_life_cycle) events for the SDK to work. 
* The plugins uses [UnityAppController](https://docs.unity3d.com/Manual/UnityasaLibrary-iOS.html) for the lifecycle events to be invoked.
* Sometimes other plugins (Firebase, Facebook, ect) use the same UnityAppController, which creates conflicts in the lifecycle events.
* These events include didBecomeActive, didEnterBackground, didReceiveRemoteNotification, continueUserActivity and openURL.
* When a conflict occurs these methods may not be invoked. 
* The solution provided by the AppsFlyer Unity Plugin is [Swizzling](https://medium.com/rocknnull/ios-to-swizzle-or-not-to-swizzle-f8b0ed4a1ce6).
* Starting from `v6.0.7` there is an option to enable swizzling automatically. 

To enable Swizzling, you have 3 options: 
* For versions up to `6.5.3`
    - [Using info .plist](#using-info-plist)
    - [Using a c# Script](#using-a-c-script)
* From version `6.5.3`
    - [Using macroprocessor starting v6.5.3](#using-macroprocessor)


## Using info .plist

* To enable swizzling, in the info.plist file, a boolean K/V called `AppsFlyerShouldSwizzle` should be set to 1 (true).
* This will automatically enable swizzling and solve conflicts with other plugins.
* Validate that the code in the [AppsFlyer+AppController](https://github.com/AppsFlyerSDK/appsflyer-unity-plugin/blob/master/Assets/AppsFlyer/Plugins/iOS/AppsFlyer%2BAppController.m) is called on the native side.
* Comment out `IMPL_APP_CONTROLLER_SUBCLASS(AppsFlyerAppController)` in AppsFlyerAppController.mm.

---

## Using a c# Script
1. Create a new c# script. (we called ours AFUpdatePlist.cs)
2. Place the script in a editor folder (Assets > Editor > AFUpdatePlist.cs)
3. The code in the script should look like this:

```c#
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;

public class MyBuildPostprocessor {
    
    [PostProcessBuildAttribute]
    public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject) {
        
        if (target == BuildTarget.iOS)
        {
            string plistPath = pathToBuiltProject + "/Info.plist";
            PlistDocument plist = new PlistDocument();
            plist.ReadFromString(File.ReadAllText(plistPath));
            
            PlistElementDict rootDict = plist.root;
            rootDict.SetBoolean("AppsFlyerShouldSwizzle", true);
            
            File.WriteAllText(plistPath, plist.WriteToString());
            
            Debug.Log("Info.plist updated with AppsFlyerShouldSwizzle");
        }
        
    }
}
```

4. Validate that the code in the [AppsFlyer+AppController](https://github.com/AppsFlyerSDK/appsflyer-unity-plugin/blob/master/Assets/AppsFlyer/Plugins/iOS/AppsFlyer%2BAppController.m) is called on the native side.
5. Comment out `IMPL_APP_CONTROLLER_SUBCLASS(AppsFlyerAppController)` in AppsFlyerAppController.mm.

---

## Using macroprocessor
* Add the [preprocessor macro](https://stackoverflow.com/a/26928784) flag `​AFSDK_SHOULD_SWIZZLE=1` to the build settings of the project. 

![alt text](https://user-images.githubusercontent.com/61788924/199495968-7aa911ed-27c4-4e5b-a496-3771d0405fd4.jpeg)

* Validate that the code in the [AppsFlyer+AppController](https://github.com/AppsFlyerSDK/appsflyer-unity-plugin/blob/master/Assets/AppsFlyer/Plugins/iOS/AppsFlyer%2BAppController.m) is called on the native side.
    
--- 
    
# Updating the info.plist
In this example, we will update the info.plist to send SKAN postbacks to AppsFlyer, but the script can be adjusted to update any key in the info.plist
    
1. Create a new c# script. (we called ours AFUpdatePlist.cs)
2. Place the script in a editor folder (Assets > Editor > AFUpdatePlist.cs)
3. The code in the script should look like this:

```c#
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;

public class MyBuildPostprocessor
{

    [PostProcessBuildAttribute]
    public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
    {

        if (target == BuildTarget.iOS)
        {
            string plistPath = pathToBuiltProject + "/Info.plist";
            PlistDocument plist = new PlistDocument();
            plist.ReadFromString(File.ReadAllText(plistPath));

            PlistElementDict rootDict = plist.root;
            rootDict.SetString("NSAdvertisingAttributionReportEndpoint", "https://appsflyer-skadnetwork.com/");
    
            /*** To add more keys :
            ** rootDict.SetString("<your key>", "<your value>");
            ***/

            File.WriteAllText(plistPath, plist.WriteToString());

            Debug.Log("Info.plist updated with NSAdvertisingAttributionReportEndpoint");
        }

    }
}
```
