# Troubleshooting

### iOS Swizzling 

* AppsFlyer Unity Plugin uses the [iOS life cycle](https://developer.apple.com/documentation/uikit/app_and_environment/managing_your_app_s_life_cycle) events for the SDK to work. 
* The plugins uses [UnityAppController](https://docs.unity3d.com/Manual/UnityasaLibrary-iOS.html) for the lifecycle events to be invoked.
* Sometimes other plugins (Firebase, Facebook, ect) use the same UnityAppController, which creates conflicts in the lifecycle events.
* These events include didBecomeActive, didEnterBackground, didReceiveRemoteNotification, continueUserActivity and openURL.
* When a conflict occurs these methods may not be invoked. 
* The solution provided by the AppsFlyer Unity Plugin is [Swizzling](https://medium.com/rocknnull/ios-to-swizzle-or-not-to-swizzle-f8b0ed4a1ce6).
* Starting from `v6.0.7` there is an option to enable swizzling automatically. 
* To enable swizzling, in the info.plist file, a boolean K/V called `AppsFlyerShouldSwizzle` should be set to 1 (true).
* This will automatically enable swizzling and solve conflicts with other plugins.
* This can also be accomplished on the c# side by following these steps:

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

