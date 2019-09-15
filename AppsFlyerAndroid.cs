using System;
using System.Collections.Generic;
using UnityEngine;

namespace AppsFlyerSDK
{

#if UNITY_ANDROID || !UNITY_EDITOR
    public class AppsFlyerAndroid
    {

        private static AndroidJavaClass appsFlyerAndroid = new AndroidJavaClass("com.appsflyer.AppsFlyerAndroidWrapper");

        /// <summary>
        /// Use this method to start the sdk for the application.
        /// Notice that AppsFlyer's Dev-Key must be provided.
        /// </summary>
        /// <param name="devkey"> AppsFlyer's Dev-Key, which is accessible from your AppsFlyer account under 'App Settings' in the dashboard.</param>
        public static void startSDK(string devkey)
        {
            appsFlyerAndroid.CallStatic("startTracking", devkey);
        }

        /// <summary>
        /// Once this API is invoked, our SDK no longer communicates with our servers and stops functioning.
        /// In some extreme cases you might want to shut down all SDK activity due to legal and privacy compliance.
        /// This can be achieved with the stopSDK API
        /// </summary>
        /// <param name="isSDKStopped">boolean should SDK be stopped.</param>
        public static void stopSDK(bool isSDKStopped)
        {
            appsFlyerAndroid.CallStatic("stopTracking", isSDKStopped);
        }

        /// <summary>
        /// Get the AppsFlyer SDK version used in app.
        /// </summary>
        /// <returns>AppsFlyer SDK version.</returns>
        public static string getSdkVersion()
        {
            return appsFlyerAndroid.CallStatic<string>("getSdkVersion");
        }

        /// <summary>
        /// Manually pass the Firebase / GCM Device Token for Uninstall measurement.
        /// </summary>
        /// <param name="token">Firebase Device Token.</param>
        public static void updateServerUninstallToken(string token)
        {
            //TODO check uninstall API
            appsFlyerAndroid.CallStatic("updateServerUninstallToken", token);
        }

        /// <summary>
        /// Enables Debug logs for the AppsFlyer SDK.
        /// Should only be set to true in development / debug.
        /// </summary>
        /// <param name="shouldEnable">shouldEnable boolean.</param>
        public static void setIsDebug(bool shouldEnable)
        {
            appsFlyerAndroid.CallStatic("setIsDebug", shouldEnable);
        }

        /// <summary>
        /// By default, IMEI and Android ID are not collected by the SDK if the OS version is higher than KitKat (4.4)
        /// and the device contains Google Play Services(on SDK versions 4.8.8 and below the specific app needed GPS).
        /// Use this API to explicitly send IMEI to AppsFlyer.
        /// </summary>
        /// <param name="aImei">device's IMEI.</param>
        public static void setImeiData(string aImei)
        {
            appsFlyerAndroid.CallStatic("setImeiData", aImei);
        }

        /// <summary>
        /// By default, IMEI and Android ID are not collected by the SDK if the OS version is higher than KitKat(4.4)
        /// and the device contains Google Play Services(on SDK versions 4.8.8 and below the specific app needed GPS).
        /// Use this API to explicitly send Android ID to AppsFlyer.
        /// </summary>
        /// <param name="aAndroidId">device's Android ID.</param>
        public static void setAndroidIdData(string aAndroidId)
        {
            appsFlyerAndroid.CallStatic("setAndroidIdData", aAndroidId);
        }

        /// <summary>
        /// Setting your own customer ID enables you to cross-reference your own unique ID with AppsFlyer’s unique ID and the other devices’ IDs.
        /// This ID is available in AppsFlyer CSV reports along with Postback APIs for cross-referencing with your internal IDs.
        /// </summary>
        /// <param name="id">Customer ID for client.</param>
        public static void setCustomerUserId(string id)
        {
            appsFlyerAndroid.CallStatic("setCustomerUserId", id);
        }

