using System.Collections.Generic;

namespace AppsFlyerSDK
{
    /// <summary>
    /// Common contract for a purchase to validate via <see cref="AppsFlyer.validateAndLogInAppPurchase"/>.
    /// Implemented per-platform by <see cref="AFPurchaseDetailsAndroid"/> and <see cref="AFSDKPurchaseDetailsIOS"/>,
    /// each of which knows how to shape its own RPC payload.
    /// </summary>
    public interface IAFPurchaseDetails
    {
        Dictionary<string, object> ToRpcPayload();
    }
}
