using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using AppsFlyerSDK;
using NSubstitute;

namespace AppsFlyerSDK.Tests
{
    public class NewTestScript
    {


        [Test]
        public void testStartSDK()
        {
            var AppsFlyerMOCKInterface = Substitute.For<IAppsFlyerNativeBridge>();
            AppsFlyer.instance = AppsFlyerMOCKInterface;
            AppsFlyer.startSDK();
            AppsFlyerMOCKInterface.Received().startSDK(Arg.Any<bool>(), Arg.Any<string>());
           
        }

        [Test]
        public void testSendEvent()
        {
            var AppsFlyerMOCKInterface = Substitute.For<IAppsFlyerNativeBridge>();
            AppsFlyer.instance = AppsFlyerMOCKInterface;
            AppsFlyer.sendEvent("testevent", new Dictionary<string, string>());
            AppsFlyerMOCKInterface.Received().sendEvent(Arg.Any<string>(), Arg.Any<Dictionary<string, string>>());
        }


        [Test]
        public void testIsSDKStoppedTrue()
        {
            var AppsFlyerMOCKInterface = Substitute.For<IAppsFlyerNativeBridge>();
            AppsFlyer.instance = AppsFlyerMOCKInterface;
            AppsFlyer.stopSDK(true);

            AppsFlyerMOCKInterface.Received().stopSDK(true);

        }

        [Test]
        public void testIsSDKStoppedFalse()
        {
            var AppsFlyerMOCKInterface = Substitute.For<IAppsFlyerNativeBridge>();

            AppsFlyer.instance = AppsFlyerMOCKInterface;

            AppsFlyer.stopSDK(false);

            AppsFlyerMOCKInterface.Received().stopSDK(false);

        }

        [Test]
        public void testSDKisStopped()
        {
            var AppsFlyerMOCKInterface = Substitute.For<IAppsFlyerNativeBridge>();

            AppsFlyer.instance = AppsFlyerMOCKInterface;

            var isSDKStopped = AppsFlyer.isSDKStopped();

            AppsFlyerMOCKInterface.Received().isSDKStopped();
            Assert.AreEqual(AppsFlyerMOCKInterface.Received().isSDKStopped(), false);

        }

        [Test]
        public void testGetSdkVersion()
        {
            var AppsFlyerMOCKInterface = Substitute.For<IAppsFlyerNativeBridge>();
            AppsFlyer.instance = AppsFlyerMOCKInterface;
            AppsFlyer.getSdkVersion();
            AppsFlyerMOCKInterface.Received().getSdkVersion();
        }

        [Test]
        public void testSetCustomerUserId()
        {

            var AppsFlyerMOCKInterface = Substitute.For<IAppsFlyerNativeBridge>();

            AppsFlyer.instance = AppsFlyerMOCKInterface;
            AppsFlyer.setCustomerUserId("test");
            AppsFlyerMOCKInterface.Received().setCustomerUserId("test");

        }

        [Test]
        public void TestSetAppInviteOneLinkID()
        {

            var AppsFlyerMOCKInterface = Substitute.For<IAppsFlyerNativeBridge>();

            AppsFlyer.instance = AppsFlyerMOCKInterface;
            AppsFlyer.setAppInviteOneLinkID("test");
            AppsFlyerMOCKInterface.Received().setAppInviteOneLinkID("test");

        }

        [Test]
        public void TestSetAdditionalData()
        {

            var AppsFlyerMOCKInterface = Substitute.For<IAppsFlyerNativeBridge>();

            AppsFlyer.instance = AppsFlyerMOCKInterface;
            var customData = new Dictionary<string, string>();
            customData.Add("test", "test");
            AppsFlyer.setAdditionalData(customData);
            AppsFlyerMOCKInterface.Received().setAdditionalData(Arg.Any<Dictionary<string, string>>());

        }

        [Test]
        public void TestSetResolveDeepLinkURLs()
        {

            var AppsFlyerMOCKInterface = Substitute.For<IAppsFlyerNativeBridge>();

            AppsFlyer.instance = AppsFlyerMOCKInterface;
            AppsFlyer.setResolveDeepLinkURLs("url1", "url2");
            AppsFlyerMOCKInterface.Received().setResolveDeepLinkURLs(Arg.Any<string>(), Arg.Any<string>());

        }

        [Test]
        public void TestSetOneLinkCustomDomain()
        {

            var AppsFlyerMOCKInterface = Substitute.For<IAppsFlyerNativeBridge>();

            AppsFlyer.instance = AppsFlyerMOCKInterface;
            AppsFlyer.setOneLinkCustomDomain("url1", "url2");
            AppsFlyerMOCKInterface.Received().setOneLinkCustomDomain("url1", "url2");

        }

        [Test]
        public void TestSetCurrencyCode()
        {

            var AppsFlyerMOCKInterface = Substitute.For<IAppsFlyerNativeBridge>();

            AppsFlyer.instance = AppsFlyerMOCKInterface;
            AppsFlyer.setCurrencyCode("usd");
            AppsFlyerMOCKInterface.Received().setCurrencyCode(Arg.Any<string>());

        }

        [Test]
        public void TestRecordLocation()
        {

            var AppsFlyerMOCKInterface = Substitute.For<IAppsFlyerNativeBridge>();

            AppsFlyer.instance = AppsFlyerMOCKInterface;
            AppsFlyer.recordLocation(0.3, 5.2);
            AppsFlyerMOCKInterface.Received().recordLocation(0.3, 5.2);

        }

