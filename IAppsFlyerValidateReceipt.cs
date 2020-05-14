namespace AppsFlyerSDK
{
    public interface IAppsFlyerValidateReceipt
    {
        /// <summary>
        /// The success callback for validateAndSendInAppPurchase API.
        /// For Android : the callback will return "Validate success".
        /// For iOS : the callback will return a JSON string from apples verifyReceipt API.
        /// </summary>
        /// <param name="result"></param>
        void didFinishValidateReceipt(string result);

        /// <summary>
        /// The error callback for validateAndSendInAppPurchase API.
        /// </summary>
        /// <param name="error">A string describing the error.</param>
        void didFinishValidateReceiptWithError(string error);
    }
}