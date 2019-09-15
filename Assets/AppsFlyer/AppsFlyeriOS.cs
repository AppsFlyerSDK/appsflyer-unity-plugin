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
            _startSDK();
        }

        /// <summary>
        /// Get conversion data.
        /// </summary>
        public static void getConversionData(string objectName)
        {
            _getConversionData(objectName);
        }

        /// <summary>
        /// In case you use your own user ID in your app, you can set this property to that ID.
        /// Enables you to cross-reference your own unique ID with AppsFlyer’s unique ID and the other devices’ IDs.
        /// </summary>
        /// <param name="customerUserID">Customer ID for client.</param>
        public static void setCustomerUserID(string customerUserID)
        {
            _setCustomerUserID(customerUserID);
        }

        /// <summary>
        ///  In case you use custom data and you want to receive it in the raw reports.
        /// see [Setting additional custom data] (https://support.appsflyer.com/hc/en-us/articles/207032066-AppsFlyer-SDK-Integration-iOS#setting-additional-custom-data) for more information.
        /// </summary>
        /// <param name="customData">additional data Dictionary.</param>
        public static void setAdditionalData(Dictionary<string, string> customData)
        {
            _setAdditionalData(AFMiniJSON.Json.Serialize(customData));
        }

        /// <summary>
        ///  Use this method to set your AppsFlyer's dev key.
        /// </summary>
        /// <param name="appsFlyerDevKey">AppsFlyer's Dev-Key, which is accessible from your AppsFlyer account under 'App Settings' in the dashboard.</param>
        public static void setAppsFlyerDevKey(string appsFlyerDevKey)
        {
            _setAppsFlyerDevKey(appsFlyerDevKey);
        }

        /// <summary>
        /// Use this method to set your app's Apple ID(taken from the app's page on iTunes Connect).
        /// </summary>
        /// <param name="appleAppID">your app's Apple ID.</param>
        public static void setAppleAppID(string appleAppID)
        {
            _setAppleAppID(appleAppID);
        }

        /// <summary>
        /// In case of in app purchase events, you can set the currency code your user has purchased with.
        /// The currency code is a 3 letter code according to ISO standards.
        /// </summary>
        /// <param name="currencyCode">3 character ISO 4217 code.</param>
        public static void setCurrencyCode(string currencyCode)
        {
            _setCurrencyCode(currencyCode);
        }

        /// <summary>
        ///  AppsFlyer SDK collect Apple's `advertisingIdentifier` if the `AdSupport.framework` included in the SDK.
        /// You can disable this behavior by setting the following property to true.
        /// </summary>
        /// <param name="disableCollectAppleAdSupport">boolean to disableCollectAppleAdSupport</param>
        public static void setDisableCollectAppleAdSupport(bool disableCollectAppleAdSupport)
        {
            _setDisableCollectAppleAdSupport(disableCollectAppleAdSupport);
        }

        /// <summary>
        /// Prints SDK messages to the console log.This property should only be used in `DEBUG` mode.
        /// The default value is false.
        /// </summary>
        /// <param name="isDebug">shouldEnable boolean..</param>
        public static void setIsDebug(bool isDebug)
        {
            _setIsDebug(isDebug);
        }

        /// <summary>
        /// Set this flag to true, to collect the current device name(e.g. "My iPhone"). Default value is false.
        /// </summary>
        /// <param name="shouldCollectDeviceName">boolean shouldCollectDeviceName.</param>
        public static void setShouldCollectDeviceName(bool shouldCollectDeviceName)
        {
            _setShouldCollectDeviceName(shouldCollectDeviceName);
        }

        /// <summary>
        /// Set your `OneLink ID` from OneLink configuration. Used in User Invites to generate a OneLink.
        /// </summary>
        /// <param name="appInviteOneLinkID">OneLink ID obtained from the AppsFlyer Dashboard.</param>
        public static void setAppInviteOneLinkID(string appInviteOneLinkID)
        {
            _setAppInviteOneLinkID(appInviteOneLinkID);
        }

        /// <summary>
        /// Opt-out for specific user.
        /// </summary>
        /// <param name="shouldAnonymizeUser">boolean shouldAnonymizeUser.</param>
        public static void anonymizeUser(bool shouldAnonymizeUser)
        {
            _anonymizeUser(shouldAnonymizeUser);
        }

        /// <summary>
        /// Opt-out for Apple Search Ads attributions.
        /// </summary>
        /// <param name="disableCollectIAd">boolean disableCollectIAd.</param>
        public static void setDisableCollectIAd(bool disableCollectIAd)
        {
            _setDisableCollectIAd(disableCollectIAd);
        }

        /// <summary>
        /// In app purchase receipt validation Apple environment(production or sandbox). The default value is false.
        /// </summary>
        /// <param name="useReceiptValidationSandbox">boolean useReceiptValidationSandbox.</param>
        public static void setUseReceiptValidationSandbox(bool useReceiptValidationSandbox)
        {
            _setUseReceiptValidationSandbox(useReceiptValidationSandbox);
        }

        /// <summary>
        /// Set this flag to test uninstall on Apple environment(production or sandbox). The default value is false.
        /// </summary>
        /// <param name="useUninstallSandbox">boolean useUninstallSandbox.</param>
        public static void setUseUninstallSandbox(bool useUninstallSandbox)
        {
            _setUseUninstallSandbox(useUninstallSandbox);
        }

        /// <summary>
        /// For advertisers who wrap OneLink within another Universal Link.
        /// An advertiser will be able to deeplink from a OneLink wrapped within another Universal Link and also record this retargeting conversion.
        /// </summary>
        /// <param name="resolveDeepLinkURLs">Array of urls.</param>
        public static void setResolveDeepLinkURLs(params string[] resolveDeepLinkURLs)
        {
            _setResolveDeepLinkURLs(resolveDeepLinkURLs.Length,resolveDeepLinkURLs);
        }

        /// <summary>
        /// For advertisers who use vanity OneLinks.
        /// </summary>
        /// <param name="oneLinkCustomDomains">Array of domains.</param>
        public static void setOneLinkCustomDomains(params string[] oneLinkCustomDomains)
        {
            _setOneLinkCustomDomains(oneLinkCustomDomains.Length, oneLinkCustomDomains);
        }

        /// <summary>
        /// Use this to send the user's emails.
        /// </summary>
        /// <param name="cryptType">type Hash algoritm.</param>
        /// <param name="length">length of userEmails array.</param>
        /// <param name="userEmails">userEmails The list of strings that hold mails.</param>
        public static void setUserEmails(EmailCryptType cryptType, int length, params string[] userEmails)
        {
            _setUserEmails(cryptType, length, userEmails);
        }

        /// <summary>
        /// Use this method to send events in your app like purchases or user actions.
        /// </summary>
        /// <param name="eventName">Name of event.</param>
        /// <param name="eventValues">Contains dictionary of values for handling by backend.</param>
        public static void sendEvent(string eventName, Dictionary<string, string> eventValues)
        {
            _sendEvent(eventName, AFMiniJSON.Json.Serialize(eventValues));
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
            _validateAndSendInAppPurchase(productIdentifier, price, currency, tranactionId, AFMiniJSON.Json.Serialize(additionalParameters), gameObject ? gameObject.name : null);
        }

        /// <summary>
        /// To record location for geo-fencing. Does the same as code below.
        /// </summary>
        /// <param name="longitude">The location longitude.</param>
        /// <param name="latitude">The location latitude.</param>
        public static void recordLocation(double longitude, double latitude)
        {
            _recordLocation(longitude, latitude);
        }

        /// <summary>
        /// This method returns AppsFlyer's internal id(unique for your app).
        /// </summary>
        public static string getAppsFlyerId()
        {
            return _getAppsFlyerId();
        }

        /// <summary>
        /// Register uninstall - you should register for remote notification and provide AppsFlyer the push device token.
        /// </summary>
        /// <param name="deviceToken">deviceToken The `deviceToken` from `-application:didRegisterForRemoteNotificationsWithDeviceToken:`.</param>
        public static void registerUninstall(byte[] deviceToken)
        {
            _registerUninstall(deviceToken);
        }

        /// <summary>
        /// Enable AppsFlyer to handle a push notification.
        /// </summary>
        /// <param name="pushPayload">pushPayload The `userInfo` from received remote notification. One of root keys should be @"af"..</param>
        public static void handlePushNotification(Dictionary<string, string> pushPayload)
        {
            _handlePushNotification(AFMiniJSON.Json.Serialize(pushPayload));
        }

        /// <summary>
        /// Get SDK version.
        /// </summary>
        public static string getSDKVersion()
        {
            return _getSDKVersion();
        }

        /// <summary>
        /// This property accepts a string value representing the host name for all endpoints.
        /// Can be used to Zero rate your application’s data usage.Contact your CSM for more information.
        /// </summary>
        /// <param name="host">Host Name.</param>
        /// <param name="host">Host prefix.</param>
        public static void setHost(string host, string hostPrefix)
        {
            _setHost(host, hostPrefix);
        }

        /// <summary>
        /// This property is responsible for timeout between sessions in seconds.
        /// Default value is 5 seconds.
        /// </summary>
        /// <param name="minTimeBetweenSessions">minimum time between 2 separate sessions in seconds.</param>
        public static void setMinTimeBetweenSessions(int minTimeBetweenSessions)
        {
            _setMinTimeBetweenSessions(minTimeBetweenSessions);
        }

        /// <summary>
        /// API to shut down all SDK activities.
        /// </summary>
        /// <param name="isSDKStopped">boolean isSDKStopped.</param>
        public static void stopSDK(bool isSDKStopped)
        {
            _stopSDK(isSDKStopped);
        }

         // <summary>
        /// Was the stopSDK(boolean) API set to true.
        /// </summary>
        /// <returns>boolean isSDKStopped.</returns>
        public static bool isSDKStopped()
        {
            return _isSDKStopped();
        }

        /// <summary>
        /// To record an impression use the following API call.
        /// Make sure to use the promoted App ID as it appears within the AppsFlyer dashboard.
        /// </summary>
        /// <param name="appID">promoted App ID.</param>
        /// <param name="campaign">cross promotion campaign.</param>
        public static void recordCrossPromoteImpression(string appID, string campaign)
        {
            _recordCrossPromoteImpression(appID, campaign);
        }

        /// <summary>
        /// Use the following API to attribute the click and launch the app store's app page.
        /// </summary>
        /// <param name="appID">promoted App ID</param>
        /// <param name="campaign">cross promotion campaign</param>
        /// <param name="parameters">additional user params</param>
        public static void attributeAndOpenStore(string appID, string campaign, Dictionary<string, string> parameters, MonoBehaviour gameObject)
        {
            _attributeAndOpenStore(appID, campaign, AFMiniJSON.Json.Serialize(parameters), gameObject ? gameObject.name : null);
        }

        /// <summary>
        /// The LinkGenerator class builds the invite URL according to various setter methods which allow passing on additional information on the click.
        /// See - https://support.appsflyer.com/hc/en-us/articles/115004480866-User-invite-attribution-
        /// </summary>
        /// <param name="parameters">parameters Dictionary.</param>
        public static void generateUserInviteLink(Dictionary<string, string> parameters, MonoBehaviour gameObject)
        {
            _generateUserInviteLink(AFMiniJSON.Json.Serialize(parameters), gameObject ? gameObject.name : null);
        }

        /// <summary>
        /// It is recommended to generate an in-app event after the invite is sent to track the invites from the senders' perspective. 
        /// This enables you to find the users that tend most to invite friends, and the media sources that get you these users.
        /// </summary>
        /// <param name="channel">channel string.</param>
        /// <param name="parameters">parameters Dictionary..</param>
        public static void trackInvite(string channel, Dictionary<string, string> parameters)
        {
            _trackInvite(channel, AFMiniJSON.Json.Serialize(parameters));
        }


        /*
         * AppsFlyer ios method mapping
         */

        [DllImport("__Internal")]
        private static extern void _startSDK();

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
        private static extern void _sendEvent(string eventName, string eventValues);

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
        private static extern void _recordCrossPromoteImpression(string appID, string campaign);

        [DllImport("__Internal")]
        private static extern void _attributeAndOpenStore(string appID, string campaign, string parameters, string gameObject);

        [DllImport("__Internal")]
        private static extern void _generateUserInviteLink(string parameters, string gameObject);

        [DllImport("__Internal")]
        private static extern void _trackInvite(string channel, string parameters);

    }

#endif


}