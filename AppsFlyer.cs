using System;
using System.Collections.Generic;
using UnityEngine;

namespace AppsFlyerSDK
{
    public class AppsFlyer : MonoBehaviour
    {
        public static readonly string kAppsFlyerPluginVersion = "6.17.1";
        public static string CallBackObjectName = null;
        private static EventHandler onRequestResponse;
        private static EventHandler onInAppResponse;
        private static EventHandler onDeepLinkReceived;
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
            if (instance != null)
            {
                instance.startSDK(onRequestResponse != null, CallBackObjectName);
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
            if (instance != null)
            {
                instance.stopSDK(isSDKStopped);
            }
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
#endif
        }

        /// <summary>
        /// Set the OneLink ID that should be used for User-Invite-API.
        /// The link that is generated for the user invite will use this OneLink as the base link.
        /// </summary>
        /// <param name="oneLinkId">OneLink ID obtained from the AppsFlyer Dashboard.</param>
        public static void setAppInviteOneLinkID(string oneLinkId)
        {

            if (instance != null)
            {
                instance.setAppInviteOneLinkID(oneLinkId);
            }


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


        }

        /// <summary>
        /// Anonymize user Data.
        /// Use this API during the SDK Initialization to explicitly anonymize a user's installs, events and sessions.
        /// Default is false.
        /// </summary>
        /// <param name = "shouldAnonymizeUser" >shouldAnonymizeUser boolean.</param>
        public static void anonymizeUser(bool shouldAnonymizeUser)
        {

            if (instance != null)
            {
                instance.anonymizeUser(shouldAnonymizeUser);
            }


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

            if (instance != null)
            {
                instance.setUserEmails(cryptType, userEmails);
            }

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

            if (instance != null)
            {
                instance.setPhoneNumber(phoneNumber);
            }

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
            if (instance != null && instance is IAppsFlyerIOSBridge)
            {
                IAppsFlyerIOSBridge appsFlyeriOSInstance = (IAppsFlyerIOSBridge)instance;
                appsFlyeriOSInstance.setDisableCollectAppleAdSupport(disable);
            }
        }

        public static void setShouldCollectDeviceName(bool shouldCollectDeviceName)
        {
            if (instance != null && instance is IAppsFlyerIOSBridge)
            {
                IAppsFlyerIOSBridge appsFlyeriOSInstance = (IAppsFlyerIOSBridge)instance;
                appsFlyeriOSInstance.setShouldCollectDeviceName(shouldCollectDeviceName);
            }
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
            if (instance != null && instance is IAppsFlyerIOSBridge)
            {
                IAppsFlyerIOSBridge appsFlyeriOSInstance = (IAppsFlyerIOSBridge)instance;
                appsFlyeriOSInstance.setDisableCollectIAd(disableCollectIAd);
            }
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
            if (instance != null && instance is IAppsFlyerIOSBridge)
            {
                IAppsFlyerIOSBridge appsFlyeriOSInstance = (IAppsFlyerIOSBridge)instance;
                appsFlyeriOSInstance.setUseReceiptValidationSandbox(useReceiptValidationSandbox);
            }
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
            
        }

