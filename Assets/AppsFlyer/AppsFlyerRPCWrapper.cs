#if APPSFLYER_RPC
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace AppsFlyerSDK
{
#if UNITY_IOS || UNITY_ANDROID
    public class AppsFlyerRPCWrapper : IAppsFlyerNativeBridge
    {
        public bool isInit { get; set; }

        private string callbackObjectName;

        internal static bool? pendingDebug;
        internal static string[] pendingOneLinkCustomDomains;
        internal static string pendingCurrencyCode;
        internal static (string prefix, string host)? pendingHost;

#if UNITY_ANDROID && !UNITY_EDITOR
        private static AndroidJavaClass rpcBridge;
#endif

        public AppsFlyerRPCWrapper(string devKey, string appId, MonoBehaviour gameObject)
        {
            if (gameObject != null)
            {
                callbackObjectName = gameObject.name;
            }

#if UNITY_ANDROID && !UNITY_EDITOR
            InitAndroidBridge();
#endif

            var initParams = new Dictionary<string, object>
            {
                { "devKey", devKey },
                { "appId", appId ?? "" }
            };
            ExecuteRPC("init", initParams);

            SetupEventHandler();

            if (pendingDebug.HasValue)
            {
                setIsDebug(pendingDebug.Value);
                pendingDebug = null;
            }
            if (pendingOneLinkCustomDomains != null)
            {
                setOneLinkCustomDomain(pendingOneLinkCustomDomains);
                pendingOneLinkCustomDomains = null;
            }
            if (pendingCurrencyCode != null)
            {
                setCurrencyCode(pendingCurrencyCode);
                pendingCurrencyCode = null;
            }
            if (pendingHost.HasValue)
            {
                setHost(pendingHost.Value.prefix, pendingHost.Value.host);
                pendingHost = null;
            }
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        private void InitAndroidBridge()
        {
            try
            {
                rpcBridge = new AndroidJavaClass("com.appsflyer.unity.rpc.AppsFlyerRPCAndroidWrapper");
                rpcBridge.CallStatic("init", callbackObjectName ?? "");
            }
            catch (Exception e)
            {
                Log(LogType.Error, "Failed to initialize Android RPC bridge: " + e.Message);
            }
        }
#endif

        private void SetupEventHandler()
        {
            if (string.IsNullOrEmpty(callbackObjectName))
                return;

#if UNITY_IOS && !UNITY_EDITOR
            _afRPCSetEventHandler(callbackObjectName);
#endif
        }

        private void ExecuteRPC(string method, Dictionary<string, object> parameters)
        {
            var request = new Dictionary<string, object>
            {
                { "method", method },
                { "params", parameters }
            };
            string json = AFMiniJSON.Json.Serialize(request);
            Log(LogType.Log, method + ": " + json);

#if UNITY_IOS && !UNITY_EDITOR
            _afRPCExecuteJson(json);
#elif UNITY_ANDROID && !UNITY_EDITOR
            ExecuteAndroidRPC(json);
#endif
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        private void ExecuteAndroidRPC(string json)
        {
            if (rpcBridge == null)
            {
                Log(LogType.Error, "Android RPC bridge not initialized.");
                return;
            }
            try
            {
                string response = rpcBridge.CallStatic<string>("execute", json);
                Log(LogType.Log, "Android response: " + response);
            }
            catch (Exception e)
            {
                Log(LogType.Error, "Android execute failed: " + e.Message);
            }
        }
#endif

        // ── RPC-supported methods ───────────────────────────────────────

        public void startSDK(bool onRequestResponse, string CallBackObjectName)
        {
            ExecuteRPC("start", new Dictionary<string, object>());
        }

        public void sendEvent(string eventName, Dictionary<string, string> eventValues, bool onInAppResponse, string CallBackObjectName)
        {
            var parameters = new Dictionary<string, object>
            {
                { "eventName", eventName }
            };

            if (eventValues != null)
            {
                var values = new Dictionary<string, object>();
                foreach (var kvp in eventValues)
                    values[kvp.Key] = kvp.Value;
                parameters["eventValues"] = values;
            }

            ExecuteRPC("logEvent", parameters);
        }

        public void setIsDebug(bool shouldEnable)
        {
            var parameters = new Dictionary<string, object>
            {
                { "isDebug", shouldEnable }
            };
            ExecuteRPC("isDebug", parameters);
        }

        public void getConversionData(string objectName)
        {
            var parameters = new Dictionary<string, object>
            {
                { "isEnabled", true }
            };
            ExecuteRPC("registerConversionListener", parameters);
        }

        public void subscribeForDeepLink(string objectName)
        {
            var parameters = new Dictionary<string, object>
            {
                { "isEnabled", true }
            };
            ExecuteRPC("registerDeeplinkListener", parameters);
        }

        // ── Not yet supported via RPC ───────────────────────────────────

        public void stopSDK(bool isSDKStopped) { LogUnsupported("stopSDK"); }
        public bool isSDKStopped() { LogUnsupported("isSDKStopped"); return false; }
        public string getSdkVersion() { LogUnsupported("getSdkVersion"); return ""; }
        public void setCustomerUserId(string id) { LogUnsupported("setCustomerUserId"); }
        public void setAppInviteOneLinkID(string oneLinkId) { LogUnsupported("setAppInviteOneLinkID"); }
        public void setAdditionalData(Dictionary<string, string> customData) { LogUnsupported("setAdditionalData"); }
        public void setDeepLinkTimeout(long deepLinkTimeout) { LogUnsupported("setDeepLinkTimeout"); }
        public void setResolveDeepLinkURLs(params string[] urls) { LogUnsupported("setResolveDeepLinkURLs"); }
        public void setOneLinkCustomDomain(params string[] domains)
        {
            var parameters = new Dictionary<string, object>
            {
                { "domains", new List<object>(domains) }
            };
            ExecuteRPC("setOneLinkCustomDomain", parameters);
        }
        public void setCurrencyCode(string currencyCode)
        {
            var parameters = new Dictionary<string, object>
            {
                { "currencyCode", currencyCode }
            };
            ExecuteRPC("setCurrencyCode", parameters);
        }
        public void recordLocation(double latitude, double longitude) { LogUnsupported("recordLocation"); }
        public void anonymizeUser(bool shouldAnonymizeUser) { LogUnsupported("anonymizeUser"); }
        public string getAppsFlyerId() { LogUnsupported("getAppsFlyerId"); return ""; }
        public void enableTCFDataCollection(bool shouldCollectTcfData) { LogUnsupported("enableTCFDataCollection"); }
        public void setConsentData(AppsFlyerConsent appsFlyerConsent) { LogUnsupported("setConsentData"); }
        public void logAdRevenue(AFAdRevenueData adRevenueData, Dictionary<string, string> additionalParameters) { LogUnsupported("logAdRevenue"); }
        public void setMinTimeBetweenSessions(int seconds) { LogUnsupported("setMinTimeBetweenSessions"); }
        public void setHost(string hostPrefixName, string hostName)
        {
            var parameters = new Dictionary<string, object>
            {
                { "hostPrefixName", hostPrefixName },
                { "hostName", hostName }
            };
            ExecuteRPC("setHost", parameters);
        }
        public void setPhoneNumber(string phoneNumber) { LogUnsupported("setPhoneNumber"); }
        public void setSharingFilterForAllPartners() { LogUnsupported("setSharingFilterForAllPartners"); }
        public void setSharingFilter(params string[] partners) { LogUnsupported("setSharingFilter"); }
        public void attributeAndOpenStore(string appID, string campaign, Dictionary<string, string> userParams, MonoBehaviour gameObject) { LogUnsupported("attributeAndOpenStore"); }
        public void recordCrossPromoteImpression(string appID, string campaign, Dictionary<string, string> parameters) { LogUnsupported("recordCrossPromoteImpression"); }
        public void generateUserInviteLink(Dictionary<string, string> parameters, MonoBehaviour gameObject) { LogUnsupported("generateUserInviteLink"); }
        public void addPushNotificationDeepLinkPath(params string[] paths) { LogUnsupported("addPushNotificationDeepLinkPath"); }
        public void setUserEmails(EmailCryptType cryptType, params string[] userEmails) { LogUnsupported("setUserEmails"); }
        public void setPartnerData(string partnerId, Dictionary<string, string> partnerInfo) { LogUnsupported("setPartnerData"); }

        private static void LogUnsupported(string methodName)
        {
            Log(LogType.Warning, methodName + " is not yet supported via RPC bridge.");
        }

        private static void Log(LogType type, string message)
        {
            Debug.LogFormat(type, LogOption.NoStacktrace, null, "[AppsFlyer RPC] {0}", message);
        }

        // ── Native declarations ─────────────────────────────────────────

#if UNITY_IOS
        [DllImport("__Internal")]
        private static extern void _afRPCExecuteJson(string jsonRequest);

        [DllImport("__Internal")]
        private static extern void _afRPCSetEventHandler(string objectName);
#endif
    }
#endif
}
#endif
