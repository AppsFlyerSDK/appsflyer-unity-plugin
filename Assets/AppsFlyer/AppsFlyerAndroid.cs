using System;
using System.Collections.Generic;
using UnityEngine;

namespace AppsFlyerSDK
{

#if UNITY_ANDROID 
    public class AppsFlyerAndroid : IAppsFlyerAndroidBridge
    {
        public bool isInit { get; set; }

        private static AndroidJavaClass appsFlyerAndroid = new AndroidJavaClass("com.appsflyer.unity.AppsFlyerAndroidWrapper");

        public AppsFlyerAndroid() { }

        /// <summary>
        /// Use this method to init the sdk for the application.
        /// Call this method before startSDK.
        /// </summary>
        /// <param name="devkey"> AppsFlyer's Dev-Key, which is accessible from your AppsFlyer account under 'App Settings' in the dashboard.</param>
        /// <param name="gameObject">The current game object. This is used to get the conversion data callbacks. Pass null if you do not need the callbacks.</param>
        public void initSDK(string devkey, MonoBehaviour gameObject)
        {
#if !UNITY_EDITOR
             appsFlyerAndroid.CallStatic("initSDK", devkey, gameObject ? gameObject.name : null);
#endif
        }

        /// <summary>
        /// Use this method to start the sdk for the application.
        /// The AppsFlyer's Dev-Key must be provided.
        /// </summary>
        /// <param name="devkey"> AppsFlyer's Dev-Key, which is accessible from your AppsFlyer account under 'App Settings' in the dashboard.</param>
        public void startSDK(bool onRequestResponse, string CallBackObjectName)
        {
#if !UNITY_EDITOR
            appsFlyerAndroid.CallStatic("startTracking", onRequestResponse, CallBackObjectName);
#endif
        }

        /// <summary>
        /// Once this API is invoked, our SDK no longer communicates with our servers and stops functioning.
        /// In some extreme cases you might want to shut down all SDK activity due to legal and privacy compliance.
        /// This can be achieved with the stopSDK API.
        /// </summary>
        /// <param name="isSDKStopped">boolean should SDK be stopped.</param>
        public void stopSDK(bool isSDKStopped)
        {
#if !UNITY_EDITOR
             appsFlyerAndroid.CallStatic("stopTracking", isSDKStopped);
#endif
        }

        /// <summary>
        /// Get the AppsFlyer SDK version used in app.
        /// </summary>
        /// <returns>AppsFlyer SDK version.</returns>
        public string getSdkVersion()
        {
#if !UNITY_EDITOR
            return appsFlyerAndroid.CallStatic<string>("getSdkVersion");
#else
            return "";
#endif
        }

        /// <summary>
        /// Manually pass the Firebase / GCM Device Token for Uninstall measurement.
        /// </summary>
        /// <param name="token">Firebase Device Token.</param>
        public void updateServerUninstallToken(string token)
        {
#if !UNITY_EDITOR
            appsFlyerAndroid.CallStatic("updateServerUninstallToken", token);
#endif
        }

        /// <summary>
        /// Enables Debug logs for the AppsFlyer SDK.
        /// Should only be set to true in development / debug.
        /// </summary>
        /// <param name="shouldEnable">shouldEnable boolean.</param>
        public void setIsDebug(bool shouldEnable)
        {
#if !UNITY_EDITOR
            appsFlyerAndroid.CallStatic("setIsDebug", shouldEnable);
#endif
        }

        /// <summary>
        /// By default, IMEI and Android ID are not collected by the SDK if the OS version is higher than KitKat (4.4)
        /// and the device contains Google Play Services(on SDK versions 4.8.8 and below the specific app needed GPS).
        /// Use this API to explicitly send IMEI to AppsFlyer.
        /// </summary>
        /// <param name="aImei">device's IMEI.</param>
        public void setImeiData(string aImei)
        {
#if !UNITY_EDITOR
            appsFlyerAndroid.CallStatic("setImeiData", aImei);
#endif
        }

