using System;
using System.Collections.Generic;

namespace AppsFlyerSDK
{
    public enum AFPurchaseType
    {
        Subscription = 0,
        OneTimePurchase = 1
    }

    /// <summary>
    /// Purchase details class matching Android SDK AFPurchaseDetails
    /// </summary>
    public class AFPurchaseDetailsAndroid : IAFPurchaseDetails
    {
        public AFPurchaseType purchaseType { get; private set; }
        public string purchaseToken { get; private set; }
        public string productId { get; private set; }

        public AFPurchaseDetailsAndroid(AFPurchaseType type, String purchaseToken, String productId)
        {
            this.purchaseType = type;
            this.purchaseToken = purchaseToken;
            this.productId = productId;
        }

        public Dictionary<string, object> ToRpcPayload()
        {
            return new Dictionary<string, object>
            {
                { "purchaseType", purchaseType == AFPurchaseType.Subscription ? "subscription" : "one_time_purchase" },
                { "purchaseToken", purchaseToken },
                { "productId", productId }
            };
        }
    }

}