        /// <summary>
        /// It is possible to delay the SDK Initialization until the customerUserID is set.
        /// This feature makes sure that the SDK doesn't begin functioning until the customerUserID is provided.
        /// If this API is used, all in-app events and any other SDK API calls are discarded, until the customerUserID is provided.
        /// </summary>
        /// <param name="wait">wait boolean.</param>
        public static void waitForCustomerUserId(bool wait)
        {
            appsFlyerAndroid.CallStatic("waitForCustomerUserId", wait);
        }

        /// <summary>
        /// Use this API to provide the SDK with the relevant customer user id and trigger the SDK to begin its normal activity.
        /// </summary>
        /// <param name="id">Customer ID for client.</param>
        public static void setCustomerIdStartSDK(string id)
        {
            appsFlyerAndroid.CallStatic("setCustomerIdAndTrack", id);
        }

        /// <summary>
        /// Get the current AF_STORE value.
        /// </summary>
        /// <returns>AF_Store value.</returns>
        public static string getOutOfStore()
        {
            return appsFlyerAndroid.CallStatic<string>("getOutOfStore");
        }

        /// <summary>
        /// Manually set the AF_STORE value.
        /// </summary>
        /// <param name="sourceName">value to be set.</param>
        public static void setOutOfStore(string sourceName)
        {
            appsFlyerAndroid.CallStatic("setOutOfStore", sourceName);
        }

        /// <summary>
        /// Set the OneLink ID that should be used for User-Invites.
        /// The link that is generated for the user invite will use this OneLink as the base link.
        /// </summary>
        /// <param name="oneLinkId">OneLink ID obtained from the AppsFlyer Dashboard.</param>
        public static void setAppInviteOneLinkID(string oneLinkId)
        {
            appsFlyerAndroid.CallStatic("setAppInviteOneLinkID", oneLinkId);
        }

        /// <summary>
        /// Set additional data to be sent to AppsFlyer.
        /// </summary>
        /// <param name="customData">additional data Dictionary.</param>
        public static void setAdditionalData(Dictionary<string, string> customData)
        {
            appsFlyerAndroid.CallStatic("setAdditionalData", convertDictionaryToJavaMap(customData));
        }

        /// <summary>
        /// Set the user emails
        /// </summary>
        /// <param name="emails">User emails.</param>
        public static void setUserEmails(params string[] emails)
        {
            appsFlyerAndroid.CallStatic("setUserEmails", (object)emails);
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
            appsFlyerAndroid.CallStatic("setUserEmails", getEmailType(cryptMethod), (object)emails);
        }

        /// <summary>
        /// Opt-out of collection of Android ID.
        /// If the app does NOT contain Google Play Services, Android ID is collected by the SDK.
        /// However, apps with Google play services should avoid Android ID collection as this is in violation of the Google Play policy.
        /// </summary>
        /// <param name="isCollect">boolean, false to opt-out.</param>
        public static void setCollectAndroidID(bool isCollect)
        {
            appsFlyerAndroid.CallStatic("setCollectAndroidID", isCollect);
        }

        /// <summary>
        /// Opt-out of collection of IMEI.
        /// If the app does NOT contain Google Play Services, device IMEI is collected by the SDK.
        /// However, apps with Google play services should avoid IMEI collection as this is in violation of the Google Play policy.
        /// </summary>
        /// <param name="isCollect">boolean, false to opt-out.</param>
        public static void setCollectIMEI(bool isCollect)
        {
            appsFlyerAndroid.CallStatic("setCollectIMEI", isCollect);
        }

        /// <summary>
        /// Advertisers can wrap AppsFlyer OneLink within another Universal Link.
        /// This Universal Link will invoke the app but any deep linking data will not propagate to AppsFlyer.
        /// </summary>
        /// <param name="urls">Array of urls.</param>
        public static void setResolveDeepLinkURLs(params string[] urls)
        {
            appsFlyerAndroid.CallStatic("setResolveDeepLinkURLs", (object)urls);
        }


