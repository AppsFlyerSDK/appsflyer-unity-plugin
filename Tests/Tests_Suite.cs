using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using NSubstitute;

namespace AppsFlyerSDK.Tests
{
    public class NewTestScript
    {

        [Test]
        public void test_startSDK_called()
        {
            var AppsFlyerMOCKInterface = Substitute.For<IAppsFlyerNativeBridge>();
            AppsFlyer.instance = AppsFlyerMOCKInterface;
            AppsFlyer.startSDK();
            AppsFlyerMOCKInterface.Received().startSDK(Arg.Any<bool>(), Arg.Any<string>());

        }

        [Test]
        public void test_sendEvent_withValues()
        {
            var AppsFlyerMOCKInterface = Substitute.For<IAppsFlyerNativeBridge>();
            AppsFlyer.instance = AppsFlyerMOCKInterface;
            var eventParams = new Dictionary<string, string>();
            eventParams.Add("key", "value");
            AppsFlyer.sendEvent("testevent", eventParams);
            AppsFlyerMOCKInterface.Received().sendEvent("testevent", eventParams, false, null);
        }

        [Test]
        public void test_sendEvent_withNullParams()
        {
            var AppsFlyerMOCKInterface = Substitute.For<IAppsFlyerNativeBridge>();
            AppsFlyer.instance = AppsFlyerMOCKInterface;
            AppsFlyer.sendEvent("testevent", null);
            AppsFlyerMOCKInterface.Received().sendEvent("testevent", null,false, null);
        }


        [Test]
        public void test_isSDKStopped_true()
        {
            var AppsFlyerMOCKInterface = Substitute.For<IAppsFlyerNativeBridge>();
            AppsFlyer.instance = AppsFlyerMOCKInterface;
            AppsFlyer.stopSDK(true);

            AppsFlyerMOCKInterface.Received().stopSDK(true);

        }

        [Test]
        public void test_isSDKStopped_false()
        {
            var AppsFlyerMOCKInterface = Substitute.For<IAppsFlyerNativeBridge>();

            AppsFlyer.instance = AppsFlyerMOCKInterface;

            AppsFlyer.stopSDK(false);

            AppsFlyerMOCKInterface.Received().stopSDK(false);

        }

        [Test]
        public void test_isSDKStopped_called()
        {
            var AppsFlyerMOCKInterface = Substitute.For<IAppsFlyerNativeBridge>();

            AppsFlyer.instance = AppsFlyerMOCKInterface;

            var isSDKStopped = AppsFlyer.isSDKStopped();

            AppsFlyerMOCKInterface.Received().isSDKStopped();

        }

        [Test]
        public void test_isSDKStopped_receveivedFalse()
        {
            var AppsFlyerMOCKInterface = Substitute.For<IAppsFlyerNativeBridge>();

            AppsFlyer.instance = AppsFlyerMOCKInterface;

            var isSDKStopped = AppsFlyer.isSDKStopped();

            Assert.AreEqual(AppsFlyerMOCKInterface.Received().isSDKStopped(), false);


        }


        [Test]
        public void test_getSdkVersion_called()
        {
            var AppsFlyerMOCKInterface = Substitute.For<IAppsFlyerNativeBridge>();
            AppsFlyer.instance = AppsFlyerMOCKInterface;
            AppsFlyer.getSdkVersion();
            AppsFlyerMOCKInterface.Received().getSdkVersion();
        }

        [Test]
        public void test_setCustomerUserId_called()
        {

            var AppsFlyerMOCKInterface = Substitute.For<IAppsFlyerNativeBridge>();

            AppsFlyer.instance = AppsFlyerMOCKInterface;
            AppsFlyer.setCustomerUserId("test");
            AppsFlyerMOCKInterface.Received().setCustomerUserId("test");

        }

