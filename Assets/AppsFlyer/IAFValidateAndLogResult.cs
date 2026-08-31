using System.Collections.Generic;

namespace AppsFlyerSDK
{
    /// <summary>
    /// Result contract for <see cref="AppsFlyer.validateAndLogInAppPurchase"/>, implemented by
    /// <see cref="AFSDKValidateAndLogResult"/>.
    /// </summary>
    public interface IAFValidateAndLogResult
    {
        AFSDKValidateAndLogStatus status { get; }
        Dictionary<string, object> result { get; }
        Dictionary<string, object> errorData { get; }
        string error { get; }
    }
}
