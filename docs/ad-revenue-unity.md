---
title: "Ad revenue"
slug: "ad-revenue-unity"
category: 5f9705393c689a065c409b23
parentDoc: 6370c9e2441a4504d6bca3bd
excerpt: "Impression-level ad revenue reporting by SDK"
hidden: false
order: 12
---
The app sends impression revenue data to the SDK which then sends it to AppsFlyer. The revenue data is collected and processed in AppsFlyer, and the revenue is attributed to the original UA source. To learn more about ad revenue see [here](https://support.appsflyer.com/hc/en-us/articles/217490046#connect-to-ad-revenue-integrated-partners).

There are two ways for the SDK to generate an ad revenue event, depending on your SDK version. Use the correct method for your SDK version:
- [For SDK 6.15.0 and above](#log-ad-revenue-for-sdk-6150-and-above). Uses the ad revenue SDK API.
- [For SDK 6.14.2 and below](#legacy-log-ad-revenue-for-sdk-6142-and-below). Uses the ad revenue SDK connector.

## Log ad revenue (for SDK 6.15.0 and above)

When an impression with revenue occurs, invoke the [`logAdRevenue`](doc:api#logadrevenue) method with the revenue details of the impression.  

**To implement the method:**

1. Create an instance of `AFAdRevenueData` with the revenue details of the impression to be logged. Version 6.15.0 of the SDK removes the need for using a connector for sending Ad Revenue data to AppsFlyer.
2. If you want to add additional details to the ad revenue event, populate a map with key-value pairs.
3. Invoke the  `logAdRevenue` method with the following arguments:
    - The `AFAdRevenueData` object you created in step 1.
    - The `Map` instance with the additional details you created in step 2.

### Code Example

```c#
Dictionary<string, string> additionalParams = new Dictionary<string, string>();
additionalParams.Add(AdRevenueScheme.COUNTRY, "USA");
additionalParams.Add(AFAdRevenueEvent.AD_UNIT, "89b8c0159a50ebd1");
additionalParams.Add(AFAdRevenueEvent.AD_TYPE, "Banner");
additionalParams.Add(AFAdRevenueEvent.PLACEMENT, "place");
var logRevenue = new AFAdRevenueData("monetizationNetworkEx", MediationNetwork.GoogleAdMob, "USD", 0.99);
AppsFlyer.logAdRevenue(logRevenue, additionalParams);
```

> 📘 Note 
>  The AdMob iLTV SDK reports impression revenue in micro-units. To display the correct ad revenue amount in USD in AppsFlyer, divide the amount extracted from the iLTV event handler by 1 million before sending it to AppsFlyer.

## [LEGACY] Log ad revenue (for SDK 6.14.2 and below)
For SDK v6.14.2 and below - the AdRevenue Connector should be used along side the AppsFlyer SDK to send Ad Revenue data to AppsFlyer.

### Using Unity Package

1. Clone or download [the Ad revenue connector](https://github.com/AppsFlyerSDK/appsflyer-unity-adrevenue-generic-connector/tree/main) repository.
2. Import the Adrevenue Unity package into your Unity project (To learn how to import to Unity refer to the [Unity documentation](https://docs.unity3d.com/Manual/AssetPackages.html)).
   1. In Unity, go to **Assets** > **Import Package** > **Custom Package**
   2. From the repository root select the  `appsflyer-unity-adrevenue-plugin-x.x.x.unitypackage` file.

### Using Unity Package Manager

1. Add the dependency in your `manifest.json` file:

```
 "appsflyer-unity-adrevenue-generic-connector": "https://github.com/AppsFlyerSDK/appsflyer-unity-adrevenue-generic-connector.git#upm"
```

2. If you haven't already done so, download the [External Dependency Manager for Unity](https://github.com/googlesamples/unity-jar-resolver) to be able to resolve our Android / iOS dependencies.

**Note:** To choose a specific version and not the latest, you can replace the `upm` with the specific version tag, `v6.9.4-upm` for example.

### Initialize the connector

Make sure to initialize the AppsFlyer SDK before initializing the connector. 

```java
using AppsFlyerSDK;

public class AppsFlyerObjectScript : MonoBehaviour
{
  void Start()
  {
  	AppsFlyerAdRevenue.start();
  	/* AppsFlyerAdRevenue.setIsDebug(true); */
  }
}

```

### Ad revenue connector API

#### `start`

`public static void start()`

Start sending AdRevenue data to AppsFlyer.

_Example:_

```java
using AppsFlyerSDK;
  void Start()
  {
    AppsFlyerAdRevenue.start();
  }
```

#### `setIsDebug`

 `public static void setIsDebug(bool isDebug)`

Set to true to view debug logs. (development only!)

| parameter | type | description                     |
| --------- | ---- | ------------------------------- |
| isDebug   | bool | set to true in development only |

_Example:_

```java
  AppsFlyerAdRevenue.setIsDebug(true);
```

**Note:** This API will only set the debug logs for iOS. For Android the debug logs are controlled by the native SDK.  
To turn on the debug logs on Android call `AppsFlyer.setIsDebug(true);`

#### `logAdRevenue`

`public static void logAdRevenue(string monetizationNetwork, AppsFlyerAdRevenueMediationNetworkType mediationNetwork, double eventRevenue, string revenueCurrency, Dictionary<string, string> additionalParameters)`

Send ad revenue data from the impression payload to AppsFlyer regardless of the mediation network you use.

| parameter            | type                                   | description                      |
| -------------------- | -------------------------------------- | -------------------------------- |
| monetizationNetwork  | string                                 | monetization network             |
| mediationNetwork     | AppsFlyerAdRevenueMediationNetworkType | Enum for mediaton network type   |
| eventRevenue         | string                                 | event revenue                    |
| revenueCurrency      | string                                 | revenue currency                 |
| additionalParameters | Dictionary<string, string>            | Any custom additional parameters |
|                      |                                        |                                  |

_Example:_

```java
Dictionary<string, string> additionalParams = new Dictionary<string, string>();
additionalParams.Add(AFAdRevenueEvent.COUNTRY, "US");
additionalParams.Add(AFAdRevenueEvent.AD_UNIT, "89b8c0159a50ebd1");
additionalParams.Add(AFAdRevenueEvent.AD_TYPE, "Banner");
additionalParams.Add(AFAdRevenueEvent.PLACEMENT, "place");

additionalParams.Add("custom", "foo");
additionalParams.Add("custom_2", "bar");
additionalParams.Add("af_quantity", "1");
AppsFlyerAdRevenue.logAdRevenue("facebook",
                                AppsFlyerAdRevenueMediationNetworkType.AppsFlyerAdRevenueMediationNetworkTypeGoogleAdMob,                                   
                                0.026,
                                "USD",
                                additionalParams);
```