using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace AppsFlyerSDK
{
#if UNITY_IOS

    public class AppsFlyeriOS
    {

        /// <summary>
        /// Start Session.
        /// This will record a session and then record all background forground sessions during the lifecycle of the app.
        /// </summary>
        public static void startSDK()
        {
                startSDK(false, AppsFlyer.CallBackObjectName);
        }
        
        public static void startSDK(bool shouldCallback, string callBackObjectName)
        {
#if !UNITY_EDITOR
                _startSDK(shouldCallback, callBackObjectName); 
#endif
        }

        /// <summary>
        /// Send an In-App Event.
        /// In-App Events provide insight on what is happening in your app.
        /// </summary>
        /// <param name="eventName">Name of event.</param>
        /// <param name="eventValues">Contains dictionary of values for handling by backend.</param>
        public static void sendEvent(string eventName, Dictionary<string, string> eventValues)
        {
                sendEvent(eventName, eventValues, false, AppsFlyer.CallBackObjectName);
        }
        
        public static void sendEvent(string eventName, Dictionary<string, string> eventValues, bool shouldCallback, string callBackObjectName)
        {
#if !UNITY_EDITOR
           _afSendEvent(eventName, AFMiniJSON.Json.Serialize(eventValues), shouldCallback, callBackObjectName);
#endif
        }

        /// <summary>
        /// Get the conversion data.
        /// Allows the developer to access the user attribution data in real-time for every new install, directly from the SDK level.
        /// By doing this you can serve users with personalized content or send them to specific activities within the app,
        /// which can greatly enhance their engagement with your app.
        /// </summary>
        public static void getConversionData(string objectName)
        {
#if !UNITY_EDITOR
            _getConversionData(objectName);
#endif
        }

        /// <summary>
        /// In case you use your own user ID in your app, you can set this property to that ID.
        /// Enables you to cross-reference your own unique ID with AppsFlyer’s unique ID and the other devices’ IDs.
        /// </summary>
        /// <param name="customerUserID">Customer ID for client.</param>
        public static void setCustomerUserID(string customerUserID)
        {
#if !UNITY_EDITOR
            _setCustomerUserID(customerUserID);
#endif
        }

        /// <summary>
        ///  In case you use custom data and you want to receive it in the raw reports.
        /// see [Setting additional custom data] (https://support.appsflyer.com/hc/en-us/articles/207032066-AppsFlyer-SDK-Integration-iOS#setting-additional-custom-data) for more information.
        /// </summary>
        /// <param name="customData">additional data Dictionary.</param>
        public static void setAdditionalData(Dictionary<string, string> customData)
        {
#if !UNITY_EDITOR
           _setAdditionalData(AFMiniJSON.Json.Serialize(customData));
#endif
        }

        /// <summary>
        ///  Use this method to set your AppsFlyer's dev key.
        /// </summary>
        /// <param name="appsFlyerDevKey">AppsFlyer's Dev-Key, which is accessible from your AppsFlyer account under 'App Settings' in the dashboard.</param>
        public static void setAppsFlyerDevKey(string appsFlyerDevKey)
        {
#if !UNITY_EDITOR
           _setAppsFlyerDevKey(appsFlyerDevKey);
#endif
        }

        /// <summary>
        /// Use this method to set your app's Apple ID(taken from the app's page on iTunes Connect).
        /// </summary>
        /// <param name="appleAppID">your app's Apple ID.</param>
        public static void setAppleAppID(string appleAppID)
        {
#if !UNITY_EDITOR
           _setAppleAppID(appleAppID);
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
#if !UNITY_EDITOR
           _setCurrencyCode(currencyCode);
#endif
        }

        /// <summary>
        ///  AppsFlyer SDK collect Apple's `advertisingIdentifier` if the `AdSupport.framework` included in the SDK.
        /// You can disable this behavior by setting the following property to true.
        /// </summary>
        /// <param name="disableCollectAppleAdSupport">boolean to disableCollectAppleAdSupport</param>
        public static void setDisableCollectAppleAdSupport(bool disableCollectAppleAdSupport)
        {
#if !UNITY_EDITOR
           _setDisableCollectAppleAdSupport(disableCollectAppleAdSupport);
#endif
        }

        /// <summary>
        /// Enables Debug logs for the AppsFlyer SDK.
        /// Should only be set to true in development / debug.
        /// The default value is false.
        /// </summary>
        /// <param name="isDebug">shouldEnable boolean..</param>
        public static void setIsDebug(bool isDebug)
        {
#if !UNITY_EDITOR
           _setIsDebug(isDebug);
#endif
        }

        /// <summary>
        /// Set this flag to true, to collect the current device name(e.g. "My iPhone"). Default value is false.
        /// </summary>
        /// <param name="shouldCollectDeviceName">boolean shouldCollectDeviceName.</param>
         [System.Obsolete("This is deprecated")]
        public static void setShouldCollectDeviceName(bool shouldCollectDeviceName)
        {
#if !UNITY_EDITOR
            _setShouldCollectDeviceName(shouldCollectDeviceName);
#endif
        }

        /// <summary>
        /// Set the OneLink ID that should be used for User-Invites.
        /// The link that is generated for the user invite will use this OneLink as the base link.
        /// </summary>
        /// <param name="appInviteOneLinkID">OneLink ID obtained from the AppsFlyer Dashboard.</param>
        public static void setAppInviteOneLinkID(string appInviteOneLinkID)
        {
#if !UNITY_EDITOR
            _setAppInviteOneLinkID(appInviteOneLinkID);
#endif
        }

        /// <summary>
        /// Anonymize user Data.
        /// Use this API during the SDK Initialization to explicitly anonymize a user's installs, events and sessions.
        /// Default is false
        /// </summary>
        /// <param name="shouldAnonymizeUser">boolean shouldAnonymizeUser.</param>
        public static void anonymizeUser(bool shouldAnonymizeUser)
        {
#if !UNITY_EDITOR
           _anonymizeUser(shouldAnonymizeUser);
#endif
        }

        /// <summary>
        /// Opt-out for Apple Search Ads attributions.
        /// </summary>
        /// <param name="disableCollectIAd">boolean disableCollectIAd.</param>
        public static void setDisableCollectIAd(bool disableCollectIAd)
        {
#if !UNITY_EDITOR
           _setDisableCollectIAd(disableCollectIAd);
#endif
        }

        /// <summary>
        /// In app purchase receipt validation Apple environment(production or sandbox). The default value is false.
        /// </summary>
        /// <param name="useReceiptValidationSandbox">boolean useReceiptValidationSandbox.</param>
        public static void setUseReceiptValidationSandbox(bool useReceiptValidationSandbox)
        {
#if !UNITY_EDITOR
            _setUseReceiptValidationSandbox(useReceiptValidationSandbox);
#endif
        }

        /// <summary>
        /// Set this flag to test uninstall on Apple environment(production or sandbox). The default value is false.
        /// </summary>
        /// <param name="useUninstallSandbox">boolean useUninstallSandbox.</param>
        public static void setUseUninstallSandbox(bool useUninstallSandbox)
        {
#if !UNITY_EDITOR
           _setUseUninstallSandbox(useUninstallSandbox);
#endif
        }

        /// <summary>
        /// For advertisers who wrap OneLink within another Universal Link.
        /// An advertiser will be able to deeplink from a OneLink wrapped within another Universal Link and also record this retargeting conversion.
        /// </summary>
        /// <param name="resolveDeepLinkURLs">Array of urls.</param>
        public static void setResolveDeepLinkURLs(params string[] resolveDeepLinkURLs)
        {
#if !UNITY_EDITOR
           _setResolveDeepLinkURLs(resolveDeepLinkURLs.Length,resolveDeepLinkURLs);
#endif
        }

        /// <summary>
        /// For advertisers who use vanity OneLinks.
        /// </summary>
        /// <param name="oneLinkCustomDomains">Array of domains.</param>
        public static void setOneLinkCustomDomains(params string[] oneLinkCustomDomains)
        {
#if !UNITY_EDITOR
            _setOneLinkCustomDomains(oneLinkCustomDomains.Length, oneLinkCustomDomains);
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
        /// <param name="cryptType">type Hash algoritm.</param>
        /// <param name="length">length of userEmails array.</param>
        /// <param name="userEmails">userEmails The list of strings that hold mails.</param>
        public static void setUserEmails(EmailCryptType cryptType, int length, params string[] userEmails)
        {
#if !UNITY_EDITOR
           _setUserEmails(cryptType, length, userEmails);
#endif
        }

        /// <summary>
        /// Set the user phone number.
        /// </summary>
        /// <param name="phoneNumber">User phoneNumber.</param>
        public static void setPhoneNumber(string phoneNumber){
#if !UNITY_EDITOR
            _setPhoneNumber(phoneNumber);
#endif
        }

        /// <summary>
        ///  To send and validate in app purchases you can call this method from the processPurchase method.
        /// </summary>
        /// <param name="productIdentifier">The product identifier.</param>
        /// <param name="price">The product price.</param>
        /// <param name="currency">The product currency.</param>
        /// <param name="tranactionId">The purchase transaction Id.</param>
        /// <param name="additionalParameters">The additional param, which you want to receive it in the raw reports.</param>
        public static void validateAndSendInAppPurchase(string productIdentifier, string price, string currency, string tranactionId, Dictionary<string, string> additionalParameters, MonoBehaviour gameObject)
        {
#if !UNITY_EDITOR
            _validateAndSendInAppPurchase(productIdentifier, price, currency, tranactionId, AFMiniJSON.Json.Serialize(additionalParameters), gameObject ? gameObject.name : null);
#endif
        }

        /// <summary>
        /// To record location for geo-fencing. Does the same as code below.
        /// </summary>
        /// <param name="longitude">The location longitude.</param>
        /// <param name="latitude">The location latitude.</param>
        public static void recordLocation(double longitude, double latitude)
        {
#if !UNITY_EDITOR
            _recordLocation(longitude, latitude);
#endif
        }

        /// <summary>
        /// Get AppsFlyer's unique device ID, which is created for every new install of an app.
        /// </summary>
        public static string getAppsFlyerId()
        {
#if !UNITY_EDITOR
           return _getAppsFlyerId();
#else
            return "";
#endif
        }

        /// <summary>
        /// Register uninstall - you should register for remote notification and provide AppsFlyer the push device token.
        /// </summary>
        /// <param name="deviceToken">deviceToken The `deviceToken` from `-application:didRegisterForRemoteNotificationsWithDeviceToken:`.</param>
        public static void registerUninstall(byte[] deviceToken)
        {
#if !UNITY_EDITOR
           _registerUninstall(deviceToken);
#endif
        }

        /// <summary>
        /// Enable AppsFlyer to handle a push notification.
        /// </summary>
        /// <param name="pushPayload">pushPayload The `userInfo` from received remote notification. One of root keys should be @"af"..</param>
        public static void handlePushNotification(Dictionary<string, string> pushPayload)
        {
#if !UNITY_EDITOR
           _handlePushNotification(AFMiniJSON.Json.Serialize(pushPayload));
#endif
        }

        /// <summary>
        /// Get SDK version.
        /// </summary>
        public static string getSDKVersion()
        {
#if !UNITY_EDITOR
           return _getSDKVersion();
#else
            return "";
#endif
        }

        /// <summary>
        /// This property accepts a string value representing the host name for all endpoints.
        /// Can be used to Zero rate your application’s data usage.Contact your CSM for more information.
        /// </summary>
        /// <param name="host">Host Name.</param>
        /// <param name="host">Host prefix.</param>
        public static void setHost(string host, string hostPrefix)
        {
#if !UNITY_EDITOR
            _setHost(host, hostPrefix);
#endif
        }

        /// <summary>
        /// This property is responsible for timeout between sessions in seconds.
        /// Default value is 5 seconds.
        /// </summary>
        /// <param name="minTimeBetweenSessions">minimum time between 2 separate sessions in seconds.</param>
        public static void setMinTimeBetweenSessions(int minTimeBetweenSessions)
        {
#if !UNITY_EDITOR
           _setMinTimeBetweenSessions(minTimeBetweenSessions);
#endif
        }

        /// <summary>
        /// Once this API is invoked, our SDK no longer communicates with our servers and stops functioning.
        /// In some extreme cases you might want to shut down all SDK activity due to legal and privacy compliance.
        /// This can be achieved with the stopSDK API.
        /// </summary>
        /// <param name="isSDKStopped">boolean isSDKStopped.</param>
        public static void stopSDK(bool isSDKStopped)
        {
#if !UNITY_EDITOR
            _stopSDK(isSDKStopped);
#endif
        }

        // <summary>
        /// Was the stopSDK(boolean) API set to true.
        /// </summary>
        /// <returns>boolean isSDKStopped.</returns>
        public static bool isSDKStopped()
        {
#if !UNITY_EDITOR
           return _isSDKStopped();
#else
            return false;
#endif
        }

        /// <summary>
        /// In case you want to track deep linking manually call handleOpenUrl.
        /// The continueUserActivity and onOpenURL are implemented in the AppsFlyerAppController.mm class, so 
        /// only use this method if the other methods do not cover your apps deeplinking needs.
        /// </summary>
        /// <param name="url">The URL to be passed to your AppDelegate.</param>
        /// <param name="sourceApplication">The sourceApplication to be passed to your AppDelegate.</param>
        /// <param name="annotation">The annotation to be passed to your app delegate.</param>
        public static void handleOpenUrl(string url, string sourceApplication, string annotation)
        {
#if !UNITY_EDITOR
            _handleOpenUrl(url, sourceApplication, annotation);
#endif
        }

        /// <summary>
        /// Used by advertisers to exclude all networks/integrated partners from getting data.
        /// </summary>
        public static void setSharingFilterForAllPartners()
        {
#if !UNITY_EDITOR
            _setSharingFilterForAllPartners();
#endif
        }

        /// <summary>
        /// Used by advertisers to set some (one or more) networks/integrated partners to exclude from getting data.
        /// </summary>
        /// <param name="partners">partners to exclude from getting data</param>
        public static void setSharingFilter(params string[] partners)
        {
#if !UNITY_EDITOR
            _setSharingFilter(partners.Length, partners);
#endif
        }


        /// <summary>
        /// Lets you configure how which partners should the SDK exclude from data-sharing.
        /// <param name="partners">partners to exclude from getting data</param>
        public static void setSharingFilterForPartners(params string[] partners)
        {
#if !UNITY_EDITOR
            _setSharingFilterForPartners(partners.Length, partners);
#endif
        }

        /// <summary>
        /// To record an impression use the following API call.
        /// Make sure to use the promoted App ID as it appears within the AppsFlyer dashboard.
        /// </summary>
        /// <param name="appID">promoted App ID.</param>
        /// <param name="campaign">cross promotion campaign.</param>
        /// <param name="parameters">parameters Dictionary.</param>
        public static void recordCrossPromoteImpression(string appID, string campaign, Dictionary<string, string> parameters)
        {
#if !UNITY_EDITOR
            _recordCrossPromoteImpression(appID, campaign, AFMiniJSON.Json.Serialize(parameters));
#endif
        }

        /// <summary>
        /// Use the following API to attribute the click and launch the app store's app page.
        /// </summary>
        /// <param name="appID">promoted App ID</param>
        /// <param name="campaign">cross promotion campaign</param>
        /// <param name="parameters">additional user params</param>
        public static void attributeAndOpenStore(string appID, string campaign, Dictionary<string, string> parameters, MonoBehaviour gameObject)
        {
#if !UNITY_EDITOR
           _attributeAndOpenStore(appID, campaign, AFMiniJSON.Json.Serialize(parameters), gameObject ? gameObject.name : null);
#endif
        }

        /// <summary>
        /// The LinkGenerator class builds the invite URL according to various setter methods which allow passing on additional information on the click.
        /// See - https://support.appsflyer.com/hc/en-us/articles/115004480866-User-invite-attribution-
        /// </summary>
        /// <param name="parameters">parameters Dictionary.</param>
        public static void generateUserInviteLink(Dictionary<string, string> parameters, MonoBehaviour gameObject)
        {
#if !UNITY_EDITOR
            _generateUserInviteLink(AFMiniJSON.Json.Serialize(parameters), gameObject ? gameObject.name : null);
#endif
        }

        /// <summary>
        /// It is recommended to generate an in-app event after the invite is sent to record the invites from the senders' perspective. 
        /// This enables you to find the users that tend most to invite friends, and the media sources that get you these users.
        /// </summary>
        /// <param name="channel">channel string.</param>
        /// <param name="parameters">parameters Dictionary..</param>
        public static void recordInvite(string channel, Dictionary<string, string> parameters)
        {
#if !UNITY_EDITOR
            _recordInvite(channel, AFMiniJSON.Json.Serialize(parameters));
#endif
        }

        /// <summary>
        /// Waits for request user authorization to access app-related data
        /// </summary>
        /// <param name="timeoutInterval">time to wait until session starts</param>
        public static void waitForATTUserAuthorizationWithTimeoutInterval(int timeoutInterval)
        {
#if !UNITY_EDITOR
            _waitForATTUserAuthorizationWithTimeoutInterval(timeoutInterval);
#endif
        }

        /// <summary>
        /// </summary>
        /// <param name="isDisabled">bool should diable</param>
        public static void disableSKAdNetwork(bool isDisabled)
        {
#if !UNITY_EDITOR
            _disableSKAdNetwork(isDisabled);
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
#if !UNITY_EDITOR
            _addPushNotificationDeepLinkPath(paths.Length, paths);
#endif
        }

        /// <summary>
        /// subscribe to unified deep link callbacks
        /// </summary>
        public static void subscribeForDeepLink(string objectName){
#if !UNITY_EDITOR
            _subscribeForDeepLink(objectName);
#endif
        }

           /// <summary>
        /// Set the language of the device.
        /// </summary>
        public static void setCurrentDeviceLanguage(string language){
#if !UNITY_EDITOR
            _setCurrentDeviceLanguage(language);
#endif
        }

        /*
         * AppsFlyer ios method mapping
         */

        [DllImport("__Internal")]
        private static extern void _startSDK(bool shouldCallback, string objectName);

        [DllImport("__Internal")]
        private static extern void _getConversionData(string objectName);

        [DllImport("__Internal")]
        private static extern void _setCustomerUserID(string customerUserID);

        [DllImport("__Internal")]
        private static extern void _setAdditionalData(string customData);

        [DllImport("__Internal")]
        private static extern void _setAppsFlyerDevKey(string appsFlyerDevKey);

        [DllImport("__Internal")]
        private static extern void _setAppleAppID(string appleAppID);

        [DllImport("__Internal")]
        private static extern void _setCurrencyCode(string currencyCode);

        [DllImport("__Internal")]
        private static extern void _setDisableCollectAppleAdSupport(bool disableCollectAppleAdSupport);

        [DllImport("__Internal")]
        private static extern void _setIsDebug(bool isDebug);
     
        [DllImport("__Internal")]
        private static extern void _setShouldCollectDeviceName(bool shouldCollectDeviceName);

        [DllImport("__Internal")]
        private static extern void _setAppInviteOneLinkID(string appInviteOneLinkID);

        [DllImport("__Internal")]
        private static extern void _anonymizeUser(bool shouldAnonymizeUser);
      
        [DllImport("__Internal")]
        private static extern void _setDisableCollectIAd(bool disableCollectIAd);

        [DllImport("__Internal")]
        private static extern void _setUseReceiptValidationSandbox(bool useReceiptValidationSandbox);

        [DllImport("__Internal")]
        private static extern void _setUseUninstallSandbox(bool useUninstallSandbox);

        [DllImport("__Internal")]
        private static extern void _setResolveDeepLinkURLs(int length, params string[] resolveDeepLinkURLs);

        [DllImport("__Internal")]
        private static extern void _setOneLinkCustomDomains(int length, params string[] oneLinkCustomDomains);

        [DllImport("__Internal")]
        private static extern void _setUserEmails(EmailCryptType cryptType, int length, params string[] userEmails);

        [DllImport("__Internal")]
        private static extern void _setPhoneNumber(string phoneNumber);

        [DllImport("__Internal")]
        private static extern void _afSendEvent(string eventName, string eventValues, bool shouldCallback, string objectName);

        [DllImport("__Internal")]
        private static extern void _validateAndSendInAppPurchase(string productIdentifier, string price, string currency, string tranactionId, string additionalParameters, string objectName);

        [DllImport("__Internal")]
        private static extern void _recordLocation(double longitude, double latitude);

        [DllImport("__Internal")]
        private static extern string _getAppsFlyerId();

        [DllImport("__Internal")]
        private static extern void _registerUninstall(byte[] deviceToken);

        [DllImport("__Internal")]
        private static extern void _handlePushNotification(string pushPayload);

        [DllImport("__Internal")]
        private static extern string _getSDKVersion();

        [DllImport("__Internal")]
        private static extern void _setHost(string host, string hostPrefix);

        [DllImport("__Internal")]
        private static extern void _setMinTimeBetweenSessions(int minTimeBetweenSessions);

        [DllImport("__Internal")]
        private static extern void _stopSDK(bool isStopSDK);

        [DllImport("__Internal")]
        private static extern bool _isSDKStopped();

        [DllImport("__Internal")]
        private static extern void _handleOpenUrl(string url, string sourceApplication, string annotation);

        [DllImport("__Internal")]
        private static extern void _setSharingFilterForAllPartners();

        [DllImport("__Internal")]
        private static extern void _setSharingFilter(int length, params string[] partners);

        [DllImport("__Internal")]
        private static extern void _setSharingFilterForPartners(int length, params string[] partners);

        [DllImport("__Internal")]
        private static extern void _recordCrossPromoteImpression(string appID, string campaign, string parameters);

        [DllImport("__Internal")]
        private static extern void _attributeAndOpenStore(string appID, string campaign, string parameters, string gameObject);

        [DllImport("__Internal")]
        private static extern void _generateUserInviteLink(string parameters, string gameObject);

        [DllImport("__Internal")]
        private static extern void _recordInvite(string channel, string parameters);

        [DllImport("__Internal")]
        private static extern void _waitForATTUserAuthorizationWithTimeoutInterval(int timeoutInterval);

        [DllImport("__Internal")]
        private static extern void _disableSKAdNetwork(bool isDisabled);

        [DllImport("__Internal")]
        private static extern void _addPushNotificationDeepLinkPath(int length, params string[] paths);

        [DllImport("__Internal")]
        private static extern void _subscribeForDeepLink(string objectName);

        [DllImport("__Internal")]
        private static extern void _setCurrentDeviceLanguage(string language);

    }

#endif


}