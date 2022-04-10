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
            AppsFlyerMOCKInterface.Received().sendEvent("testevent", eventParams);
        }

        [Test]
        public void test_sendEvent_withNullParams()
        {
            var AppsFlyerMOCKInterface = Substitute.For<IAppsFlyerNativeBridge>();
            AppsFlyer.instance = AppsFlyerMOCKInterface;
            AppsFlyer.sendEvent("testevent", null);
            AppsFlyerMOCKInterface.Received().sendEvent("testevent", null);
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
        public void Test_setSharingFilterForAllPartners_called()
        {

            var AppsFlyerMOCKInterface = Substitute.For<IAppsFlyerNativeBridge>();

            AppsFlyer.instance = AppsFlyerMOCKInterface;
            AppsFlyer.setSharingFilterForAllPartners();
            AppsFlyerMOCKInterface.Received().setSharingFilterForAllPartners();

        }

        [Test]
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
    }
}
