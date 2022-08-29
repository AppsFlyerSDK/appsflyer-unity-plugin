using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AppsFlyerSDK
{

    public interface IAppsFlyerNativeBridge
    {
        bool isInit { get; set; }

        void startSDK(bool onRequestResponse, string CallBackObjectName);

        void sendEvent(string eventName, Dictionary<string, string> eventValues, bool onInAppResponse, string CallBackObjectName);

        void stopSDK(bool isSDKStopped);

        bool isSDKStopped();

        string getSdkVersion();

        void setCustomerUserId(string id);

        void setAppInviteOneLinkID(string oneLinkId);

        void setAdditionalData(Dictionary<string, string> customData);

        void setResolveDeepLinkURLs(params string[] urls);

        void setOneLinkCustomDomain(params string[] domains);

        void setCurrencyCode(string currencyCode);

        void recordLocation(double latitude, double longitude);

        void anonymizeUser(bool shouldAnonymizeUser);

        string getAppsFlyerId();

        void setMinTimeBetweenSessions(int seconds);

        void setHost(string hostPrefixName, string hostName);

        void setPhoneNumber(string phoneNumber);

        void setSharingFilterForAllPartners();

        void setSharingFilter(params string[] partners);

        void getConversionData(string objectName);

        void attributeAndOpenStore(string appID, string campaign, Dictionary<string, string> userParams, MonoBehaviour gameObject);

        void recordCrossPromoteImpression(string appID, string campaign, Dictionary<string, string> parameters);

        void generateUserInviteLink(Dictionary<string, string> parameters, MonoBehaviour gameObject);

        void addPushNotificationDeepLinkPath(params string[] paths);

        void setUserEmails(EmailCryptType cryptType, params string[] userEmails);

        void subscribeForDeepLink(string objectName);

        void setIsDebug(bool shouldEnable);

        void setPartnerData(string partnerId, Dictionary<string, string> partnerInfo);
    }
}
