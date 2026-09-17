using System;
using System.Collections.Generic;

namespace AppsFlyerSDK
{
    /// <summary>
    /// Purchase type enum matching iOS SDK AFSDKPurchaseType
    /// </summary>
    public enum AFSDKPurchaseType
    {
        Subscription,
        OneTimePurchase
    }

    /// <summary>
    /// Purchase details class matching iOS SDK AFSDKPurchaseDetails
    /// </summary>
    public class AFSDKPurchaseDetailsIOS : IAFPurchaseDetails
    {
        public string productId { get; private set; }
        public string transactionId { get; private set; }
        public AFSDKPurchaseType purchaseType { get; private set; }

        private AFSDKPurchaseDetailsIOS(string productId, string transactionId, AFSDKPurchaseType purchaseType)
        {
            this.productId = productId;
            this.transactionId = transactionId;
            this.purchaseType = purchaseType;
        }

        public static AFSDKPurchaseDetailsIOS Init(string productId, string transactionId, AFSDKPurchaseType purchaseType)
        {
            return new AFSDKPurchaseDetailsIOS(productId, transactionId, purchaseType);
        }

        public Dictionary<string, object> ToRpcPayload()
        {
            return new Dictionary<string, object>
            {
                { "product", new Dictionary<string, object> { { "productId", productId } } },
                { "transaction", new Dictionary<string, object>
                    {
                        { "transactionId", transactionId },
                        { "purchaseType", purchaseType == AFSDKPurchaseType.Subscription ? "subscription" : "oneTimePurchase" }
                    }
                }
            };
        }
    }

}