using System;
using System.Collections.Generic;

namespace AppsFlyerSDK
{
    public enum AFSDKValidateAndLogStatus
    {
        AFSDKValidateAndLogStatusSuccess,
        AFSDKValidateAndLogStatusFailure,
        AFSDKValidateAndLogStatusError
    }


    /// <summary>
    // 
    /// </summary>
    public class AFSDKValidateAndLogResult
    {
        public AFSDKValidateAndLogStatus status { get; private set; }
        public Dictionary<string, object> result { get; private set; }
        public Dictionary<string, object> errorData { get; private set; }
        public string error { get; private set; }

        private AFSDKValidateAndLogResult(AFSDKValidateAndLogStatus status, Dictionary<string, object> result, Dictionary<string, object> errorData, string error)
        {
            this.status = status;
            this.result = result;
            this.errorData = errorData;
            this.error = error;
        }

        public static AFSDKValidateAndLogResult Init(AFSDKValidateAndLogStatus status, Dictionary<string, object> result, Dictionary<string, object> errorData, string error)
        {
            return new AFSDKValidateAndLogResult(status, result, errorData, error);
        }
    }

}