        [Test]
        public void Test_setAppInviteOneLinkID_called()
        {

            var AppsFlyerMOCKInterface = Substitute.For<IAppsFlyerNativeBridge>();

            AppsFlyer.instance = AppsFlyerMOCKInterface;
            AppsFlyer.setAppInviteOneLinkID("2f36");
            AppsFlyerMOCKInterface.Received().setAppInviteOneLinkID("2f36");

        }

        [Test]
        public void Test_setAdditionalData_called()
        {

            var AppsFlyerMOCKInterface = Substitute.For<IAppsFlyerNativeBridge>();

            AppsFlyer.instance = AppsFlyerMOCKInterface;
            var customData = new Dictionary<string, string>();
            customData.Add("test", "test");
            AppsFlyer.setAdditionalData(customData);
            AppsFlyerMOCKInterface.Received().setAdditionalData(customData);
        }

        [Test]
        public void Test_setResolveDeepLinkURLs_called()
        {

            var AppsFlyerMOCKInterface = Substitute.For<IAppsFlyerNativeBridge>();

            AppsFlyer.instance = AppsFlyerMOCKInterface;
            AppsFlyer.setResolveDeepLinkURLs("url1", "url2");
            AppsFlyerMOCKInterface.Received().setResolveDeepLinkURLs("url1", "url2");

        }

        [Test]
        public void Test_setOneLinkCustomDomain_called()
        {

            var AppsFlyerMOCKInterface = Substitute.For<IAppsFlyerNativeBridge>();

            AppsFlyer.instance = AppsFlyerMOCKInterface;
            AppsFlyer.setOneLinkCustomDomain("url1", "url2");
            AppsFlyerMOCKInterface.Received().setOneLinkCustomDomain("url1", "url2");

        }

        [Test]
        public void Test_setCurrencyCode_called()
        {

            var AppsFlyerMOCKInterface = Substitute.For<IAppsFlyerNativeBridge>();

            AppsFlyer.instance = AppsFlyerMOCKInterface;
            AppsFlyer.setCurrencyCode("usd");
            AppsFlyerMOCKInterface.Received().setCurrencyCode("usd");

        }

        [Test]
        public void Test_recordLocation_called()
        {

            var AppsFlyerMOCKInterface = Substitute.For<IAppsFlyerNativeBridge>();

            AppsFlyer.instance = AppsFlyerMOCKInterface;
            AppsFlyer.recordLocation(0.3, 5.2);
            AppsFlyerMOCKInterface.Received().recordLocation(0.3, 5.2);

        }

        [Test]
        public void Test_anonymizeUser_true()
        {

            var AppsFlyerMOCKInterface = Substitute.For<IAppsFlyerNativeBridge>();

            AppsFlyer.instance = AppsFlyerMOCKInterface;
            AppsFlyer.anonymizeUser(true);
            AppsFlyerMOCKInterface.Received().anonymizeUser(true);

        }

        [Test]
        public void Test_anonymizeUser_false()
        {

            var AppsFlyerMOCKInterface = Substitute.For<IAppsFlyerNativeBridge>();

            AppsFlyer.instance = AppsFlyerMOCKInterface;
            AppsFlyer.anonymizeUser(false);
            AppsFlyerMOCKInterface.Received().anonymizeUser(false);

        }

        [Test]
        public void Test_getAppsFlyerId_called()
        {

            var AppsFlyerMOCKInterface = Substitute.For<IAppsFlyerNativeBridge>();

            AppsFlyer.instance = AppsFlyerMOCKInterface;
            AppsFlyer.getAppsFlyerId();
            AppsFlyerMOCKInterface.Received().getAppsFlyerId();

        }

        [Test]
        public void Test_setMinTimeBetweenSessions_called()
        {

            var AppsFlyerMOCKInterface = Substitute.For<IAppsFlyerNativeBridge>();

            AppsFlyer.instance = AppsFlyerMOCKInterface;
            AppsFlyer.setMinTimeBetweenSessions(3);
            AppsFlyerMOCKInterface.Received().setMinTimeBetweenSessions(3);

        }

