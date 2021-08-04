# Collect Idfa with ATTrackingManager
    
   
1. Add the `AppTrackingTransparency` framework to your xcode project. 
2. In the `Info.plist`:
    1. Add an entry to the list: Press +  next to `Information Property List`.
    2. Scroll down and select `Privacy - Tracking Usage Description`.
    3. Add as the value the wording you want to present to the user when asking for permission to collect the IDFA.
3. Call the `waitForATTUserAuthorizationWithTimeoutInterval` api before `startSDK()`
    
    ```c#
    #if UNITY_IOS && !UNITY_EDITOR
    AppsFlyeriOS.waitForATTUserAuthorizationWithTimeoutInterval(60);
    #endif
    ```
        
4. In the `AppsFlyerAppController` class, add:
    
    ```objectivec
    #import <AppTrackingTransparency/ATTrackingManager.h>
    
    ...
    
    - (void)didFinishLaunching:(NSNotification*)notification {
    
    if (@available(iOS 14, *)) {
          [ATTrackingManager requestTrackingAuthorizationWithCompletionHandler:^(ATTrackingManagerAuthorizationStatus status){
          }];
      }
    ...
    }
    
    ```
    