        /// <summary>
        /// By default, IMEI and Android ID are not collected by the SDK if the OS version is higher than KitKat(4.4)
        /// and the device contains Google Play Services(on SDK versions 4.8.8 and below the specific app needed GPS).
        /// Use this API to explicitly send Android ID to AppsFlyer.
        /// </summary>
        /// <param name="aAndroidId">device's Android ID.</param>
        public void setAndroidIdData(string aAndroidId)
        {
#if !UNITY_EDITOR
            appsFlyerAndroid.CallStatic("setAndroidIdData", aAndroidId);
#endif
        }

        /// <summary>
        /// Setting your own customer ID enables you to cross-reference your own unique ID with AppsFlyer’s unique ID and the other devices’ IDs.
        /// This ID is available in AppsFlyer CSV reports along with Postback APIs for cross-referencing with your internal IDs.
        /// </summary>
        /// <param name="id">Customer ID for client.</param>
        public void setCustomerUserId(string id)
        {
#if !UNITY_EDITOR
            appsFlyerAndroid.CallStatic("setCustomerUserId", id);
#endif
        }

        /// <summary>
        /// It is possible to delay the SDK Initialization until the customerUserID is set.
        /// This feature makes sure that the SDK doesn't begin functioning until the customerUserID is provided.
        /// If this API is used, all in-app events and any other SDK API calls are discarded, until the customerUserID is provided.
        /// </summary>
        /// <param name="wait">wait boolean.</param>
        public void waitForCustomerUserId(bool wait)
        {
#if !UNITY_EDITOR
            appsFlyerAndroid.CallStatic("waitForCustomerUserId", wait);
#endif
        }

        /// <summary>
        /// Use this API to provide the SDK with the relevant customer user id and trigger the SDK to begin its normal activity.
        /// </summary>
        /// <param name="id">Customer ID for client.</param>
        public void setCustomerIdAndStartSDK(string id)
        {
#if !UNITY_EDITOR
            appsFlyerAndroid.CallStatic("setCustomerIdAndTrack", id);
#endif
        }

        /// <summary>
        /// Get the current AF_STORE value.
        /// </summary>
        /// <returns>AF_Store value.</returns>
        public string getOutOfStore()
        {
#if !UNITY_EDITOR
            return appsFlyerAndroid.CallStatic<string>("getOutOfStore");
#else
            return "";
#endif
        }

        /// <summary>
        /// Manually set the AF_STORE value.
        /// </summary>
        /// <param name="sourceName">value to be set.</param>
        public void setOutOfStore(string sourceName)
        {
#if !UNITY_EDITOR
            appsFlyerAndroid.CallStatic("setOutOfStore", sourceName);
#endif
        }

        /// <summary>
        /// Set the OneLink ID that should be used for User-Invites.
        /// The link that is generated for the user invite will use this OneLink as the base link.
        /// </summary>
        /// <param name="oneLinkId">OneLink ID obtained from the AppsFlyer Dashboard.</param>
        public void setAppInviteOneLinkID(string oneLinkId)
        {
#if !UNITY_EDITOR
            appsFlyerAndroid.CallStatic("setAppInviteOneLinkID", oneLinkId);
#endif
        }

        /// <summary>
        /// Set additional data to be sent to AppsFlyer.
        /// </summary>
        /// <param name="customData">additional data Dictionary.</param>
        public void setAdditionalData(Dictionary<string, string> customData)
        {
#if !UNITY_EDITOR
            appsFlyerAndroid.CallStatic("setAdditionalData", convertDictionaryToJavaMap(customData));
#endif
        }

        //// <summary>
        /// Set the deepLink timeout value that should be used for DDL.
        /// </summary>
        /// <param name="deepLinkTimeout">deepLink timeout in milliseconds.</param>
        public void setDeepLinkTimeout(long deepLinkTimeout)
        {
#if !UNITY_EDITOR
             appsFlyerAndroid.CallStatic("setDeepLinkTimeout", deepLinkTimeout);
#endif
        }

        /// <summary>
        /// Set the user emails.
        /// </summary>
        /// <param name="emails">User emails.</param>
        public void setUserEmails(params string[] userEmails)
        {
#if !UNITY_EDITOR
            appsFlyerAndroid.CallStatic("setUserEmails", (object)userEmails);
#endif
        }