        [Test]
        public void Test_setHost_called()
        {

            var AppsFlyerMOCKInterface = Substitute.For<IAppsFlyerNativeBridge>();

            AppsFlyer.instance = AppsFlyerMOCKInterface;
            AppsFlyer.setHost("prefix", "name");
            AppsFlyerMOCKInterface.Received().setHost("prefix", "name");

        }

        [Test]
        public void Test_setPhoneNumber_called()
        {

            var AppsFlyerMOCKInterface = Substitute.For<IAppsFlyerNativeBridge>();

            AppsFlyer.instance = AppsFlyerMOCKInterface;
            AppsFlyer.setPhoneNumber("002");
            AppsFlyerMOCKInterface.Received().setPhoneNumber("002");

        }


        [Test]
        [System.Obsolete]
        public void Test_setSharingFilterForAllPartners_called()
        {

            var AppsFlyerMOCKInterface = Substitute.For<IAppsFlyerNativeBridge>();

            AppsFlyer.instance = AppsFlyerMOCKInterface;
            AppsFlyer.setSharingFilterForAllPartners();
            AppsFlyerMOCKInterface.Received().setSharingFilterForAllPartners();

        }

        [Test]
        [System.Obsolete]
        public void Test_setSharingFilter_called()
        {

            var AppsFlyerMOCKInterface = Substitute.For<IAppsFlyerNativeBridge>();

            AppsFlyer.instance = AppsFlyerMOCKInterface;


            AppsFlyer.setSharingFilter("filter1", "filter2");
            AppsFlyerMOCKInterface.Received().setSharingFilter("filter1", "filter2");

        }

        [Test]
        public void Test_getConversionData_called()
        {

            var AppsFlyerMOCKInterface = Substitute.For<IAppsFlyerNativeBridge>();

            AppsFlyer.instance = AppsFlyerMOCKInterface;

            AppsFlyer.getConversionData("ObjectName");
            AppsFlyerMOCKInterface.Received().getConversionData("ObjectName");

        }

        [Test]
        public void Test_attributeAndOpenStore_called_withParams()
        {

            var AppsFlyerMOCKInterface = Substitute.For<IAppsFlyerNativeBridge>();

            AppsFlyer.instance = AppsFlyerMOCKInterface;
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("af_sub1", "val");
            parameters.Add("custom_param", "val2");
            AppsFlyer.attributeAndOpenStore("appid", "campaign", parameters, new MonoBehaviour());
            AppsFlyerMOCKInterface.Received().attributeAndOpenStore("appid", "campaign", parameters, Arg.Any<MonoBehaviour>());

        }

        [Test]
        public void Test_attributeAndOpenStore_called_nullParams()
        {

            var AppsFlyerMOCKInterface = Substitute.For<IAppsFlyerNativeBridge>();

            AppsFlyer.instance = AppsFlyerMOCKInterface;

            AppsFlyer.attributeAndOpenStore("appid", "campaign", null, new MonoBehaviour());
            AppsFlyerMOCKInterface.Received().attributeAndOpenStore("appid", "campaign", null, Arg.Any<MonoBehaviour>());

        }

        [Test]
        public void Test_recordCrossPromoteImpression_calledWithParameters()
        {

            var AppsFlyerMOCKInterface = Substitute.For<IAppsFlyerNativeBridge>();

            AppsFlyer.instance = AppsFlyerMOCKInterface;
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("af_sub1", "val");
            parameters.Add("custom_param", "val2");
            AppsFlyer.recordCrossPromoteImpression("appid", "campaign", parameters);
            AppsFlyerMOCKInterface.Received().recordCrossPromoteImpression("appid", "campaign", parameters);

        }



        [Test]
        public void Test_recordCrossPromoteImpression_calledWithoutParameters()
        {

            var AppsFlyerMOCKInterface = Substitute.For<IAppsFlyerNativeBridge>();

            AppsFlyer.instance = AppsFlyerMOCKInterface;
            AppsFlyer.recordCrossPromoteImpression("appid", "campaign", null);
            AppsFlyerMOCKInterface.Received().recordCrossPromoteImpression("appid", "campaign", null);

        }

