using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using AFMiniJSON;

namespace AppsFlyerSDK
{
    public class AppsFlyerRPCException : Exception
    {
        public int Code { get; }
        public object Details { get; }

        public AppsFlyerRPCException(int code, string message, object details = null)
            : base(message)
        {
            Code = code;
            Details = details;
        }
    }

    public interface IAppsFlyerRPCClient
    {
        void ExecuteFire(string method, Dictionary<string, object> parameters = null);
        object Execute(string method, Dictionary<string, object> parameters = null);
    }

    public class AppsFlyerRPCClient : IAppsFlyerRPCClient
    {
        public static readonly AppsFlyerRPCClient DefaultInstance = new AppsFlyerRPCClient();
        public static IAppsFlyerRPCClient instance { get; set; } = DefaultInstance;
        private AppsFlyerRPCClient() { }

        private long _requestCounter = 0;

        public string BuildRequest(string method, Dictionary<string, object> parameters)
        {
            _requestCounter++;
            var request = new Dictionary<string, object>
            {
                { "id", method + "-" + _requestCounter },
                { "method", method },
                { "params", parameters ?? new Dictionary<string, object>() }
            };
            return Json.Serialize(request);
        }

        public void Fire(string jsonRequest)
        {
#if UNITY_IOS && !UNITY_EDITOR
            _afFireJson(jsonRequest);
#elif UNITY_ANDROID && !UNITY_EDITOR
            if (_rpcBridge != null)
                _rpcBridge.CallStatic("fireJson", jsonRequest);
#endif
        }

        public string Dispatch(string jsonRequest)
        {
#if UNITY_IOS && !UNITY_EDITOR
            return _afExecuteJson(jsonRequest);
#elif UNITY_ANDROID && !UNITY_EDITOR
            if (_rpcBridge != null)
                return _rpcBridge.CallStatic<string>("executeJson", jsonRequest);
            return "{\"id\":\"android-stub\",\"result\":{\"data\":null}}";
#else
            return "{\"id\":\"editor\",\"result\":{\"data\":null}}";
#endif
        }

        public object ParseResponse(string jsonResponse)
        {
            if (string.IsNullOrEmpty(jsonResponse))
                throw new AppsFlyerRPCException(-1, "Empty response from native");

            var root = Json.Deserialize(jsonResponse) as Dictionary<string, object>;
            if (root == null)
                throw new AppsFlyerRPCException(-1, "Malformed response: " + jsonResponse);

            if (root.ContainsKey("error"))
            {
                var error = root["error"] as Dictionary<string, object>;
                int code = error != null && error.ContainsKey("code")
                    ? (int)(long)error["code"] : -1;
                string msg = error != null && error.ContainsKey("message")
                    ? (string)error["message"] : "Unknown RPC error";
                object details = error != null && error.ContainsKey("details")
                    ? error["details"] : null;
                throw new AppsFlyerRPCException(code, msg, details);
            }

            if (root.ContainsKey("result"))
            {
                var result = root["result"] as Dictionary<string, object>;
                return result != null && result.ContainsKey("data") ? result["data"] : null;
            }

            return null;
        }

        // Fire-and-forget for setter methods — no return value needed, no main-thread block.
        public void ExecuteFire(string method, Dictionary<string, object> parameters = null)
        {
            string request = BuildRequest(method, parameters);
            Fire(request);
        }

        // Synchronous execute — use only for getter methods that return data.
        public object Execute(string method, Dictionary<string, object> parameters = null)
        {
            string request = BuildRequest(method, parameters);
            string response = Dispatch(request);
            return ParseResponse(response);
        }

#if UNITY_IOS && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void _afFireJson(string jsonRequest);
        [DllImport("__Internal")]
        private static extern string _afExecuteJson(string jsonRequest);
#elif UNITY_ANDROID && !UNITY_EDITOR
        private static readonly AndroidJavaClass _rpcBridge = TryLoadAndroidBridge();
        private static AndroidJavaClass TryLoadAndroidBridge()
        {
            try { return new AndroidJavaClass("com.appsflyer.unity.AppsFlyerRPCBridge"); }
            catch { return null; }
        }
#endif

        public static void InitAndroidBridge(string callbackObjectName)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            _rpcBridge?.CallStatic("init", callbackObjectName ?? "");
#endif
        }
    }
}