        [Test]
        public void TestAnonymizeUser()
        {

            var AppsFlyerMOCKInterface = Substitute.For<IAppsFlyerNativeBridge>();

            AppsFlyer.instance = AppsFlyerMOCKInterface;
            AppsFlyer.anonymizeUser(true);
            AppsFlyerMOCKInterface.Received().anonymizeUser(true);

        }

        [Test]
        public void TestGetAppsFlyerId()
        {

            var AppsFlyerMOCKInterface = Substitute.For<IAppsFlyerNativeBridge>();

            AppsFlyer.instance = AppsFlyerMOCKInterface;
            AppsFlyer.getAppsFlyerId();
            AppsFlyerMOCKInterface.Received().getAppsFlyerId();

        }

        [Test]
        public void TestSetMinTimeBetweenSessions()
        {

            var AppsFlyerMOCKInterface = Substitute.For<IAppsFlyerNativeBridge>();

            AppsFlyer.instance = AppsFlyerMOCKInterface;
            AppsFlyer.setMinTimeBetweenSessions(3);
            AppsFlyerMOCKInterface.Received().setMinTimeBetweenSessions(3);

        }

        [Test]
        public void TestSetHost()
        {

            var AppsFlyerMOCKInterface = Substitute.For<IAppsFlyerNativeBridge>();

            AppsFlyer.instance = AppsFlyerMOCKInterface;
            AppsFlyer.setHost("prefix", "name");
            AppsFlyerMOCKInterface.Received().setHost("prefix", "name");

        }

        [Test]
        public void TestSetPhoneNumber()
        {

            var AppsFlyerMOCKInterface = Substitute.For<IAppsFlyerNativeBridge>();

            AppsFlyer.instance = AppsFlyerMOCKInterface;
            AppsFlyer.setPhoneNumber("002");
            AppsFlyerMOCKInterface.Received().setPhoneNumber("002");

        }


        [Test]
        public void TestSetSharingFilterForAllPartners()
        {

            var AppsFlyerMOCKInterface = Substitute.For<IAppsFlyerNativeBridge>();

            AppsFlyer.instance = AppsFlyerMOCKInterface;
            AppsFlyer.setSharingFilterForAllPartners();
            AppsFlyerMOCKInterface.Received().setSharingFilterForAllPartners();

        }

        [Test]
        public void TestSetSharingFilter()
        {

            var AppsFlyerMOCKInterface = Substitute.For<IAppsFlyerNativeBridge>();

            AppsFlyer.instance = AppsFlyerMOCKInterface;


            AppsFlyer.setSharingFilter("filter1", "filter2");
            AppsFlyerMOCKInterface.Received().setSharingFilter("filter1", "filter2");

        }

        [Test]
        public void TestGetConversionData()
        {

            var AppsFlyerMOCKInterface = Substitute.For<IAppsFlyerNativeBridge>();

            AppsFlyer.instance = AppsFlyerMOCKInterface;

            AppsFlyer.getConversionData("ObjectName");
            AppsFlyerMOCKInterface.Received().getConversionData("ObjectName");

        }

        [Test]
        public void TestAttributeAndOpenStore()
        {

            var AppsFlyerMOCKInterface = Substitute.For<IAppsFlyerNativeBridge>();

            AppsFlyer.instance = AppsFlyerMOCKInterface;

            AppsFlyer.attributeAndOpenStore("appid", "campaign", new Dictionary<string, string>(), new MonoBehaviour());
            AppsFlyerMOCKInterface.Received().attributeAndOpenStore("appid", "campaign", Arg.Any<Dictionary<string, string>>(), Arg.Any<MonoBehaviour>());

        }

        [Test]
        public void TestRecordCrossPromoteImpression()
        {

            var AppsFlyerMOCKInterface = Substitute.For<IAppsFlyerNativeBridge>();

            AppsFlyer.instance = AppsFlyerMOCKInterface;

            AppsFlyer.recordCrossPromoteImpression("appid", "campaign", new Dictionary<string, string>());
            AppsFlyerMOCKInterface.Received().recordCrossPromoteImpression("appid", "campaign", Arg.Any<Dictionary<string, string>>());

        }

        [Test]
        public void TestGenerateUserInviteLink()
        {

            var AppsFlyerMOCKInterface = Substitute.For<IAppsFlyerNativeBridge>();

            AppsFlyer.instance = AppsFlyerMOCKInterface;

            AppsFlyer.generateUserInviteLink(new Dictionary<string, string>(), new MonoBehaviour());
            AppsFlyerMOCKInterface.Received().generateUserInviteLink(Arg.Any<Dictionary<string, string>>(), Arg.Any<MonoBehaviour>());

        }


        [Test]
        public void TestAddPushNotificationDeepLinkPath()
        {

            var AppsFlyerMOCKInterface = Substitute.For<IAppsFlyerNativeBridge>();

            AppsFlyer.instance = AppsFlyerMOCKInterface;
            AppsFlyer.addPushNotificationDeepLinkPath("path1", "path2");
            AppsFlyerMOCKInterface.Received().addPushNotificationDeepLinkPath("path1", "path2");

        }
    }
}
