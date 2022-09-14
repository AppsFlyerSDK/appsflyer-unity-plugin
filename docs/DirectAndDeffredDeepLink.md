###  <a id="deferred-deep-linking"> 1. Deferred Deep Linking (Get Conversion Data)

Check out the deferred deeplinkg guide from the AppFlyer knowledge base [here](https://support.appsflyer.com/hc/en-us/articles/207032096-Accessing-AppsFlyer-Attribution-Conversion-Data-from-the-SDK-Deferred-Deeplinking-#Introduction).

Code Sample to handle the conversion data:

```c#
public void onConversionDataSuccess(string conversionData)
{
    AppsFlyer.AFLog("onConversionDataSuccess", conversionData);
    Dictionary<string, object> conversionDataDictionary = AppsFlyer.CallbackStringToDictionary(conversionData);
    // add deferred deeplink logic here
}

public void onConversionDataFail(string error)
{
    AppsFlyer.AFLog("onConversionDataFail", error);
}
```

###  <a id="handle-deeplinking"> 2. Direct Deeplinking
    
When a deeplink is clicked on the device the AppsFlyer SDK will return the resolved link in the [onAppOpenAttribution](https://support.appsflyer.com/hc/en-us/articles/208874366-OneLink-Deep-Linking-Guide#deep-linking-data-the-onappopenattribution-method-) method.



```c#
public void onAppOpenAttribution(string attributionData)
{
    AppsFlyer.AFLog("onAppOpenAttribution", attributionData);
    Dictionary<string, object> attributionDataDictionary = AppsFlyer.CallbackStringToDictionary(attributionData);
    // add direct deeplink logic here
}

public void onAppOpenAttributionFailure(string error)
{
    AppsFlyer.AFLog("onAppOpenAttributionFailure", error);
}
```

