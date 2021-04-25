## Strict Mode SDK


* Use the [strict mode SDK](https://support.appsflyer.com/hc/en-us/articles/360001422989-User-opt-in-opt-out-in-the-AppsFlyer-SDK#strict-mode-sdk) to completely remove IDFA collection functionality and AdSupport framework dependencies (for example, when developing apps for kids).
* [Strict mode](https://support.appsflyer.com/hc/en-us/articles/207032066-iOS-SDK-V6-X-integration-guide-for-developers#integration-strict-mode-sdk) SDK used pod `pod 'AppsFlyerFramework/Strict','6.1.3'`
* In addition the following API are removed: `disableAdvertisingIdentifier` and `waitForATTUserAuthorizationWithTimeoutInterval`
* To implement strict mode use the appsflyer-unity-plugin-\*.\*.\*-strict-mode.unitypackage