        [Test]
        public void Test_generateUserInviteLink_called()
        {

            var AppsFlyerMOCKInterface = Substitute.For<IAppsFlyerNativeBridge>();

            AppsFlyer.instance = AppsFlyerMOCKInterface;

            AppsFlyer.generateUserInviteLink(new Dictionary<string, string>(), new MonoBehaviour());
            AppsFlyerMOCKInterface.Received().generateUserInviteLink(Arg.Any<Dictionary<string, string>>(), Arg.Any<MonoBehaviour>());

        }


        [Test]
        public void Test_addPushNotificationDeepLinkPath_called()
        {

            var AppsFlyerMOCKInterface = Substitute.For<IAppsFlyerNativeBridge>();

            AppsFlyer.instance = AppsFlyerMOCKInterface;
            AppsFlyer.addPushNotificationDeepLinkPath("path1", "path2");
            AppsFlyerMOCKInterface.Received().addPushNotificationDeepLinkPath("path1", "path2");

        }

#if UNITY_ANDROID
        [Test]
        public void updateServerUninstallToken_called()
        {

            var AppsFlyerMOCKInterface = Substitute.For<IAppsFlyerAndroidBridge>();

            AppsFlyer.instance = AppsFlyerMOCKInterface;
            AppsFlyer.updateServerUninstallToken("tokenTest");
            AppsFlyerMOCKInterface.Received().updateServerUninstallToken("tokenTest");

        }

        [Test]
        public void setImeiData_called()
        {

            var AppsFlyerMOCKInterface = Substitute.For<IAppsFlyerAndroidBridge>();

            AppsFlyer.instance = AppsFlyerMOCKInterface;
            AppsFlyer.setImeiData("imei");
            AppsFlyerMOCKInterface.Received().setImeiData("imei");

        }

        [Test]
        public void setAndroidIdData_called()
        {

            var AppsFlyerMOCKInterface = Substitute.For<IAppsFlyerAndroidBridge>();

            AppsFlyer.instance = AppsFlyerMOCKInterface;
            AppsFlyer.setAndroidIdData("androidId");
            AppsFlyerMOCKInterface.Received().setAndroidIdData("androidId");

        }

        [Test]
        public void waitForCustomerUserId_called()
        {

            var AppsFlyerMOCKInterface = Substitute.For<IAppsFlyerAndroidBridge>();

            AppsFlyer.instance = AppsFlyerMOCKInterface;
            AppsFlyer.waitForCustomerUserId(true);
            AppsFlyerMOCKInterface.Received().waitForCustomerUserId(true);

        }

        [Test]
        public void setCustomerIdAndStartSDK_called()
        {

            var AppsFlyerMOCKInterface = Substitute.For<IAppsFlyerAndroidBridge>();

            AppsFlyer.instance = AppsFlyerMOCKInterface;
            AppsFlyer.setCustomerIdAndStartSDK("01234");
            AppsFlyerMOCKInterface.Received().setCustomerIdAndStartSDK("01234");

        }


        [Test]
        public void getOutOfStore_called()
        {

            var AppsFlyerMOCKInterface = Substitute.For<IAppsFlyerAndroidBridge>();

            AppsFlyer.instance = AppsFlyerMOCKInterface;
            AppsFlyer.getOutOfStore();
            AppsFlyerMOCKInterface.Received().getOutOfStore();

        }

        [Test]
        public void setOutOfStore_called()
        {

            var AppsFlyerMOCKInterface = Substitute.For<IAppsFlyerAndroidBridge>();

            AppsFlyer.instance = AppsFlyerMOCKInterface;
            AppsFlyer.setOutOfStore("test");
            AppsFlyerMOCKInterface.Received().setOutOfStore("test");

        }

