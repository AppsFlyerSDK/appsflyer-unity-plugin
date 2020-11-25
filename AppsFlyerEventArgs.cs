using System;

namespace AppsFlyerSDK
{
    
    /// <summary>
    /// Event args for AppsFlyer requests.
    /// Used for sessions and in-app events.
    /// Used to handle post request logic.
    /// 
    /// Examples:
    /// statusCode / errorDescription
    /// 
    /// 200 - null
    /// 
    /// 10 - "Event timeout. Check 'minTimeBetweenSessions' param"
    /// 11 - "Skipping event because 'isStopTracking' enabled"
    /// 40 - Network error: Error description comes from Android
    /// 41 - "No dev key"
    /// 50 - "Status code failure" + actual response code from the server
    /// 
    /// </summary>
    public class AppsFlyerRequestEventArgs : EventArgs
    {
        public AppsFlyerRequestEventArgs(int code, string description)
        {
            statusCode = code;
            errorDescription = description;
        }

        public int statusCode { get; }
        public string errorDescription { get; }
    }
}