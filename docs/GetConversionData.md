# Get Conversion Data

 Use conversion data to identify various conversion scenarios, customize user experience, and more.
 
You can find more info [here](https://support.appsflyer.com/hc/en-us/articles/360000726098-Get-conversion-data-using-AppsFlyer-SDK#introduction)


##### <a id="getConversionData"> **`void getConversionData(string objectName);`**

Register a Conversion Data Listener.
Get the callbacks by implementing the IAppsFlyerConversionData interface.

| parameter    | type     | description                                             |
| -----------  |----------|-------------------------------------------------------- |
| `objectName` | `string` | game object with the IAppsFlyerConversionData interface |

*Example:*

```c#
AppsFlyer.getConversionData(gameObject.name);
```

---
