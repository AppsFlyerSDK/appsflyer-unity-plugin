---
title: Conversion data
category: 5f9705393c689a065c409b23
parentDoc: 6370c9e2441a4504d6bca3bd
order: 4
hidden: false
---

In this guide, you will learn how to get conversion data using [`IAppsFlyerConversionData`](https://dev.appsflyer.com/hc/docs/api#iappsflyerconversiondata), as well as examples for using the conversion data.

Learn more about [what is conversion data](https://dev.appsflyer.com/hc/docs/conversion-data).

## Obtain AppsFlyer conversion data

1. Implement the [`IAppsFlyerConversionData`](https://dev.appsflyer.com/hc/docs/api#iappsflyerconversiondata) class.
2. Call the [`initSDK`](https://dev.appsflyer.com/hc/docs/api#initsdk) method with `this` as the last parameter.
3. Use the [`onConversionDataSuccess`](https://dev.appsflyer.com/hc/docs/api#onconversiondatasuccess) method to redirect the user.

## Example

```c#
using AppsFlyerSDK;

public class AppsFlyerObjectScript : MonoBehaviour , IAppsFlyerConversionData{
    void Start()
    {
        /* AppsFlyer.setDebugLog(true); */
        AppsFlyer.initSDK("devkey", "appID", this);
        AppsFlyer.startSDK();
    }

    public void onConversionDataSuccess(string conversionData)
    {
        AppsFlyer.AFLog("onConversionDataSuccess", conversionData);
        Dictionary<string, object> conversionDataDictionary = AppsFlyer.CallbackStringToDictionary(conversionData);
    }

    public void onConversionDataFail(string error)
    {
        AppsFlyer.AFLog("onConversionDataFail", error);
    }

    public void onAppOpenAttribution(string attributionData)
    {
        AppsFlyer.AFLog("onAppOpenAttribution: This method was replaced by UDL. This is a fake call.", attributionData);
    }

    public void onAppOpenAttributionFailure(string error)
    {
        AppsFlyer.AFLog("onAppOpenAttributionFailure: This method was replaced by UDL. This is a fake call.", error);
    }
}
```