        public static void setUseUninstallSandbox(bool useUninstallSandbox)
        {
            if (instance != null && instance is IAppsFlyerIOSBridge)
            {
                IAppsFlyerIOSBridge appsFlyeriOSInstance = (IAppsFlyerIOSBridge)instance;
                appsFlyeriOSInstance.setUseUninstallSandbox(useUninstallSandbox);
            }
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

        public static void validateAndSendInAppPurchase(string productIdentifier, string price, string currency, string transactionId, Dictionary<string, string> additionalParameters, MonoBehaviour gameObject)
        {
            if (instance != null && instance is IAppsFlyerIOSBridge)
            {
                IAppsFlyerIOSBridge appsFlyeriOSInstance = (IAppsFlyerIOSBridge)instance;
                appsFlyeriOSInstance.validateAndSendInAppPurchase(productIdentifier, price, currency, transactionId, additionalParameters, gameObject);
            }
        }

        // V2 
        public static void validateAndSendInAppPurchase(AFSDKPurchaseDetailsIOS details, Dictionary<string, string> extraEventValues, MonoBehaviour gameObject)
        {
            if (instance != null && instance is IAppsFlyerIOSBridge)
            {
                IAppsFlyerIOSBridge appsFlyeriOSInstance = (IAppsFlyerIOSBridge)instance;
                appsFlyeriOSInstance.validateAndSendInAppPurchase(details, extraEventValues, gameObject);
            }
        }

        public static void validateAndSendInAppPurchase(string publicKey, string signature, string purchaseData, string price, string currency, Dictionary<string, string> additionalParameters, MonoBehaviour gameObject)
        {
            if (instance != null && instance is IAppsFlyerAndroidBridge)
            {
                IAppsFlyerAndroidBridge appsFlyerAndroidInstance = (IAppsFlyerAndroidBridge)instance;
                appsFlyerAndroidInstance.validateAndSendInAppPurchase(publicKey, signature,purchaseData, price, currency, additionalParameters, gameObject);
            }
        }

        // V2
        public static void validateAndSendInAppPurchase(AFPurchaseDetailsAndroid details, Dictionary<string, string> additionalParameters, MonoBehaviour gameObject)
        {
            if (instance != null && instance is IAppsFlyerAndroidBridge)
            {
                IAppsFlyerAndroidBridge appsFlyerAndroidInstance = (IAppsFlyerAndroidBridge)instance;
                appsFlyerAndroidInstance.validateAndSendInAppPurchase(details, additionalParameters, gameObject);
            }
        }

        public static void handleOpenUrl(string url, string sourceApplication, string annotation)
        { 
            if (instance != null && instance is IAppsFlyerIOSBridge)
            {
                IAppsFlyerIOSBridge appsFlyeriOSInstance = (IAppsFlyerIOSBridge)instance;
                appsFlyeriOSInstance.handleOpenUrl(url, sourceApplication, annotation);
            }
        }

        public static void registerUninstall(byte[] deviceToken)
        {
            if (instance != null && instance is IAppsFlyerIOSBridge)
            {
                IAppsFlyerIOSBridge appsFlyeriOSInstance = (IAppsFlyerIOSBridge)instance;
                appsFlyeriOSInstance.registerUninstall(deviceToken);
            }
        }

        public static void waitForATTUserAuthorizationWithTimeoutInterval(int timeoutInterval)
        {
            if (instance != null && instance is IAppsFlyerIOSBridge)
            {
                IAppsFlyerIOSBridge appsFlyeriOSInstance = (IAppsFlyerIOSBridge)instance;
                appsFlyeriOSInstance.waitForATTUserAuthorizationWithTimeoutInterval(timeoutInterval);
            }
        }

        public static void setCurrentDeviceLanguage(string language)
        {
            if (instance != null && instance is IAppsFlyerIOSBridge)
            {
                IAppsFlyerIOSBridge appsFlyeriOSInstance = (IAppsFlyerIOSBridge)instance;
                appsFlyeriOSInstance.setCurrentDeviceLanguage(language);
            }
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
            
        }

        public static void disableSKAdNetwork(bool isDisabled)
        {
            if (instance != null && instance is IAppsFlyerIOSBridge)
            {
                IAppsFlyerIOSBridge appsFlyeriOSInstance = (IAppsFlyerIOSBridge)instance;
                appsFlyeriOSInstance.disableSKAdNetwork(isDisabled);
            } else {
#if UNITY_IOS || UNITY_STANDALONE_OSX
                instance = new AppsFlyeriOS();
                IAppsFlyerIOSBridge appsFlyeriOSInstance = (IAppsFlyerIOSBridge)instance;
                appsFlyeriOSInstance.disableSKAdNetwork(isDisabled);
#else
#endif
        }
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

        }

        public static void setDisableAdvertisingIdentifiers(bool disable)
        {
            if (instance != null && instance is IAppsFlyerAndroidBridge)
            {
                IAppsFlyerAndroidBridge appsFlyerAndroidInstance = (IAppsFlyerAndroidBridge)instance;
                appsFlyerAndroidInstance.setDisableAdvertisingIdentifiers(disable);
            }
        }

        /// <summary>
        /// Subscribe for unified deeplink API.
        /// This is called automatically from OnDeepLinkReceived.
        /// CallBackObjectName is set in the init method.
        /// </summary>
        public static void subscribeForDeepLink()
        {

            if (instance != null)
            {
                instance.subscribeForDeepLink(CallBackObjectName);
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
#if UNITY_IOS || UNITY_STANDALONE_OSX
            if (instance == null) { 
                instance = new AppsFlyeriOS();
            }
            if (instance != null && instance is IAppsFlyerIOSBridge) {
                IAppsFlyerIOSBridge appsFlyeriOSInstance = (IAppsFlyerIOSBridge)instance;
                appsFlyeriOSInstance.disableIDFVCollection(isDisabled);
            }
#else
#endif
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