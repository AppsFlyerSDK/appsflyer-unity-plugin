using System;
using System.Collections.Generic;
using UnityEngine;
using AFMiniJSON;

namespace AppsFlyerSDK
{
    public class AppsFlyer : MonoBehaviour
    {
        public static readonly string kAppsFlyerPluginVersion = "6.17.900";
        public static string CallBackObjectName = null;
        private static EventHandler onRequestResponse;
        private static EventHandler onInAppResponse;
        private static EventHandler onDeepLinkReceived;
        private static EventHandler onSessionReady;
        public static IAppsFlyerNativeBridge instance = null;
        public delegate void unityCallBack(string message);


        /// <summary>
        /// Initialize the AppsFlyer SDK with your devKey and appID.
        /// The dev key is required on all platforms, and the appID is required for iOS. 
        /// If you app is for Android only pass null for the appID.
        /// </summary>
        /// <param name="devKey"> AppsFlyer's Dev-Key, which is accessible from your AppsFlyer account under 'App Settings' in the dashboard.</param>
        /// <param name="appID">Your app's Apple ID.</param>
        /// <example>
        /// <code>
        /// AppsFlyer.initSDK("K2***********99", "41*****44"");
        /// </code>
        /// </example>
        public static void initSDK(string devKey, string appID)
        {
            initSDK(devKey, appID, null);
        }

        /// <summary>
        /// Initialize the AppsFlyer SDK with your devKey and appID.
        /// The dev key is required on all platforms, and the appID is required for iOS. 
        /// If you app is for Android only pass null for the appID.
        /// </summary>
        /// <param name="devKey"> AppsFlyer's Dev-Key, which is accessible from your AppsFlyer account under 'App Settings' in the dashboard.</param>
        /// <param name="appID">Your app's Apple ID.</param>
        /// <param name="gameObject">pass the script of the game object being used.</param>
        /// <example>
        /// <code>
        /// AppsFlyer.initSDK("K2***********99", 41*****44, this);
        /// </code>
        /// </example>
        public static void initSDK(string devKey, string appID, MonoBehaviour gameObject)
        {

            if (gameObject != null)
            {
#if UNITY_STANDALONE_OSX
                CallBackObjectName = gameObject.GetType().ToString();
#else
                CallBackObjectName = gameObject.name;
#endif
            }

#if UNITY_IOS || UNITY_STANDALONE_OSX
            if (instance == null || !instance.isInit)
            {
                instance = new AppsFlyeriOS(devKey, appID, gameObject);
                instance.isInit = true;
            }
#elif UNITY_ANDROID
            if (instance == null || !instance.isInit)
            {
                AppsFlyerAndroid appsFlyerAndroid = new AppsFlyerAndroid();
                appsFlyerAndroid.initSDK(devKey, gameObject);
                instance = appsFlyerAndroid;
                instance.isInit = true;
                
            }
#elif UNITY_WSA_10_0
            AppsFlyerWindows.InitSDK(devKey, appID, gameObject);
            if (gameObject != null)
            {
                AppsFlyerWindows.GetConversionData(gameObject.name);
            }
#else

#endif
            try
            {
#if UNITY_ANDROID
                AppsFlyerRPCClient.instance.ExecuteFire("init", new Dictionary<string, object>
                {
                    { "devKey", devKey }
                });
#elif UNITY_IOS || UNITY_STANDALONE_OSX
                AppsFlyerRPCClient.instance.ExecuteFire("initialize", new Dictionary<string, object>
                {
                    { "devKey", devKey },
                    { "appId", appID }
                });
#endif
            }
            catch (AppsFlyerRPCException e)
            {
                AFLog("initSDK", "RPC error: " + e.Message);
            }
        }


        /// <summary>
        /// Once this API is invoked, our SDK will start.
        /// Once the API is called a sessions will be immediately sent, and all background forground transitions will send a session.
        /// </summary>
        public static void startSDK()
        {
#if UNITY_WSA_10_0
              AppsFlyerWindows.Start();
           
#else
#if UNITY_IOS || UNITY_STANDALONE_OSX
            if (instance != null)
            {
                instance.startSDK(onRequestResponse != null, CallBackObjectName);
            }
#endif
            try
            {
                AppsFlyerRPCClient.instance.ExecuteFire("start");
            }
            catch (AppsFlyerRPCException e)
            {
                AFLog("startSDK", "RPC error: " + e.Message);
            }
#endif
        }

        
  

     

        /// <summary>
        /// Send an In-App Event.
        /// In-App Events provide insight on what is happening in your app.
        /// </summary>
        /// <param name="eventName">Event Name as String.</param>
        /// <param name="eventValues">Event Values as Dictionary.</param>
        public static void sendEvent(string eventName, Dictionary<string, string> eventValues)
        {
#if UNITY_WSA_10_0 && !UNITY_EDITOR
            AppsFlyerWindows.LogEvent(eventName, eventValues);
#else
            if (instance != null)
            {
                instance.sendEvent(eventName, eventValues, onInAppResponse != null, CallBackObjectName);
            }
            try
            {
                AppsFlyerRPCClient.instance.ExecuteFire("logEvent", new Dictionary<string, object>
                {
                    { "eventName", eventName },
                    { "eventValues", eventValues }
                });
            }
            catch (AppsFlyerRPCException e)
            {
                AFLog("sendEvent", "RPC error: " + e.Message);
            }
#endif
        }
        /// <summary>
        /// Once this API is invoked, our SDK no longer communicates with our servers and stops functioning.
        /// In some extreme cases you might want to shut down all SDK activity due to legal and privacy compliance.
        /// This can be achieved with the stopSDK API.
        /// </summary>
        /// <param name="isSDKStopped"> should sdk be stopped.</param>
        public static void stopSDK(bool isSDKStopped)
        {
            try
            {
                AppsFlyerRPCClient.instance.ExecuteFire("stop", new Dictionary<string, object>
                {
                    { "stopped", isSDKStopped }
                });
            }
            catch (AppsFlyerRPCException e) { AFLog("stopSDK", "RPC error: " + e.Message); }
        }

        // <summary>
        /// Was the stopSDK(boolean) API set to true.
        /// </summary>
        /// <returns>boolean isSDKStopped.</returns>
        public static bool isSDKStopped()
        {
            if (instance != null)
            {
                return instance.isSDKStopped();
            }

            return false;
        }

        /// <summary>
        /// Get the AppsFlyer SDK version used in app.
        /// </summary>
        /// <returns>The current SDK version.</returns>
        public static string getSdkVersion()
        {
            if (instance != null)
            {
                return instance.getSdkVersion();
            }

            return "";

        }