        [Test]
        public void setCollectAndroidID_called()
        {

            var AppsFlyerMOCKInterface = Substitute.For<IAppsFlyerAndroidBridge>();

            AppsFlyer.instance = AppsFlyerMOCKInterface;
            AppsFlyer.setCollectAndroidID(true);
            AppsFlyerMOCKInterface.Received().setCollectAndroidID(true);

        }

        [Test]
        public void setCollectIMEI_called()
        {

            var AppsFlyerMOCKInterface = Substitute.For<IAppsFlyerAndroidBridge>();

            AppsFlyer.instance = AppsFlyerMOCKInterface;
            AppsFlyer.setCollectIMEI(true);
            AppsFlyerMOCKInterface.Received().setCollectIMEI(true);

        }

        [Test]
        public void setIsUpdate_called()
        {

            var AppsFlyerMOCKInterface = Substitute.For<IAppsFlyerAndroidBridge>();

            AppsFlyer.instance = AppsFlyerMOCKInterface;
            AppsFlyer.setIsUpdate(true);
            AppsFlyerMOCKInterface.Received().setIsUpdate(true);

        }

        [Test]
        public void setPreinstallAttribution_called()
        {

            var AppsFlyerMOCKInterface = Substitute.For<IAppsFlyerAndroidBridge>();

            AppsFlyer.instance = AppsFlyerMOCKInterface;
            AppsFlyer.setPreinstallAttribution("mediaSourceTestt", "campaign", "sideId");
            AppsFlyerMOCKInterface.Received().setPreinstallAttribution("mediaSourceTestt", "campaign", "sideId");

        }

        [Test]
        public void isPreInstalledApp_called()
        {

            var AppsFlyerMOCKInterface = Substitute.For<IAppsFlyerAndroidBridge>();

            AppsFlyer.instance = AppsFlyerMOCKInterface;
            AppsFlyer.isPreInstalledApp();
            AppsFlyerMOCKInterface.Received().isPreInstalledApp();

        }

        [Test]
        public void getAttributionId_called()
        {

            var AppsFlyerMOCKInterface = Substitute.For<IAppsFlyerAndroidBridge>();

            AppsFlyer.instance = AppsFlyerMOCKInterface;
            AppsFlyer.getAttributionId();
            AppsFlyerMOCKInterface.Received().getAttributionId();

        }

        [Test]
        public void handlePushNotifications_called()
        {

            var AppsFlyerMOCKInterface = Substitute.For<IAppsFlyerAndroidBridge>();

            AppsFlyer.instance = AppsFlyerMOCKInterface;
            AppsFlyer.handlePushNotifications();
            AppsFlyerMOCKInterface.Received().handlePushNotifications();

        }

        [Test]
        public void validateAndSendInAppPurchase_called()
        {

            var AppsFlyerMOCKInterface = Substitute.For<IAppsFlyerAndroidBridge>();

            AppsFlyer.instance = AppsFlyerMOCKInterface;
            AppsFlyer.validateAndSendInAppPurchase("ewjkekwjekw","hewjehwj", "purchaseData", "3.0", "USD", null, null);
            AppsFlyerMOCKInterface.Received().validateAndSendInAppPurchase("ewjkekwjekw", "hewjehwj", "purchaseData", "3.0", "USD", null, null);

        }

        [Test]
        public void setCollectOaid_called()
        {

            var AppsFlyerMOCKInterface = Substitute.For<IAppsFlyerAndroidBridge>();

            AppsFlyer.instance = AppsFlyerMOCKInterface;
            AppsFlyer.setCollectOaid(true);
            AppsFlyerMOCKInterface.Received().setCollectOaid(true);

        }

        [Test]
        public void setDisableAdvertisingIdentifiers_called()
        {

            var AppsFlyerMOCKInterface = Substitute.For<IAppsFlyerAndroidBridge>();

            AppsFlyer.instance = AppsFlyerMOCKInterface;
            AppsFlyer.setDisableAdvertisingIdentifiers(true);
            AppsFlyerMOCKInterface.Received().setDisableAdvertisingIdentifiers(true);

        }

