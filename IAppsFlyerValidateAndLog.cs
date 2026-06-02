namespace AppsFlyerSDK
{
    public interface IAppsFlyerValidateAndLog
    {
        /// <summary>
        /// The success callback for validateAndSendInAppPurchase API.
        /// For Android : the callback will return JSON string.
        /// For iOS : the callback will return a JSON string from apples verifyReceipt API.
        /// </summary>
        /// <param name="result"></param>
        void onValidateAndLogComplete(string result);

        /// <summary>
        /// The error callback for validateAndSendInAppPurchase API.
        /// </summary>
        /// <param name="error">A string describing the error.</param>
        void onValidateAndLogFailure(string error);
    }
}