        /// <summary>
        /// Advertisers can use this method to set vanity onelink domains.
        /// </summary>
        /// <param name="domains">Array of domains.</param>
        public static void setOneLinkCustomDomain(params string[] domains)
        {
            appsFlyerAndroid.CallStatic("setOneLinkCustomDomain", (object)domains);
        }

        /// <summary>
        /// Manually set that the application was updated.
        /// </summary>
        /// <param name="isUpdate">isUpdate boolean value.</param>
        public static void setIsUpdate(bool isUpdate)
        {
            appsFlyerAndroid.CallStatic("setIsUpdate", isUpdate);
        }

        /// <summary>
        /// Setting user local currency code for in-app purchases.
        /// The currency code should be a 3 character ISO 4217 code. (default is USD).
        /// You can set the currency code for all events by calling the following method
        /// </summary>
        /// <param name="currencyCode">3 character ISO 4217 code.</param>
        public static void setCurrencyCode(string currencyCode)
        {
            appsFlyerAndroid.CallStatic("setCurrencyCode", currencyCode);
        }

        /// <summary>
        /// Manually record the location of the user
        /// </summary>
        /// <param name="latitude">latitude as double.</param>
        /// <param name="longitude">longitude as double.</param>
        public static void recordLocation(double latitude, double longitude)
        {
            appsFlyerAndroid.CallStatic("trackLocation", latitude, longitude);
        }

        /// <summary>
        /// Send an In-App Event.
        /// In-App Events provide insight on what is happening in your app.
        /// </summary>
        /// <param name="eventName">Event Name as String.</param>
        /// <param name="eventValues">Event Values as Dictionary.</param>
        public static void sendEvent(string eventName, Dictionary<string, string> eventValues)
        {
            appsFlyerAndroid.CallStatic("trackEvent", eventName, convertDictionaryToJavaMap(eventValues));
        }

        /// <summary>
        /// Anonymize user Data.
        /// Use this API during the SDK Initialization to explicitly anonymize a user's installs, events and sessions.
        /// Default is false
        /// </summary>
        /// <param name="isDisabled">isDisabled boolean.</param>
        public static void anonymizeUser(bool isDisabled)
        {
            appsFlyerAndroid.CallStatic("setDeviceTrackingDisabled", isDisabled);
        }

        /// <summary>
        /// Enable the collection of Facebook Deferred AppLinks
        /// Requires Facebook SDK and Facebook app on target/client device.
        /// This API must be invoked prior to initializing the AppsFlyer SDK in order to function properly.
        /// </summary>
        /// <param name="isEnabled">should Facebook's deferred app links be processed by the AppsFlyer SDK.</param>
        public static void enableFacebookDeferredApplinks(bool isEnabled)
        {
            appsFlyerAndroid.CallStatic("enableFacebookDeferredApplinks", isEnabled);
        }


        /// <summary>
        /// Restrict reengagement via deep-link to once per each unique deep-link.
        /// Otherwise deep re-occurring deep-links will be permitted for non-singleTask Activities and deep-linking via AppsFlyer deep-links.
        /// The default value is false
        /// </summary>
        /// <param name="doConsume">doConsume boolean.</param>
        public static void setConsumeAFDeepLinks(bool doConsume)
        {
            appsFlyerAndroid.CallStatic("setConsumeAFDeepLinks", doConsume);
        }

        /// <summary>
        /// Specify the manufacturer or media source name to which the preinstall is attributed.
        /// </summary>
        /// <param name="mediaSource">Manufacturer or media source name for preinstall attribution.</param>
        /// <param name="campaign">Campaign name for preinstall attribution.</param>
        /// <param name="siteId">Site ID for preinstall attribution.</param>
        public static void setPreinstallAttribution(string mediaSource, string campaign, string siteId)
        {
            appsFlyerAndroid.CallStatic("setPreinstallAttribution", mediaSource, campaign, siteId);
        }

        /// <summary>
        /// Boolean indicator for preinstall by Manufacturer.
        /// </summary>
        /// <returns>boolean isPreInstalledApp.</returns>
        public static bool isPreInstalledApp()
        {
            return appsFlyerAndroid.CallStatic<bool>("isPreInstalledApp");
        }

