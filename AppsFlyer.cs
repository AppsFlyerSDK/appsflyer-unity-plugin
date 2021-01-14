using System;
using System.Collections.Generic;
using UnityEngine;

namespace AppsFlyerSDK
{
    public class AppsFlyer : MonoBehaviour
    {

        public static readonly string kAppsFlyerPluginVersion = "6.1.4";
        public static string CallBackObjectName = null;
        private static EventHandler onRequestResponse;
        private static EventHandler onInAppResponse;
        private static EventHandler onDeepLinkReceived;


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
            
            if(gameObject != null)
            {
                CallBackObjectName = gameObject.name;
            }

#if UNITY_IOS && !UNITY_EDITOR
            AppsFlyeriOS.setAppsFlyerDevKey(devKey);
            AppsFlyeriOS.setAppleAppID(appID);
            if(gameObject != null)
            {
                AppsFlyeriOS.getConversionData(gameObject.name);
            }
#elif UNITY_ANDROID && !UNITY_EDITOR
            AppsFlyerAndroid.initSDK(devKey, gameObject);
#else

#endif
        }


        /// <summary>
        /// Once this API is invoked, our SDK will start.
        /// Once the API is called a sessions will be immediately sent, and all background forground transitions will send a session.
        /// </summary>
        public static void startSDK()
        {
#if UNITY_IOS && !UNITY_EDITOR
            AppsFlyeriOS.startSDK(onRequestResponse != null, CallBackObjectName);
#elif UNITY_ANDROID && !UNITY_EDITOR
            AppsFlyerAndroid.startSDK(onRequestResponse != null, CallBackObjectName);
#else

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
#if UNITY_IOS && !UNITY_EDITOR
            AppsFlyeriOS.sendEvent(eventName, eventValues, onInAppResponse != null, CallBackObjectName);
#elif UNITY_ANDROID && !UNITY_EDITOR
            AppsFlyerAndroid.sendEvent(eventName, eventValues, onInAppResponse != null, CallBackObjectName);
#else

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
#if UNITY_IOS && !UNITY_EDITOR
            AppsFlyeriOS.stopSDK(isSDKStopped);
#elif UNITY_ANDROID && !UNITY_EDITOR
            AppsFlyerAndroid.stopSDK(isSDKStopped);
#else

#endif
        }

        // <summary>
        /// Was the stopSDK(boolean) API set to true.
        /// </summary>
        /// <returns>boolean isSDKStopped.</returns>
        public static bool isSDKStopped()
        {
#if UNITY_IOS && !UNITY_EDITOR
            return AppsFlyeriOS.isSDKStopped();
#elif UNITY_ANDROID && !UNITY_EDITOR
            return AppsFlyerAndroid.isSDKStopped();
#else
            return false;
#endif
        }

        /// <summary>
        /// Get the AppsFlyer SDK version used in app.
        /// </summary>
        /// <returns>The current SDK version.</returns>
        public static string getSdkVersion()
        {
#if UNITY_IOS && !UNITY_EDITOR
            return AppsFlyeriOS.getSDKVersion();
#elif UNITY_ANDROID && !UNITY_EDITOR
            return AppsFlyerAndroid.getSdkVersion();
#else
            return "";
#endif

        }

        /// <summary>
        /// Enables Debug logs for the AppsFlyer SDK.
        /// Should only be set to true in development / debug.
        /// </summary>
        /// <param name="shouldEnable">shouldEnable boolean.</param>
        public static void setIsDebug(bool shouldEnable)
        {
#if UNITY_IOS && !UNITY_EDITOR
            AppsFlyeriOS.setIsDebug(shouldEnable);
#elif UNITY_ANDROID && !UNITY_EDITOR
            AppsFlyerAndroid.setIsDebug(shouldEnable);
#else

#endif
        }

        /// <summary>
        /// Setting your own customer ID enables you to cross-reference your own unique ID with AppsFlyer’s unique ID and the other devices’ IDs.
        /// This ID is available in AppsFlyer CSV reports along with Postback APIs for cross-referencing with your internal IDs.
        /// </summary>
        /// <param name="id">Customer ID for client.</param>
        public static void setCustomerUserId(string id)
        {
#if UNITY_IOS && !UNITY_EDITOR
            AppsFlyeriOS.setCustomerUserID(id);
#elif UNITY_ANDROID && !UNITY_EDITOR
            AppsFlyerAndroid.setCustomerUserId(id);
#else

#endif
        }

        /// <summary>
        /// Set the OneLink ID that should be used for User-Invite-API.
        /// The link that is generated for the user invite will use this OneLink as the base link.
        /// </summary>
        /// <param name="oneLinkId">OneLink ID obtained from the AppsFlyer Dashboard.</param>
        public static void setAppInviteOneLinkID(string oneLinkId)
        {
#if UNITY_IOS && !UNITY_EDITOR
            AppsFlyeriOS.setAppInviteOneLinkID(oneLinkId);
#elif UNITY_ANDROID && !UNITY_EDITOR
            AppsFlyerAndroid.setAppInviteOneLinkID(oneLinkId);
#else

#endif
        }

