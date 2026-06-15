using System.Collections.Generic;
using UnityEngine;

namespace AppsFlyerSDK
{
#if UNITY_ANDROID 
    /// <summary>
    /// RPC Wrapper - Builds JSON requests and calls Java RPC handler
    /// </summary>
    public class AppsFlyerRPCWrapper
    {
        private static readonly string TAG = "AppsFlyerRPCWrapper";
        
        // Reference to Java RPC handler
        private static AndroidJavaClass rpcHandler = new AndroidJavaClass("com.appsflyer.unity.AppsFlyerAndroidWrapperRPC");
        
        /// <summary>
        /// Initialize the SDK via RPC
        /// </summary>
        /// <param name="devKey">AppsFlyer Dev Key</param>
        /// <param name="hasConversionListener">Whether conversion listener is registered</param>
        /// <returns>JSON response from RPC</returns>
        public static string Init(string devKey, bool hasConversionListener)
        {
            // Build JSON request following Flutter pattern
            var request = new Dictionary<string, object>
            {
                {"method", "init"},
                {"params", new Dictionary<string, object>
                    {
                        {"devKey", devKey},
                        {"hasConversionListener", hasConversionListener}
                    }
                }
            };
            
            // Serialize to JSON string
            string jsonRequest = AFMiniJSON.Json.Serialize(request);
            
            Debug.Log(TAG + " Init Request: " + jsonRequest);
            
            // Call Java RPC handler
            string jsonResponse = rpcHandler.CallStatic<string>("executeJson", jsonRequest);
            
            Debug.Log(TAG + " Init Response: " + jsonResponse);
            
            return jsonResponse;
        }
    }
#endif
}