        [Test]
        public void setDisableNetworkData_called() {
            var AppsFlyerMOCKInterface = Substitute.For<IAppsFlyerAndroidBridge>();

            AppsFlyer.instance = AppsFlyerMOCKInterface;
            AppsFlyer.setDisableNetworkData(true);
            AppsFlyerMOCKInterface.Received().setDisableNetworkData(true);
        }
#elif UNITY_IOS

        [Test]
        public void setDisableCollectAppleAdSupport_called_true()
        {

            var AppsFlyerMOCKInterface = Substitute.For<IAppsFlyerIOSBridge>();

            AppsFlyer.instance = AppsFlyerMOCKInterface;
            AppsFlyer.setDisableCollectAppleAdSupport(true);
            AppsFlyerMOCKInterface.Received().setDisableCollectAppleAdSupport(true);

        }

        [Test]
        public void setDisableCollectAppleAdSupport_called_false()
        {

            var AppsFlyerMOCKInterface = Substitute.For<IAppsFlyerIOSBridge>();

            AppsFlyer.instance = AppsFlyerMOCKInterface;
            AppsFlyer.setDisableCollectAppleAdSupport(false);
            AppsFlyerMOCKInterface.Received().setDisableCollectAppleAdSupport(false);

        }

        [Test]
        [System.Obsolete]
        public void setShouldCollectDeviceName_called_true()
        {

            var AppsFlyerMOCKInterface = Substitute.For<IAppsFlyerIOSBridge>();

            AppsFlyer.instance = AppsFlyerMOCKInterface;
            AppsFlyer.setShouldCollectDeviceName(true);
            AppsFlyerMOCKInterface.Received().setShouldCollectDeviceName(true);

        }

        [Test]
        [System.Obsolete]
        public void setShouldCollectDeviceName_called_false()
        {

            var AppsFlyerMOCKInterface = Substitute.For<IAppsFlyerIOSBridge>();

            AppsFlyer.instance = AppsFlyerMOCKInterface;
            AppsFlyer.setShouldCollectDeviceName(false);
            AppsFlyerMOCKInterface.Received().setShouldCollectDeviceName(false);

        }

        [Test]
        public void setDisableCollectIAd_called_true()
        {

            var AppsFlyerMOCKInterface = Substitute.For<IAppsFlyerIOSBridge>();

            AppsFlyer.instance = AppsFlyerMOCKInterface;
            AppsFlyer.setDisableCollectIAd(true);
            AppsFlyerMOCKInterface.Received().setDisableCollectIAd(true);

        }

        [Test]
        public void setDisableCollectIAd_called_false()
        {

            var AppsFlyerMOCKInterface = Substitute.For<IAppsFlyerIOSBridge>();

            AppsFlyer.instance = AppsFlyerMOCKInterface;
            AppsFlyer.setDisableCollectIAd(false);
            AppsFlyerMOCKInterface.Received().setDisableCollectIAd(false);

        }

        [Test]
        public void setUseReceiptValidationSandbox_called_true()
        {

            var AppsFlyerMOCKInterface = Substitute.For<IAppsFlyerIOSBridge>();

            AppsFlyer.instance = AppsFlyerMOCKInterface;
            AppsFlyer.setUseReceiptValidationSandbox(true);
            AppsFlyerMOCKInterface.Received().setUseReceiptValidationSandbox(true);

        }

        [Test]
        public void setUseReceiptValidationSandbox_called_false()
        {

            var AppsFlyerMOCKInterface = Substitute.For<IAppsFlyerIOSBridge>();

            AppsFlyer.instance = AppsFlyerMOCKInterface;
            AppsFlyer.setUseReceiptValidationSandbox(false);
            AppsFlyerMOCKInterface.Received().setUseReceiptValidationSandbox(false);

        }

