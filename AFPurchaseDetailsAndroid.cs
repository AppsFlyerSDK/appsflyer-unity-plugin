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
    // 
    /// </summary>
    public class AFPurchaseDetailsAndroid

    {
        public AFPurchaseType purchaseType { get; private set; }
        public string purchaseToken { get; private set; }
        public string productId { get; private set; }
        public string price { get; private set; }
        public string currency { get; private set; }

        public AFPurchaseDetailsAndroid(AFPurchaseType type, String purchaseToken, String productId, String price, String currency)
        {
            this.purchaseType = type;
            this.purchaseToken = purchaseToken;
            this.productId = productId;
            this.price = price;
            this.currency = currency;
        }

    }

}