        /// <summary>
        /// Enables Debug logs for the AppsFlyer SDK.
        /// Should only be set to true in development / debug.
        /// </summary>
        /// <param name="shouldEnable">shouldEnable boolean.</param>
        public static void setIsDebug(bool shouldEnable)
        {
            if (instance != null)
            {
                instance.setIsDebug(shouldEnable);
            } else {
#if UNITY_IOS || UNITY_STANDALONE_OSX
                instance = new AppsFlyeriOS();
                instance.setIsDebug(shouldEnable);
#elif UNITY_ANDROID
                instance = new AppsFlyerAndroid();
                instance.setIsDebug(shouldEnable);
#else

#endif
            }
            try
            {
                AppsFlyerRPCClient.instance.ExecuteFire("isDebug", new Dictionary<string, object>
                {
                    { "enabled", shouldEnable }
                });
            }
            catch (AppsFlyerRPCException e)
            {
                AFLog("setIsDebug", "RPC error: " + e.Message);
            }

        }

        /// <summary>
        /// Setting your own customer ID enables you to cross-reference your own unique ID with AppsFlyer’s unique ID and the other devices’ IDs.
        /// This ID is available in AppsFlyer CSV reports along with Postback APIs for cross-referencing with your internal IDs.
        /// </summary>
        /// <param name="id">Customer ID for client.</param>
        public static void setCustomerUserId(string id)
        {
#if UNITY_WSA_10_0 && !UNITY_EDITOR
             AppsFlyerWindows.SetCustomerUserId(id);
#else
            if (instance != null)
            {
                instance.setCustomerUserId(id);
            }
            try
            {
                AppsFlyerRPCClient.instance.ExecuteFire("setCustomerUserId", new Dictionary<string, object>
                {
                    { "userId", id }
                });
            }
            catch (AppsFlyerRPCException e) { AFLog("setCustomerUserId", "RPC error: " + e.Message); }
#endif
        }

        /// <summary>
        /// Set the OneLink ID that should be used for User-Invite-API.
        /// The link that is generated for the user invite will use this OneLink as the base link.
        /// </summary>
        /// <param name="oneLinkId">OneLink ID obtained from the AppsFlyer Dashboard.</param>
        public static void setAppInviteOneLinkID(string oneLinkId)
        {
            try
            {
                AppsFlyerRPCClient.instance.ExecuteFire("setAppInviteOneLink", new Dictionary<string, object>
                {
                    { "oneLinkId", oneLinkId }
                });
            }
            catch (AppsFlyerRPCException e) { AFLog("setAppInviteOneLinkID", "RPC error: " + e.Message); }
        }

        /// <summary>
        /// Set the deepLink timeout value that should be used for DDL.
        /// </summary>
        /// <param name="deepLinkTimeout">deepLink timeout in milliseconds.</param>
        public static void setDeepLinkTimeout(long deepLinkTimeout)
        {
            if (instance != null)
            {
                instance.setDeepLinkTimeout(deepLinkTimeout);
            }
            try
            {
                AppsFlyerRPCClient.instance.ExecuteFire("setDeepLinkTimeout", new Dictionary<string, object>
                {
                    { "timeout", deepLinkTimeout }
                });
            }
            catch (AppsFlyerRPCException e) { AFLog("setDeepLinkTimeout", "RPC error: " + e.Message); }
        }

        /// <summary>
        /// Set additional data to be sent to AppsFlyer.
        /// </summary>
        /// <param name="customData">additional data Dictionary.</param>
        public static void setAdditionalData(Dictionary<string, string> customData)
        {
            if (instance != null)
            {
                instance.setAdditionalData(customData);
            }
            try
            {
                AppsFlyerRPCClient.instance.ExecuteFire("setAdditionalData", new Dictionary<string, object>
                {
                    { "customData", customData }
                });
            }
            catch (AppsFlyerRPCException e) { AFLog("setAdditionalData", "RPC error: " + e.Message); }
        }

        /// <summary>
        /// Advertisers can wrap AppsFlyer OneLink within another Universal Link.
        /// This Universal Link will invoke the app but any deep linking data will not propagate to AppsFlyer.
        /// </summary>
        /// <param name="urls">Array of urls.</param>
        public static void setResolveDeepLinkURLs(params string[] urls)
        {
            if (instance != null)
            {
                instance.setResolveDeepLinkURLs(urls);
            }
            try
            {
                AppsFlyerRPCClient.instance.ExecuteFire("setResolveDeepLinkURLs", new Dictionary<string, object>
                {
                    { "urls", urls }
                });
            }
            catch (AppsFlyerRPCException e) { AFLog("setResolveDeepLinkURLs", "RPC error: " + e.Message); }
        }

        /// <summary>
        /// Advertisers can use this method to set vanity onelink domains.
        /// </summary>
        /// <param name="domains">Array of domains.</param>
        public static void setOneLinkCustomDomain(params string[] domains)
        {
            if (instance != null)
            {
                instance.setOneLinkCustomDomain(domains);
            }
            else
            {
#if UNITY_IOS || UNITY_STANDALONE_OSX
                instance = new AppsFlyeriOS();
#elif UNITY_ANDROID
                instance = new AppsFlyerAndroid();
#else
#endif
            }
            try
            {
                AppsFlyerRPCClient.instance.ExecuteFire("setOneLinkCustomDomain", new Dictionary<string, object>
                {
                    { "domains", domains }
                });
            }
            catch (AppsFlyerRPCException e) { AFLog("setOneLinkCustomDomain", "RPC error: " + e.Message); }
        }

        /// <summary>
        /// Setting user local currency code for in-app purchases.
        /// The currency code should be a 3 character ISO 4217 code. (default is USD).
        /// You can set the currency code for all events by calling the following method.
        /// </summary>
        /// <param name="currencyCode">3 character ISO 4217 code.</param>
        public static void setCurrencyCode(string currencyCode)
        {
            if (instance != null)
            {
                instance.setCurrencyCode(currencyCode);
            } else {
#if UNITY_IOS || UNITY_STANDALONE_OSX
                instance = new AppsFlyeriOS();
                instance.setCurrencyCode(currencyCode);
#elif UNITY_ANDROID
                instance = new AppsFlyerAndroid();
                instance.setCurrencyCode(currencyCode);
#else
#endif
            }
            try
            {
                AppsFlyerRPCClient.instance.ExecuteFire("setCurrencyCode", new Dictionary<string, object>
                {
                    { "currencyCode", currencyCode }
                });
            }
            catch (AppsFlyerRPCException e) { AFLog("setCurrencyCode", "RPC error: " + e.Message); }
        }

