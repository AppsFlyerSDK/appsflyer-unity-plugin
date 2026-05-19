---
title: "Validate and log purchase"
slug: "validate-and-log-unity"
category: 5f9705393c689a065c409b23
parentDoc: 694bc4503a665449be928691
excerpt: "Used to validate and report in-app purchase and subscription revenue events"
hidden: false
order: 1
---

## Validate and log purchase

Follow the instructions according to your operating system.

Calling `validateAndSendInAppPurchase` automatically generates an `af_purchase` in-app event, so you don't need to send this event yourself.
The validate purchase response is triggered in the `AppsFlyerTrackerCallbacks.cs` class.

**Android**

`void validateAndSendInAppPurchase(AFPurchaseDetailsAndroid details, Dictionary<string, string> additionalParameters, MonoBehaviour gameObject)`

**iOS**

`void validateAndSendInAppPurchase(AFSDKPurchaseDetailsIOS details, Dictionary<string, string> extraEventValues, MonoBehaviour gameObject)`

> In the C# SDK, the dictionary parameter is named `purchaseAdditionalDetails` on both platforms.

Use `AFPurchaseType` / `AFSDKPurchaseType` (`OneTimePurchase` or `Subscription`) to match the product. On iOS, call `setUseReceiptValidationSandbox(true)` before validation when testing in the sandbox.

```c#
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;
using AppsFlyerSDK;

public class AppsFlyerObject : MonoBehaviour, IAppsFlyerValidateAndLog
{

    public static string kProductIDConsumable = "com.test.cons";

    void Start()
    {
        AppsFlyer.initSDK("devKey", "appId");
        AppsFlyer.startSDK();
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {
        string prodID = args.purchasedProduct.definition.id;
        string transactionID = args.purchasedProduct.transactionID;

        var recptToJSON = (Dictionary<string, object>)AFMiniJSON.Json.Deserialize(args.purchasedProduct.receipt);
        var receiptPayload = (Dictionary<string, object>)AFMiniJSON.Json.Deserialize((string)recptToJSON["Payload"]);

        if (String.Equals(prodID, kProductIDConsumable, StringComparison.Ordinal))
        {
            var purchaseAdditionalDetails = new Dictionary<string, string>
            {
                { "paywall", "123" }
            };

#if UNITY_IOS

            if (isSandbox)
            {
                AppsFlyeriOS.setUseReceiptValidationSandbox(true);
            }

            AFSDKPurchaseDetailsIOS details = AFSDKPurchaseDetailsIOS.Init(
                prodID,
                transactionID,
                AFSDKPurchaseType.OneTimePurchase);

            AppsFlyer.validateAndSendInAppPurchase(details, purchaseAdditionalDetails, this);
#elif UNITY_ANDROID

            var purchaseData = (string)receiptPayload["json"];
            var purchaseDataJson = (Dictionary<string, object>)AFMiniJSON.Json.Deserialize(purchaseData);
            string purchaseToken = (string)purchaseDataJson["purchaseToken"];

            AFPurchaseDetailsAndroid details = new AFPurchaseDetailsAndroid(
                AFPurchaseType.OneTimePurchase,
                purchaseToken,
                prodID);

            AppsFlyer.validateAndSendInAppPurchase(details, purchaseAdditionalDetails, this);
#endif
        }

        return PurchaseProcessingResult.Complete;
    }

    public void onValidateAndLogComplete(string result)
    {
        AppsFlyer.AFLog("onValidateAndLogComplete", result);
        Dictionary<string, object> validateAndLogDataDictionary = AppsFlyer.CallbackStringToDictionary(result);
    }

    public void onValidateAndLogFailure(string error)
    {
        AppsFlyer.AFLog("onValidateAndLogFailure", error);
        Dictionary<string, object> validateAndLogErrorDictionary = AppsFlyer.CallbackStringToDictionary(error);
    }

}

```

## Receipt validation [Legacy]
<span class="annotation-deprecated">Deprecated since V6.17.8</span>  

For current integration, use the [Validate and log purchase](#validate-and-log-purchase) section above.

For Receipt Validation, follow the instructions according to your operating system.

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


