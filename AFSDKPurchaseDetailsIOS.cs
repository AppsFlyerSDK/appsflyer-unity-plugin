using System;
using System.Collections.Generic;

namespace AppsFlyerSDK
{
    /// <summary>
    // 
    /// </summary>
    public class AFSDKPurchaseDetailsIOS
    {
        public string productId { get; private set; }
        public string price { get; private set; }
        public string currency { get; private set; }
        public string transactionId { get; private set; }

        private AFSDKPurchaseDetailsIOS(string productId, string price, string currency, string transactionId)
        {
            this.productId = productId;
            this.price = price;
            this.currency = currency;
            this.transactionId = transactionId;
        }

        public static AFSDKPurchaseDetailsIOS Init(string productId, string price, string currency, string transactionId)
        {
            return new AFSDKPurchaseDetailsIOS(productId, price, currency, transactionId);
        }
    }

}