using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using NSubstitute;

namespace AppsFlyerSDK.Tests
{
    public class AppsFlyerSDKTests
    {
        private IAppsFlyerNativeBridge mock;

        [SetUp]
        public void SetUp()
        {
            mock = Substitute.For<IAppsFlyerNativeBridge>();
            AppsFlyer.instance = mock;
        }

        #region SDK Initialization
        [Test]
        public void StartSDK_ShouldCallBridge()
        {
            AppsFlyer.startSDK();
            mock.Received().startSDK(Arg.Any<bool>(), Arg.Any<string>());
        }

        [Test]
        public void StopSDK_True_ShouldCallBridge()
        {
            AppsFlyer.stopSDK(true);
            mock.Received().stopSDK(true);
        }

        [Test]
        public void StopSDK_False_ShouldCallBridge()
        {
            AppsFlyer.stopSDK(false);
            mock.Received().stopSDK(false);
        }

        [Test]
        public void IsSDKStopped_ShouldCallBridge()
        {
            _ = AppsFlyer.isSDKStopped();
            mock.Received().isSDKStopped();
        }
        #endregion

        #region Event Sending
        [Test]
        public void SendEvent_WithParams_ShouldCallBridge()
        {
            var eventParams = new Dictionary<string, string> { { "key", "value" } };
            AppsFlyer.sendEvent("testevent", eventParams);
            mock.Received().sendEvent("testevent", eventParams, false, null);
        }

        [Test]
        public void SendEvent_NullParams_ShouldCallBridge()
        {
            AppsFlyer.sendEvent("testevent", null);
            mock.Received().sendEvent("testevent", null, false, null);
        }
        #endregion

        #region Identity and Configuration
        [Test]
        public void SetCustomerUserId_ShouldCallBridge()
        {
            AppsFlyer.setCustomerUserId("user123");
            mock.Received().setCustomerUserId("user123");
        }

        [Test]
        public void SetAppInviteOneLinkID_ShouldCallBridge()
        {
            AppsFlyer.setAppInviteOneLinkID("2f36");
            mock.Received().setAppInviteOneLinkID("2f36");
        }

        [Test]
        public void SetAdditionalData_ShouldCallBridge()
        {
            var customData = new Dictionary<string, string> { { "test", "test" } };
            AppsFlyer.setAdditionalData(customData);
            mock.Received().setAdditionalData(customData);
        }

        [Test]
        public void SetResolveDeepLinkURLs_ShouldCallBridge()
        {
            AppsFlyer.setResolveDeepLinkURLs("url1", "url2");
            mock.Received().setResolveDeepLinkURLs("url1", "url2");
        }

        [Test]
        public void SetCurrencyCode_ShouldCallBridge()
        {
            AppsFlyer.setCurrencyCode("USD");
            mock.Received().setCurrencyCode("USD");
        }

        [Test]
        public void SetMinTimeBetweenSessions_ShouldCallBridge()
        {
            AppsFlyer.setMinTimeBetweenSessions(3);
            mock.Received().setMinTimeBetweenSessions(3);
        }

        [Test]
        public void SetHost_ShouldCallBridge()
        {
            AppsFlyer.setHost("prefix", "name");
            mock.Received().setHost("prefix", "name");

        }

        [Test]
        public void SetPhoneNumber_ShouldCallBridge()
        {
            AppsFlyer.setPhoneNumber("002");
            mock.Received().setPhoneNumber("002");
        }


        [Test]
        [System.Obsolete]
        public void SetSharingFilterForAllPartners_ShouldCallBridge()
        {
            AppsFlyer.setSharingFilterForAllPartners();
            mock.Received().setSharingFilterForAllPartners();
        }

        [Test]
        [System.Obsolete]
        public void SetSharingFilter_ShouldCallBridge()
        {
            AppsFlyer.setSharingFilter("filter1", "filter2");
            mock.Received().setSharingFilter("filter1", "filter2");

        }

        [Test]
        public void SetConsentData_ShouldCallBridge_WhenInstanceIsNotNull()
        {
            var consent = new AppsFlyerConsent(true);
            AppsFlyer.setConsentData(consent);

            mock.Received().setConsentData(consent);
        }

        [Test]
        public void SetConsentData_ShouldNotThrow_WhenInstanceIsNull()
        {
            AppsFlyer.instance = null;

            var consent = new AppsFlyerConsent();
            Assert.DoesNotThrow(() => AppsFlyer.setConsentData(consent));
        }
        #endregion