        /// <summary>
        /// Sets or updates the user consent data related to GDPR and DMA regulations for advertising and data usage purposes within the application.
        /// </summary>
        /// <param name = "appsFlyerConsent" >instance of AppsFlyerConsent.</param>
        public static void setConsentData(AppsFlyerConsent appsFlyerConsent)
        {
            if (instance != null)
            {
                instance.setConsentData(appsFlyerConsent);
            }
            try
            {
                AppsFlyerRPCClient.instance.ExecuteFire("setConsentData", new Dictionary<string, object>
                {
                    { "isUserSubjectToGDPR", appsFlyerConsent?.isUserSubjectToGDPR },
                    { "hasConsentForDataUsage", appsFlyerConsent?.hasConsentForDataUsage },
                    { "hasConsentForAdsPersonalization", appsFlyerConsent?.hasConsentForAdsPersonalization },
                    { "hasConsentForAdStorage", appsFlyerConsent?.hasConsentForAdStorage }
                });
            }
            catch (AppsFlyerRPCException e) { AFLog("setConsentData", "RPC error: " + e.Message); }
        }

        /// <summary>
        /// Logs ad revenue data along with additional parameters if provided.
        /// </summary>
        /// <param name = "adRevenueData" >instance of AFAdRevenueData containing ad revenue information.</param>
        /// <param name = "additionalParameters" >An optional map of additional parameters to be logged with ad revenue data. This can be null if there are no additional parameters.</param>
        public static void logAdRevenue(AFAdRevenueData adRevenueData, Dictionary<string, string> additionalParameters)
        {
            if (instance != null)
            {
                instance.logAdRevenue(adRevenueData, additionalParameters);
            }
            try
            {
                AppsFlyerRPCClient.instance.ExecuteFire("logAdRevenue", new Dictionary<string, object>
                {
                    { "monetizationNetwork", adRevenueData?.monetizationNetwork },
                    { "mediationNetwork", adRevenueData != null ? (int)adRevenueData.mediationNetwork : 0 },
                    { "currencyIso4217Code", adRevenueData?.currencyIso4217Code },
                    { "eventRevenue", adRevenueData?.eventRevenue },
                    { "additionalParameters", additionalParameters }
                });
            }
            catch (AppsFlyerRPCException e) { AFLog("logAdRevenue", "RPC error: " + e.Message); }
        }

        /// <summary>
        /// Manually record the location of the user.
        /// </summary>
        /// <param name="latitude">latitude as double.</param>
        /// <param name="longitude">longitude as double.</param>
        public static void recordLocation(double latitude, double longitude)
        {
            if (instance != null)
            {
                instance.recordLocation(latitude, longitude);
            }
            try
            {
                AppsFlyerRPCClient.instance.ExecuteFire("logLocation", new Dictionary<string, object>
                {
                    { "latitude", latitude },
                    { "longitude", longitude }
                });
            }
            catch (AppsFlyerRPCException e) { AFLog("recordLocation", "RPC error: " + e.Message); }
        }

        /// <summary>
        /// Anonymize user Data.
        /// Use this API during the SDK Initialization to explicitly anonymize a user's installs, events and sessions.
        /// Default is false.
        /// </summary>
        /// <param name = "shouldAnonymizeUser" >shouldAnonymizeUser boolean.</param>
        public static void anonymizeUser(bool shouldAnonymizeUser)
        {
            try
            {
                AppsFlyerRPCClient.instance.ExecuteFire("anonymizeUser", new Dictionary<string, object>
                {
                    { "anonymize", shouldAnonymizeUser }
                });
            }
            catch (AppsFlyerRPCException e) { AFLog("anonymizeUser", "RPC error: " + e.Message); }
        }

        /// <summary>
        /// Calling enableTCFDataCollection(true) will enable collecting and sending any TCF related data.
        /// Calling enableTCFDataCollection(false) will disable the collection of TCF related data and from sending it.
        /// </summary>
        /// <param name = "shouldCollectTcfData" >should start TCF Data collection boolean.</param>
        public static void enableTCFDataCollection(bool shouldCollectTcfData)
        {
            if (instance != null)
            {
                instance.enableTCFDataCollection(shouldCollectTcfData);
            }
            try
            {
                AppsFlyerRPCClient.instance.ExecuteFire("enableTCFDataCollection", new Dictionary<string, object>
                {
                    { "enable", shouldCollectTcfData }
                });
            }
            catch (AppsFlyerRPCException e) { AFLog("enableTCFDataCollection", "RPC error: " + e.Message); }
        }

        /// <summary>
        /// Get AppsFlyer's unique device ID which is created for every new install of an app.
        /// </summary>
        /// <returns>AppsFlyer's unique device ID.</returns>
        public static string getAppsFlyerId()
        {
#if UNITY_WSA_10_0 && !UNITY_EDITOR
            return AppsFlyerWindows.GetAppsFlyerId();
#else
            if (instance != null)
            {
                return instance.getAppsFlyerId();
            }
#endif
            try { AppsFlyerRPCClient.instance.Execute("getAppsFlyerUID"); }
            catch (AppsFlyerRPCException e) { AFLog("getAppsFlyerId", "RPC error: " + e.Message); }
            return string.Empty;
        }

        /// <summary>
        /// Set a custom value for the minimum required time between sessions.
        /// By default, at least 5 seconds must lapse between 2 app launches to count as separate 2 sessions.
        /// </summary>
        /// <param name="seconds">minimum time between 2 separate sessions in seconds.</param>
        public static void setMinTimeBetweenSessions(int seconds)
        {
            if (instance != null)
            {
                instance.setMinTimeBetweenSessions(seconds);
            }
            try
            {
                AppsFlyerRPCClient.instance.ExecuteFire("setMinTimeBetweenSessions", new Dictionary<string, object>
                {
                    { "seconds", seconds }
                });
            }
            catch (AppsFlyerRPCException e) { AFLog("setMinTimeBetweenSessions", "RPC error: " + e.Message); }
        }

