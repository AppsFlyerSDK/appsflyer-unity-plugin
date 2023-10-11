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
***

## Logging revenue

You can send revenue with any in-app event. Use the `AFInAppEvents.REVENUE` event parameter to include revenue in the in-app event. You can populate it with any numeric value, positive or negative.

The revenue value should not contain comma separators, currency signs, or text. A revenue event should be similar to 1234.56, for example.

Currency code requirements when sending revenue events

- Default currency: USD
- Use a [3-character ISO 4217 code](https://en.wikipedia.org/wiki/ISO_4217#Active_codes) (an example follows).
- Set the currency code by calling the API:
    

    ```c#
    AppsFlyer.setCurrencyCode("ZZZ")
    ```   

**Example: In-app purchase event with revenue**
This purchase event is for 200.12 Euros. For the revenue to reflect in the dashboard use the following.

```c#
System.Collections.Generic.Dictionary<string, string> purchaseEvent = new
System.Collections.Generic.Dictionary<string, string> ();
purchaseEvent.Add(AFInAppEvents.CURRENCY, "EUR");
purchaseEvent.Add(AFInAppEvents.REVENUE, "200.12");
purchaseEvent.Add(AFInAppEvents.QUANTITY, "1");
purchaseEvent.Add(AFInAppEvents.CONTENT_TYPE, "category_a",);
AppsFlyer.sendEvent ("af_purchase", purchaseEvent);
```

> 📘 Note
> 
> Do not add currency symbols to the revenue value.

### Logging negative revenue
Record negative revenue using a minus sign.
- Revenue value is preceded by a minus sign.
- The event name has a unique value, "cancel_purchase". This lets you identify negative revenue events in raw data reports and in the Dashboard.

**Example: App user receives a refund or cancels a subscription**

```c#
System.Collections.Generic.Dictionary<string, string> purchaseEvent = new 
System.Collections.Generic.Dictionary<string, string> ();
purchaseEvent.Add(AFInAppEvents.CURRENCY, "USD");
purchaseEvent.Add(AFInAppEvents.REVENUE, "-200");
purchaseEvent.Add(AFInAppEvents.QUANTITY, "1");
purchaseEvent.Add(AFInAppEvents.CONTENT_TYPE, "category_a");
AppsFlyer.sendEvent ("cancel_purchase", purchaseEvent);
```

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