        /// <summary>
        /// Set the user phone number.
        /// </summary>
        /// <param name="phoneNumber">User phoneNumber.</param>
        public void setPhoneNumber(string phoneNumber){
#if !UNITY_EDITOR
            appsFlyerAndroid.CallStatic("setPhoneNumber", phoneNumber);
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
        public void setUserEmails(EmailCryptType cryptMethod, params string[] emails)
        {
#if !UNITY_EDITOR
            appsFlyerAndroid.CallStatic("setUserEmails", getEmailType(cryptMethod), (object)emails);
#endif
        }

        /// <summary>
        /// Opt-out of collection of Android ID.
        /// If the app does NOT contain Google Play Services, Android ID is collected by the SDK.
        /// However, apps with Google play services should avoid Android ID collection as this is in violation of the Google Play policy.
        /// </summary>
        /// <param name="isCollect">boolean, false to opt-out.</param>
        public void setCollectAndroidID(bool isCollect)
        {
#if !UNITY_EDITOR
            appsFlyerAndroid.CallStatic("setCollectAndroidID", isCollect);
#endif
        }

        /// <summary>
        /// Opt-out of collection of IMEI.
        /// If the app does NOT contain Google Play Services, device IMEI is collected by the SDK.
        /// However, apps with Google play services should avoid IMEI collection as this is in violation of the Google Play policy.
        /// </summary>
        /// <param name="isCollect">boolean, false to opt-out.</param>
        public void setCollectIMEI(bool isCollect)
        {
#if !UNITY_EDITOR
            appsFlyerAndroid.CallStatic("setCollectIMEI", isCollect);
#endif
        }

        /// <summary>
        /// Advertisers can wrap AppsFlyer OneLink within another Universal Link.
        /// This Universal Link will invoke the app but any deep linking data will not propagate to AppsFlyer.
        /// </summary>
        /// <param name="urls">Array of urls.</param>
        public void setResolveDeepLinkURLs(params string[] urls)
        {
#if !UNITY_EDITOR
             appsFlyerAndroid.CallStatic("setResolveDeepLinkURLs", (object)urls);
#endif
        }


        /// <summary>
        /// Advertisers can use this method to set vanity onelink domains.
        /// </summary>
        /// <param name="domains">Array of domains.</param>
        public void setOneLinkCustomDomain(params string[] domains)
        {
#if !UNITY_EDITOR
            appsFlyerAndroid.CallStatic("setOneLinkCustomDomain", (object)domains);
#endif
        }

        /// <summary>
        /// Manually set that the application was updated.
        /// </summary>
        /// <param name="isUpdate">isUpdate boolean value.</param>
        public void setIsUpdate(bool isUpdate)
        {
#if !UNITY_EDITOR
            appsFlyerAndroid.CallStatic("setIsUpdate", isUpdate);
#endif
        }

        /// <summary>
        /// Setting user local currency code for in-app purchases.
        /// The currency code should be a 3 character ISO 4217 code. (default is USD).
        /// You can set the currency code for all events by calling the following method.
        /// </summary>
        /// <param name="currencyCode">3 character ISO 4217 code.</param>
        public void setCurrencyCode(string currencyCode)
        {
#if !UNITY_EDITOR
            appsFlyerAndroid.CallStatic("setCurrencyCode", currencyCode);
#endif
        }

        /// <summary>
        /// Manually record the location of the user.
        /// </summary>
        /// <param name="latitude">latitude as double.</param>
        /// <param name="longitude">longitude as double.</param>
        public void recordLocation(double latitude, double longitude)
        {
#if !UNITY_EDITOR
            appsFlyerAndroid.CallStatic("trackLocation", latitude, longitude);
#endif
        }

        /// <summary>
        /// Send an In-App Event.
        /// In-App Events provide insight on what is happening in your app.
        /// </summary>
        /// <param name="eventName">Event Name as String.</param>
        /// <param name="eventValues">Event Values as Dictionary.</param>
        public void sendEvent(string eventName, Dictionary<string, string> eventValues)
        {
            sendEvent(eventName, eventValues, false, AppsFlyer.CallBackObjectName);
        }
        
        public void sendEvent(string eventName, Dictionary<string, string> eventValues, bool shouldCallback, string callBackObjectName)
        {
#if !UNITY_EDITOR
            appsFlyerAndroid.CallStatic("trackEvent", eventName, convertDictionaryToJavaMap(eventValues), shouldCallback, callBackObjectName);
#endif
        }

        /// <summary>
        /// Anonymize user Data.
        /// Use this API during the SDK Initialization to explicitly anonymize a user's installs, events and sessions.
        /// Default is false.
        /// </summary>
        /// <param name="isDisabled">isDisabled boolean.</param>
        public void anonymizeUser(bool isDisabled)
        {
#if !UNITY_EDITOR
             appsFlyerAndroid.CallStatic("setDeviceTrackingDisabled", isDisabled);
#endif
        }

        /// <summary>
        /// Calling enableTCFDataCollection(true) will enable collecting and sending any TCF related data.
        /// Calling enableTCFDataCollection(false) will disable the collection of TCF related data and from sending it.
        /// </summary>
        /// <param name = "shouldCollectTcfData" >should start TCF Data collection boolean.</param>
        public void enableTCFDataCollection(bool shouldCollectTcfData)
        {
#if !UNITY_EDITOR
             appsFlyerAndroid.CallStatic("enableTCFDataCollection", shouldCollectTcfData);
#endif
        }

        /// <summary>
        /// Enable the collection of Facebook Deferred AppLinks.
        /// Requires Facebook SDK and Facebook app on target/client device.
        /// This API must be invoked prior to initializing the AppsFlyer SDK in order to function properly.
        /// </summary>
        /// <param name="isEnabled">should Facebook's deferred app links be processed by the AppsFlyer SDK.</param>
        public void enableFacebookDeferredApplinks(bool isEnabled)
        {
#if !UNITY_EDITOR
            appsFlyerAndroid.CallStatic("enableFacebookDeferredApplinks", isEnabled);
#endif
        }

        /// <summary>
        /// Sets or updates the user consent data related to GDPR and DMA regulations for advertising and data usage purposes within the application.
        /// call this method when GDPR user is true
        /// </summary>
        /// <param name = "hasConsentForDataUsage" >hasConsentForDataUsage boolean.</param>
        /// <param name = "hasConsentForAdsPersonalization" >hasConsentForAdsPersonalization boolean.</param>
        public void setConsentData(AppsFlyerConsent appsFlyerConsent)
        {
#if !UNITY_EDITOR
            appsFlyerAndroid.CallStatic("setConsentData", appsFlyerConsent.isUserSubjectToGDPR, appsFlyerConsent.hasConsentForDataUsage, appsFlyerConsent.hasConsentForAdsPersonalization);
#endif
        }

        /// <summary>
        /// Restrict reengagement via deep-link to once per each unique deep-link.
        /// Otherwise deep re-occurring deep-links will be permitted for non-singleTask Activities and deep-linking via AppsFlyer deep-links.
        /// The default value is false.
        /// </summary>
        /// <param name="doConsume">doConsume boolean.</param>
        public void setConsumeAFDeepLinks(bool doConsume)
        {
#if !UNITY_EDITOR
            appsFlyerAndroid.CallStatic("setConsumeAFDeepLinks", doConsume);
#endif
        }

        /// <summary>
        /// Specify the manufacturer or media source name to which the preinstall is attributed.
        /// </summary>
        /// <param name="mediaSource">Manufacturer or media source name for preinstall attribution.</param>
        /// <param name="campaign">Campaign name for preinstall attribution.</param>
        /// <param name="siteId">Site ID for preinstall attribution.</param>
        public void setPreinstallAttribution(string mediaSource, string campaign, string siteId)
        {
#if !UNITY_EDITOR
            appsFlyerAndroid.CallStatic("setPreinstallAttribution", mediaSource, campaign, siteId);
#endif
        }

        /// <summary>
        /// Boolean indicator for preinstall by Manufacturer.
        /// </summary>
        /// <returns>boolean isPreInstalledApp.</returns>
        public bool isPreInstalledApp()
        {
#if !UNITY_EDITOR
            return appsFlyerAndroid.CallStatic<bool>("isPreInstalledApp");
#else
            return false;
#endif
        }

        /// <summary>
        /// Get the Facebook attribution ID, if one exists.
        /// </summary>
        /// <returns>string Facebook attribution ID.</returns>
        public string getAttributionId()
        {
#if !UNITY_EDITOR
            return appsFlyerAndroid.CallStatic<string>("getAttributionId");
#else
            return "";
#endif
        }

        /// <summary>
        /// Get AppsFlyer's unique device ID is created for every new install of an app.
        /// </summary>
        /// <returns>AppsFlyer's unique device ID.</returns>
        public string getAppsFlyerId()
        {
#if !UNITY_EDITOR
            return appsFlyerAndroid.CallStatic<string>("getAppsFlyerId");
#else
            return "";
#endif
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
        public void validateAndSendInAppPurchase(string publicKey, string signature, string purchaseData, string price, string currency, Dictionary<string, string> additionalParameters, MonoBehaviour gameObject)
        {
#if !UNITY_EDITOR
           appsFlyerAndroid.CallStatic("validateAndTrackInAppPurchase", publicKey, signature, purchaseData, price, currency, convertDictionaryToJavaMap(additionalParameters), gameObject ? gameObject.name : null);
#endif
        }

        /// <summary>
        /// API for server verification of in-app purchases.
        /// An af_purchase event with the relevant values will be automatically sent if the validation is successful.
        /// </summary>
        /// <param name="details">AFPurchaseDetailsAndroid instance.</param>
        /// <param name="additionalParameters">additionalParameters Freehand parameters to be sent with the purchase (if validated).</param>
        public void validateAndSendInAppPurchase(AFPurchaseDetailsAndroid details, Dictionary<string, string> additionalParameters, MonoBehaviour gameObject)
        {
#if !UNITY_EDITOR
           appsFlyerAndroid.CallStatic("validateAndTrackInAppPurchaseV2", (int)details.purchaseType, details.purchaseToken, details.productId, details.price, details.currency, convertDictionaryToJavaMap(additionalParameters), gameObject ? gameObject.name : null);
#endif
        }

        /// <summary>
        /// Was the stopSDK(boolean) API set to true.
        /// </summary>
        /// <returns>boolean isSDKStopped.</returns>
        public bool isSDKStopped()
        {
#if !UNITY_EDITOR
            return appsFlyerAndroid.CallStatic<bool>("isTrackingStopped");
#else
            return false;
#endif
        }

        /// <summary>
        /// Set a custom value for the minimum required time between sessions.
        /// By default, at least 5 seconds must lapse between 2 app launches to count as separate 2 sessions.
        /// </summary>
        /// <param name="seconds">minimum time between 2 separate sessions in seconds.</param>
        public void setMinTimeBetweenSessions(int seconds)
        {
#if !UNITY_EDITOR
            appsFlyerAndroid.CallStatic("setMinTimeBetweenSessions", seconds);
#endif
        }

        /// <summary>
        /// Set a custom host.
        /// </summary>
        /// <param name="hostPrefixName">Host prefix.</param>
        /// <param name="hostName">Host name.</param>
        public void setHost(string hostPrefixName, string hostName)
        {
#if !UNITY_EDITOR
            appsFlyerAndroid.CallStatic("setHost", hostPrefixName, hostName);
#endif
        }

        /// <summary>
        /// Get the host name.
        /// Default value is  "appsflyer.com".
        /// </summary>
        /// <returns>Host name.</returns>
        public string getHostName()
        {
#if !UNITY_EDITOR
            return appsFlyerAndroid.CallStatic<string>("getHostName");
#else
            return "";
#endif
        }

        /// <summary>
        /// Get the custom host prefix.
        /// </summary>
        /// <returns>Host prefix.</returns>
        public string getHostPrefix()
        {
#if !UNITY_EDITOR
            return appsFlyerAndroid.CallStatic<string>("getHostPrefix");
#else
            return "";
#endif
        }

        /// <summary>
        /// Used by advertisers to exclude all networks/integrated partners from getting data.
        /// </summary>
        public void setSharingFilterForAllPartners()
        {
#if !UNITY_EDITOR
            appsFlyerAndroid.CallStatic("setSharingFilterForAllPartners");
#endif
        }

        /// <summary>
        /// Used by advertisers to set some (one or more) networks/integrated partners to exclude from getting data.
        /// </summary>
        /// <param name="partners">partners to exclude from getting data</param>
        public void setSharingFilter(params string[] partners)
        {
#if !UNITY_EDITOR
            appsFlyerAndroid.CallStatic("setSharingFilter", (object)partners);
#endif
        }

        /// <summary>
        /// Lets you configure how which partners should the SDK exclude from data-sharing.
        /// </summary>
        /// <param name="partners">partners to exclude from getting data</param>
        public static void setSharingFilterForPartners(params string[] partners)
        {
#if !UNITY_EDITOR
            appsFlyerAndroid.CallStatic("setSharingFilterForPartners", (object)partners);
#endif
        }

        /// <summary>
        /// Register a Conversion Data Listener.
        /// Allows the developer to access the user attribution data in real-time for every new install, directly from the SDK level.
        /// By doing this you can serve users with personalized content or send them to specific activities within the app,
        /// which can greatly enhance their engagement with your app.
        /// </summary>
        public void getConversionData(string objectName)
        {
#if !UNITY_EDITOR
            appsFlyerAndroid.CallStatic("getConversionData", objectName);
#endif
        }

        /// <summary>
        /// Register a validation listener for the validateAndSendInAppPurchase API.
        /// </summary>
        public void initInAppPurchaseValidatorListener(MonoBehaviour gameObject)
        {
#if !UNITY_EDITOR
            appsFlyerAndroid.CallStatic("initInAppPurchaseValidatorListener", gameObject ? gameObject.name : null);
#endif
        }

        /// <summary>
        /// setCollectOaid
        /// You must include the appsflyer oaid library for this api to work.
        /// </summary>
        /// <param name="isCollect">isCollect oaid - set fasle to opt out</param>
        public void setCollectOaid(bool isCollect)
        {
#if !UNITY_EDITOR
            appsFlyerAndroid.CallStatic("setCollectOaid", isCollect);
#endif
        }

        /// <summary>
        /// Use the following API to attribute the click and launch the app store's app page.
        /// </summary>
        /// <param name="promoted_app_id">promoted App ID</param>
        /// <param name="campaign">cross promotion campaign</param>
        /// <param name="userParams">additional user params</param>
        public void attributeAndOpenStore(string promoted_app_id, string campaign, Dictionary<string, string> userParams, MonoBehaviour gameObject)
        {
#if !UNITY_EDITOR
            appsFlyerAndroid.CallStatic("attributeAndOpenStore", promoted_app_id, campaign, convertDictionaryToJavaMap(userParams));
#endif
        }

        /// <summary>
        /// To attribute an impression use the following API call.
        /// Make sure to use the promoted App ID as it appears within the AppsFlyer dashboard.
        /// </summary>
        /// <param name="appID">promoted App ID.</param>
        /// <param name="campaign">cross promotion campaign.</param>
        /// <param name="parameters">parameters Dictionary.</param>
        public void recordCrossPromoteImpression(string appID, string campaign, Dictionary<string, string> parameters)
        {
#if !UNITY_EDITOR
            appsFlyerAndroid.CallStatic("recordCrossPromoteImpression", appID, campaign, convertDictionaryToJavaMap(parameters));
#endif
        }

        /// <summary>
        /// The LinkGenerator class builds the invite URL according to various setter methods which allow passing on additional information on the click.
        /// See - https://support.appsflyer.com/hc/en-us/articles/115004480866-User-invite-attribution-
        /// </summary>
        /// <param name="parameters">parameters Dictionary.</param>
        public void generateUserInviteLink(Dictionary<string, string> parameters, MonoBehaviour gameObject)
        {
#if !UNITY_EDITOR
            appsFlyerAndroid.CallStatic("createOneLinkInviteListener", convertDictionaryToJavaMap(parameters), gameObject ? gameObject.name : null);
#endif
        }

        /// <summary>
        /// To measure push notifications as part of a retargeting campaign.
        /// </summary>
        public void handlePushNotifications(){
#if !UNITY_EDITOR
            appsFlyerAndroid.CallStatic("handlePushNotifications");
#endif
        }

        /// <summary>
        /// Use this method if you’re integrating your app with push providers 
        /// that don’t use the default push notification JSON schema the SDK expects.
        /// See docs for more info.
        /// </summary>
        /// <param name="paths">array of nested json path</param>
        public void addPushNotificationDeepLinkPath(params string[] paths)
        {
#if !UNITY_EDITOR
            appsFlyerAndroid.CallStatic("addPushNotificationDeepLinkPath", (object)paths);
#endif
        }

        /// <summary>
        /// subscribe to unified deep link callbacks
        /// </summary>
        public void subscribeForDeepLink(string objectName){
#if !UNITY_EDITOR
            appsFlyerAndroid.CallStatic("subscribeForDeepLink", objectName);
#endif
        }

        /// <summary>
        /// Disables collection of various Advertising IDs by the SDK. This includes Google Advertising ID (GAID), OAID and Amazon Advertising ID (AAID)
        /// </summary>
        /// <param name="disable">disable boolean.</param>
        public void setDisableAdvertisingIdentifiers(bool disable)
        {
#if !UNITY_EDITOR
             appsFlyerAndroid.CallStatic("setDisableAdvertisingIdentifiers", disable);
#endif
        }

        /// <summary>
        /// Allows sending custom data for partner integration purposes.
        /// </summary>
        public void setPartnerData(string partnerId, Dictionary<string, string> partnerInfo)
        {
#if !UNITY_EDITOR
             appsFlyerAndroid.CallStatic("setPartnerData", partnerId, convertDictionaryToJavaMap(partnerInfo));
#endif
        }

        /// <summary>
        /// Use to opt-out of collecting the network operator name (carrier) and sim operator name from the device.
        /// </summary>
        public void setDisableNetworkData(bool disable) {
#if !UNITY_EDITOR
                appsFlyerAndroid.CallStatic("setDisableNetworkData", disable);
#endif
        }

        /// <summary>
        /// Internal Helper Method.
        /// </summary>
        private static AndroidJavaObject getEmailType(EmailCryptType cryptType)
        {
            AndroidJavaClass emailsCryptTypeEnum = new AndroidJavaClass("com.appsflyer.AppsFlyerProperties$EmailsCryptType");
            AndroidJavaObject emailsCryptType;

            switch (cryptType)
            {
                case EmailCryptType.EmailCryptTypeSHA256:
                    emailsCryptType = emailsCryptTypeEnum.GetStatic<AndroidJavaObject>("SHA256");
                    break;
                default:
                    emailsCryptType = emailsCryptTypeEnum.GetStatic<AndroidJavaObject>("NONE");
                    break;
            }

            return emailsCryptType;
        }

        /// <summary>
        /// Internal Helper Method.
        /// </summary>
        private static AndroidJavaObject convertDictionaryToJavaMap(Dictionary<string, string> dictionary)
        {
            AndroidJavaObject map = new AndroidJavaObject("java.util.HashMap");
            IntPtr putMethod = AndroidJNIHelper.GetMethodID(map.GetRawClass(), "put", "(Ljava/lang/Object;Ljava/lang/Object;)Ljava/lang/Object;");
            jvalue[] val;
            if (dictionary != null)
            {
                foreach (var entry in dictionary)
                {
                    val = AndroidJNIHelper.CreateJNIArgArray(new object[] { entry.Key, entry.Value });
                    AndroidJNI.CallObjectMethod(map.GetRawObject(), putMethod,val);
                    AndroidJNI.DeleteLocalRef(val[0].l);
                    AndroidJNI.DeleteLocalRef(val[1].l);
                }
            }
            
            return map;
        }
    }

#endif



}