        /// <summary>
        /// Set a custom host.
        /// </summary>
        /// <param name="hostPrefixName">Host prefix.</param>
        /// <param name="hostName">Host name.</param>
        public static void setHost(string hostPrefixName, string hostName)
        {
            if (instance != null)
            {
                instance.setHost(hostPrefixName, hostName);
            } else {
#if UNITY_IOS || UNITY_STANDALONE_OSX
                instance = new AppsFlyeriOS();
                instance.setHost(hostPrefixName, hostName);
#elif UNITY_ANDROID
                instance = new AppsFlyerAndroid();
                instance.setHost(hostPrefixName, hostName);
#else
#endif
            }
            try
            {
                AppsFlyerRPCClient.instance.ExecuteFire("setHost", new Dictionary<string, object>
                {
                    { "hostPrefixName", hostPrefixName },
                    { "hostName", hostName }
                });
            }
            catch (AppsFlyerRPCException e) { AFLog("setHost", "RPC error: " + e.Message); }
        }

        /// <summary>
        /// Set the user emails and encrypt them.
        /// cryptMethod Encryption method:
        /// EmailCryptType.EmailCryptTypeMD5
        /// EmailCryptType.EmailCryptTypeSHA1
        /// EmailCryptType.EmailCryptTypeSHA256
        /// EmailCryptType.EmailCryptTypeNone
        /// </summary>
        /// <param name="cryptMethod">Encryption method.</param>
        /// <param name="emails">User emails.</param>
        public static void setUserEmails(EmailCryptType cryptType, params string[] userEmails)
        {
            try
            {
#if UNITY_ANDROID
                if (userEmails != null && userEmails.Length > 0)
                    AppsFlyerRPCClient.instance.ExecuteFire("setUserEmail", new Dictionary<string, object>
                    {
                        { "email", userEmails[0] }
                    });
#elif UNITY_IOS || UNITY_STANDALONE_OSX
                AppsFlyerRPCClient.instance.ExecuteFire("setUserEmails", new Dictionary<string, object>
                {
                    { "cryptType", (int)cryptType },
                    { "emails", userEmails }
                });
#endif
            }
            catch (AppsFlyerRPCException e) { AFLog("setUserEmails", "RPC error: " + e.Message); }
        }

        public static void updateServerUninstallToken(string token)
        {
            if (instance != null && instance is IAppsFlyerAndroidBridge)
            {
                IAppsFlyerAndroidBridge appsFlyerAndroidInstance = (IAppsFlyerAndroidBridge)instance;
                appsFlyerAndroidInstance.updateServerUninstallToken(token);
            }
        }

        /// <summary>
        /// Set the user phone number.
        /// </summary>
        /// <param name="phoneNumber">phoneNumber string</param>
        public static void setPhoneNumber(string phoneNumber)
        {
            // Android RPC bridge requires countryCode for setUserPhone; no single-arg
            // phone setter is exposed. iOS uses setPhoneNumber without countryCode.
#if UNITY_IOS || UNITY_STANDALONE_OSX
            try
            {
                AppsFlyerRPCClient.instance.ExecuteFire("setPhoneNumber", new Dictionary<string, object>
                {
                    { "phoneNumber", phoneNumber }
                });
            }
            catch (AppsFlyerRPCException e) { AFLog("setPhoneNumber", "RPC error: " + e.Message); }
#endif
        }

        public static void setImeiData(string aImei)
        {
            if (instance != null && instance is IAppsFlyerAndroidBridge)
            {
                IAppsFlyerAndroidBridge appsFlyerAndroidInstance = (IAppsFlyerAndroidBridge)instance;
                appsFlyerAndroidInstance.setImeiData(aImei);
            }
        }

        /// <summary>
        /// Used by advertisers to exclude all networks/integrated partners from getting data.
        /// </summary>
        [Obsolete("Please use setSharingFilterForPartners api")]
        public static void setSharingFilterForAllPartners()
        {
            if (instance != null)
            {
                instance.setSharingFilterForAllPartners();
            }
            try
            {
                AppsFlyerRPCClient.instance.ExecuteFire("setSharingFilterForPartners", new Dictionary<string, object>
                {
                    { "partners", new string[] { "all" } }
                });
            }
            catch (AppsFlyerRPCException e) { AFLog("setSharingFilterForAllPartners", "RPC error: " + e.Message); }
        }

        public static void setAndroidIdData(string aAndroidId)
        {
            if (instance != null && instance is IAppsFlyerAndroidBridge)
            {
                IAppsFlyerAndroidBridge appsFlyerAndroidInstance = (IAppsFlyerAndroidBridge)instance;
                appsFlyerAndroidInstance.setAndroidIdData(aAndroidId);
            }
        }

        public static void waitForCustomerUserId(bool wait)
        {
            if (instance != null && instance is IAppsFlyerAndroidBridge)
            {
                IAppsFlyerAndroidBridge appsFlyerAndroidInstance = (IAppsFlyerAndroidBridge)instance;
                appsFlyerAndroidInstance.waitForCustomerUserId(wait);
            }
        }

        /// <summary>
        /// Used by advertisers to set some (one or more) networks/integrated partners to exclude from getting data.
        /// </summary>
        /// <param name="partners">partners to exclude from getting data</param>
        [Obsolete("Please use setSharingFilterForPartners api")]
        public static void setSharingFilter(params string[] partners)
        {
            if (instance != null)
            {
                instance.setSharingFilter(partners);
            }
            try
            {
                AppsFlyerRPCClient.instance.ExecuteFire("setSharingFilterForPartners", new Dictionary<string, object>
                {
                    { "partners", partners }
                });
            }
            catch (AppsFlyerRPCException e) { AFLog("setSharingFilter", "RPC error: " + e.Message); }
        }

        public static void setCustomerIdAndStartSDK(string id)
        {
            if (instance != null && instance is IAppsFlyerAndroidBridge)
            {
                IAppsFlyerAndroidBridge appsFlyerAndroidInstance = (IAppsFlyerAndroidBridge)instance;
                appsFlyerAndroidInstance.setCustomerIdAndStartSDK(id);
            }
        }

        /// <summary>
        /// Lets you configure how which partners should the SDK exclude from data-sharing.
        /// </summary>
        /// <param name="partners">partners to exclude from getting data</param>
        public static void setSharingFilterForPartners(params string[] partners)
        {
#if UNITY_IOS || UNITY_STANDALONE_OSX
            AppsFlyeriOS.setSharingFilterForPartners(partners);
#elif UNITY_ANDROID
            AppsFlyerAndroid.setSharingFilterForPartners(partners);
#else
#endif
            try
            {
                AppsFlyerRPCClient.instance.ExecuteFire("setSharingFilterForPartners", new Dictionary<string, object>
                {
                    { "partners", partners }
                });
            }
            catch (AppsFlyerRPCException e) { AFLog("setSharingFilterForPartners", "RPC error: " + e.Message); }
        }

