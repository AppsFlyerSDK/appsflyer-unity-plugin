using System.Collections.Generic;
using UnityEngine;

namespace AppsFlyerSDK
{
    public class AppsFlyerDummy : IAppsFlyerNativeBridge
    {
        public bool isInit { get; set; }
        public void initSDK(string devKey, string appID, MonoBehaviour gameObject)
        {
            // ...
        }

        public void startSDK(bool onRequestResponse, string CallBackObjectName)
        {
            // ...
        }

        public void sendEvent(string eventName, Dictionary<string, string> eventValues, bool onInAppResponse, string CallBackObjectName)
        {
            // ...
        }

        public void stopSDK(bool isSDKStopped)
        {
            // ...
        }

        public bool isSDKStopped()
        {
            // ...
            return true;
        }

        public string getSdkVersion()
        {
            // ...
            return "";
        }

        public void setCustomerUserId(string id)
        {
            // ...
        }

        public void setAppInviteOneLinkID(string oneLinkId)
        {
            // ...
        }

        public void setAdditionalData(Dictionary<string, string> customData)
        {
            // ...
        }

        public void setResolveDeepLinkURLs(params string[] urls)
        {
            // ...
        }

        public void setOneLinkCustomDomain(params string[] domains)
        {
            // ...
        }

        public void setCurrencyCode(string currencyCode)
        {
            // ...
        }

        public void recordLocation(double latitude, double longitude)
        {
            // ...
        }

        public void anonymizeUser(bool shouldAnonymizeUser)
        {
            // ...
        }

        public string getAppsFlyerId()
        {
            // ...
            return "";
        }

        public void setMinTimeBetweenSessions(int seconds)
        {
            // ...
        }

        public void setHost(string hostPrefixName, string hostName)
        {
            // ...
        }

        public void setPhoneNumber(string phoneNumber)
        {
            // ...
        }

        public void setSharingFilterForAllPartners()
        {
            // ...
        }

        public void setSharingFilter(params string[] partners)
        {
            // ...
        }

        public void getConversionData(string objectName)
        {
            // ...
        }

        public void attributeAndOpenStore(string appID, string campaign, Dictionary<string, string> userParams, MonoBehaviour gameObject)
        {
            // ...
        }

        public void recordCrossPromoteImpression(string appID, string campaign, Dictionary<string, string> parameters)
        {
            // ...
        }

        public void generateUserInviteLink(Dictionary<string, string> parameters, MonoBehaviour gameObject)
        {
            // ...
        }

        public void addPushNotificationDeepLinkPath(params string[] paths)
        {
            // ...
        }

        public void setUserEmails(EmailCryptType cryptType, params string[] userEmails)
        {
            // ...
        }

        public void subscribeForDeepLink(string objectName)
        {
            // ...
        }

        public void setIsDebug(bool shouldEnable)
        {
            // ...
        }

        public void setPartnerData(string partnerId, Dictionary<string, string> partnerInfo)
        {
            // ...
        }
    }
}