        /// <summary>
        /// Set additional data to be sent to AppsFlyer.
        /// </summary>
        /// <param name="customData">additional data Dictionary.</param>
        public static void setAdditionalData(Dictionary<string, string> customData)
        {
#if UNITY_IOS && !UNITY_EDITOR
            AppsFlyeriOS.setAdditionalData(customData);
#elif UNITY_ANDROID && !UNITY_EDITOR
            AppsFlyerAndroid.setAdditionalData(customData);
#else

#endif
        }

        /// <summary>
        /// Advertisers can wrap AppsFlyer OneLink within another Universal Link.
        /// This Universal Link will invoke the app but any deep linking data will not propagate to AppsFlyer.
        /// </summary>
        /// <param name="urls">Array of urls.</param>
        public static void setResolveDeepLinkURLs(params string[] urls)
        {
#if UNITY_IOS && !UNITY_EDITOR
            AppsFlyeriOS.setResolveDeepLinkURLs(urls);
#elif UNITY_ANDROID && !UNITY_EDITOR
            AppsFlyerAndroid.setResolveDeepLinkURLs(urls);
#else

#endif
        }


        /// <summary>
        /// Advertisers can use this method to set vanity onelink domains.
        /// </summary>
        /// <param name="domains">Array of domains.</param>
        public static void setOneLinkCustomDomain(params string[] domains)
        {
#if UNITY_IOS && !UNITY_EDITOR
            AppsFlyeriOS.setOneLinkCustomDomains(domains);
#elif UNITY_ANDROID && !UNITY_EDITOR
            AppsFlyerAndroid.setOneLinkCustomDomain(domains);
#else

#endif
        }

        /// <summary>
        /// Setting user local currency code for in-app purchases.
        /// The currency code should be a 3 character ISO 4217 code. (default is USD).
        /// You can set the currency code for all events by calling the following method.
        /// </summary>
        /// <param name="currencyCode">3 character ISO 4217 code.</param>
        public static void setCurrencyCode(string currencyCode)
        {
#if UNITY_IOS && !UNITY_EDITOR
            AppsFlyeriOS.setCurrencyCode(currencyCode);
#elif UNITY_ANDROID && !UNITY_EDITOR
            AppsFlyerAndroid.setCurrencyCode(currencyCode);
#else

#endif
        }

        /// <summary>
        /// Manually record the location of the user.
        /// </summary>
        /// <param name="latitude">latitude as double.</param>
        /// <param name="longitude">longitude as double.</param>
        public static void recordLocation(double latitude, double longitude)
        {
#if UNITY_IOS && !UNITY_EDITOR
            AppsFlyeriOS.recordLocation(latitude, longitude);
#elif UNITY_ANDROID && !UNITY_EDITOR
            AppsFlyerAndroid.recordLocation(latitude, longitude);
#else

#endif
        }

        /// <summary>
        /// Anonymize user Data.
        /// Use this API during the SDK Initialization to explicitly anonymize a user's installs, events and sessions.
        /// Default is false.
        /// </summary>
        /// <param name = "shouldAnonymizeUser" >shouldAnonymizeUser boolean.</param>
        public static void anonymizeUser(bool shouldAnonymizeUser)
        {
#if UNITY_IOS && !UNITY_EDITOR
            AppsFlyeriOS.anonymizeUser(shouldAnonymizeUser);
#elif UNITY_ANDROID && !UNITY_EDITOR
            AppsFlyerAndroid.anonymizeUser(shouldAnonymizeUser);
#else

#endif
        }

        /// <summary>
        /// Get AppsFlyer's unique device ID which is created for every new install of an app.
        /// </summary>
        /// <returns>AppsFlyer's unique device ID.</returns>
        public static string getAppsFlyerId()
        {
#if UNITY_IOS && !UNITY_EDITOR
            return AppsFlyeriOS.getAppsFlyerId();
#elif UNITY_ANDROID && !UNITY_EDITOR
            return AppsFlyerAndroid.getAppsFlyerId();
#else
            return "";
#endif

        }

        /// <summary>
        /// Set a custom value for the minimum required time between sessions.
        /// By default, at least 5 seconds must lapse between 2 app launches to count as separate 2 sessions.
        /// </summary>
        /// <param name="seconds">minimum time between 2 separate sessions in seconds.</param>
        public static void setMinTimeBetweenSessions(int seconds)
        {
#if UNITY_IOS && !UNITY_EDITOR
            AppsFlyeriOS.setMinTimeBetweenSessions(seconds);
#elif UNITY_ANDROID && !UNITY_EDITOR
            AppsFlyerAndroid.setMinTimeBetweenSessions(seconds);
#else

#endif
        }

