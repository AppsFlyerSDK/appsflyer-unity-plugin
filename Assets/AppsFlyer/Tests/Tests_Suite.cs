using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using NSubstitute;
using AFMiniJSON;

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
        // startSDK no longer calls the legacy native bridge directly on Android (removed to fix double init).
        // On iOS it still calls instance.startSDK(); on all platforms it fires ExecuteFire("start") via RPC.
        // Covered by AppsFlyerRPCContractTests.StartSDK_FiresStartViaRPC.

        // stopSDK no longer calls the legacy bridge — it fires RPC only.
        // Covered by AppsFlyerRPCContractTests.StopSDK_iOS_SendsStopNotSetStopped.

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
            mock.Received().sendEvent("testevent", eventParams, Arg.Any<bool>(), Arg.Any<string>());
        }

        [Test]
        public void SendEvent_NullParams_ShouldCallBridge()
        {
            AppsFlyer.sendEvent("testevent", null);
            mock.Received().sendEvent("testevent", null, Arg.Any<bool>(), Arg.Any<string>());
        }
        #endregion

        #region Identity and Configuration
        [Test]
        public void SetCustomerUserId_ShouldCallBridge()
        {
            AppsFlyer.setCustomerUserId("user123");
            mock.Received().setCustomerUserId("user123");
        }

        // setAppInviteOneLinkID no longer calls the legacy bridge — fires RPC only.
        // Covered by AppsFlyerRPCContractTests.SetAppInviteOneLinkID_iOS_SendsSetAppInviteOneLink.

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

        // setPhoneNumber no longer calls the legacy bridge — fires RPC only (platform-split).
        // Covered by AppsFlyerRPCContractTests (iOS and Android variants).


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

        // anonymizeUser no longer calls the legacy bridge — fires RPC only.
        // Covered by AppsFlyerRPCContractTests.AnonymizeUser_iOS_SendsAnonymizeUserNotSetAnonymizeUser.
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

    [TestFixture]
    public class AppsFlyerRPCClientTests
    {
        private AppsFlyerRPCClient rpc;

        [SetUp]
        public void SetUp()
        {
            rpc = AppsFlyerRPCClient.DefaultInstance;
        }

        // --- BuildRequest tests ---

        [Test]
        public void BuildRequest_ProducesCorrectMethod()
        {
            string json = rpc.BuildRequest("init", null);
            var dict = Json.Deserialize(json) as Dictionary<string, object>;
            Assert.AreEqual("init", dict["method"]);
        }

        [Test]
        public void BuildRequest_IdContainsMethodName()
        {
            string json = rpc.BuildRequest("start", null);
            var dict = Json.Deserialize(json) as Dictionary<string, object>;
            StringAssert.Contains("start", (string)dict["id"]);
        }

        [Test]
        public void BuildRequest_WithParams_IncludesParams()
        {
            var parameters = new Dictionary<string, object> { { "devKey", "abc123" } };
            string json = rpc.BuildRequest("init", parameters);
            var dict = Json.Deserialize(json) as Dictionary<string, object>;
            var paramsDict = dict["params"] as Dictionary<string, object>;
            Assert.AreEqual("abc123", paramsDict["devKey"]);
        }

        [Test]
        public void BuildRequest_NullParams_ProducesEmptyParamsObject()
        {
            string json = rpc.BuildRequest("start", null);
            var dict = Json.Deserialize(json) as Dictionary<string, object>;
            var paramsDict = dict["params"] as Dictionary<string, object>;
            Assert.IsNotNull(paramsDict);
            Assert.AreEqual(0, paramsDict.Count);
        }

        [Test]
        public void BuildRequest_IdIsUniqueAcrossCalls()
        {
            string json1 = rpc.BuildRequest("start", null);
            string json2 = rpc.BuildRequest("start", null);
            var id1 = (Json.Deserialize(json1) as Dictionary<string, object>)["id"];
            var id2 = (Json.Deserialize(json2) as Dictionary<string, object>)["id"];
            Assert.AreNotEqual(id1, id2);
        }

        // --- ParseResponse tests ---

        [Test]
        public void ParseResponse_Success_ReturnsData()
        {
            string response = "{\"id\":\"x\",\"result\":{\"data\":null}}";
            Assert.IsNull(rpc.ParseResponse(response));
        }

        [Test]
        public void ParseResponse_SuccessWithData_ReturnsData()
        {
            string response = "{\"id\":\"x\",\"result\":{\"data\":{\"uid\":\"abc\"}}}";
            var data = rpc.ParseResponse(response) as Dictionary<string, object>;
            Assert.AreEqual("abc", data["uid"]);
        }

        [Test]
        public void ParseResponse_ErrorResponse_ThrowsRPCException()
        {
            string response = "{\"id\":\"x\",\"error\":{\"code\":422,\"message\":\"bad devKey\"}}";
            var ex = Assert.Throws<AppsFlyerRPCException>(() => rpc.ParseResponse(response));
            Assert.AreEqual(422, ex.Code);
            StringAssert.Contains("bad devKey", ex.Message);
        }

        [Test]
        public void ParseResponse_EmptyString_ThrowsRPCException()
        {
            Assert.Throws<AppsFlyerRPCException>(() => rpc.ParseResponse(""));
        }

        [Test]
        public void ParseResponse_NullString_ThrowsRPCException()
        {
            Assert.Throws<AppsFlyerRPCException>(() => rpc.ParseResponse(null));
        }

        [Test]
        public void ParseResponse_MalformedJson_ThrowsRPCException()
        {
            Assert.Throws<AppsFlyerRPCException>(() => rpc.ParseResponse("{not valid json"));
        }

        [Test]
        public void ParseResponse_ErrorCode_IsLongSafe()
        {
            string response = "{\"id\":\"x\",\"error\":{\"code\":500,\"message\":\"server error\"}}";
            var ex = Assert.Throws<AppsFlyerRPCException>(() => rpc.ParseResponse(response));
            Assert.AreEqual(500, ex.Code);
        }

        [Test]
        public void ParseResponse_404_UnknownMethod_ThrowsWithCode404()
        {
            // 404 is the exact error the iOS RPC layer returns when an unknown method string is sent
            // (e.g. "init" instead of "initialize" throws unknownMethod("init") → 404)
            string response = "{\"id\":\"x\",\"error\":{\"code\":404,\"message\":\"unknownMethod: init\"}}";
            var ex = Assert.Throws<AppsFlyerRPCException>(() => rpc.ParseResponse(response));
            Assert.AreEqual(404, ex.Code);
            StringAssert.Contains("init", ex.Message);
        }

        [Test]
        public void ParseResponse_400_BadRequest_ThrowsWithCode400()
        {
            string response = "{\"id\":\"x\",\"error\":{\"code\":400,\"message\":\"bad request\"}}";
            var ex = Assert.Throws<AppsFlyerRPCException>(() => rpc.ParseResponse(response));
            Assert.AreEqual(400, ex.Code);
        }

        [Test]
        public void ParseResponse_503_SDKNotReady_ThrowsWithCode503()
        {
            // 503 is returned by the iOS RPC layer when start() is called before the SDK is ready
            string response = "{\"id\":\"x\",\"error\":{\"code\":503,\"message\":\"SDK not ready\"}}";
            var ex = Assert.Throws<AppsFlyerRPCException>(() => rpc.ParseResponse(response));
            Assert.AreEqual(503, ex.Code);
        }

        // --- onRPCEvent routing tests ---

        [Test]
        public void OnRPCEvent_StartEvent_FiresOnRequestResponse()
        {
            bool fired = false;
            EventHandler handler = (s, e) => { fired = true; };
            AppsFlyer.OnRequestResponse += handler;
            var af = new GameObject().AddComponent<AppsFlyer>();
            af.onRPCEvent("{\"event\":\"start\",\"data\":{\"statusCode\":200,\"errorDescription\":\"\"}}");
            Assert.IsTrue(fired);
            AppsFlyer.OnRequestResponse -= handler;
        }

        [Test]
        public void OnRPCEvent_UnknownEvent_DoesNotThrow()
        {
            var af = new GameObject().AddComponent<AppsFlyer>();
            Assert.DoesNotThrow(() =>
                af.onRPCEvent("{\"event\":\"unknownEvent\",\"data\":{}}"));
        }

        [Test]
        public void OnRPCEvent_EmptyString_DoesNotThrow()
        {
            var af = new GameObject().AddComponent<AppsFlyer>();
            Assert.DoesNotThrow(() => af.onRPCEvent(""));
        }
    }

    /// <summary>
    /// Contract tests: verify the exact RPC method names and parameter shapes that AppsFlyer.cs
    /// sends to AppsFlyerRPCClient per platform.  Platform-specific tests are compiled only for
    /// their target platform so they run in CI platform builds (Android / iOS simulator).
    /// </summary>
    [TestFixture]
    public class AppsFlyerRPCContractTests
    {
        private IAppsFlyerRPCClient mockRpc;
        private IAppsFlyerNativeBridge mockNative;

        [SetUp]
        public void SetUp()
        {
            mockRpc = Substitute.For<IAppsFlyerRPCClient>();
            AppsFlyerRPCClient.instance = mockRpc;

            // isInit=true prevents initSDK from trying to construct platform-native instances
            mockNative = Substitute.For<IAppsFlyerNativeBridge>();
            mockNative.isInit.Returns(true);
            AppsFlyer.instance = mockNative;
        }

        [TearDown]
        public void TearDown()
        {
            AppsFlyerRPCClient.instance = AppsFlyerRPCClient.DefaultInstance;
            AppsFlyer.instance = null;
        }

        // ── Platform-agnostic: startSDK fires RPC on all platforms ───────────────

        [Test]
        public void StartSDK_FiresStartViaRPC()
        {
            AppsFlyer.startSDK();
            mockRpc.Received(1).ExecuteFire("start");
        }

        // ── Platform-agnostic: verify BuildRequest envelope shape ─────────────────

        [Test]
        public void BuildRequest_AndroidInit_ContainsDevKeyOnly()
        {
            string json = AppsFlyerRPCClient.DefaultInstance.BuildRequest(
                "init", new Dictionary<string, object> { { "devKey", "abc" } });
            var p = (Json.Deserialize(json) as Dictionary<string, object>)["params"]
                    as Dictionary<string, object>;
            Assert.IsTrue(p.ContainsKey("devKey"));
            Assert.IsFalse(p.ContainsKey("appId"), "Android init must NOT include appId");
        }

        [Test]
        public void BuildRequest_iOSInitialize_ContainsBothDevKeyAndAppId()
        {
            string json = AppsFlyerRPCClient.DefaultInstance.BuildRequest(
                "initialize", new Dictionary<string, object>
                { { "devKey", "abc" }, { "appId", "123" } });
            var p = (Json.Deserialize(json) as Dictionary<string, object>)["params"]
                    as Dictionary<string, object>;
            Assert.IsTrue(p.ContainsKey("devKey"));
            Assert.IsTrue(p.ContainsKey("appId"), "iOS initialize must include appId");
        }

        [Test]
        public void BuildRequest_SetHost_UsesHostPrefixNameKey()
        {
            string json = AppsFlyerRPCClient.DefaultInstance.BuildRequest(
                "setHost", new Dictionary<string, object>
                { { "hostPrefixName", "pre" }, { "hostName", "host" } });
            var p = (Json.Deserialize(json) as Dictionary<string, object>)["params"]
                    as Dictionary<string, object>;
            Assert.IsTrue(p.ContainsKey("hostPrefixName"), "SDK7 renamed param must be hostPrefixName");
            Assert.IsFalse(p.ContainsKey("prefix"), "legacy 'prefix' key must not be used");
        }

        [Test]
        public void BuildRequest_LogEvent_HasEventNameAndEventValues()
        {
            string json = AppsFlyerRPCClient.DefaultInstance.BuildRequest(
                "logEvent", new Dictionary<string, object>
                { { "eventName", "purchase" }, { "eventValues", new Dictionary<string, object>() } });
            var p = (Json.Deserialize(json) as Dictionary<string, object>)["params"]
                    as Dictionary<string, object>;
            Assert.IsTrue(p.ContainsKey("eventName"), "iOS parser requires 'eventName'");
            Assert.IsTrue(p.ContainsKey("eventValues"), "iOS parser requires 'eventValues'");
        }

        [Test]
        public void BuildRequest_SetUserEmails_iOS_HasCryptTypeAndEmailsKeys()
        {
            // iOS parser validates both 'cryptType' (int) and 'emails' (array) are present
            string json = AppsFlyerRPCClient.DefaultInstance.BuildRequest(
                "setUserEmails", new Dictionary<string, object>
                { { "cryptType", 2 }, { "emails", new string[] { "a@b.com" } } });
            var p = (Json.Deserialize(json) as Dictionary<string, object>)["params"]
                    as Dictionary<string, object>;
            Assert.IsTrue(p.ContainsKey("cryptType"), "iOS parser requires 'cryptType'");
            Assert.IsTrue(p.ContainsKey("emails"), "iOS parser requires 'emails' (not 'email')");
            Assert.IsFalse(p.ContainsKey("email"), "'email' singular is the Android key, not iOS");
        }

        [Test]
        public void BuildRequest_ValidateAndLogInAppPurchase_HasRequiredFields()
        {
            // iOS parser requires productId, transactionId, purchaseType
            string json = AppsFlyerRPCClient.DefaultInstance.BuildRequest(
                "validateAndLogInAppPurchase", new Dictionary<string, object>
                { { "productId", "com.app.item" }, { "transactionId", "txn123" }, { "purchaseType", 0 } });
            var p = (Json.Deserialize(json) as Dictionary<string, object>)["params"]
                    as Dictionary<string, object>;
            Assert.IsTrue(p.ContainsKey("productId"));
            Assert.IsTrue(p.ContainsKey("transactionId"));
            Assert.IsTrue(p.ContainsKey("purchaseType"));
        }

#if UNITY_ANDROID

        // ── Android routing ───────────────────────────────────────────────────────

        [Test]
        public void InitSDK_Android_SendsInitWithDevKeyOnly()
        {
            AppsFlyer.initSDK("key123", "appId456", null);
            mockRpc.Received(1).ExecuteFire("init",
                Arg.Is<Dictionary<string, object>>(d =>
                    (string)d["devKey"] == "key123" && !d.ContainsKey("appId")));
        }

        [Test]
        public void SubscribeForDeepLink_Android_SendsSubscribeForDeepLink()
        {
            AppsFlyer.subscribeForDeepLink();
            mockRpc.Received(1).ExecuteFire("subscribeForDeepLink",
                Arg.Any<Dictionary<string, object>>());
        }

        [Test]
        public void SetUserEmails_Android_SendsFirstEmailAsSingularMethod()
        {
            AppsFlyer.setUserEmails(EmailCryptType.EmailCryptTypeNone, "a@b.com", "c@d.com");
            mockRpc.Received(1).ExecuteFire("setUserEmail",
                Arg.Is<Dictionary<string, object>>(d => (string)d["email"] == "a@b.com"));
            mockRpc.DidNotReceive().ExecuteFire("setUserEmails",
                Arg.Any<Dictionary<string, object>>());
        }

        [Test]
        public void SetUserEmails_Android_EmptyArray_DoesNotFire()
        {
            AppsFlyer.setUserEmails(EmailCryptType.EmailCryptTypeNone);
            mockRpc.DidNotReceive().ExecuteFire(Arg.Any<string>(),
                Arg.Any<Dictionary<string, object>>());
        }

        [Test]
        public void SetPhoneNumber_Android_DoesNotFire()
        {
            // Android RPC bridge requires countryCode for setUserPhone; no single-arg
            // setter exists. setPhoneNumber is a no-op on Android until bridge adds it.
            AppsFlyer.setPhoneNumber("0501234567");
            mockRpc.DidNotReceive().ExecuteFire(Arg.Any<string>(),
                Arg.Any<Dictionary<string, object>>());
        }

        [Test]
        public void SetCurrentDeviceLanguage_Android_DoesNotFire()
        {
            AppsFlyer.setCurrentDeviceLanguage("en");
            mockRpc.DidNotReceive().ExecuteFire(Arg.Any<string>(),
                Arg.Any<Dictionary<string, object>>());
        }

        [Test]
        public void SetShouldCollectDeviceName_Android_DoesNotFire()
        {
            AppsFlyer.setShouldCollectDeviceName(true);
            mockRpc.DidNotReceive().ExecuteFire(Arg.Any<string>(),
                Arg.Any<Dictionary<string, object>>());
        }

#endif // UNITY_ANDROID

#if (UNITY_IOS || UNITY_STANDALONE_OSX) && !UNITY_ANDROID

        // ── iOS routing ───────────────────────────────────────────────────────────

        [Test]
        public void InitSDK_iOS_SendsInitializeWithDevKeyAndAppId()
        {
            AppsFlyer.initSDK("key123", "appId456", null);
            mockRpc.Received(1).ExecuteFire("initialize",
                Arg.Is<Dictionary<string, object>>(d =>
                    (string)d["devKey"] == "key123" && (string)d["appId"] == "appId456"));
        }

        [Test]
        public void SubscribeForDeepLink_iOS_SendsRegisterDeeplinkListener()
        {
            AppsFlyer.subscribeForDeepLink();
            mockRpc.Received(1).ExecuteFire("registerDeeplinkListener",
                Arg.Any<Dictionary<string, object>>());
        }

        [Test]
        public void StopSDK_iOS_SendsStopNotSetStopped()
        {
            AppsFlyer.stopSDK(true);
            mockRpc.Received(1).ExecuteFire("stop",
                Arg.Is<Dictionary<string, object>>(d =>
                    d.ContainsKey("shouldStop") && (bool)d["shouldStop"] == true));
            mockRpc.DidNotReceive().ExecuteFire("setStopped",
                Arg.Any<Dictionary<string, object>>());
        }

        [Test]
        public void StopSDK_Resume_SendsShouldStopFalse()
        {
            AppsFlyer.stopSDK(false);
            mockRpc.Received(1).ExecuteFire("stop",
                Arg.Is<Dictionary<string, object>>(d =>
                    d.ContainsKey("shouldStop") && (bool)d["shouldStop"] == false));
        }

        [Test]
        public void AnonymizeUser_iOS_SendsAnonymizeUserNotSetAnonymizeUser()
        {
            AppsFlyer.anonymizeUser(true);
            mockRpc.Received(1).ExecuteFire("anonymizeUser",
                Arg.Any<Dictionary<string, object>>());
            mockRpc.DidNotReceive().ExecuteFire("setAnonymizeUser",
                Arg.Any<Dictionary<string, object>>());
        }

        [Test]
        public void SetAppInviteOneLinkID_iOS_SendsSetAppInviteOneLink()
        {
            AppsFlyer.setAppInviteOneLinkID("2f36");
            mockRpc.Received(1).ExecuteFire("setAppInviteOneLink",
                Arg.Any<Dictionary<string, object>>());
            mockRpc.DidNotReceive().ExecuteFire("setAppInviteOneLinkID",
                Arg.Any<Dictionary<string, object>>());
        }

        [Test]
        public void SetOneLinkCustomDomain_iOS_SendsSingularNotPlural()
        {
            AppsFlyer.setOneLinkCustomDomain("domain1", "domain2");
            mockRpc.Received(1).ExecuteFire("setOneLinkCustomDomain",
                Arg.Any<Dictionary<string, object>>());
            mockRpc.DidNotReceive().ExecuteFire("setOneLinkCustomDomains",
                Arg.Any<Dictionary<string, object>>());
        }

        [Test]
        public void SetHost_iOS_UsesHostPrefixNameParam()
        {
            AppsFlyer.setHost("myprefix", "myhost");
            mockRpc.Received(1).ExecuteFire("setHost",
                Arg.Is<Dictionary<string, object>>(d =>
                    d.ContainsKey("hostPrefixName") && !d.ContainsKey("prefix")));
        }

        [Test]
        public void SetUserEmails_iOS_SendsPluralMethodWithArrayAndCryptType()
        {
            AppsFlyer.setUserEmails(EmailCryptType.EmailCryptTypeSHA256, "a@b.com", "c@d.com");
            mockRpc.Received(1).ExecuteFire("setUserEmails",
                Arg.Is<Dictionary<string, object>>(d =>
                    d.ContainsKey("cryptType") && d.ContainsKey("emails")));
            mockRpc.DidNotReceive().ExecuteFire("setUserEmail",
                Arg.Any<Dictionary<string, object>>());
        }

        [Test]
        public void SetPhoneNumber_iOS_SendsSetPhoneNumberNotSetUserPhone()
        {
            AppsFlyer.setPhoneNumber("0501234567");
            mockRpc.Received(1).ExecuteFire("setPhoneNumber",
                Arg.Is<Dictionary<string, object>>(d =>
                    (string)d["phoneNumber"] == "0501234567"));
            mockRpc.DidNotReceive().ExecuteFire("setUserPhone",
                Arg.Any<Dictionary<string, object>>());
        }

        [Test]
        public void SetShouldCollectDeviceName_iOS_Fires()
        {
            AppsFlyer.setShouldCollectDeviceName(true);
            mockRpc.Received(1).ExecuteFire("setShouldCollectDeviceName",
                Arg.Is<Dictionary<string, object>>(d => (bool)d["collect"] == true));
        }

        [Test]
        public void SetDisableAdvertisingIdentifiers_iOS_SendsPluralForm()
        {
            AppsFlyer.setDisableAdvertisingIdentifiers(true);
            mockRpc.Received(1).ExecuteFire("setDisableAdvertisingIdentifiers",
                Arg.Any<Dictionary<string, object>>());
            mockRpc.DidNotReceive().ExecuteFire("setDisableAdvertisingIdentifier",
                Arg.Any<Dictionary<string, object>>());
        }

        [Test]
        public void ValidateAndSendInAppPurchase_iOS_SendsValidateAndLogInAppPurchase()
        {
            // Method was "validateAndLogInAppPurchaseV2" in old code; iOS parser only accepts "validateAndLogInAppPurchase"
            AppsFlyer.validateAndSendInAppPurchase((AFSDKPurchaseDetailsIOS)null, null, null);
            mockRpc.Received(1).ExecuteFire("validateAndLogInAppPurchase",
                Arg.Any<Dictionary<string, object>>());
        }

        [Test]
        public void SetDisableCollectAppleAdSupport_iOS_SendsSetDisableCollectASA()
        {
            AppsFlyer.setDisableCollectAppleAdSupport(true);
            mockRpc.Received(1).ExecuteFire("setDisableCollectASA",
                Arg.Any<Dictionary<string, object>>());
        }

        [Test]
        public void SetDisableCollectIAd_iOS_SendsSetDisableAppleAdsAttribution()
        {
            // C# API is setDisableCollectIAd; it maps to "setDisableAppleAdsAttribution" RPC method
            AppsFlyer.setDisableCollectIAd(true);
            mockRpc.Received(1).ExecuteFire("setDisableAppleAdsAttribution",
                Arg.Any<Dictionary<string, object>>());
        }

#endif // (UNITY_IOS || UNITY_STANDALONE_OSX) && !UNITY_ANDROID
    }
}
