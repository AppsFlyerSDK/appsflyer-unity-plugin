using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AppsFlyerSDK
{

    public interface IAppsFlyerAndroidBridge : IAppsFlyerNativeBridge
    {
        void updateServerUninstallToken(string token);
        void setImeiData(string imei);
        void setAndroidIdData(string androidId);
        void waitForCustomerUserId(bool wait);
        void setCustomerIdAndStartSDK(string id);
        string getOutOfStore();
        void setOutOfStore(string sourceName);
        void setCollectAndroidID(bool isCollect);
        void setCollectIMEI(bool isCollect);
        void setIsUpdate(bool isUpdate);
        void setPreinstallAttribution(string mediaSource, string campaign, string siteId);
        bool isPreInstalledApp();
        string getAttributionId();
        void handlePushNotifications();
        void validateAndSendInAppPurchase(string publicKey, string signature, string purchaseData, string price, string currency, Dictionary<string, string> additionalParameters, MonoBehaviour gameObject);
        void validateAndSendInAppPurchase(AFPurchaseDetailsAndroid details, Dictionary<string, string> additionalParameters, MonoBehaviour gameObject);
        void setCollectOaid(bool isCollect);
        void setDisableAdvertisingIdentifiers(bool disable);
        void setDisableNetworkData(bool disable);

    }
}