        [Test]
        public void ssetUseUninstallSandbox_called_true()
        {

            var AppsFlyerMOCKInterface = Substitute.For<IAppsFlyerIOSBridge>();

            AppsFlyer.instance = AppsFlyerMOCKInterface;
            AppsFlyer.setUseUninstallSandbox(true);
            AppsFlyerMOCKInterface.Received().setUseUninstallSandbox(true);

        }

        [Test]
        public void setUseUninstallSandbox_called_false()
        {

            var AppsFlyerMOCKInterface = Substitute.For<IAppsFlyerIOSBridge>();

            AppsFlyer.instance = AppsFlyerMOCKInterface;
            AppsFlyer.setUseUninstallSandbox(false);
            AppsFlyerMOCKInterface.Received().setUseUninstallSandbox(false);

        }

        [Test]
        public void validateAndSendInAppPurchase_called()
        {

            var AppsFlyerMOCKInterface = Substitute.For<IAppsFlyerIOSBridge>();

            AppsFlyer.instance = AppsFlyerMOCKInterface;
            AppsFlyer.validateAndSendInAppPurchase("3d2", "5.0","USD", "45", null, null);
            AppsFlyerMOCKInterface.Received().validateAndSendInAppPurchase("3d2", "5.0", "USD", "45", null, null);

        }

        [Test]
        public void registerUninstall_called()
        {

            var AppsFlyerMOCKInterface = Substitute.For<IAppsFlyerIOSBridge>();

            AppsFlyer.instance = AppsFlyerMOCKInterface;
            byte[] token = System.Text.Encoding.UTF8.GetBytes("740f4707 bebcf74f 9b7c25d4 8e335894 5f6aa01d a5ddb387 462c7eaf 61bb78ad");
            AppsFlyer.registerUninstall(token);
            AppsFlyerMOCKInterface.Received().registerUninstall(token);

        }

        [Test]
        public void handleOpenUrl_called()
        {

            var AppsFlyerMOCKInterface = Substitute.For<IAppsFlyerIOSBridge>();

            AppsFlyer.instance = AppsFlyerMOCKInterface;
            AppsFlyer.handleOpenUrl("www.test.com", "appTest", "test");
            AppsFlyerMOCKInterface.Received().handleOpenUrl("www.test.com", "appTest", "test");

        }

        [Test]
        public void waitForATTUserAuthorizationWithTimeoutInterval_called()
        {

            var AppsFlyerMOCKInterface = Substitute.For<IAppsFlyerIOSBridge>();

            AppsFlyer.instance = AppsFlyerMOCKInterface;
            AppsFlyer.waitForATTUserAuthorizationWithTimeoutInterval(30);
            AppsFlyerMOCKInterface.Received().waitForATTUserAuthorizationWithTimeoutInterval(30);

        }

        [Test]
        public void setCurrentDeviceLanguage_called()
        {

            var AppsFlyerMOCKInterface = Substitute.For<IAppsFlyerIOSBridge>();

            AppsFlyer.instance = AppsFlyerMOCKInterface;
            AppsFlyer.setCurrentDeviceLanguage("en");
            AppsFlyerMOCKInterface.Received().setCurrentDeviceLanguage("en");

        }

        [Test]
        public void disableSKAdNetwork_called_true()
        {

            var AppsFlyerMOCKInterface = Substitute.For<IAppsFlyerIOSBridge>();

            AppsFlyer.instance = AppsFlyerMOCKInterface;
            AppsFlyer.disableSKAdNetwork(true);
            AppsFlyerMOCKInterface.Received().disableSKAdNetwork(true);

        }

        [Test]
        public void disableSKAdNetwork_called_false()
        {

            var AppsFlyerMOCKInterface = Substitute.For<IAppsFlyerIOSBridge>();

            AppsFlyer.instance = AppsFlyerMOCKInterface;
            AppsFlyer.disableSKAdNetwork(false);
            AppsFlyerMOCKInterface.Received().disableSKAdNetwork(false);

        }

#endif
    }
}