        /// <summary>
        /// Get the Facebook attribution ID, if one exists.
        /// </summary>
        /// <returns>string Facebook attribution ID.</returns>
        public static string getAttributionId()
        {
            return appsFlyerAndroid.CallStatic<string>("getAttributionId");
        }

        /// <summary>
        /// Get AppsFlyer's unique device ID is created for every new install of an app.
        /// </summary>
        /// <returns>AppsFlyer's unique device ID.</returns>
        public static string getAppsFlyerId()
        {
            return appsFlyerAndroid.CallStatic<string>("getAppsFlyerId");
        }

        /// <summary>
        /// API for server verification of in-app purchases.
        /// An af_purchase event with the relevant values will be automatically sent if the validation is successful.
        /// </summary>
        /// <param name="publicKey">License Key obtained from the Google Play Console.</param>
        /// <param name="signature"><code>data.INAPP_DATA_SIGNATURE</code> from <code>onActivityResult(int requestCode, int resultCode, Intent data)</code></param>
        /// <param name="purchaseData"><code>data.INAPP_PURCHASE_DATA</code> from <code>onActivityResult(int requestCode, int resultCode, Intent data)</code></param>
        /// <param name="price">Purchase price, should be derived from <code>skuDetails.getStringArrayList("DETAILS_LIST")</code></param>
        /// <param name="currency">Purchase currency, should be derived from <code>skuDetails.getStringArrayList("DETAILS_LIST")</code></param>
        /// <param name="additionalParameters">additionalParameters Freehand parameters to be sent with the purchase (if validated).</param>
        public static void validateAndSendInAppPurchase(string publicKey, string signature, string purchaseData, string price, string currency, Dictionary<string, string> additionalParameters, MonoBehaviour gameObject)
        {
            appsFlyerAndroid.CallStatic("validateAndTrackInAppPurchase", publicKey, signature, purchaseData, price, currency, convertDictionaryToJavaMap(additionalParameters), gameObject ? gameObject.name : null);
        }

        /// <summary>
        /// Was the stopSDK(boolean) API set to true.
        /// </summary>
        /// <returns>boolean isSDKStopped.</returns>
        public static bool isSDKStopped()
        {
            return appsFlyerAndroid.CallStatic<bool>("isTrackingStopped");
        }

        /// <summary>
        /// Set a custom value for the minimum required time between sessions.
        /// By default, at least 5 seconds must lapse between 2 app launches to count as separate 2 sessions.
        /// </summary>
        /// <param name="seconds">minimum time between 2 separate sessions in seconds.</param>
        public static void setMinTimeBetweenSessions(int seconds)
        {
            appsFlyerAndroid.CallStatic("setMinTimeBetweenSessions", seconds);
        }

        /// <summary>
        /// Set a custom host.
        /// </summary>
        /// <param name="hostPrefixName">Host prefix.</param>
        /// <param name="hostName">Host name.</param>
        public static void setHost(string hostPrefixName, string hostName)
        {
            appsFlyerAndroid.CallStatic("setHost", hostPrefixName, hostName);
        }

        /// <summary>
        /// Get the host name.
        /// Default value is  "appsflyer.com"
        /// </summary>
        /// <returns>Host name.</returns>
        public static string getHostName()
        {
            return appsFlyerAndroid.CallStatic<string>("getHostName");
        }

        /// <summary>
        /// Get the custom host prefix.
        /// </summary>
        /// <returns>Host prefix.</returns>
        public static string getHostPrefix()
        {
            return appsFlyerAndroid.CallStatic<string>("getHostPrefix");
        }

        /// <summary>
        /// Register a Conversion Data Listener.
        /// Allows the developer to access the user attribution data in real-time for every new install, directly from the SDK level.
        /// By doing this you can serve users with personalized content or send them to specific activities within the app,
        /// which can greatly enhance their engagement with your app.
        /// </summary>
        public static void getConversionData(string objectName)
        {
            appsFlyerAndroid.CallStatic("getConversionData", objectName);
        }

