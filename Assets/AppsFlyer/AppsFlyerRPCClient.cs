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

        // Wires up the native RPC bridge (Android AndroidJavaClass.init / iOS _setRPCEventHandler) so
        // it knows which Unity GameObject to route onRPCEvent callbacks to. Each implementation owns its
        // own platform #if split, instead of forcing callers to branch on UNITY_ANDROID/UNITY_IOS.
        void InitBridge(string callbackObjectName);
    }

    public class AppsFlyerRPCClient : IAppsFlyerRPCClient
    {
        public static readonly AppsFlyerRPCClient DefaultInstance = new AppsFlyerRPCClient();
        internal static IAppsFlyerRPCClient instance { get; set; } = DefaultInstance;
        private AppsFlyerRPCClient() { }

        // Must match "schemaVersion" in Assets/AppsFlyer/appsflyer-plugins-rpc-schema.json. Native
        // RPC handlers don't validate this yet, so it's currently inert on the wire, but declaring
        // it now future-proofs the envelope for when native adds schema-version validation.
        private const string SchemaVersion = "1.0.6";

        private long _requestCounter = 0;

        public string BuildRequest(string method, Dictionary<string, object> parameters)
        {
            return BuildRequest(method, parameters, out _);
        }

        public string BuildRequest(string method, Dictionary<string, object> parameters, out string id)
        {
            id = method + "-" + System.Threading.Interlocked.Increment(ref _requestCounter);
            var request = new Dictionary<string, object>
            {
                { "id", id },
                { "method", method },
                { "params", parameters ?? new Dictionary<string, object>() },
                { "schemaVersion", SchemaVersion }
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
            else
                Debug.LogWarning("AppsFlyer: dropped fire-and-forget call, RPC bridge failed to load — " + jsonRequest);
#endif
        }

        // jsonRequest/id: Dispatch is an opaque string->string transport (jsonRequest goes over the
        // native bridge as-is); id is passed separately so the editor/no-bridge stub paths can echo it
        // without re-parsing jsonRequest — Execute already has it from BuildRequest.
        public string Dispatch(string jsonRequest, string id)
        {
#if UNITY_IOS && !UNITY_EDITOR
            IntPtr responsePtr = _afExecuteJson(jsonRequest);
            // AppsFlyerRPCWrapper.swift's _afExecuteJson returns strdup() of a Swift String, which is
            // NUL-terminated UTF-8 — PtrToStringAnsi decodes as Latin-1 and would corrupt non-ASCII content.
            try { return Marshal.PtrToStringUTF8(responsePtr); }
            finally { _afFreeCString(responsePtr); }
#elif UNITY_ANDROID && !UNITY_EDITOR
            if (_rpcBridge != null)
                return _rpcBridge.CallStatic<string>("executeJson", jsonRequest);
            return StubResponse(id, "\"error\":{\"code\":503,\"message\":\"AppsFlyer Android RPC bridge failed to load\"}");
#else
            return StubResponse(id, "\"result\":{\"data\":null}");
#endif
        }

#if !UNITY_IOS || UNITY_EDITOR
        // Echoes the request's own id so stub responses still pass ParseResponse's id check.
        private static string StubResponse(string id, string payload)
        {
            return "{\"id\":\"" + id + "\"," + payload + "}";
        }
#endif

        public object ParseResponse(string jsonResponse, string expectedId = null)
        {
            if (string.IsNullOrEmpty(jsonResponse))
                throw new AppsFlyerRPCException(-1, "Empty response from native");

            var root = Json.Deserialize(jsonResponse) as Dictionary<string, object>;
            if (root == null)
                throw new AppsFlyerRPCException(-1, "Malformed response: " + jsonResponse);

            if (expectedId != null)
            {
                object actualId = root.ContainsKey("id") ? root["id"] : null;
                if (!expectedId.Equals(actualId))
                    throw new AppsFlyerRPCException(-1,
                        "RPC response id mismatch: expected " + expectedId + " but got " + actualId);
            }

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
            string request = BuildRequest(method, parameters, out string id);
            string response = Dispatch(request, id);
            return ParseResponse(response, id);
        }

#if UNITY_IOS && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void _afFireJson(string jsonRequest);
        [DllImport("__Internal")]
        private static extern IntPtr _afExecuteJson(string jsonRequest);
        [DllImport("__Internal")]
        private static extern void _afFreeCString(IntPtr ptr);
        [DllImport("__Internal")]
        private static extern void _setRPCEventHandler(string objectName);
#elif UNITY_ANDROID && !UNITY_EDITOR
        private static readonly AndroidJavaClass _rpcBridge = TryLoadAndroidBridge();
        private static AndroidJavaClass TryLoadAndroidBridge()
        {
            try { return new AndroidJavaClass("com.appsflyer.unity.AppsFlyerRPCBridge"); }
            catch (Exception e)
            {
                Debug.LogError("AppsFlyer: failed to load AppsFlyerRPCBridge — " + e);
                return null;
            }
        }
#endif

        public void InitBridge(string callbackObjectName)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            _rpcBridge?.CallStatic("init", callbackObjectName ?? "");
#elif UNITY_IOS && !UNITY_EDITOR
            _setRPCEventHandler(callbackObjectName ?? "");
#endif
        }
    }
}
