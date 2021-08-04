
# Request Listeners
    
1. Attach the 'AppsFlyer.cs' script to the game object with the AppsFlyer init code. (AppsFlyerObject, ect)
2. Add the following code **before** startSDK()

Sessions response example:

```c#
    void Start()
    {
        AppsFlyer.OnRequestResponse += AppsFlyerOnRequestResponse;
        
        AppsFlyer.initSDK(devKey, appID, this);
        AppsFlyer.startSDK();
    }

    void AppsFlyerOnRequestResponse(object sender, EventArgs e)
    {
        var args = e as AppsFlyerRequestEventArgs;
        AppsFlyer.AFLog("AppsFlyerOnRequestResponse", " status code " + args.statusCode);
    }
```

In-App response example:

```c#
    void Start()
    {
        AppsFlyer.OnInAppResponse += (sender, args) =>
        {
            var af_args = args as AppsFlyerRequestEventArgs;
            AppsFlyer.AFLog("AppsFlyerOnRequestResponse", " status code " + af_args.statusCode);
        };
        
        AppsFlyer.initSDK(devKey, appID, this);
        AppsFlyer.startSDK();
    }


```

| statusCode      | errorDescription | 
| ----------- | ----------- | 
| 200      | null       | 
| 10   | "Event timeout. Check 'minTimeBetweenSessions' param"        | 
| 11   | "Skipping event because 'isStopTracking' enabled"        | 
| 40   | Network error: Error description comes from Android        | 
| 41   | "No dev key"        | 
| 50   | "Status code failure" + actual response code from the server        | 