        public static string getOutOfStore()
        {
            if (instance != null && instance is IAppsFlyerAndroidBridge)
            {
                IAppsFlyerAndroidBridge appsFlyerAndroidInstance = (IAppsFlyerAndroidBridge)instance;
                return appsFlyerAndroidInstance.getOutOfStore();
            }
            return string.Empty;
        }

        public static void setOutOfStore(string sourceName)
        {
            if (instance != null && instance is IAppsFlyerAndroidBridge)
            {
                IAppsFlyerAndroidBridge appsFlyerAndroidInstance = (IAppsFlyerAndroidBridge)instance;
                appsFlyerAndroidInstance.setOutOfStore(sourceName);
            }
        }

        /// <summary>
        /// Register a Conversion Data Listener.
        /// Allows the developer to access the user attribution data in real-time for every new install, directly from the SDK level.
        /// By doing this you can serve users with personalized content or send them to specific activities within the app,
        /// which can greatly enhance their engagement with your app.
        /// </summary>
        /// <example>
        /// <code>
        /// AppsFlyer.getConversionData(this.name);
        /// </code>
        /// </example>
        public static void getConversionData(string objectName)
        {
#if UNITY_WSA_10_0 && !UNITY_EDITOR
            AppsFlyerWindows.GetConversionData("");
#else
            if (instance != null)
            {
                instance.getConversionData(objectName);
            }
            try
            {
                AppsFlyerRPCClient.instance.ExecuteFire("registerConversionListener", new Dictionary<string, object>
                {
                    { "register", true },
                    { "callbackObjectName", objectName }
                });
            }
            catch (AppsFlyerRPCException e)
            {
                AFLog("getConversionData", "RPC error: " + e.Message);
            }
#endif

        }

        public static void setCollectAndroidID(bool isCollect)
        {
            if (instance != null && instance is IAppsFlyerAndroidBridge)
            {
                IAppsFlyerAndroidBridge appsFlyerAndroidInstance = (IAppsFlyerAndroidBridge)instance;
                appsFlyerAndroidInstance.setCollectAndroidID(isCollect);
            }
        }

        public static void setIsUpdate(bool isUpdate)
        {
            if (instance != null && instance is IAppsFlyerAndroidBridge)
            {
                IAppsFlyerAndroidBridge appsFlyerAndroidInstance = (IAppsFlyerAndroidBridge)instance;
                appsFlyerAndroidInstance.setIsUpdate(isUpdate);
            }
        }

        public static void setCollectIMEI(bool isCollect)
        {
            if (instance != null && instance is IAppsFlyerAndroidBridge)
            {
                IAppsFlyerAndroidBridge appsFlyerAndroidInstance = (IAppsFlyerAndroidBridge)instance;
                appsFlyerAndroidInstance.setCollectIMEI(isCollect);
            }
        }

        public static void setDisableCollectAppleAdSupport(bool disable)
        {
            try
            {
                AppsFlyerRPCClient.instance.ExecuteFire("setDisableCollectASA", new Dictionary<string, object>
                {
                    { "disable", disable }
                });
            }
            catch (AppsFlyerRPCException e) { AFLog("setDisableCollectAppleAdSupport", "RPC error: " + e.Message); }
        }

        public static void setShouldCollectDeviceName(bool shouldCollectDeviceName)
        {
#if (UNITY_IOS || UNITY_STANDALONE_OSX) && !UNITY_ANDROID
            try
            {
                AppsFlyerRPCClient.instance.ExecuteFire("setShouldCollectDeviceName", new Dictionary<string, object>
                {
                    { "collect", shouldCollectDeviceName }
                });
            }
            catch (AppsFlyerRPCException e) { AFLog("setShouldCollectDeviceName", "RPC error: " + e.Message); }
#endif
        }


        /// <summary>
        /// Use the following API to attribute the click and launch the app store's app page.
        /// </summary>
        /// <param name="appID">promoted App ID</param>
        /// <param name="campaign">cross promotion campaign</param>
        /// <param name="userParams">additional user params</param>
        /// <example>
        /// <code>
        /// Dictionary<string, string> parameters = new Dictionary<string, string>();
        /// parameters.Add("af_sub1", "val");
        /// parameters.Add("custom_param", "val2");
        /// AppsFlyer.attributeAndOpenStore("123456789", "test campaign", parameters, this);
        /// </code>
        /// </example>
        public static void attributeAndOpenStore(string appID, string campaign, Dictionary<string, string> userParams, MonoBehaviour gameObject)
        {
            if (instance != null)
            {
                instance.attributeAndOpenStore(appID, campaign, userParams, gameObject);
            }
            try
            {
                AppsFlyerRPCClient.instance.ExecuteFire("logAndOpenStore", new Dictionary<string, object>
                {
                    { "appId", appID },
                    { "campaign", campaign },
                    { "userParams", userParams }
                });
            }
            catch (AppsFlyerRPCException e) { AFLog("attributeAndOpenStore", "RPC error: " + e.Message); }
        }

        public static void setPreinstallAttribution(string mediaSource, string campaign, string siteId)
        {
            if (instance != null && instance is IAppsFlyerAndroidBridge)
            {
                IAppsFlyerAndroidBridge appsFlyerAndroidInstance = (IAppsFlyerAndroidBridge)instance;
                appsFlyerAndroidInstance.setPreinstallAttribution(mediaSource, campaign, siteId);
            }
        }

        public static void setDisableCollectIAd(bool disableCollectIAd)
        {
            try
            {
                AppsFlyerRPCClient.instance.ExecuteFire("setDisableAppleAdsAttribution", new Dictionary<string, object>
                {
                    { "disable", disableCollectIAd }
                });
            }
            catch (AppsFlyerRPCException e) { AFLog("setDisableCollectIAd", "RPC error: " + e.Message); }
        }

        public static bool isPreInstalledApp()
        {
            if (instance != null && instance is IAppsFlyerAndroidBridge)
            {
                IAppsFlyerAndroidBridge appsFlyerAndroidInstance = (IAppsFlyerAndroidBridge)instance;
                return appsFlyerAndroidInstance.isPreInstalledApp();
            }
            return false;
        }

        public static void setUseReceiptValidationSandbox(bool useReceiptValidationSandbox)
        {
            try
            {
                AppsFlyerRPCClient.instance.ExecuteFire("setUseReceiptValidationSandbox", new Dictionary<string, object>
                {
                    { "sandbox", useReceiptValidationSandbox }
                });
            }
            catch (AppsFlyerRPCException e) { AFLog("setUseReceiptValidationSandbox", "RPC error: " + e.Message); }
        }