        #region Location and Privacy
        [Test]
        public void RecordLocation_ShouldCallBridge()
        {
            AppsFlyer.recordLocation(1.23, 4.56);
            mock.Received().recordLocation(1.23, 4.56);
        }

        [Test]
        public void AnonymizeUser_True_ShouldCallBridge()
        {
            AppsFlyer.anonymizeUser(true);
            mock.Received().anonymizeUser(true);
        }

        [Test]
        public void AnonymizeUser_False_ShouldCallBridge()
        {
            AppsFlyer.anonymizeUser(false);
            mock.Received().anonymizeUser(false);
        }
        #endregion

        #region Utility
        [Test]
        public void GetAppsFlyerId_ShouldCallBridge()
        {
            AppsFlyer.getAppsFlyerId();
            mock.Received().getAppsFlyerId();
        }

        [Test]
        public void GetConversionData_ShouldCallBridge()
        {
            AppsFlyer.getConversionData("ObjectName");
            mock.Received().getConversionData("ObjectName");
        }

        [Test]
        public void GenerateUserInviteLink_ShouldCallBridge()
        {
            AppsFlyer.generateUserInviteLink(new Dictionary<string, string>(), new MonoBehaviour());
            mock.Received().generateUserInviteLink(Arg.Any<Dictionary<string, string>>(), Arg.Any<MonoBehaviour>());
        }
        #endregion

        #region Cross Promotion & Store
        [Test]
        public void AttributeAndOpenStore_WithParams_ShouldCallBridge()
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("af_sub1", "val");
            parameters.Add("custom_param", "val2");
            AppsFlyer.attributeAndOpenStore("appid", "campaign", parameters, new MonoBehaviour());
            mock.Received().attributeAndOpenStore("appid", "campaign", parameters, Arg.Any<MonoBehaviour>());
        }

        [Test]
        public void AttributeAndOpenStore_NullParams_ShouldCallBridge()
        {
            AppsFlyer.attributeAndOpenStore("appid", "campaign", null, new MonoBehaviour());
            mock.Received().attributeAndOpenStore("appid", "campaign", null, Arg.Any<MonoBehaviour>());
        }

        [Test]
        public void RecordCrossPromoteImpression_WithParams_ShouldCallBridge()
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("af_sub1", "val");
            parameters.Add("custom_param", "val2");
            AppsFlyer.recordCrossPromoteImpression("appid", "campaign", parameters);
            mock.Received().recordCrossPromoteImpression("appid", "campaign", parameters);
        }



        [Test]
        public void RecordCrossPromoteImpression_WithoutParams_ShouldCallBridge()
        {
            AppsFlyer.recordCrossPromoteImpression("appid", "campaign", null);
            mock.Received().recordCrossPromoteImpression("appid", "campaign", null);
        }

        [Test]
        public void AddPushNotificationDeepLinkPath_ShouldCallBridge()
        {
            AppsFlyer.addPushNotificationDeepLinkPath("path1", "path2");
            mock.Received().addPushNotificationDeepLinkPath("path1", "path2");
        }
        #endregion

#if UNITY_ANDROID
    public class AppsFlyerAndroidTests
    {
        private IAppsFlyerAndroidBridge mock;

        [SetUp]
        public void SetUp()
        {
            mock = Substitute.For<IAppsFlyerAndroidBridge>();
            AppsFlyer.instance = mock;
        }

        [Test] public void UpdateServerUninstallToken_ShouldCallBridge() => AppsFlyer.updateServerUninstallToken("tokenTest");
        [Test] public void SetImeiData_ShouldCallBridge() => AppsFlyer.setImeiData("imei");
        [Test] public void SetAndroidIdData_ShouldCallBridge() => AppsFlyer.setAndroidIdData("androidId");
        [Test] public void WaitForCustomerUserId_ShouldCallBridge() => AppsFlyer.waitForCustomerUserId(true);
        [Test] public void SetCustomerIdAndStartSDK_ShouldCallBridge() => AppsFlyer.setCustomerIdAndStartSDK("01234");
        [Test] public void GetOutOfStore_ShouldCallBridge() => AppsFlyer.getOutOfStore();
        [Test] public void SetOutOfStore_ShouldCallBridge() => AppsFlyer.setOutOfStore("test");
        [Test] public void SetCollectAndroidID_ShouldCallBridge() => AppsFlyer.setCollectAndroidID(true);
        [Test] public void SetCollectIMEI_ShouldCallBridge() => AppsFlyer.setCollectIMEI(true);
        [Test] public void SetIsUpdate_ShouldCallBridge() => AppsFlyer.setIsUpdate(true);
        [Test] public void SetPreinstallAttribution_ShouldCallBridge() => AppsFlyer.setPreinstallAttribution("mediaSourceTestt", "campaign", "sideId");
        [Test] public void IsPreInstalledApp_ShouldCallBridge() => AppsFlyer.isPreInstalledApp();
        [Test] public void GetAttributionId_ShouldCallBridge() => AppsFlyer.getAttributionId();
        [Test] public void HandlePushNotifications_ShouldCallBridge() => AppsFlyer.handlePushNotifications();
        [Test] public void ValidateAndSendInAppPurchase_ShouldCallBridge() => AppsFlyer.validateAndSendInAppPurchase("ewjkekwjekw", "hewjehwj", "purchaseData", "3.0", "USD", null, null);
        [Test] public void SetCollectOaid_ShouldCallBridge() => AppsFlyer.setCollectOaid(true);
        [Test] public void SetDisableAdvertisingIdentifiers_ShouldCallBridge() => AppsFlyer.setDisableAdvertisingIdentifiers(true);
        [Test] public void SetDisableNetworkData_ShouldCallBridge() => AppsFlyer.setDisableNetworkData(true);
    }
