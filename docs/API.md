# API

<img src="https://massets.appsflyer.com/wp-content/uploads/2018/06/20092440/static-ziv_1TP.png"  width="400" >

The list of available methods for this plugin is described below.
-  Android & iOS API
    - [initSDK](#initSDK)
    - [startSDK](#startSDK)
    - [sendEvent](#sendEvent)
    - [stopSDK](#stopSDK)
    - [isSDKStopped](#isSDKStopped)
    - [getSdkVersion](#getSdkVersion)
    - [setIsDebug](#setIsDebug)
    - [setCustomerUserId](#setCustomerUserId)
    - [setAppInviteOneLinkID](#setAppInviteOneLinkID)
    - [setAdditionalData](#setAdditionalData)
    - [setResolveDeepLinkURLs](#setResolveDeepLinkURLs)
    - [setOneLinkCustomDomain](#setOneLinkCustomDomain)
    - [setCurrencyCode](#setCurrencyCode)
    - [recordLocation](#recordLocation)
    - [anonymizeUser](#anonymizeUser)
    - [getAppsFlyerId](#getAppsFlyerId)
    - [setMinTimeBetweenSessions](#setMinTimeBetweenSessions)
    - [setHost](#setHost)
    - [setUserEmails](#setUserEmails)
    - [getConversionData](#getConversionData)
    - [attributeAndOpenStore](#attributeAndOpenStore)
    - [recordCrossPromoteImpression](#recordCrossPromoteImpression)
    - [generateUserInviteLink](#generateUserInviteLink)
- [Android Only API](#androidOnly)
    - [updateServerUninstallToken](#updateServerUninstallToken)
    - [setImeiData](#setImeiData)
    - [setAndroidIdData](#setAndroidIdData)
    - [waitForCustomerUserId](#waitForCustomerUserId)
    - [setCustomerIdStartSDK](#setCustomerIdAndStartSDK)
    - [getOutOfStore](#getOutOfStore)
    - [setOutOfStore](#setOutOfStore)
    - [setCollectAndroidID](#setCollectAndroidID)
    - [setCollectIMEI](#setCollectIMEI)
    - [setIsUpdate](#setIsUpdate)
    - [setPreinstallAttribution](#setPreinstallAttribution)
    - [isPreInstalledApp](#isPreInstalledApp)
    - [getAttributionId](#getAttributionId)
    - [handlePushNotifications](#handlePushNotifications)
    - [validateAndSendInAppPurchase](#validateAndSendInAppPurchase)
- [iOS Only API](#iOSOnly)
    - [setShouldCollectDeviceName](#setShouldCollectDeviceName)
    - [setDisableCollectIAd](#setDisableCollectIAd)
    - [setUseReceiptValidationSandbox](#setUseReceiptValidationSandbox)
    - [setUseUninstallSandbox](#setUseUninstallSandbox)
    - [validateAndSendInAppPurchase](#validateAndSendInAppPurchase)
    - [registerUninstall](#registerUninstall)
    - [handleOpenUrl](#handleOpenUrl)

---

##### <a id="initSDK"> **`void initSDK(string devKey, string appID, MonoBehaviour gameObject)`**

Initialize the AppsFlyer SDK with the devKey and appID.
The dev key is required for all apps and the appID is required only for iOS. 
If you app is for Android only pass null for the appID.


| parameter               | type                        | description  |
| -----------             |-----------------------------|--------------|
| `dev_key`               | `string`                    | AppsFlyer's Dev-Key, which is accessible from your AppsFlyer account under 'App Settings' in the dashboard.                  |
| `app_id`                | `string`                    | Your app's Apple ID. |
| `gameObject` (optional) | `MonoBehaviour`             |  The game object containing the IAppsFlyerConversionData interface.           |

*Example:*

```c#
AppsFlyer.initSDK("dev_key", "app_id"); // without deeplinking
AppsFlyer.initSDK("dev_key", "app_id", this); // with deeplinking
```

**Note :** You only need to implement the SDK **with deeplinking** if you are using the `IAppsFlyerConversionData` interface.

---

##### <a id="startSDK"> **`void startSDK()`**
    
Once this API is invoked the SDK will start,  sessions will be immediately sent, and all background foreground transitions will record a session.

*Example:*

```c#
 AppsFlyer.startSDK();
```

---

##### <a id="sendEvent"> **`void sendEvent(string eventName, Dictionary<string, string> eventValues)`**

In-App Events provide insight on what is happening in your app. It is recommended to take the time and define the events you want to measure to allow you to measure ROI (Return on Investment) and LTV (Lifetime Value).

Recording in-app events is performed by calling sendEvent with event name and value parameters. See In-App Events documentation for more details.

**Note:** An In-App Event name must be no longer than 45 characters. Events names with more than 45 characters do not appear in the dashboard, but only in the raw Data, Pull and Push APIs.


| parameter      | type                         | description                                   |
| -----------    |----------------------------- |------------------------------------------     |
| `eventName`    | `string`                     | The name of the event                         |
| `eventValues`  | `Dictionary<string, string>` | The event values that are sent with the event |


*Example:*

```c#
Dictionary<string, string> eventValues = new Dictionary<string, string>();
eventValues.Add(AFInAppEvents.CURRENCY, "USD");
eventValues.Add(AFInAppEvents.REVENUE, "0.99");
eventValues.Add("af_quantity", "1");
AppsFlyer.sendEvent(AFInAppEvents.PURCHASE, eventValues);
```

---

##### <a id="stopSDK"> **`void stopSDK(bool isSDKStopped)`**

In some extreme cases you might want to shut down all SDK functions due to legal and privacy compliance. This can be achieved with the stopSDK API. Once this API is invoked, our SDK no longer communicates with our servers and stops functioning.

There are several different scenarios for user opt-out. We highly recommend following the exact instructions for the scenario, that is relevant for your app.

In any event, the SDK can be reactivated by calling the same API, by passing false.

 **Important :**
Do not call startSDK() if stopSDK() is set to true.

To restart SDK functions again, use the following API:

`AppsFlyer.stopSDK(false);`

 **Warning**
Use the stopSDK API only in cases where you want to fully ignore the user's SDK functions. Using this API SEVERELY impacts your attribution, data collection and deep linking mechanism.

| parameter       | type    | description                                          |
| -------------   |---------|----------------------------                          |
| `isSDKStopped`  | `bool`  | True if the SDK is stopped (default value is false). |

*Example:*

```c#
AppsFlyer.stopSDK(true);
```

---

##### <a id="isSDKStopped"> **`bool isSDKStopped()`**

Was the stopSDK(boolean) API set to true.

*Example:*

```c#
if (!AppsFlyer.isSDKStopped())
{
  
}
```

---

##### <a id="getSdkVersion"> **`string getSdkVersion()`**

Get the AppsFlyer SDK version used in the app.

*Example:*

```c#
string version = AppsFlyer.getSdkVersion();
```

---

##### <a id="setIsDebug"> **`void setIsDebug(bool shouldEnable)`**

Enables Debug logs for the AppsFlyer SDK.

 **Warning**
Only set to true in development / debug.

| parameter      | type   | description                                 |
| -----------    |------- |---------------------------------------------|
| `shouldEnable` | `bool` | True if debug mode is on (default is false) |

*Example:*

```c#
AppsFlyer.setIsDebug(true);
```

---

##### <a id="setCustomerUserId"> **`void setCustomerUserId(string id)`**

Setting your own Custom ID enables you to cross-reference your own unique ID with AppsFlyer’s user ID and the other devices’ IDs. This ID is available in AppsFlyer CSV reports along with postbacks APIs for cross-referencing with your internal IDs.

| parameter | type     | description     |
| --------- |--------- |--------------   |
| `id`      | `string` | custom user ID  |

*Example:*

```c#
AppsFlyer.setCustomerUserId("custom_user_id");
```

---

##### <a id="setAppInviteOneLinkID"> **`void setAppInviteOneLinkID(string oneLinkId)`**

Set the OneLink ID that should be used for User-Invite-API
The link that is generated for the user invite will use this OneLink as the base link.

| parameter   | type     | description                            |
| ----------- |--------- |----------------------------------------|
| `oneLinkId` | `string` | OneLink ID for User-Invite attribution |

*Example:*

```c#
AppsFlyer.setAppInviteOneLinkID("abcd");
```

---

##### <a id="setAdditionalData"> **`void setAdditionalData(Dictionary<string, string> customData)`**

The setAdditionalData API is required to integrate on the SDK level with several external partner platforms, including Segment, Adobe and Urban Airship. Use this API only if the integration article of the platform specifically states setAdditionalData API is needed. 

| parameter    | type                         | description     |
| -----------  |----------------------------- |--------------   |
| `customData` | `Dictionary<string, string>` | additional data |

*Example:*

```c#
Dictionary<string, string> customData = new Dictionary<string, string>();
customData.Add("custom1", "someData");
AppsFlyer.setAdditionalData(customData);
```
---

##### <a id="setResolveDeepLinkURLs"> **`void setResolveDeepLinkURLs(params string[] urls)`**

If you are using OneLinks which support Android App Links and wrapping them with a 3rd Party Universal Link, you can use the setResolveDeepLinkURLs API to notify the AppsFlyer SDK which click domains that invoke the app should be resolved by the SDK and have the underlying OneLink extracted from them. This will allow you to maintain deep linking and attribution while wrapping the OneLink with a 3rd party Universal Link. Make sure to call this API before SDK initialization.

| parameter | type              | description   |
| ----------|-------------------|-------------- |
| `urls`    | `params string[]` | array of urls |

*Example:*

```c#
AppsFlyer.setResolveDeepLinkURLs("test.com", "test2.ca");
```

---

##### <a id="setOneLinkCustomDomain"> **`void setOneLinkCustomDomain(params string[] domains)`**
    
Advertisers can use this method to set vanity onelink domains.

| parameter  | type              | description             |
| ---------- | ----------------- |-------------------------|
| `domains`  | `params string[]` | array of custom domains |

*Example:*

```c#
 AppsFlyer.setOneLinkCustomDomain("test.domain", "test2.domain");
```

---

##### <a id="setCurrencyCode"> **`void setCurrencyCode(string currencyCode)`**

Setting user local currency code for in-app purchases.
The currency code should be a 3 character ISO 4217 code. (default is USD).
You can set the currency code for all events by calling the following method.

| parameter      | type     | description                                 |
| -----------    |----------|---------------------------------------------|
| `currencyCode` | `string` | 3 character ISO 4217 code. (default is USD) |

*Example:*

```c#
AppsFlyer.setCurrencyCode("GBP");
```

---

##### <a id="recordLocation"> **`void recordLocation(double latitude, double longitude)`**

Manually record the location of the user.

| parameter   | type     | description       |
| ----------- |--------- |--------------     |
| `latitude`  | `double` | latitude of user  |
| `longitude` | `double` | longitude of user |


*Example:*

```c#
AppsFlyer.recordLocation(40.7128, 74.0060);
```

---

##### <a id="anonymizeUser"> **`void anonymizeUser(bool shouldAnonymizeUser)`**

AppsFlyer provides you with a method to anonymize specific user identifiers in AppsFlyer analytics. This method complies with the latest privacy requirements and complies with Facebook data and privacy policies. Default is NO, meaning no anonymization is performed by default.
Use this API during the SDK Initialization to explicitly anonymize a user's installs, events and sessions.
You can cancel anonymization by calling anonymizeUser again, set to false.

**Warning**
Anonymizing users SEVERELY impacts your attribution information. Use this option ONLY for regions which legally prevents you from collecting your users' information.

| parameter             | type   | description                   |
| --------------------  |--------|-------------------------------|
| `shouldAnonymizeUser` | `bool` | true to perform anonymization |

*Example:*

```c#
AppsFlyer.anonymizeUser(true);
```

---

##### <a id="getAppsFlyerId"> **`string getAppsFlyerId()`**

AppsFlyer's unique device ID is created for every new install of an app. Use the following API to obtain AppsFlyer’s Unique ID.

*Example:*

```c#
string uid = AppsFlyer.getAppsFlyerId(); 
```

---

##### <a id="setMinTimeBetweenSessions"> **`void setMinTimeBetweenSessions(int seconds)`**

By default, at least 5 seconds must lapse between 2 app launches to count as separate 2 sessions (more about counting sessions). However, you can use the following API to set your custom value for the minimum required time between sessions.

**Note:** Setting a high value to the custom time between launches may badly impact APIs relying on sessions data, such as deep linking.

| parameter   | type   | description                                  |
| ----------- |------- |--------------------------------------------- |
| `seconds`   | `int`  | time between sessions (default is 5 seconds) |

*Example:*

```c#
AppsFlyer.setMinTimeBetweenSessions(4);
```

---

##### <a id="setHost"> **`void setHost(string hostPrefixName, string hostName)`**

Set a custom host.

| parameter        | type      | description  |
| -----------      |---------- |--------------|
| `hostPrefixName` | `string`  |              |
| `hostName`       | `string`  |              |


*Example:*

```c#
AppsFlyer.setHost("hostPrefixName","hostName");
```

---

##### <a id="setUserEmails"> **`void setUserEmails(EmailCryptType cryptMethod, params string[] emails)`**

Set the user emails and encrypt them.

cryptMethod Encryption methods:
EmailCryptType.EmailCryptTypeSHA256
EmailCryptType.EmailCryptTypeNone


| parameter     | type              | description               |
| -----------   |-------------------|---------------------------|
| `cryptMethod` | `EmailCryptType`  | none, or sha256 |
| `emails`      | `params string[]` | list of emails            |


*Example:*

```c#
AppsFlyer.setUserEmails(EmailCryptType.EmailCryptTypeSHA256, "test1@test1.com", "test2@test2.com");
```

---

##### <a id="getConversionData"> **`void getConversionData(string objectName);`**

Register a Conversion Data Listener.
Allows the developer to access the user attribution data in real-time for every new install, directly from the SDK level.
By doing this you can serve users with personalized content or send them to specific activities within the app,
which can greatly enhance their engagement with your app.

Get the callbacks by implementing the IAppsFlyerConversionData interface.

| parameter    | type     | description                                             |
| -----------  |----------|-------------------------------------------------------- |
| `objectName` | `string` | game object with the IAppsFlyerConversionData interface |

*Example:*

```c#
AppsFlyer.getConversionData(gameObject.name);
```

---

##### <a id="attributeAndOpenStore"> **`void attributeAndOpenStore(string appID, string campaign, Dictionary<string, string> userParams, MonoBehaviour gameObject)`**

Use the following API to attribute the click and launch the app store's app page.

Get the callbacks by implementing the IAppsFlyerUserInvite interface.

| parameter     | type                         | description                                         |
| -----------   |----------------------------- |-----------------------------------------------------|
| `appID`       | `string`                     |                                                     |
| `campaign`    | `string`                     |                                                     |
| `userParams`  | `Dictionary<string, string> `|                                                     |
| `gameObject`  | `MonoBehaviour`              | game object with the IAppsFlyerUserInvite interface |

*Example:*

```c#
Dictionary<string, string> parameters = new Dictionary<string, string>();
parameters.Add("af_sub1", "val");
parameters.Add("custom_param", "val2");
AppsFlyer.attributeAndOpenStore("123456789", "test campaign", parameters, this);
```

---

##### <a id="recordCrossPromoteImpression"> **`void recordCrossPromoteImpression(string appID, string campaign);`**

To attribute an impression use the following API call.
Make sure to use the promoted App ID as it appears within the AppsFlyer dashboard.

| parameter    | type      | description  |
| -----------  |-----------|--------------|
| `appID`      | `string`  | appID        |
| `campaign`   | `string`  | campaign     |


*Example:*

```c#
AppsFlyer.recordCrossPromoteImpression("appID", "campaign");
```

---

##### <a id="generateUserInviteLink"> **`void generateUserInviteLink(Dictionary<string, string> parameters, MonoBehaviour gameObject)`**

The LinkGenerator class builds the invite URL according to various setter methods which allow passing on additional information on the click.
See - https://support.appsflyer.com/hc/en-us/articles/115004480866-User-invite-attribution-


| parameter    | type                         | description                                         |
| -----------  |----------------------------- |-----------------------------------------------------|
| `parameters` | `Dictionary<string, string>` |                                                     |
| `gameObject` | `MonoBehaviour`              | game object with the IAppsFlyerUserInvite interface |

*Example:*

```c#
AppsFlyer.generateUserInviteLink(params, this);
```

---

## <a id="androidOnly"> Android Only API
  
##### <a id="updateServerUninstallToken"> **`void updateServerUninstallToken(string token)`**
 
 Manually pass the Firebase Device Token for Uninstall measurement.

| parameter   | type      | description        |
| ----------  |---------- |--------------------|
| `token`     | `string`  | Firebase FCM token |


*Example:*

```c#
#if UNITY_ANDROID && !UNITY_EDITOR
        AppsFlyerAndroid.updateServerUninstallToken("token");
#endif
```

---

##### <a id="setImeiData"> **`void setImeiData(string imei)`**
 
By default, IMEI and Android ID are not collected by the SDK if the OS version is higher than KitKat (4.4)
and the device contains Google Play Services(on SDK versions 4.8.8 and below the specific app needed GPS).
Use this API to explicitly send IMEI to AppsFlyer.


| parameter   | type     | description   |
| ----------- |----------|-------------- |
| `imei`      | `string` | device's IMEI |


*Example:*

```c#
#if UNITY_ANDROID && !UNITY_EDITOR
        AppsFlyerAndroid.setImeiData("imei");
#endif
```

---

##### <a id="setAndroidIdData"> **`void setAndroidIdData(string androidId)`**
 
By default, IMEI and Android ID are not collected by the SDK if the OS version is higher than KitKat(4.4)and the device contains Google Play Services(on SDK versions 4.8.8 and below the specific app needed GPS).
Use this API to explicitly send Android ID to AppsFlyer.

| parameter   | type     | description          |
| ----------- | ---------|--------------------- |
| `androidId` | `string` | device's Android ID  |

*Example:*

```c#
#if UNITY_ANDROID && !UNITY_EDITOR
        AppsFlyerAndroid.setAndroidIdData("androidId");
#endif
```

---

##### <a id="waitForCustomerUserId"> **`void waitForCustomerUserId(bool wait)`**
 
It is possible to delay the SDK Initialization until the customerUserID is set.
This feature makes sure that the SDK doesn't begin functioning until the customerUserID is provided.
If this API is used, all in-app events and any other SDK API calls are discarded, until the customerUserID is provided.


| parameter   | type    | description                                         |
| ----------  |-------- |-----------------------------------------------------|
| `wait`      | `bool`  | True if you want the SDK to wait for customerUserID |


*Example:*

```c#
#if UNITY_ANDROID && !UNITY_EDITOR
        AppsFlyerAndroid.waitForCustomerUserId(true);
#endif
```

---

 ##### <a id="setCustomerIdAndStartSDK"> **`void setCustomerIdAndStartSDK(string id)`**
 
Use this API to provide the SDK with the relevant customer user id and trigger the SDK to begin its normal activity.

| parameter   | type      | description             |
| ----------- |---------- |------------------------ |
| `id`        | `string`  | Customer ID for client. |

*Example:*

```c#
#if UNITY_ANDROID && !UNITY_EDITOR
        AppsFlyerAndroid.setCustomerIdStartSDK("id");
#endif
```

---

 ##### <a id="getOutOfStore"> **`string getOutOfStore()`**
 
 Get the current AF_STORE value.

*Example:*

```c#
#if UNITY_ANDROID && !UNITY_EDITOR
        string af_store = AppsFlyerAndroid.getOutOfStore();
#endif
```

---

 ##### <a id="setOutOfStore"> **`void setOutOfStore(string sourceName)`**
 
 Manually set the AF_STORE value.

| parameter    | type      | description  |
| -----------  |---------- |--------------|
| `sourceName` | `string`  |              |

*Example:*

```c#
#if UNITY_ANDROID && !UNITY_EDITOR
        AppsFlyerAndroid.setOutOfStore("sourceName");
#endif
```

---

 ##### <a id="setCollectAndroidID"> **`void setCollectAndroidID(bool isCollect)`**

Opt-out of collection of Android ID.
If the app does NOT contain Google Play Services, Android ID is collected by the SDK.
However, apps with Google play services should avoid Android ID collection as this is in violation of the Google Play policy.
 
| parameter   | type     | description  |
| ----------- |--------- |--------------|
| `isCollect` | `bool`   |              |


*Example:*

```c#
#if UNITY_ANDROID && !UNITY_EDITOR
        AppsFlyerAndroid.setCollectAndroidID(true);
#endif
```

---


 ##### <a id="setCollectIMEI"> **`void setCollectIMEI(bool isCollect)`**
 
Opt-out of collection of IMEI.
If the app does NOT contain Google Play Services, device IMEI is collected by the SDK.
However, apps with Google play services should avoid IMEI collection as this is in violation of the Google Play policy.

| parameter          | type                        | description  |
| -----------        |-----------------------------|--------------|
| `isCollect`        | `bool`                      |              |

*Example:*

```c#
#if UNITY_ANDROID && !UNITY_EDITOR
        AppsFlyerAndroid.setCollectIMEI(true);
#endif
```

---

 ##### <a id="setIsUpdate"> **`void setIsUpdate(bool isUpdate)`**
 
Manually set that the application was updated.

| parameter   | type    | description             |
| ----------- |-------- |-------------------------|
| `isUpdate`  | `bool`  | true if app was updated |

*Example:*

```c#
#if UNITY_ANDROID && !UNITY_EDITOR
        AppsFlyerAndroid.setIsUpdate(true);
#endif
```

---

 ##### <a id="setPreinstallAttribution"> **`void setPreinstallAttribution(string mediaSource, string campaign, string siteId)`**
 
 Specify the manufacturer or media source name to which the preinstall is attributed.

| parameter      | type     | description                                                   |
| -----------    |----------|---------------------------------------------------------------|
| `mediaSource`  | `string` | Manufacturer or media source name for preinstall attribution. |
| `campaign`     | `string` | Campaign name for preinstall attribution.                     |
| `siteId`       | `string` | Site ID for preinstall attribution.                           |

*Example:*

```c#
#if UNITY_ANDROID && !UNITY_EDITOR
        AppsFlyerAndroid.setPreinstallAttribution("mediaSource", "campaign", "siteId");
#endif
```

---


 ##### <a id="isPreInstalledApp"> **`bool isPreInstalledApp()`**
 
Boolean indicator for preinstall by Manufacturer.

*Example:*

```c#
#if UNITY_ANDROID && !UNITY_EDITOR
        if (AppsFlyerAndroid.isPreInstalledApp())
        {

        }
#endif
```

---


##### <a id="getAttributionId"> **`string getAttributionId()`**
 
Get the Facebook attribution ID, if one exists.

*Example:*

```c#
#if UNITY_ANDROID && !UNITY_EDITOR
        string attributionId = AppsFlyerAndroid.getAttributionId();
#endif
```

---

##### <a id="handlePushNotifications"> **`void handlePushNotifications()`**
 
When the handlePushNotifications API is called push notifications will be recorded.

*Example:*

```c#
#if UNITY_ANDROID && !UNITY_EDITOR
        AppsFlyerAndroid.handlePushNotifications();
#endif
```

---

##### <a id="validateAndSendInAppPurchase"> **`void validateAndSendInAppPurchase(string publicKey, string signature, string purchaseData, string price, string currency, Dictionary<string, string> additionalParameters, MonoBehaviour gameObject)`**
 
API for server verification of in-app purchases.
An af_purchase event with the relevant values will be automatically sent if the validation is successful.


| parameter              | type     | description  |
| -----------            |----------|--------------|
| `publicKey`            | `string` | License Key obtained from the Google Play Console. |
| `signature`            | `string` | data.INAPP_DATA_SIGNATURE.                         |
| `purchaseData`         | `string` | data.INAPP_PURCHASE_DATA                           |
| `price`                | `string` | Purchase price                                     |
| `currency`             | `string` | Site ID for preinstall attribution.                |
|`additionalParameters`  | `Dictionary<string, string>` | parameters to be sent with the purchase.|
| `gameObject`           | `MonoBehaviour` | Game object for the callbacks to be sent   |

*Example:*

```c#
#if UNITY_ANDROID && !UNITY_EDITOR
        AppsFlyerAndroid.validateAndSendInAppPurchase(
        "publicKey", 
        "signature", 
        "purchaseData", 
        "price", 
        "currency", 
        null, 
        this);
#endif
```

---

## <a id="iOSOnly"> iOS Only API
 
           
##### <a id="setDisableCollectAppleAdSupport"> **`void setDisableCollectAppleAdSupport(bool disable)`**
 
AppsFlyer SDK collects Apple's `advertisingIdentifier` if the `AdSupport.framework` is included in the SDK.
You can disable this behavior by setting the following property to true.

| parameter      | type     | description  |
| -----------    |----------|--------------|
| `disable`      | `bool`   |              |

*Example:*

```c#
#if UNITY_IOS && !UNITY_EDITOR
        AppsFlyeriOS.setDisableCollectAppleAdSupport(true);
#endif
```

---

 ##### <a id="setShouldCollectDeviceName"> **`void setShouldCollectDeviceName(bool shouldCollectDeviceName)`**
 
Set this flag to true, to collect the current device name(e.g. "My iPhone"). Default value is false.
        
| parameter                 | type      | description  |
| ----------------------    |---------- |--------------|
| `shouldCollectDeviceName` | `bool`    |              |

*Example:*

```c#
#if UNITY_IOS && !UNITY_EDITOR
        AppsFlyeriOS.setShouldCollectDeviceName(true);
#endif
```

---

 ##### <a id="setDisableCollectIAd"> **`void setDisableCollectIAd(bool disableCollectIAd)`**
 
Opt-out for Apple Search Ads attributions.

| parameter           | type     | description  |
| -----------         |----------|--------------|
| `disableCollectIAd` | `bool`   |              |
*Example:*

```c#
#if UNITY_IOS && !UNITY_EDITOR
        AppsFlyeriOS.setDisableCollectIAd(true);
#endif
```

---

##### <a id="setUseReceiptValidationSandbox"> **`void setUseReceiptValidationSandbox(bool useReceiptValidationSandbox)`**
 

In app purchase receipt validation Apple environment(production or sandbox). The default value is false.

| parameter                     | type      | description                                  |
| ----------------------------  |---------- |--------------------------------------------- |
| `useReceiptValidationSandbox` | `bool`    | true if In app purchase is done with sandbox |

*Example:*

```c#
#if UNITY_IOS && !UNITY_EDITOR
        AppsFlyeriOS.setUseReceiptValidationSandbox(true);
#endif
```

---

##### <a id="setUseUninstallSandbox"> **`void setUseUninstallSandbox(bool useUninstallSandbox)`**
 
Set this flag to test uninstall on Apple environment(production or sandbox). The default value is false.

| parameter             | type    | description                             |
| -----------           |-------  |---------------------------------------- |
| `useUninstallSandbox` | `bool`  | true if you are using a APN certificate |

*Example:*

```c#
#if UNITY_IOS && !UNITY_EDITOR
        AppsFlyeriOS.setUseUninstallSandbox(true);
#endif
```

---


##### <a id="validateAndSendInAppPurchase"> **`void validateAndSendInAppPurchase(string productIdentifier, string price, string currency, string tranactionId, Dictionary<string, string> additionalParameters, MonoBehaviour gameObject)`**
 
To send and validate in app purchases you can call this method from the processPurchase method.

| parameter              | type                           | description  |
| -----------            |-------------------             | --------------|
| `productIdentifier`    | `string`                       |         The product identifier.     |
| `price`                | `string`                       |   The product price.           |
| `currency`             | `string`                       |    The product currency.          |
| `tranactionId`         | `string`                       |     The purchase transaction Id.         |
| `additionalParameters` | `Dictionary<string, string>`   |    The additional param, which you want to receive it in the raw reports.          |
| `gameObject` | `MonoBehaviour`   |      the game object for the callbacks        |

*Example:*

```c#
#if UNITY_IOS && !UNITY_EDITOR
        AppsFlyeriOS.validateAndSendInAppPurchase(
        "productIdentifier", 
        "price", 
        "currency", 
        "tranactionId", 
        null, 
        this);
#endif
```

---


##### <a id="registerUninstall"> **` void registerUninstall(byte[] deviceToken)`**
 

Register uninstall - you should register for remote notification and provide AppsFlyer the push device token.

| parameter     | type       | description  |
| -----------   |----------  |--------------|
| `deviceToken` | `byte[]`   | APN token    |

*Example:*

```c#
    private bool tokenSent;

    void Update()
    {
#if UNITY_IOS && !UNITY_EDITOR
        if (!tokenSent)
        {
            byte[] token = UnityEngine.iOS.NotificationServices.deviceToken;
            if (token != null)
            {
                AppsFlyeriOS.registerUninstall(token);
                tokenSent = true;
            }
        }
#endif
    }

```

---

##### <a id="handleOpenUrl"> **` void handleOpenUrl(string url, string sourceApplication, string annotation)`**


    In case you want to track deep linking manually call handleOpenUrl.
    The continueUserActivity and onOpenURL are implemented in the AppsFlyerAppController.mm class, so 
    only use this method if the other methods do not cover your apps deeplinking needs.

| parameter     | type       | description  |
| -----------   |----------  |--------------|
| `url`         | `string`   |      The URL to be passed to your AppDelegate        |
| `sourceApplication` | `string`   |    The sourceApplication to be passed to your AppDelegate    |
| `annotation`  | `string`   |       The annotation to be passed to your app delegate       |

*Example:*

```c#
#if UNITY_IOS && !UNITY_EDITOR
    AppsFlyeriOS.handleOpenUrl(string url, string sourceApplication, string annotation);
#endif
```

---