        /// <summary>
        /// To attribute an impression use the following API call.
        /// Make sure to use the promoted App ID as it appears within the AppsFlyer dashboard.
        /// </summary>
        /// <param name="appID">promoted App ID.</param>
        /// <param name="campaign">cross promotion campaign.</param>
        /// <param name="parameters">parameters Dictionary.</param>
        public static void recordCrossPromoteImpression(string appID, string campaign, Dictionary<string, string> parameters)
        {
            if (instance != null)
            {
                instance.recordCrossPromoteImpression(appID, campaign, parameters);
            }
            try
            {
                AppsFlyerRPCClient.instance.ExecuteFire("logCrossPromoteImpression", new Dictionary<string, object>
                {
                    { "appId", appID },
                    { "campaign", campaign },
                    { "params", parameters }
                });
            }
            catch (AppsFlyerRPCException e) { AFLog("recordCrossPromoteImpression", "RPC error: " + e.Message); }
        }

        public static void setUseUninstallSandbox(bool useUninstallSandbox)
        {
            try
            {
                AppsFlyerRPCClient.instance.ExecuteFire("setUseUninstallSandbox", new Dictionary<string, object>
                {
                    { "sandbox", useUninstallSandbox }
                });
            }
            catch (AppsFlyerRPCException e) { AFLog("setUseUninstallSandbox", "RPC error: " + e.Message); }
        }

        public static string getAttributionId()
        {
            if (instance != null && instance is IAppsFlyerAndroidBridge)
            {
                IAppsFlyerAndroidBridge appsFlyerAndroidInstance = (IAppsFlyerAndroidBridge)instance;
                return appsFlyerAndroidInstance.getAttributionId();
            }
            return string.Empty;
        }

        public static void handlePushNotifications()
        {
            if (instance != null && instance is IAppsFlyerAndroidBridge)
            {
                IAppsFlyerAndroidBridge appsFlyerAndroidInstance = (IAppsFlyerAndroidBridge)instance;
                appsFlyerAndroidInstance.handlePushNotifications();
            }
        }

        /// <summary>
        /// [Deprecated] Validates an in-app purchase on iOS.
        /// Use the V2 overload with AFSDKPurchaseDetailsIOS instead.
        /// </summary>
        [System.Obsolete("This method is deprecated. Use validateAndSendInAppPurchase(AFSDKPurchaseDetailsIOS details, Dictionary<string, string> purchaseAdditionalDetails, MonoBehaviour gameObject) instead.")]
        public static void validateAndSendInAppPurchase(string productIdentifier, string price, string currency, string transactionId, Dictionary<string, string> additionalParameters, MonoBehaviour gameObject)
        {
            if (instance != null && instance is IAppsFlyerIOSBridge)
            {
                IAppsFlyerIOSBridge appsFlyeriOSInstance = (IAppsFlyerIOSBridge)instance;
                appsFlyeriOSInstance.validateAndSendInAppPurchase(productIdentifier, price, currency, transactionId, additionalParameters, gameObject);
            }
        }

        /// <summary>
        /// Validates an in-app purchase on iOS using the V2 API.
        /// </summary>
        public static void validateAndSendInAppPurchase(AFSDKPurchaseDetailsIOS details, Dictionary<string, string> purchaseAdditionalDetails, MonoBehaviour gameObject)
        {
            try
            {
                AppsFlyerRPCClient.instance.ExecuteFire("validateAndLogInAppPurchase", new Dictionary<string, object>
                {
                    { "productId", details?.productId },
                    { "transactionId", details?.transactionId },
                    { "purchaseType", details != null ? (int)details.purchaseType : 0 },
                    { "additionalDetails", purchaseAdditionalDetails },
                    { "callbackObjectName", gameObject != null ? gameObject.name : null }
                });
            }
            catch (AppsFlyerRPCException e) { AFLog("validateAndSendInAppPurchase", "RPC error: " + e.Message); }
        }

        /// <summary>
        /// [Deprecated] Validates an in-app purchase on Android.
        /// Use the V2 overload with AFPurchaseDetailsAndroid instead.
        /// </summary>
        [System.Obsolete("This method is deprecated. Use validateAndSendInAppPurchase(AFPurchaseDetailsAndroid details, Dictionary<string, string> purchaseAdditionalDetails, MonoBehaviour gameObject) instead.")]
        public static void validateAndSendInAppPurchase(string publicKey, string signature, string purchaseData, string price, string currency, Dictionary<string, string> additionalParameters, MonoBehaviour gameObject)
        {
            if (instance != null && instance is IAppsFlyerAndroidBridge)
            {
                IAppsFlyerAndroidBridge appsFlyerAndroidInstance = (IAppsFlyerAndroidBridge)instance;
                appsFlyerAndroidInstance.validateAndSendInAppPurchase(publicKey, signature,purchaseData, price, currency, additionalParameters, gameObject);
            }
        }

        /// <summary>
        /// Validates an in-app purchase on Android using the V2 API.
        /// </summary>
        public static void validateAndSendInAppPurchase(AFPurchaseDetailsAndroid details, Dictionary<string, string> purchaseAdditionalDetails, MonoBehaviour gameObject)
        {
            if (instance != null && instance is IAppsFlyerAndroidBridge)
            {
                IAppsFlyerAndroidBridge appsFlyerAndroidInstance = (IAppsFlyerAndroidBridge)instance;
                appsFlyerAndroidInstance.validateAndSendInAppPurchase(details, purchaseAdditionalDetails, gameObject);
            }
        }

        public static void handleOpenUrl(string url, string sourceApplication, string annotation)
        {
            try
            {
                AppsFlyerRPCClient.instance.ExecuteFire("handleOpenUrl", new Dictionary<string, object>
                {
                    { "url", url },
                    { "source", sourceApplication },
                    { "annotation", annotation }
                });
            }
            catch (AppsFlyerRPCException e) { AFLog("handleOpenUrl", "RPC error: " + e.Message); }
        }

        public static void registerUninstall(byte[] deviceToken)
        {
            try
            {
                AppsFlyerRPCClient.instance.ExecuteFire("registerUninstall", new Dictionary<string, object>
                {
                    { "token", deviceToken != null ? System.Convert.ToBase64String(deviceToken) : null }
                });
            }
            catch (AppsFlyerRPCException e) { AFLog("registerUninstall", "RPC error: " + e.Message); }
        }