        /// <summary>
        /// Register a validation listener for the validateAndSendInAppPurchase API.
        /// </summary>
        public static void initInAppPurchaseValidatorListener(MonoBehaviour gameObject)
        {
            appsFlyerAndroid.CallStatic("initInAppPurchaseValidatorListener", gameObject ? gameObject.name : null);
        }

        /// <summary>
        /// Use the following API to attribute the click and launch the app store's app page.
        /// </summary>
        /// <param name="promoted_app_id">promoted App ID</param>
        /// <param name="campaign">cross promotion campaign</param>
        /// <param name="userParams">additional user params</param>
        public static void attributeAndOpenStore(string promoted_app_id, string campaign, Dictionary<string, string> userParams)
        {
            appsFlyerAndroid.CallStatic("attributeAndOpenStore", promoted_app_id, campaign, convertDictionaryToJavaMap(userParams));
        }

        /// <summary>
        /// To attribute an impression use the following API call.
        /// Make sure to use the promoted App ID as it appears within the AppsFlyer dashboard.
        /// </summary>
        /// <param name="appID">promoted App ID.</param>
        /// <param name="campaign">cross promotion campaign.</param>
        public static void recordCrossPromoteImpression(string appID, string campaign)
        {
            appsFlyerAndroid.CallStatic("recordCrossPromoteImpression", appID, campaign);
        }

        /// <summary>
        /// The LinkGenerator class builds the invite URL according to various setter methods which allow passing on additional information on the click.
        /// See - https://support.appsflyer.com/hc/en-us/articles/115004480866-User-invite-attribution-
        /// </summary>
        /// <param name="parameters">parameters Dictionary.</param>
        public static void generateUserInviteLink(Dictionary<string, string> parameters, MonoBehaviour gameObject)
        {
            appsFlyerAndroid.CallStatic("createOneLinkInviteListener", convertDictionaryToJavaMap(parameters), gameObject ? gameObject.name : null);
        }

        /// <summary>
        /// To measure push notifications as part of a retargeting campaign.
        /// </summary>
        public static void handlePushNotification(){
            appsFlyerAndroid.CallStatic("handlePushNotification");
        }
        
        private static AndroidJavaObject getEmailType(EmailCryptType cryptType)
        {
            AndroidJavaClass emailsCryptTypeEnum = new AndroidJavaClass("com.appsflyer.AppsFlyerProperties$EmailsCryptType");
            AndroidJavaObject emailsCryptType;

            switch (cryptType)
            {
                case EmailCryptType.EmailCryptTypeSHA1:
                    emailsCryptType = emailsCryptTypeEnum.GetStatic<AndroidJavaObject>("SHA1");
                    break;
                case EmailCryptType.EmailCryptTypeMD5:
                    emailsCryptType = emailsCryptTypeEnum.GetStatic<AndroidJavaObject>("MD5");
                    break;
                case EmailCryptType.EmailCryptTypeSHA256:
                    emailsCryptType = emailsCryptTypeEnum.GetStatic<AndroidJavaObject>("SHA256");
                    break;
                default:
                    emailsCryptType = emailsCryptTypeEnum.GetStatic<AndroidJavaObject>("NONE");
                    break;
            }

            return emailsCryptType;
        }

        private static AndroidJavaObject convertDictionaryToJavaMap(Dictionary<string, string> dictionary)
        {
            AndroidJavaObject map = new AndroidJavaObject("java.util.HashMap");
            IntPtr putMethod = AndroidJNIHelper.GetMethodID(map.GetRawClass(), "put", "(Ljava/lang/Object;Ljava/lang/Object;)Ljava/lang/Object;");
            if (dictionary != null)
            {
                foreach (var entry in dictionary)
                {
                    AndroidJNI.CallObjectMethod(map.GetRawObject(), putMethod, AndroidJNIHelper.CreateJNIArgArray(new object[] { entry.Key, entry.Value }));
                }
            }
            
            return map;
        }
    }

#endif



}