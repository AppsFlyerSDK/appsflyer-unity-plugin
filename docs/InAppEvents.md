---
title: In-App Events
category: 5f9705393c689a065c409b23
parentDoc: 6370c9e2441a4504d6bca3bd
order: 3
hidden: false
---

- [Overview](#overview)
- [Send Event](#send-event)
- [In-app purchase validation](#in-app-purchase-validation)

## Overview

In-App Events provide insight on what is happening in your app. It is recommended to take the time and define the events you want to measure to allow you to measure *ROI* (Return on Investment) and *LTV* (Lifetime Value).

Recording in-app events is performed by calling `sendEvent` with event name and value parameters. See In-App Events [documentation](https://support.appsflyer.com/hc/en-us/articles/115005544169-Rich-in-app-events-for-Android-and-iOS#introduction-predefined-and-custom-events) for more details.

Find more info about recording events [here](https://dev.appsflyer.com/hc/docs/in-app-events-sdk).

## Send Event

`void sendEvent(string eventName, Dictionary<string, string> eventValues)`


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

> 📘 Note
> 
> Do not add currency symbols to the revenue value.

---
## In-app purchase validation

For In-App Purchase Receipt Validation, follow the instructions according to your operating system.

**Notes**
Calling validateReceipt automatically generates an `af_purchase` in-app event, so you don't need to send this event yourself.
The validate purchase response is triggered in the `AppsFlyerTrackerCallbacks.cs` class.

`void validateAndSendInAppPurchase(string productIdentifier, string price, string currency, string tranactionId, Dictionary<string, string> additionalParameters, MonoBehaviour gameObject)`

```c#
//To get the callbacks
//AppsFlyer.createValidateInAppListener ("AppsFlyerTrackerCallbacks", "onInAppBillingSuccess", "onInAppBillingFailure");
AppsFlyer.validateReceipt(string publicKey, string purchaseData, string signature, string price, string currency, Dictionary additionalParametes);
```

```c#
using UnityEngine.Purchasing;
using AppsFlyerSDK;

public class AppsFlyerObject : MonoBehaviour, IStoreListener, IAppsFlyerValidateReceipt
{

    public static string kProductIDConsumable = "com.test.cons";

    void Start()
    {
        AppsFlyer.initSDK("devKey", "devKey");
        AppsFlyer.startSDK();
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {
        string prodID = args.purchasedProduct.definition.id;
        string price = args.purchasedProduct.metadata.localizedPrice.ToString();
        string currency = args.purchasedProduct.metadata.isoCurrencyCode;

        string receipt = args.purchasedProduct.receipt;
        var recptToJSON = (Dictionary<string, object>)AFMiniJSON.Json.Deserialize(product.receipt);
        var receiptPayload = (Dictionary<string, object>)AFMiniJSON.Json.Deserialize((string)recptToJSON["Payload"]);
        var transactionID = product.transactionID;

        if (String.Equals(args.purchasedProduct.definition.id, kProductIDConsumable, StringComparison.Ordinal))
        {
#if UNITY_IOS

            if(isSandbox)
            {
                AppsFlyeriOS.setUseReceiptValidationSandbox(true);
            }

            AppsFlyeriOS.validateAndSendInAppPurchase(prodID, price, currency, transactionID, null, this);
#elif UNITY_ANDROID
        var purchaseData = (string)receiptPayload["json"];
        var signature = (string)receiptPayload["signature"];
        AppsFlyerAndroid.validateAndSendInAppPurchase(
        "<google_public_key>", 
        signature, 
        purchaseData, 
        price, 
        currency, 
        null, 
        this);
#endif
        }

        return PurchaseProcessingResult.Complete;
    }

    public void didFinishValidateReceipt(string result)
    {
        AppsFlyer.AFLog("didFinishValidateReceipt", result);
    }

    public void didFinishValidateReceiptWithError(string error)
    {
        AppsFlyer.AFLog("didFinishValidateReceiptWithError", error);
    }

}

```