        public static void waitForATTUserAuthorizationWithTimeoutInterval(int timeoutInterval)
        {
#if UNITY_IOS && !UNITY_EDITOR
            try
            {
                AppsFlyerRPCClient.instance.ExecuteFire("waitForATT", new Dictionary<string, object>
                {
                    { "timeout", timeoutInterval }
                });
            }
            catch (AppsFlyerRPCException e)
            {
                AFLog("waitForATTUserAuthorizationWithTimeoutInterval", "RPC error: " + e.Message);
            }
#endif
        }

        public static void setCurrentDeviceLanguage(string language)
        {
#if (UNITY_IOS || UNITY_STANDALONE_OSX) && !UNITY_ANDROID
            try
            {
                AppsFlyerRPCClient.instance.ExecuteFire("setCurrentDeviceLanguage", new Dictionary<string, object>
                {
                    { "language", language }
                });
            }
            catch (AppsFlyerRPCException e) { AFLog("setCurrentDeviceLanguage", "RPC error: " + e.Message); }
#endif
        }

        /// <summary>
        /// The LinkGenerator class builds the invite URL according to various setter methods which allow passing on additional information on the click.
        /// See - https://support.appsflyer.com/hc/en-us/articles/115004480866-User-invite-attribution-
        /// </summary>
        /// <param name="parameters">parameters Dictionary.</param>
        public static void generateUserInviteLink(Dictionary<string, string> parameters, MonoBehaviour gameObject)
        {
            if (instance != null)
            {
                instance.generateUserInviteLink(parameters, gameObject);
            }
            try
            {
                AppsFlyerRPCClient.instance.ExecuteFire("generateInviteLink", new Dictionary<string, object>
                {
                    { "parameters", parameters }
                });
            }
            catch (AppsFlyerRPCException e) { AFLog("generateUserInviteLink", "RPC error: " + e.Message); }
        }

        public static void disableSKAdNetwork(bool isDisabled)
        {
            try
            {
                AppsFlyerRPCClient.instance.ExecuteFire("setDisableSKAdNetwork", new Dictionary<string, object>
                {
                    { "disable", isDisabled }
                });
            }
            catch (AppsFlyerRPCException e) { AFLog("disableSKAdNetwork", "RPC error: " + e.Message); }
        }

        public static void setCollectOaid(bool isCollect)
        {
            if (instance != null && instance is IAppsFlyerAndroidBridge)
            {
                IAppsFlyerAndroidBridge appsFlyerAndroidInstance = (IAppsFlyerAndroidBridge)instance;
                appsFlyerAndroidInstance.setCollectOaid(isCollect);
            }
        }


        /// <summary>
        /// Use this method if you’re integrating your app with push providers 
        /// that don’t use the default push notification JSON schema the SDK expects.
        /// See docs for more info.
        /// </summary>
        /// <param name="paths">array of nested json path</param>
        public static void addPushNotificationDeepLinkPath(params string[] paths)
        {
            if (instance != null)
            {
                instance.addPushNotificationDeepLinkPath(paths);
            }
            try
            {
                AppsFlyerRPCClient.instance.ExecuteFire("addPushNotificationDeepLinkPath", new Dictionary<string, object>
                {
                    { "paths", paths }
                });
            }
            catch (AppsFlyerRPCException e) { AFLog("addPushNotificationDeepLinkPath", "RPC error: " + e.Message); }
        }

        public static void setDisableAdvertisingIdentifiers(bool disable)
        {
            try
            {
                AppsFlyerRPCClient.instance.ExecuteFire("setDisableAdvertisingIdentifiers", new Dictionary<string, object>
                {
                    { "disable", disable }
                });
            }
            catch (AppsFlyerRPCException e) { AFLog("setDisableAdvertisingIdentifiers", "RPC error: " + e.Message); }
        }

        /// <summary>
        /// Subscribe for unified deeplink API.
        /// This is called automatically from OnDeepLinkReceived.
        /// CallBackObjectName is set in the init method.
        /// </summary>
        public static void subscribeForDeepLink()
        {

            try
            {
#if UNITY_ANDROID
                AppsFlyerRPCClient.instance.ExecuteFire("subscribeForDeepLink", new Dictionary<string, object>
                {
                    { "callbackObjectName", CallBackObjectName }
                });
#elif UNITY_IOS || UNITY_STANDALONE_OSX
                // AppsFlyerRPC bridge does not yet handle registerDeeplinkListener;
                // use the direct P/Invoke path to set AppsFlyerLib.deepLinkDelegate.
                instance.subscribeForDeepLink(CallBackObjectName);
#endif
            }
            catch (AppsFlyerRPCException e)
            {
                AFLog("subscribeForDeepLink", "RPC error: " + e.Message);
            }

        }

        /// <summary>
        /// Allows sending custom data for partner integration purposes.
        /// partnerId : id of the partner
        /// partnerInfo: customer data
        /// </summary>
        public static void setPartnerData(string partnerId, Dictionary<string, string> partnerInfo)
        {
            if (instance != null)
            {
                instance.setPartnerData(partnerId, partnerInfo);
            }
            try
            {
                AppsFlyerRPCClient.instance.ExecuteFire("setPartnerData", new Dictionary<string, object>
                {
                    { "partnerId", partnerId },
                    { "data", partnerInfo }
                });
            }
            catch (AppsFlyerRPCException e) { AFLog("setPartnerData", "RPC error: " + e.Message); }
        }

        /// <summary>
        /// Use to opt-out of collecting the network operator name (carrier) and sim operator name from the device.
        /// </summary>
        public static void setDisableNetworkData(bool disable) {
            if (instance != null && instance is IAppsFlyerAndroidBridge) {
                IAppsFlyerAndroidBridge appsFlyerAndroidInstance = (IAppsFlyerAndroidBridge)instance;
                appsFlyerAndroidInstance.setDisableNetworkData(disable);
            }
        }


        /// <summary>
        /// Use to disable app vendor identifier (IDFV) collection, 'true' to disable.
        /// </summary>
        public static void disableIDFVCollection(bool isDisabled)
        {
            try
            {
                AppsFlyerRPCClient.instance.ExecuteFire("setDisableIDFVCollection", new Dictionary<string, object>
                {
                    { "disable", isDisabled }
                });
            }
            catch (AppsFlyerRPCException e) { AFLog("disableIDFVCollection", "RPC error: " + e.Message); }
        }