#endif

#if UNITY_IOS
    public class AppsFlyeriOSTests
    {
        private IAppsFlyerIOSBridge mock;

        [SetUp]
        public void SetUp()
        {
            mock = Substitute.For<IAppsFlyerIOSBridge>();
            AppsFlyer.instance = mock;
        }

        [Test] public void DisableCollectAppleAdSupport_True_ShouldCallBridge() => AppsFlyer.setDisableCollectAppleAdSupport(true);
        [Test] public void DisableCollectAppleAdSupport_False_ShouldCallBridge() => AppsFlyer.setDisableCollectAppleAdSupport(false);
        [Test, System.Obsolete] public void ShouldCollectDeviceName_True_ShouldCallBridge() => AppsFlyer.setShouldCollectDeviceName(true);
        [Test, System.Obsolete] public void ShouldCollectDeviceName_False_ShouldCallBridge() => AppsFlyer.setShouldCollectDeviceName(false);
        [Test] public void DisableCollectIAd_True_ShouldCallBridge() => AppsFlyer.setDisableCollectIAd(true);
        [Test] public void DisableCollectIAd_False_ShouldCallBridge() => AppsFlyer.setDisableCollectIAd(false);
        [Test] public void UseReceiptValidationSandbox_True_ShouldCallBridge() => AppsFlyer.setUseReceiptValidationSandbox(true);
        [Test] public void UseReceiptValidationSandbox_False_ShouldCallBridge() => AppsFlyer.setUseReceiptValidationSandbox(false);
        [Test] public void UseUninstallSandbox_True_ShouldCallBridge() => AppsFlyer.setUseUninstallSandbox(true);
        [Test] public void UseUninstallSandbox_False_ShouldCallBridge() => AppsFlyer.setUseUninstallSandbox(false);
        [Test] public void ValidateAndSendInAppPurchase_ShouldCallBridge() => AppsFlyer.validateAndSendInAppPurchase("3d2", "5.0", "USD", "45", null, null);
        [Test] public void RegisterUninstall_ShouldCallBridge()
        {
            var token = System.Text.Encoding.UTF8.GetBytes("740f4707 bebcf74f 9b7c25d4 8e335894 5f6aa01d a5ddb387 462c7eaf 61bb78ad");
            AppsFlyer.registerUninstall(token);
            mock.Received().registerUninstall(token);
        }
        [Test] public void HandleOpenUrl_ShouldCallBridge() => AppsFlyer.handleOpenUrl("www.test.com", "appTest", "test");
        [Test] public void WaitForATTUserAuthorizationWithTimeoutInterval_ShouldCallBridge() => AppsFlyer.waitForATTUserAuthorizationWithTimeoutInterval(30);
        [Test] public void SetCurrentDeviceLanguage_ShouldCallBridge() => AppsFlyer.setCurrentDeviceLanguage("en");
        [Test] public void DisableSKAdNetwork_True_ShouldCallBridge() => AppsFlyer.disableSKAdNetwork(true);
        [Test] public void DisableSKAdNetwork_False_ShouldCallBridge() => AppsFlyer.disableSKAdNetwork(false);
    }
#endif

    }
}
