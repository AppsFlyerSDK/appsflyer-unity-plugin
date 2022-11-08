---
title: API reference
category: 600892a5042c550044d58e1b
parentDoc: 6358e561b49b560010d89e2e
order: 9
hidden: false
---

The list of available methods for this plugin is described below.
- [Android, iOS and Windows API](#android-ios-and-windows-api)
  - [initSDK](#initsdk)
  - [startSDK](#startsdk)
- [Android and iOS API](#android-and-ios-api)
  - [stopSDK](#stopsdk)
  - [isSDKStopped](#issdkstopped)
  - [getSdkVersion](#getsdkversion)
  - [setIsDebug](#setisdebug)
  - [setCustomerUserId](#setcustomeruserid)
  - [setAppInviteOneLinkID](#setappinviteonelinkid)
  - [setAdditionalData](#setadditionaldata)
  - [setResolveDeepLinkURLs](#setresolvedeeplinkurls)
  - [setOneLinkCustomDomain](#setonelinkcustomdomain)
  - [setCurrencyCode](#setcurrencycode)
  - [recordLocation](#recordlocation)
  - [anonymizeUser](#anonymizeuser)
  - [getAppsFlyerId](#getappsflyerid)
  - [setMinTimeBetweenSessions](#setmintimebetweensessions)
  - [setHost](#sethost)
  - [setUserEmails](#setuseremails)
  - [setPhoneNumber](#setphonenumber)
  - [getConversionData](#getconversiondata)
  - [attributeAndOpenStore](#attributeandopenstore)
  - [recordCrossPromoteImpression](#recordcrosspromoteimpression)
  - [generateUserInviteLink](#generateuserinvitelink)
  - [setSharingFilterForAllPartners *Deprecated*](#setsharingfilterforallpartners-deprecated)
  - [setSharingFilter *Deprecated*](#setsharingfilter-deprecated)
  - [setSharingFilterForPartners](#setsharingfilterforpartners)
  - [setPartnerData](#setpartnerdata)
- [Android Only API](#android-only-api)
  - [updateServerUninstallToken](#updateserveruninstalltoken)
  - [setImeiData](#setimeidata)
  - [setAndroidIdData](#setandroididdata)
  - [waitForCustomerUserId](#waitforcustomeruserid)
  - [setCustomerIdAndStartSDK](#setcustomeridandstartsdk)
  - [getOutOfStore](#getoutofstore)
  - [setOutOfStore](#setoutofstore)
  - [setCollectAndroidID](#setcollectandroidid)
  - [setCollectIMEI](#setcollectimei)
  - [setIsUpdate](#setisupdate)
  - [setPreinstallAttribution](#setpreinstallattribution)
  - [isPreInstalledApp](#ispreinstalledapp)
  - [getAttributionId](#getattributionid)
  - [handlePushNotifications](#handlepushnotifications)
  - [validateAndSendInAppPurchase](#validateandsendinapppurchase)
  - [setCollectOaid](#setcollectoaid)
  - [setDisableAdvertisingIdentifiers](#setdisableadvertisingidentifiers)
  - [setDisableNetworkData](#setdisablenetworkdata)
- [iOS Only API](#ios-only-api)
  - [setDisableCollectAppleAdSupport](#setdisablecollectappleadsupport)
  - [setShouldCollectDeviceName](#setshouldcollectdevicename)
  - [setDisableCollectIAd](#setdisablecollectiad)
  - [setUseReceiptValidationSandbox](#setusereceiptvalidationsandbox)
  - [setUseUninstallSandbox](#setuseuninstallsandbox)
  - [validateAndSendInAppPurchase](#validateandsendinapppurchase-1)
  - [registerUninstall](#registeruninstall)
  - [handleOpenUrl](#handleopenurl)
  - [waitForATTUserAuthorizationWithTimeoutInterval](#waitforattuserauthorizationwithtimeoutinterval)
  - [disableSKAdNetwork](#disableskadnetwork)
  - [setLanguage](#setlanguage)
- [IAppsFlyerConversionData](#iappsflyerconversiondata)
  - [onConversionDataSuccess](#onconversiondatasuccess)
  - [onConversionDataFail](#onconversiondatafail)
  - [onAppOpenAttribution](#onappopenattribution)
  - [onAppOpenAttributionFailure](#onappopenattributionfailure)
- [IAppsFlyerUserInvite](#iappsflyeruserinvite)
  - [onInviteLinkGenerated](#oninvitelinkgenerated)
  - [onInviteLinkGeneratedFailure](#oninvitelinkgeneratedfailure)
  - [onOpenStoreLinkGenerated](#onopenstorelinkgenerated)
- [IAppsFlyerValidateReceipt](#iappsflyervalidatereceipt)
  - [didFinishValidateReceipt](#didfinishvalidatereceipt)
  - [didFinishValidateReceiptWithError](#didfinishvalidatereceiptwitherror)
- [Events](#events)
  - [onRequestResponse](#onrequestresponse)
  - [onInAppResponse](#oninappresponse)
  - [onDeepLinkReceived](#ondeeplinkreceived)


---

## Android, iOS and Windows API

### initSDK 
**`void initSDK(string devKey, string appID, MonoBehaviour gameObject)`**

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

### startSDK
**`void startSDK()`**
    
Once this API is invoked the SDK will start,  sessions will be immediately sent, and all background foreground transitions will record a session.

*Example:*

```c#
 AppsFlyer.startSDK();
```

---

## Android and iOS API

### stopSDK 
**`void stopSDK(bool isSDKStopped)`**

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

### isSDKStopped
**`bool isSDKStopped()`**

Was the stopSDK(boolean) API set to `true`.

*Example:*

```c#
if (!AppsFlyer.isSDKStopped())
{
  
}
```

---

### getSdkVersion 
**`string getSdkVersion()`**

Get the AppsFlyer SDK version used in the app.

*Example:*

```c#
string version = AppsFlyer.getSdkVersion();
```

---

### setIsDebug
**`void setIsDebug(bool shouldEnable)`**

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

### setCustomerUserId
**`void setCustomerUserId(string id)`**

Setting your own Custom ID enables you to cross-reference your own unique ID with AppsFlyer’s user ID and the other devices’ IDs. This ID is available in AppsFlyer CSV reports along with postbacks APIs for cross-referencing with your internal IDs.

| parameter | type     | description     |
| --------- |--------- |--------------   |
| `id`      | `string` | custom user ID  |

*Example:*

```c#
AppsFlyer.setCustomerUserId("custom_user_id");
```

---

### setAppInviteOneLinkID 
**`void setAppInviteOneLinkID(string oneLinkId)`**

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

### setAdditionalData
**`void setAdditionalData(Dictionary<string, string> customData)`**

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

### setResolveDeepLinkURLs
**`void setResolveDeepLinkURLs(params string[] urls)`**

If you are using OneLinks which support Android App Links and wrapping them with a 3rd Party Universal Link, you can use the setResolveDeepLinkURLs API to notify the AppsFlyer SDK which click domains that invoke the app should be resolved by the SDK and have the underlying OneLink extracted from them. This will allow you to maintain deep linking and attribution while wrapping the OneLink with a 3rd party Universal Link. Make sure to call this API before SDK initialization.

| parameter | type              | description   |
| ----------|-------------------|-------------- |
| `urls`    | `params string[]` | array of urls |

*Example:*

```c#
AppsFlyer.setResolveDeepLinkURLs("test.com", "test2.ca");
```

---

### setOneLinkCustomDomain 
**`void setOneLinkCustomDomain(params string[] domains)`**
    
Advertisers can use this method to set vanity onelink domains.

| parameter  | type              | description             |
| ---------- | ----------------- |-------------------------|
| `domains`  | `params string[]` | array of custom domains |

*Example:*

```c#
 AppsFlyer.setOneLinkCustomDomain("test.domain", "test2.domain");
```

---

### setCurrencyCode 
**`void setCurrencyCode(string currencyCode)`**

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

### recordLocation
**`void recordLocation(double latitude, double longitude)`**

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

### anonymizeUser
**`void anonymizeUser(bool shouldAnonymizeUser)`**

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

### getAppsFlyerId 
**`string getAppsFlyerId()`**

AppsFlyer's unique device ID is created for every new install of an app. Use the following API to obtain AppsFlyer’s Unique ID.

*Example:*

```c#
string uid = AppsFlyer.getAppsFlyerId(); 
```

---

### setMinTimeBetweenSessions 
**`void setMinTimeBetweenSessions(int seconds)`**

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

### setHost
**`void setHost(string hostPrefixName, string hostName)`**

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

### setUserEmails 
**`void setUserEmails(EmailCryptType cryptMethod, params string[] emails)`**

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

### setPhoneNumber
**`void setPhoneNumber(string phoneNumber)`**

Set the user phone number.


| parameter     | type              | description               |
| -----------   |-------------------|---------------------------|
| `phoneNumber` | `string`  |  |


*Example:*

```c#
AppsFlyer.setPhoneNumber("4166358181");
```

---

### getConversionData
**`void getConversionData(string objectName);`**

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

### attributeAndOpenStore
**`void attributeAndOpenStore(string appID, string campaign, Dictionary<string, string> userParams, MonoBehaviour gameObject)`**

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

### recordCrossPromoteImpression 
**`void recordCrossPromoteImpression(string appID, string campaign);`**

To attribute an impression use the following API call.
Make sure to use the promoted App ID as it appears within the AppsFlyer dashboard.

| parameter    | type      | description  |
| -----------  |-----------|--------------|
| `appID`      | `string`  | appID        |
| `campaign`   | `string`  | campaign     |
| `params`     | `Dictionary<string, string>`    | additional params     |


*Example:*

```c#
Dictionary<string, string> parameters = new Dictionary<string, string>();
parameters.Add("af_sub1", "val");
parameters.Add("custom_param", "val2");
AppsFlyer.recordCrossPromoteImpression("appID", "campaign", parameters);
```

---

### generateUserInviteLink
**`void generateUserInviteLink(Dictionary<string, string> parameters, MonoBehaviour gameObject)`**

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

### setSharingFilterForAllPartners *Deprecated*
**`void setSharingFilterForAllPartners()`** 

Used by advertisers to exclude all networks/integrated partners from getting data.

*Example:*

```c#
AppsFlyer.setSharingFilterForAllPartners();
```

---

### setSharingFilter *Deprecated*
**`void setSharingFilter(params string[] partners)`** 


 Used by advertisers to set some (one or more) networks/integrated partners to exclude from getting data.


| parameter    | type                         | description                                         |
| -----------  |----------------------------- |-----------------------------------------------------|
| `partners` | `params string[] partners` | partners to exclude from getting data                                                    |


*Example:*

```c#
AppsFlyer.setSharingFilter("googleadwords_int","snapchat_int","doubleclick_int");
```

---

### setSharingFilterForPartners 
**`void setSharingFilterForPartners(params string[] partners)`** 


 Used by advertisers to set some (one or more) networks/integrated partners to exclude from getting data.


| parameter    | type                         | description                                         |
| -----------  |----------------------------- |-----------------------------------------------------|
| `partners` | `params string[] partners` | partners to exclude from getting data                                                    |

*Example:*

```c#
AppsFlyer.setSharingFilterForPartners("partner1_int"); // Single partner
AppsFlyer.setSharingFilterForPartners("partner1_int", "partner2_int"); // Multiple partners
AppsFlyer.setSharingFilterForPartners("all"); // All partners
AppsFlyer.setSharingFilterForPartners(""); // Reset list (default)
AppsFlyer.setSharingFilterForPartners(); // Reset list (default)
```

---

### setPartnerData
**`void setPartnerData(string partnerID, params string[] partnerInfo)`** 

Allows sending custom data for partner integration purposes.

| parameter    | type                         |description       |
| -----------  |------------------------------| -----------------------------------------|
| `partnerID` | `string` | ID of the partner (usually suffixed with "_int").|
|`partnerInfo` | `params string[]` | Customer data, depends on the integration configuration with the specific partner. |


*Example:*

```c#
   Dictionary<string, string> partnerInfo = new Dictionary<string, string>();
        partnerInfo.Add("puid", "1234567890");
        AppsFlyer.setPartnerData("partner_test", partnerInfo);
```

---- 

## Android Only API
  
### updateServerUninstallToken
**`void updateServerUninstallToken(string token)`**
 
 Manually pass the Firebase Device Token for Uninstall measurement.

| parameter   | type      | description        |
| ----------  |---------- |--------------------|
| `token`     | `string`  | Firebase FCM token |


*Example:*

```c#
#if UNITY_ANDROID && !UNITY_EDITOR
        AppsFlyer.updateServerUninstallToken("token");
#endif
```

---

### setImeiData 
**`void setImeiData(string imei)`**
 
By default, IMEI and Android ID are not collected by the SDK if the OS version is higher than KitKat (4.4)
and the device contains Google Play Services(on SDK versions 4.8.8 and below the specific app needed GPS).
Use this API to explicitly send IMEI to AppsFlyer.


| parameter   | type     | description   |
| ----------- |----------|-------------- |
| `imei`      | `string` | device's IMEI |


*Example:*

```c#
#if UNITY_ANDROID && !UNITY_EDITOR
        AppsFlyer.setImeiData("imei");
#endif
```

---

### setAndroidIdData
**`void setAndroidIdData(string androidId)`**
 
By default, IMEI and Android ID are not collected by the SDK if the OS version is higher than KitKat(4.4)and the device contains Google Play Services(on SDK versions 4.8.8 and below the specific app needed GPS).
Use this API to explicitly send Android ID to AppsFlyer.

| parameter   | type     | description          |
| ----------- | ---------|--------------------- |
| `androidId` | `string` | device's Android ID  |

*Example:*

```c#
#if UNITY_ANDROID && !UNITY_EDITOR
        AppsFlyer.setAndroidIdData("androidId");
#endif
```

---

### waitForCustomerUserId 
**`void waitForCustomerUserId(bool wait)`**
 
It is possible to delay the SDK Initialization until the customerUserID is set.
This feature makes sure that the SDK doesn't begin functioning until the customerUserID is provided.
If this API is used, all in-app events and any other SDK API calls are discarded, until the customerUserID is provided.


| parameter   | type    | description                                         |
| ----------  |-------- |-----------------------------------------------------|
| `wait`      | `bool`  | True if you want the SDK to wait for customerUserID |


*Example:*

```c#
#if UNITY_ANDROID && !UNITY_EDITOR
        AppsFlyer.waitForCustomerUserId(true);
#endif
```

---

 ### setCustomerIdAndStartSDK 
 **`void setCustomerIdAndStartSDK(string id)`**
 
Use this API to provide the SDK with the relevant customer user id and trigger the SDK to begin its normal activity.

| parameter   | type      | description             |
| ----------- |---------- |------------------------ |
| `id`        | `string`  | Customer ID for client. |

*Example:*

```c#
#if UNITY_ANDROID && !UNITY_EDITOR
        AppsFlyer.setCustomerIdStartSDK("id");
#endif
```

---

 ### getOutOfStore 
 **`string getOutOfStore()`**
 
 Get the current AF_STORE value.

*Example:*

```c#
#if UNITY_ANDROID && !UNITY_EDITOR
        string af_store = AppsFlyer.getOutOfStore();
#endif
```

---

 ### setOutOfStore 
 **`void setOutOfStore(string sourceName)`**
 
 Manually set the AF_STORE value.

| parameter    | type      | description  |
| -----------  |---------- |--------------|
| `sourceName` | `string`  |              |

*Example:*

```c#
#if UNITY_ANDROID && !UNITY_EDITOR
        AppsFlyer.setOutOfStore("sourceName");
#endif
```

---

 ### setCollectAndroidID
 **`void setCollectAndroidID(bool isCollect)`**

Opt-out of collection of Android ID.
If the app does NOT contain Google Play Services, Android ID is collected by the SDK.
However, apps with Google play services should avoid Android ID collection as this is in violation of the Google Play policy.
 
| parameter   | type     | description  |
| ----------- |--------- |--------------|
| `isCollect` | `bool`   |              |


*Example:*

```c#
#if UNITY_ANDROID && !UNITY_EDITOR
        AppsFlyer.setCollectAndroidID(true);
#endif
```

---


 ### setCollectIMEI
 **`void setCollectIMEI(bool isCollect)`**
 
Opt-out of collection of IMEI.
If the app does NOT contain Google Play Services, device IMEI is collected by the SDK.
However, apps with Google play services should avoid IMEI collection as this is in violation of the Google Play policy.

| parameter          | type                        | description  |
| -----------        |-----------------------------|--------------|
| `isCollect`        | `bool`                      |              |

*Example:*

```c#
#if UNITY_ANDROID && !UNITY_EDITOR
        AppsFlyer.setCollectIMEI(true);
#endif
```

---

 ### setIsUpdate
 **`void setIsUpdate(bool isUpdate)`**
 
Manually set that the application was updated.

| parameter   | type    | description             |
| ----------- |-------- |-------------------------|
| `isUpdate`  | `bool`  | true if app was updated |

*Example:*

```c#
#if UNITY_ANDROID && !UNITY_EDITOR
        AppsFlyer.setIsUpdate(true);
#endif
```

---

 ### setPreinstallAttribution
 **`void setPreinstallAttribution(string mediaSource, string campaign, string siteId)`**
 
 Specify the manufacturer or media source name to which the preinstall is attributed.

| parameter      | type     | description                                                   |
| -----------    |----------|---------------------------------------------------------------|
| `mediaSource`  | `string` | Manufacturer or media source name for preinstall attribution. |
| `campaign`     | `string` | Campaign name for preinstall attribution.                     |
| `siteId`       | `string` | Site ID for preinstall attribution.                           |

*Example:*

```c#
#if UNITY_ANDROID && !UNITY_EDITOR
        AppsFlyer.setPreinstallAttribution("mediaSource", "campaign", "siteId");
#endif
```

---


 ### isPreInstalledApp
 **`bool isPreInstalledApp()`**
 
Boolean indicator for preinstall by Manufacturer.

*Example:*

```c#
#if UNITY_ANDROID && !UNITY_EDITOR
        if (AppsFlyer.isPreInstalledApp())
        {

        }
#endif
```

---


### getAttributionId
**`string getAttributionId()`**
 
Get the Facebook attribution ID, if one exists.

*Example:*

```c#
#if UNITY_ANDROID && !UNITY_EDITOR
        string attributionId = AppsFlyer.getAttributionId();
#endif
```

---

### handlePushNotifications
**`void handlePushNotifications()`**
 
When the handlePushNotifications API is called push notifications will be recorded.

*Example:*

```c#
#if UNITY_ANDROID && !UNITY_EDITOR
        AppsFlyer.handlePushNotifications();
#endif
```

---

### validateAndSendInAppPurchase 
**`void validateAndSendInAppPurchase(string publicKey, string signature, string purchaseData, string price, string currency, Dictionary<string, string> additionalParameters, MonoBehaviour gameObject)`**
 
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
        AppsFlyer.validateAndSendInAppPurchase(
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

### setCollectOaid
**`void setCollectOaid(boolean isCollect)`**

    setCollectOaid

    You must include the appsflyer oaid library for this api to work.

| parameter   | type    | description             |
| ----------- |-------- |-------------------------|
| `isCollect` | `bool`  | true to allow oaid collection |

*Example:*

```c#
#if UNITY_ANDROID && !UNITY_EDITOR
        AppsFlyer.setCollectOaid(true);
#endif
```

---
    
### setDisableAdvertisingIdentifiers 
**`void setDisableAdvertisingIdentifiers(boolean disable)`**

    setDisableAdvertisingIdentifiers

    Disables collection of various Advertising IDs by the SDK. This includes Google Advertising ID (GAID), OAID and Amazon Advertising ID (AAID)

| parameter   | type    | description             |
| ----------- |-------- |-------------------------|
| `disable` | `bool`  | true to disable|

*Example:*

```c#
#if UNITY_ANDROID && !UNITY_EDITOR
        AppsFlyer.setDisableAdvertisingIdentifiers(true);
#endif
```

---

---
    
### setDisableNetworkData
**`void setDisableNetworkData(boolean disable)`**

    setDisableNetworkData

    Use to opt-out of collecting the network operator name (carrier) and sim operator name from the device.

| parameter   | type    | description             |
| ----------- |-------- |-------------------------|
| `disable` | `bool`  | true to opt-out|

*Example:*

```c#
#if UNITY_ANDROID && !UNITY_EDITOR
        AppsFlyer.setDisableNetworkData(true);
#endif
```

---

##  iOS Only API
           
### setDisableCollectAppleAdSupport 
**`void setDisableCollectAppleAdSupport(bool disable)`**
 
AppsFlyer SDK collects Apple's `advertisingIdentifier` if the `AdSupport.framework` is included in the SDK.
You can disable this behavior by setting the following property to true.

| parameter      | type     | description  |
| -----------    |----------|--------------|
| `disable`      | `bool`   |              |

*Example:*

```c#
#if UNITY_IOS && !UNITY_EDITOR
        AppsFlyer.setDisableCollectAppleAdSupport(true);
#endif
```

---

### setShouldCollectDeviceName
**`void setShouldCollectDeviceName(bool shouldCollectDeviceName)`**
 
Set this flag to true, to collect the current device name(e.g. "My iPhone"). Default value is false.
        
| parameter                 | type      | description  |
| ----------------------    |---------- |--------------|
| `shouldCollectDeviceName` | `bool`    |              |

*Example:*

```c#
#if UNITY_IOS && !UNITY_EDITOR
        AppsFlyer.setShouldCollectDeviceName(true);
#endif
```

---

 ### setDisableCollectIAd
 **`void setDisableCollectIAd(bool disableCollectIAd)`**
 
Opt-out for Apple Search Ads attributions.

| parameter           | type     | description  |
| -----------         |----------|--------------|
| `disableCollectIAd` | `bool`   |              |
*Example:*

```c#
#if UNITY_IOS && !UNITY_EDITOR
        AppsFlyer.setDisableCollectIAd(true);
#endif
```

---

### setUseReceiptValidationSandbox
**`void setUseReceiptValidationSandbox(bool useReceiptValidationSandbox)`**
 

In app purchase receipt validation Apple environment(production or sandbox). The default value is false.

| parameter                     | type      | description                                  |
| ----------------------------  |---------- |--------------------------------------------- |
| `useReceiptValidationSandbox` | `bool`    | true if In app purchase is done with sandbox |

*Example:*

```c#
#if UNITY_IOS && !UNITY_EDITOR
        AppsFlyer.setUseReceiptValidationSandbox(true);
#endif
```

---

### setUseUninstallSandbox 
**`void setUseUninstallSandbox(bool useUninstallSandbox)`**
 
Set this flag to test uninstall on Apple environment(production or sandbox). The default value is false.

| parameter             | type    | description                             |
| -----------           |-------  |---------------------------------------- |
| `useUninstallSandbox` | `bool`  | true if you are using a APN certificate |

*Example:*

```c#
#if UNITY_IOS && !UNITY_EDITOR
        AppsFlyer.setUseUninstallSandbox(true);
#endif
```

---


### validateAndSendInAppPurchase 
**`void validateAndSendInAppPurchase(string productIdentifier, string price, string currency, string tranactionId, Dictionary<string, string> additionalParameters, MonoBehaviour gameObject)`**
 
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
        AppsFlyer.validateAndSendInAppPurchase(
        "productIdentifier", 
        "price", 
        "currency", 
        "tranactionId", 
        null, 
        this);
#endif
```

---


### registerUninstall
**` void registerUninstall(byte[] deviceToken)`**
 

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
                AppsFlyer.registerUninstall(token);
                tokenSent = true;
            }
        }
#endif
    }

```

---

### handleOpenUrl 
**`void handleOpenUrl(string url, string sourceApplication, string annotation)`**


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
    AppsFlyer.handleOpenUrl(string url, string sourceApplication, string annotation);
#endif
```

---

### waitForATTUserAuthorizationWithTimeoutInterval 
**` void waitForATTUserAuthorizationWithTimeoutInterval(int timeoutInterval)`**

See [here](https://support.appsflyer.com/hc/en-us/articles/207032066-iOS-SDK-V6-X-integration-guide-for-developers#integration-33-configuring-app-tracking-transparency-att-support) for more info. 

| parameter     | type       | description  |
| -----------   |----------  |--------------|
| `timeoutInterval`         | `int`   |      Time to wait for idfa        |

*Example:*

```c#
#if UNITY_IOS && !UNITY_EDITOR
    AppsFlyer.waitForATTUserAuthorizationWithTimeoutInterval(60);
#endif
```
---

### disableSKAdNetwork 
**` bools disableSKAdNetwork(bool isDisabled)`**


| parameter     | type       | description  |
| -----------   |----------  |--------------|
| `isDisabled`         | `bool`   |      True to disable SKAdNetwork     |

*Example:*

```c#
#if UNITY_IOS && !UNITY_EDITOR
    AppsFlyer.disableSKAdNetwork(true);
#endif
```

---

### setLanguage
**` setCurrentDeviceLanguage(string language)`**


| parameter     | type       | description  |
| -----------   |----------  |--------------|
| `language`         | `String`   |    The language to set   |

*Example:*

```c#
#if UNITY_IOS && !UNITY_EDITOR
    AppsFlyer.setCurrentDeviceLanguage("english");
#endif
```

---

## IAppsFlyerConversionData
  
### onConversionDataSuccess 
**`public void onConversionDataSuccess(string conversionData)`**
 
 ConversionData contains information about install.<br> Organic/non-organic, etc. See [here](https://support.appsflyer.com/hc/en-us/articles/360000726098-Conversion-Data-Scenarios#Introduction) for more info.

| parameter   | type      | description        |
| ----------  |---------- |--------------------|
| `conversionData`     | `string`  | JSON string of the returned conversion data |


*Example:*

```c#
   public void onConversionDataSuccess(string conversionData)
    {
        AppsFlyer.AFLog("onConversionDataSuccess", conversionData);
        Dictionary<string, object> conversionDataDictionary = AppsFlyer.CallbackStringToDictionary(conversionData);
        // add deferred deeplink logic here
    }
```

---

### onConversionDataFail
**`public void onConversionDataFail(string error)`**
 

| parameter   | type      | description        |
| ----------  |---------- |--------------------|
| `error`     | `string`  | A string describing the error |


*Example:*

```c#
    public void onConversionDataFail(string error)
    {
        AppsFlyer.AFLog("onConversionDataFail", error);
    }
```

---

### onAppOpenAttribution
**`public void onAppOpenAttribution(string attributionData)`**
 
attributionData contains information about OneLink, deeplink.

| parameter   | type      | description        |
| ----------  |---------- |--------------------|
| `attributionData`     | `string`  | JSON string of the returned deeplink data |


*Example:*

```c#
    public void onAppOpenAttribution(string attributionData)
    {
        AppsFlyer.AFLog("onAppOpenAttribution", attributionData);
        Dictionary<string, object> attributionDataDictionary = AppsFlyer.CallbackStringToDictionary(attributionData);
        // add direct deeplink logic here
    }
```

---

### onAppOpenAttributionFailure 
**`public void onAppOpenAttributionFailure(string error)`**
    
Any errors that occurred during the attribution request.

| parameter   | type      | description        |
| ----------  |---------- |--------------------|
| `error`     | `string`  | string describing the error |


*Example:*

```c#
  public void onAppOpenAttributionFailure(string error)
    {
        AppsFlyer.AFLog("onAppOpenAttributionFailure", error);
    }
```

---

## IAppsFlyerUserInvite
  
### onInviteLinkGenerated 
**`public void onInviteLinkGenerated(string link)`**
 
The success callback for generating OneLink URLs. 

| parameter   | type      | description        |
| ----------  |---------- |--------------------|
| `link`     | `string`  | generated link |


*Example:*

```c#
   public void onInviteLinkGenerated(string link)
    {

    }
```

---

### onInviteLinkGeneratedFailure 
**`public void onInviteLinkGeneratedFailure(string error)`**
 
 The error callback for generating OneLink URLs

| parameter   | type      | description        |
| ----------  |---------- |--------------------|
| `error`     | `string`  | A string describing the error |


*Example:*

```c#
    public void onInviteLinkGeneratedFailure(string error)
    {
        AppsFlyer.AFLog("onInviteLinkGeneratedFailure", error);
    }
```

---

### onOpenStoreLinkGenerated
**`public void onOpenStoreLinkGenerated(string link)`**
 
       
 (ios only) iOS allows you to utilize the StoreKit component to open
 the App Store while remaining in the context of your app.<br>
 More details at [here](https://support.appsflyer.com/hc/en-us/articles/115004481946-Cross-Promotion-Tracking#tracking-cross-promotion-impressions)
     

| parameter   | type      | description        |
| ----------  |---------- |--------------------|
| `attributionData`     | `string`  | JSON string of the returned deeplink data |


*Example:*

```c#
    public void onOpenStoreLinkGenerated(string link)
    {

    }
```

---

##  IAppsFlyerValidateReceipt
  
### didFinishValidateReceipt 
**`public void didFinishValidateReceipt(string result)`**
 
The success callback for validateAndSendInAppPurchase API.<br>
For Android : the callback will return "Validate success".<br>
For iOS : the callback will return a JSON string from apples verifyReceipt API. <br>

| parameter   | type      | description        |
| ----------  |---------- |--------------------|
| `result`     | `string`  | validate result |


*Example:*

```c#
   public void didFinishValidateReceipt(string link)
    {

    }
```

---

### didFinishValidateReceiptWithError 
**`public void didFinishValidateReceiptWithError(string error)`**
 
 The error callback for validating receipts.<br>

| parameter   | type      | description        |
| ----------  |---------- |--------------------|
| `error`     | `string`  | A string describing the error |


*Example:*

```c#
    public void didFinishValidateReceiptWithError(string error)
    {
      
    }
```

---

## Events
    
### onRequestResponse
**`public static event EventHandler OnRequestResponse`**
 
 The callback for Sessions.<br>

| statusCode      | errorDescription | 
| ----------- | ----------- | 
| 200      | null       | 
| 10   | "Event timeout. Check 'minTimeBetweenSessions' param"        | 
| 11   | "Skipping event because 'isStopTracking' enabled"        | 
| 40   | Network error: Error description comes from Android        | 
| 41   | "No dev key"        | 
| 50   | "Status code failure" + actual response code from the server        | 

*Example:*

```c#
    AppsFlyer.OnRequestResponse += (sender, args) =>
    {
        var af_args = args as AppsFlyerRequestEventArgs;
        AppsFlyer.AFLog("AppsFlyerOnRequestResponse", "status code" + af_args.statusCode);
    };
```

---

### onInAppResponse
**`public static event EventHandler OnInAppResponse`**
 
 The callback for In-App Events.<br>

| statusCode      | errorDescription | 
| ----------- | ----------- | 
| 200      | null       | 
| 10   | "Event timeout. Check 'minTimeBetweenSessions' param"        | 
| 11   | "Skipping event because 'isStopTracking' enabled"        | 
| 40   | Network error: Error description comes from Android        | 
| 41   | "No dev key"        | 
| 50   | "Status code failure" + actual response code from the server        | 

*Example:*

```c#

    AppsFlyer.OnInAppResponse += (sender, args) =>
    {
        var af_args = args as AppsFlyerRequestEventArgs;
        AppsFlyer.AFLog("OnRequestResponse", "status code" + af_args.statusCode);
    }; 

```

---

### onDeepLinkReceived 
**`public static event EventHandler OnDeepLinkReceived`**
 
 The callback for Unified Deeplink API.<br>


*Example:*

```c#

    // First call init with devKey, appId and gameObject
    AppsFlyer.initSDK(devKey, appID, this);


    AppsFlyer.OnDeepLinkReceived += (sender, args) =>
    {
        var deepLinkEventArgs = args as DeepLinkEventsArgs;

        // DEEPLINK LOGIC HERE
    }; 

```

---