        /// <summary>
        /// Registers a listener to receive session-ready callbacks from the SDK.
        /// The callbackObjectName is the name of the Unity GameObject that will receive the callback.
        /// </summary>
        /// <param name="callbackObjectName">Name of the Unity GameObject to receive session-ready callbacks.</param>
        public static void registerSessionReadyListener(string callbackObjectName)
        {
#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
            try
            {
                AppsFlyerRPCClient.instance.ExecuteFire("registerSessionReadyListener");
            }
            catch (AppsFlyerRPCException e) { AFLog("registerSessionReadyListener", "RPC error: " + e.Message); }
#else
            try
            {
                AppsFlyerRPCClient.instance.ExecuteFire("registerSessionReadyListener", new Dictionary<string, object>
                {
                    { "callbackObjectName", callbackObjectName }
                });
            }
            catch (AppsFlyerRPCException e) { AFLog("registerSessionReadyListener", "RPC error: " + e.Message); }
#endif
        }

        /// <summary>
        /// Unregisters the session-ready listener previously registered with registerSessionReadyListener.
        /// </summary>
        public static void unregisterSessionReadyListener()
        {
            try
            {
                AppsFlyerRPCClient.instance.ExecuteFire("unregisterSessionReadyListener");
            }
            catch (AppsFlyerRPCException e) { AFLog("unregisterSessionReadyListener", "RPC error: " + e.Message); }
        }

        /// <summary>
        /// Start callback event.
        /// </summary>
        public static event EventHandler OnRequestResponse
        {
            add
            {
                onRequestResponse += value;
            }  
            remove  
            {  
                onRequestResponse -= value;
            }     
        }
        
        /// <summary>
        /// In-App callback event.
        /// </summary>
        public static event EventHandler OnInAppResponse
        {
            add
            {
                onInAppResponse += value;
            }  
            remove  
            {  
                onInAppResponse -= value;
            }     
        }

        /// <summary>
        /// Unified DeepLink Event
        /// </summary>
        public static event EventHandler OnDeepLinkReceived
        {
            add
            {
                onDeepLinkReceived += value;
                subscribeForDeepLink();
            }
            remove
            {
                onDeepLinkReceived -= value;
            }
        }

        /// <summary>
        /// Session ready event. Fired when the SDK reports that a session is ready.
        /// Use registerSessionReadyListener(gameObject.name) to opt in on native side.
        /// </summary>
        public static event EventHandler OnSessionReady
        {
            add { onSessionReady += value; }
            remove { onSessionReady -= value; }
        }

        /// <summary>
        /// Used to accept start callback from UnitySendMessage on native side.
        /// </summary>
        public void inAppResponseReceived(string response)
        {
            if (onInAppResponse != null) 
            {
                onInAppResponse.Invoke(null, parseRequestCallback(response));
            }
        }
        
        /// <summary>
        /// Used to accept in-app callback from UnitySendMessage on native side.
        /// </summary>
        public void requestResponseReceived(string response)
        {
            if (onRequestResponse != null)
            {
                onRequestResponse.Invoke(null, parseRequestCallback(response));
            }
        }

        /// <summary>
        /// Used to accept session-ready callback from UnitySendMessage on native side.
        /// </summary>
        public void onSessionReadyReceived(string response)
        {
            if (onSessionReady != null)
            {
                onSessionReady.Invoke(null, new AppsFlyerRequestEventArgs(0, response));
            }
        }

        /// <summary>
        /// Used to accept deeplink callback from UnitySendMessage on native side.
        /// </summary>
        public void onDeepLinking(string response)
        {

            DeepLinkEventsArgs args = new DeepLinkEventsArgs(response);

            if (onDeepLinkReceived != null)
            {
                onDeepLinkReceived.Invoke(null, args);
            }
        }

        /// <summary>
        /// Receives unified RPC event envelopes from native via UnitySendMessage.
        /// Format: {"event": "onConversionDataSuccess", "data": {...}}
        /// </summary>
        public void onRPCEvent(string jsonEvent)
        {
            try
            {
                var envelope = CallbackStringToDictionary(jsonEvent);
                if (envelope == null || !envelope.ContainsKey("event")) return;

                string eventType = envelope["event"] as string;
                var data = envelope.ContainsKey("data") ? envelope["data"] : null;
                string dataStr = data != null ? Json.Serialize(data) : jsonEvent;

                switch (eventType)
                {
                    case "start":
                    case "onRequestResponse":
                        requestResponseReceived(dataStr);
                        break;
                    case "logEvent":
                    case "onInAppResponse":
                        inAppResponseReceived(dataStr);
                        break;
                    case "onDeepLinking":
                    case "onDeepLinkReceived":
                        onDeepLinking(dataStr);
                        break;
                    case "sessionReady":
                    case "onSessionReady":
                        onSessionReadyReceived(dataStr);
                        break;
                    default:
                        AFLog("onRPCEvent", "Unhandled event type: " + eventType);
                        break;
                }
            }
            catch (Exception e)
            {
                AFLog("onRPCEvent", "Exception: " + e.Message);
            }
        }

        private static AppsFlyerRequestEventArgs parseRequestCallback(string response)
        {
            int responseCode = 0;
            string errorDescription = "";
            
            try
            {
                Dictionary<string, object> dictionary = CallbackStringToDictionary(response);
                var errorResponse = dictionary.ContainsKey("errorDescription") ? dictionary["errorDescription"] : "";
                errorDescription = (string)errorResponse;
                responseCode = (int)(long) dictionary["statusCode"];
            }
            catch (Exception e)
            {
                AFLog("parseRequestCallback", String.Format("{0} Exception caught.", e));
            }

            return new AppsFlyerRequestEventArgs(responseCode, errorDescription);
        }

        /// <summary>
        /// Helper method to convert json strings to dictionary.
        /// </summary>
        /// <param name="str">json string</param>
        /// <returns>dictionary representing the input json string.</returns>
        public static Dictionary<string, object> CallbackStringToDictionary(string str)
        {
            return AFMiniJSON.Json.Deserialize(str) as Dictionary<string, object>;
        }

        /// <summary>
        /// Helper method to log AppsFlyer events and callbacks.
        /// </summary>
        /// <param name="methodName">method name</param>
        /// <param name="str">message to log</param>
        public static void AFLog(string methodName, string str)
        {
            Debug.Log(string.Format("AppsFlyer_Unity_v{0} {1} called with {2}", kAppsFlyerPluginVersion, methodName, str));
        }
    }

    public enum EmailCryptType
    {
        // None
        EmailCryptTypeNone = 0,
        // SHA256
        EmailCryptTypeSHA256 = 1,
    }

}