        /// <summary>
        /// Set a custom host.
        /// </summary>
        /// <param name="hostPrefixName">Host prefix.</param>
        /// <param name="hostName">Host name.</param>
        public static void setHost(string hostPrefixName, string hostName)
        {
#if UNITY_IOS && !UNITY_EDITOR
            AppsFlyeriOS.setHost(hostName, hostPrefixName);
#elif UNITY_ANDROID && !UNITY_EDITOR
            AppsFlyerAndroid.setHost(hostPrefixName, hostName);
#else

#endif
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
        public static void setUserEmails(EmailCryptType cryptMethod, params string[] emails)
        {
#if UNITY_IOS && !UNITY_EDITOR
            AppsFlyeriOS.setUserEmails(cryptMethod, emails.Length, emails);
#elif UNITY_ANDROID && !UNITY_EDITOR
            AppsFlyerAndroid.setUserEmails(cryptMethod, emails);
#else

#endif
        }

        /// <summary>
        /// Set the user phone number.
        /// </summary>
        /// <param name="phoneNumber">phoneNumber string</param>
        public static void setPhoneNumber(string phoneNumber)
        {
#if UNITY_IOS && !UNITY_EDITOR
            AppsFlyeriOS.setPhoneNumber(phoneNumber);
#elif UNITY_ANDROID && !UNITY_EDITOR
            AppsFlyerAndroid.setPhoneNumber(phoneNumber);
#else

#endif
        }

        /// <summary>
        /// Used by advertisers to exclude all networks/integrated partners from getting data.
        /// </summary>
        public static void setSharingFilterForAllPartners()
        {
#if UNITY_IOS && !UNITY_EDITOR
            AppsFlyeriOS.setSharingFilterForAllPartners();
#elif UNITY_ANDROID && !UNITY_EDITOR
            AppsFlyerAndroid.setSharingFilterForAllPartners();
#else

#endif
        }

        /// <summary>
        /// Used by advertisers to set some (one or more) networks/integrated partners to exclude from getting data.
        /// </summary>
        /// <param name="partners">partners to exclude from getting data</param>
        public static void setSharingFilter(params string[] partners)
        {
#if UNITY_IOS && !UNITY_EDITOR
            AppsFlyeriOS.setSharingFilter(partners);
#elif UNITY_ANDROID && !UNITY_EDITOR
            AppsFlyerAndroid.setSharingFilter(partners);
#else

#endif
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
#if UNITY_IOS && !UNITY_EDITOR
            AppsFlyeriOS.getConversionData(objectName);
#elif UNITY_ANDROID && !UNITY_EDITOR
            AppsFlyerAndroid.getConversionData(objectName);
#else

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
#if UNITY_IOS && !UNITY_EDITOR
            AppsFlyeriOS.attributeAndOpenStore(appID, campaign, userParams, gameObject);
#elif UNITY_ANDROID && !UNITY_EDITOR
            AppsFlyerAndroid.attributeAndOpenStore(appID, campaign, userParams);
#else

#endif
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
#if UNITY_IOS && !UNITY_EDITOR
            AppsFlyeriOS.recordCrossPromoteImpression(appID, campaign, parameters);
#elif UNITY_ANDROID && !UNITY_EDITOR
            AppsFlyerAndroid.recordCrossPromoteImpression(appID, campaign, parameters);
#else

#endif
        }

        /// <summary>
        /// The LinkGenerator class builds the invite URL according to various setter methods which allow passing on additional information on the click.
        /// See - https://support.appsflyer.com/hc/en-us/articles/115004480866-User-invite-attribution-
        /// </summary>
        /// <param name="parameters">parameters Dictionary.</param>
        public static void generateUserInviteLink(Dictionary<string, string> parameters, MonoBehaviour gameObject)
        {
#if UNITY_IOS && !UNITY_EDITOR
            AppsFlyeriOS.generateUserInviteLink(parameters, gameObject);
#elif UNITY_ANDROID && !UNITY_EDITOR
            AppsFlyerAndroid.generateUserInviteLink(parameters, gameObject);
#else

#endif
        }


        /// <summary>
        /// Use this method if you’re integrating your app with push providers 
        /// that don’t use the default push notification JSON schema the SDK expects.
        /// See docs for more info.
        /// </summary>
        /// <param name="paths">array of nested json path</param>
        public static void addPushNotificationDeepLinkPath(params string[] paths)
        {
#if UNITY_IOS && !UNITY_EDITOR
            AppsFlyeriOS.addPushNotificationDeepLinkPath(paths);
#elif UNITY_ANDROID && !UNITY_EDITOR
            AppsFlyerAndroid.addPushNotificationDeepLinkPath(paths);
#else

#endif
        }

        /// <summary>
        /// Subscribe for unified deeplink API.
        /// This is called automatically from OnDeepLinkReceived.
        /// CallBackObjectName is set in the init method.
        /// </summary>
        public static void subscribeForDeepLink()
        {
#if UNITY_IOS && !UNITY_EDITOR
            AppsFlyeriOS.subscribeForDeepLink(CallBackObjectName);
#elif UNITY_ANDROID && !UNITY_EDITOR
            AppsFlyerAndroid.subscribeForDeepLink(CallBackObjectName);
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