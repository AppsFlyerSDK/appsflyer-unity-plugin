using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using NSubstitute;
using AFMiniJSON;

namespace AppsFlyerSDK.Tests
{
    [TestFixture]
    public class AppsFlyerRPCClientTests
    {
        private AppsFlyerRPCClient rpc;
        private readonly List<GameObject> _spawned = new List<GameObject>();

        [SetUp]
        public void SetUp()
        {
            rpc = AppsFlyerRPCClient.DefaultInstance;
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var go in _spawned) UnityEngine.Object.DestroyImmediate(go);
            _spawned.Clear();
        }

        private AppsFlyer NewAppsFlyerComponent()
        {
            var go = new GameObject();
            _spawned.Add(go);
            return go.AddComponent<AppsFlyer>();
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
            string response = "{\"id\":\"x\",\"error\":{\"code\":503,\"message\":\"SDK not ready\"}}";
            var ex = Assert.Throws<AppsFlyerRPCException>(() => rpc.ParseResponse(response));
            Assert.AreEqual(503, ex.Code);
        }

        // --- ParseResponse expectedId tests ---

        [Test]
        public void ParseResponse_MatchingExpectedId_ReturnsData()
        {
            string response = "{\"id\":\"x\",\"result\":{\"data\":\"ok\"}}";
            var result = rpc.ParseResponse(response, "x");
            Assert.AreEqual("ok", result);
        }

        [Test]
        public void ParseResponse_MismatchedExpectedId_ThrowsRPCException()
        {
            string response = "{\"id\":\"y\",\"result\":{\"data\":\"ok\"}}";
            var ex = Assert.Throws<AppsFlyerRPCException>(() => rpc.ParseResponse(response, "x"));
            StringAssert.Contains("id mismatch", ex.Message);
        }

        [Test]
        public void ParseResponse_MissingIdWithExpectedId_ThrowsRPCException()
        {
            string response = "{\"result\":{\"data\":\"ok\"}}";
            Assert.Throws<AppsFlyerRPCException>(() => rpc.ParseResponse(response, "x"));
        }

        [Test]
        public void ParseResponse_NoExpectedId_SkipsIdCheckEvenIfMismatched()
        {
            string response = "{\"id\":\"y\",\"result\":{\"data\":\"ok\"}}";
            Assert.DoesNotThrow(() => rpc.ParseResponse(response));
        }

        // --- End-to-end: real DefaultInstance (BuildRequest -> Dispatch -> ParseResponse), not a mock ---

        [Test]
        public void Execute_EndToEnd_RealDefaultInstance_RoundTripsRequestIdThroughStubResponse()
        {
            // In the Editor, Dispatch() hits the no-native-bridge StubResponse path, which echoes the
            // BuildRequest-generated id back — this exercises the exact BuildRequest -> Dispatch ->
            // ParseResponse id round trip end to end, on the real AppsFlyerRPCClient, not a mocked
            // IAppsFlyerRPCClient. This is the code path the Android id-echo bug slipped through.
            Assert.DoesNotThrow(() => rpc.Execute("getSdkVersion"));
        }

        [Test]
        public void ExecuteFire_EndToEnd_RealDefaultInstance_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => rpc.ExecuteFire("start"));
        }

        // --- onRPCEvent routing tests ---

        [Test]
        public void OnRPCEvent_StartEvent_FiresOnRequestResponse()
        {
            bool fired = false;
            EventHandler handler = (s, e) => { fired = true; };
            AppsFlyer.OnRequestResponse += handler;
            var af = NewAppsFlyerComponent();
            af.onRPCEvent("{\"event\":\"start\",\"data\":{\"statusCode\":200,\"errorDescription\":\"\"}}");
            Assert.IsTrue(fired);
            AppsFlyer.OnRequestResponse -= handler;
        }

        [Test]
        public void OnRPCEvent_SessionReadyEvent_FiresOnSessionReady()
        {
            bool fired = false;
            EventHandler handler = (s, e) => { fired = true; };
            AppsFlyer.OnSessionReady += handler;
            var af = NewAppsFlyerComponent();
            af.onRPCEvent("{\"event\":\"sessionReady\",\"data\":{}}");
            Assert.IsTrue(fired);
            AppsFlyer.OnSessionReady -= handler;
        }

        [Test]
        public void OnRPCEvent_UnknownEvent_DoesNotThrow()
        {
            var af = NewAppsFlyerComponent();
            Assert.DoesNotThrow(() =>
                af.onRPCEvent("{\"event\":\"unknownEvent\",\"data\":{}}"));
        }

        [Test]
        public void OnRPCEvent_EmptyString_DoesNotThrow()
        {
            var af = NewAppsFlyerComponent();
            Assert.DoesNotThrow(() => af.onRPCEvent(""));
        }
    }

    /// <summary>
    /// Contract tests: verify the exact RPC method names and parameter shapes that AppsFlyer.cs
    /// sends to AppsFlyerRPCClient per platform, against appsflyer-plugins-rpc-schema.json.
    /// Platform-specific tests are compiled only for their target platform so they run in CI
    /// platform builds (Android / iOS simulator).
    /// </summary>
    [TestFixture]
    public class AppsFlyerRPCContractTests
    {
        private IAppsFlyerRPCClient mockRpc;
        private readonly List<GameObject> _spawned = new List<GameObject>();

        [SetUp]
        public void SetUp()
        {
            mockRpc = Substitute.For<IAppsFlyerRPCClient>();
            AppsFlyerRPCClient.instance = mockRpc;
        }

        [TearDown]
        public void TearDown()
        {
            AppsFlyerRPCClient.instance = AppsFlyerRPCClient.DefaultInstance;
            AppsFlyer.CallBackObjectName = null;
            foreach (var go in _spawned) UnityEngine.Object.DestroyImmediate(go);
            _spawned.Clear();
        }

        private InviteLinkCallbackRecorder NewCallbackTarget()
        {
            var go = new GameObject("InviteLinkCallbackTarget");
            _spawned.Add(go);
            AppsFlyer.CallBackObjectName = go.name;
            return go.AddComponent<InviteLinkCallbackRecorder>();
        }

        private class InviteLinkCallbackRecorder : MonoBehaviour
        {
            public int GeneratedCallCount;
            public int FailureCallCount;
            public string ReceivedLink;
            public string ReceivedFailure;

            public void onInviteLinkGenerated(string link)
            {
                GeneratedCallCount++;
                ReceivedLink = link;
            }

            public void onInviteLinkGeneratedFailure(string message)
            {
                FailureCallCount++;
                ReceivedFailure = message;
            }
        }

        // ── Init / lifecycle ───────────────────────────────────────────────────────

        [Test]
        public void Start_FiresStartWithNoParams()
        {
            AppsFlyer.start();
            mockRpc.Received(1).ExecuteFire("start", Arg.Any<Dictionary<string, object>>());
        }

        [Test]
        public void Stop_FiresStopWithShouldStop()
        {
            AppsFlyer.stop(true);
            mockRpc.Received(1).ExecuteFire("stop",
                Arg.Is<Dictionary<string, object>>(d => (bool)d["shouldStop"] == true));
        }

        [Test]
        public void LogEvent_FiresLogEventWithNameAndValues()
        {
            var values = new Dictionary<string, string> { { "key", "value" } };
            AppsFlyer.logEvent("testevent", values);
            mockRpc.Received(1).ExecuteFire("logEvent",
                Arg.Is<Dictionary<string, object>>(d => (string)d["eventName"] == "testevent" && d["eventValues"] == values));
        }

#if UNITY_ANDROID
        [Test]
        public void Init_Android_SendsInitWithDevKeyOnly()
        {
            AppsFlyer.init("key123", "appId456");
            mockRpc.Received(1).ExecuteFire("init",
                Arg.Is<Dictionary<string, object>>(d => (string)d["devKey"] == "key123" && !d.ContainsKey("appId")));
        }
#endif

#if (UNITY_IOS || UNITY_STANDALONE_OSX) && !UNITY_ANDROID
        [Test]
        public void Init_iOS_SendsInitializeWithDevKeyAndAppId()
        {
            AppsFlyer.init("key123", "appId456");
            mockRpc.Received(1).Execute("initialize",
                Arg.Is<Dictionary<string, object>>(d => (string)d["devKey"] == "key123" && (string)d["appId"] == "appId456"));
        }
#endif

        // ── Renamed / fixed methods (Category B rewrites) ───────────────────────────

        [Test]
        public void SetUserEmail_SendsSingularMethodWithEmailKey()
        {
            AppsFlyer.setUserEmail("a@b.com");
            mockRpc.Received(1).ExecuteFire("setUserEmail",
                Arg.Is<Dictionary<string, object>>(d => (string)d["email"] == "a@b.com"));
            mockRpc.DidNotReceive().ExecuteFire("setUserEmails", Arg.Any<Dictionary<string, object>>());
        }

        [Test]
        public void SetUserPhone_SendsCountryCodeAndPhoneNumber()
        {
            AppsFlyer.setUserPhone("1", "0501234567");
            mockRpc.Received(1).ExecuteFire("setUserPhone",
                Arg.Is<Dictionary<string, object>>(d =>
                    (string)d["countryCode"] == "1" && (string)d["phoneNumber"] == "0501234567"));
        }

        [Test]
        public void LogAndOpenStore_SendsPromotedAppIdNotAppId()
        {
            AppsFlyer.logAndOpenStore("appid", "campaign", null);
            mockRpc.Received(1).ExecuteFire("logAndOpenStore",
                Arg.Is<Dictionary<string, object>>(d => (string)d["promotedAppId"] == "appid" && !d.ContainsKey("appId")));
        }

        [Test]
        public void LogCrossPromoteImpression_SendsAppId()
        {
            AppsFlyer.logCrossPromoteImpression("appid", "campaign", null);
            mockRpc.Received(1).ExecuteFire("logCrossPromoteImpression",
                Arg.Is<Dictionary<string, object>>(d => (string)d["appId"] == "appid"));
        }

        [Test]
        public void LogAdRevenue_SendsMediationNetworkAsString_NotInt()
        {
            var adRevenue = new AFAdRevenueData("network", MediationNetwork.GoogleAdMob, "USD", 1.0);
            AppsFlyer.logAdRevenue(adRevenue, null);
            mockRpc.Received(1).ExecuteFire("logAdRevenue",
                Arg.Is<Dictionary<string, object>>(d => d["mediationNetwork"] is string));
        }

        [Test]
        public async Task GenerateInviteLink_SpreadsKeysTopLevel_NotNestedUnderParameters()
        {
            var parameters = new Dictionary<string, string> { { "channel", "sms" }, { "campaign", "referral" } };
            await AppsFlyer.generateInviteLinkAsync(parameters);
            mockRpc.Received(1).Execute("generateInviteLink",
                Arg.Is<Dictionary<string, object>>(d =>
                    (string)d["channel"] == "sms" && (string)d["campaign"] == "referral" && !d.ContainsKey("parameters")));
        }

#if UNITY_ANDROID
        [Test]
        public async Task GenerateInviteLink_Android_RemapsReferrerCustomerIdToCustomerId()
        {
            var parameters = new Dictionary<string, string> { { "referrerCustomerId", "cust-1" } };
            await AppsFlyer.generateInviteLinkAsync(parameters);
            mockRpc.Received(1).Execute("generateInviteLink",
                Arg.Is<Dictionary<string, object>>(d =>
                    (string)d["customerId"] == "cust-1" && !d.ContainsKey("referrerCustomerId")));
        }
#else
        [Test]
        public async Task GenerateInviteLink_NonAndroid_PassesReferrerCustomerIdThrough()
        {
            var parameters = new Dictionary<string, string> { { "referrerCustomerId", "cust-1" } };
            await AppsFlyer.generateInviteLinkAsync(parameters);
            mockRpc.Received(1).Execute("generateInviteLink",
                Arg.Is<Dictionary<string, object>>(d =>
                    (string)d["referrerCustomerId"] == "cust-1" && !d.ContainsKey("customerId")));
        }
#endif

        [Test]
        public async Task GenerateInviteLink_Success_DeliversLinkViaOnInviteLinkGenerated()
        {
            var recorder = NewCallbackTarget();
            mockRpc.Execute("generateInviteLink", Arg.Any<Dictionary<string, object>>())
                .Returns("https://example.onelink.me/abc");

            await AppsFlyer.DeliverInviteLinkAsync(new Dictionary<string, string>());

            Assert.AreEqual(1, recorder.GeneratedCallCount);
            Assert.AreEqual("https://example.onelink.me/abc", recorder.ReceivedLink);
            Assert.AreEqual(0, recorder.FailureCallCount);
        }

        [Test]
        public async Task GenerateInviteLink_RpcException_DeliversMessageViaOnInviteLinkGeneratedFailure()
        {
            var recorder = NewCallbackTarget();
            mockRpc.Execute("generateInviteLink", Arg.Any<Dictionary<string, object>>())
                .Returns(_ => throw new AppsFlyerRPCException(-1, "boom"));

            await AppsFlyer.DeliverInviteLinkAsync(new Dictionary<string, string>());

            Assert.AreEqual(1, recorder.FailureCallCount);
            Assert.AreEqual("boom", recorder.ReceivedFailure);
            Assert.AreEqual(0, recorder.GeneratedCallCount);
        }

        [Test]
        public void GenerateInviteLink_NoMatchingCallbackObject_DoesNotThrow()
        {
            AppsFlyer.CallBackObjectName = "NoSuchInviteLinkCallbackObject";
            mockRpc.Execute("generateInviteLink", Arg.Any<Dictionary<string, object>>())
                .Returns("https://example.onelink.me/abc");

            Assert.DoesNotThrowAsync(() => AppsFlyer.DeliverInviteLinkAsync(new Dictionary<string, string>()));
        }

        [Test]
        public void RegisterConversionListener_FiresWithNoParams()
        {
            // Per schema: zero declared params on both platforms. TODO (blocking) — see Notion doc:
            // native may depend on callbackObjectName via an undeclared side channel; revisit if
            // conversion-data callbacks stop routing correctly.
            AppsFlyer.registerConversionListener();
            // null and an empty dict are equivalent on the wire (BuildRequest normalizes null
            // params to "{}" — see BuildRequest_NullParams_ProducesEmptyParamsObject).
            mockRpc.Received(1).ExecuteFire("registerConversionListener",
                Arg.Is<Dictionary<string, object>>(d => d == null || d.Count == 0));
        }

        [Test]
        public void WaitForATT_NoLongerFiresAnyRPCCall()
        {
            // waitForATTUserAuthorizationWithTimeoutInterval is deprecated (confirmed out of scope) —
            // removed entirely, no longer a public method. Nothing to call; this test documents the
            // decision so a future re-add doesn't silently reintroduce the invalid "waitForATT" RPC call.
            mockRpc.DidNotReceive().ExecuteFire("waitForATT", Arg.Any<Dictionary<string, object>>());
        }

#if UNITY_ANDROID
        [Test]
        public void SetDisableAdvertisingIdentifiers_Android_SendsIsDisableKey()
        {
            AppsFlyer.setDisableAdvertisingIdentifiers(true);
            mockRpc.Received(1).ExecuteFire("setDisableAdvertisingIdentifiers",
                Arg.Is<Dictionary<string, object>>(d => d.ContainsKey("isDisable") && !d.ContainsKey("disable")));
        }

        [Test]
        public void ValidateAndLogInAppPurchase_Android_FiresWithStringPurchaseType()
        {
            var details = new AFPurchaseDetailsAndroid(AFPurchaseType.Subscription, "token123", "product1");
            AppsFlyer.validateAndLogInAppPurchase(details, null);
            mockRpc.Received(1).ExecuteFire("validateAndLogInAppPurchase",
                Arg.Is<Dictionary<string, object>>(d =>
                    (string)d["purchaseType"] == "subscription" &&
                    (string)d["purchaseToken"] == "token123" &&
                    (string)d["productId"] == "product1"));
        }

        [Test]
        public void ClearUserPii_Android_Fires()
        {
            AppsFlyer.clearUserPii();
            mockRpc.Received(1).ExecuteFire("clearUserPii", Arg.Any<Dictionary<string, object>>());
        }

        [Test]
        public void UpdateServerUninstallToken_Android_SendsTokenKey()
        {
            AppsFlyer.updateServerUninstallToken("fcmtoken");
            mockRpc.Received(1).ExecuteFire("updateServerUninstallToken",
                Arg.Is<Dictionary<string, object>>(d => (string)d["token"] == "fcmtoken"));
        }
#endif

#if (UNITY_IOS || UNITY_STANDALONE_OSX) && !UNITY_ANDROID
        [Test]
        public void SetDisableAdvertisingIdentifiers_iOS_SendsDisableKey()
        {
            AppsFlyer.setDisableAdvertisingIdentifiers(true);
            mockRpc.Received(1).ExecuteFire("setDisableAdvertisingIdentifiers",
                Arg.Is<Dictionary<string, object>>(d => d.ContainsKey("disable") && !d.ContainsKey("isDisable")));
        }

        [Test]
        public void ValidateAndLogInAppPurchase_iOS_SendsNestedProductAndTransaction()
        {
            var details = AFSDKPurchaseDetailsIOS.Init("product1", "txn123", AFSDKPurchaseType.OneTimePurchase);
            AppsFlyer.validateAndLogInAppPurchase(details, null);
            mockRpc.Received(1).ExecuteFire("validateAndLogInAppPurchase",
                Arg.Is<Dictionary<string, object>>(d =>
                    (d["product"] as Dictionary<string, object>) != null &&
                    (string)(d["product"] as Dictionary<string, object>)["productId"] == "product1" &&
                    (d["transaction"] as Dictionary<string, object>) != null &&
                    (string)(d["transaction"] as Dictionary<string, object>)["transactionId"] == "txn123" &&
                    (string)(d["transaction"] as Dictionary<string, object>)["purchaseType"] == "oneTimePurchase"));
        }

        [Test]
        public void UpdateServerUninstallToken_iOS_SendsDeviceTokenKey_NotToken()
        {
            var token = System.Text.Encoding.UTF8.GetBytes("740f4707bebcf74f");
            AppsFlyer.updateServerUninstallToken(token);
            mockRpc.Received(1).ExecuteFire("registerUninstall",
                Arg.Is<Dictionary<string, object>>(d => d.ContainsKey("deviceToken") && !d.ContainsKey("token")));
        }

        [Test]
        public void UpdateServerUninstallToken_iOS_EncodesBytesAsHexString()
        {
            var token = new byte[] { 0x74, 0x0F, 0x47, 0x07 };
            AppsFlyer.updateServerUninstallToken(token);
            mockRpc.Received(1).ExecuteFire("registerUninstall",
                Arg.Is<Dictionary<string, object>>(d => (string)d["deviceToken"] == "740F4707"));
        }

        [Test]
        public void UpdateServerUninstallToken_iOS_NullToken_SendsNullDeviceToken()
        {
            AppsFlyer.updateServerUninstallToken((byte[])null);
            mockRpc.Received(1).ExecuteFire("registerUninstall",
                Arg.Is<Dictionary<string, object>>(d => d["deviceToken"] == null));
        }

        [Test]
        public void UpdateServerUninstallToken_iOS_EmptyToken_SendsEmptyDeviceTokenString()
        {
            AppsFlyer.updateServerUninstallToken(new byte[0]);
            mockRpc.Received(1).ExecuteFire("registerUninstall",
                Arg.Is<Dictionary<string, object>>(d => (string)d["deviceToken"] == ""));
        }

        [Test]
        public void HandlePushNotifications_iOS_SendsPushPayload()
        {
            var payload = new Dictionary<string, object> { { "aps", new Dictionary<string, object>() } };
            AppsFlyer.handlePushNotifications(payload);
            mockRpc.Received(1).ExecuteFire("handlePushNotification",
                Arg.Is<Dictionary<string, object>>(d => d.ContainsKey("pushPayload")));
        }

        [Test]
        public void ClearUserPii_iOS_NowFires()
        {
            // Platform-gap fix: schema defines clearUserPii on both platforms; previously Android-only.
            AppsFlyer.clearUserPii();
            mockRpc.Received(1).ExecuteFire("clearUserPii", Arg.Any<Dictionary<string, object>>());
        }

        [Test]
        public void SetUserFirstName_iOS_NowFires()
        {
            AppsFlyer.setUserFirstName("Jane");
            mockRpc.Received(1).ExecuteFire("setUserFirstName",
                Arg.Is<Dictionary<string, object>>(d => (string)d["firstName"] == "Jane"));
        }

        [Test]
        public void SetUserLastName_iOS_NowFires()
        {
            AppsFlyer.setUserLastName("Doe");
            mockRpc.Received(1).ExecuteFire("setUserLastName",
                Arg.Is<Dictionary<string, object>>(d => (string)d["lastName"] == "Doe"));
        }

        [Test]
        public void SetUserFbLoginId_iOS_NowFires()
        {
            AppsFlyer.setUserFbLoginId(12345L);
            mockRpc.Received(1).ExecuteFire("setUserFbLoginId",
                Arg.Is<Dictionary<string, object>>(d => (long)d["fbLoginId"] == 12345L));
        }

        [Test]
        public void SetUserPhone_iOS_NowFires()
        {
            AppsFlyer.setUserPhone("1", "0501234567");
            mockRpc.Received(1).ExecuteFire("setUserPhone",
                Arg.Is<Dictionary<string, object>>(d => (string)d["phoneNumber"] == "0501234567"));
        }

        [Test]
        public void HandleOpenUrl_iOS_SendsUrlAndOptions()
        {
            // TODO (blocking — see Notion doc): exact shape of `options` unconfirmed against native.
            var options = new Dictionary<string, object>();
            AppsFlyer.handleOpenUrl("www.test.com", options);
            mockRpc.Received(1).ExecuteFire("handleOpenUrl",
                Arg.Is<Dictionary<string, object>>(d => (string)d["url"] == "www.test.com" && d.ContainsKey("options")));
        }

        [Test]
        public void ContinueUserActivity_iOS_Fires()
        {
            AppsFlyer.continueUserActivity("www.test.com", "NSUserActivityTypeBrowsingWeb");
            mockRpc.Received(1).ExecuteFire("continueUserActivity",
                Arg.Is<Dictionary<string, object>>(d => (string)d["url"] == "www.test.com"));
        }

        [Test]
        public void SetDisableCollectASA_iOS_Fires()
        {
            AppsFlyer.setDisableCollectASA(true);
            mockRpc.Received(1).ExecuteFire("setDisableCollectASA", Arg.Any<Dictionary<string, object>>());
        }

        [Test]
        public void SetDisableAppleAdsAttribution_iOS_Fires()
        {
            AppsFlyer.setDisableAppleAdsAttribution(true);
            mockRpc.Received(1).ExecuteFire("setDisableAppleAdsAttribution", Arg.Any<Dictionary<string, object>>());
        }

        [Test]
        public void SetDisableSKAdNetwork_iOS_Fires()
        {
            AppsFlyer.setDisableSKAdNetwork(true);
            mockRpc.Received(1).ExecuteFire("setDisableSKAdNetwork", Arg.Any<Dictionary<string, object>>());
        }

        [Test]
        public void SetDisableIDFVCollection_iOS_Fires()
        {
            AppsFlyer.setDisableIDFVCollection(true);
            mockRpc.Received(1).ExecuteFire("setDisableIDFVCollection", Arg.Any<Dictionary<string, object>>());
        }
#endif

        // ── Already-correct behavior carried over unchanged ─────────────────────────

        [Test]
        public void AnonymizeUser_SendsAnonymizeUser()
        {
            AppsFlyer.anonymizeUser(true);
            mockRpc.Received(1).ExecuteFire("anonymizeUser", Arg.Any<Dictionary<string, object>>());
        }

        [Test]
        public void SetAppInviteOneLink_SendsSetAppInviteOneLink()
        {
            AppsFlyer.setAppInviteOneLink("2f36");
            mockRpc.Received(1).ExecuteFire("setAppInviteOneLink", Arg.Any<Dictionary<string, object>>());
        }

        [Test]
        public void SetOneLinkCustomDomain_SendsSingularNotPlural()
        {
            AppsFlyer.setOneLinkCustomDomain("domain1", "domain2");
            mockRpc.Received(1).ExecuteFire("setOneLinkCustomDomain", Arg.Any<Dictionary<string, object>>());
            mockRpc.DidNotReceive().ExecuteFire("setOneLinkCustomDomains", Arg.Any<Dictionary<string, object>>());
        }

        [Test]
        public void SetHost_UsesHostPrefixNameParam()
        {
            AppsFlyer.setHost("myprefix", "myhost");
            mockRpc.Received(1).ExecuteFire("setHost",
                Arg.Is<Dictionary<string, object>>(d => d.ContainsKey("hostPrefixName") && !d.ContainsKey("prefix")));
        }

        [Test]
        public void EnableDebug_FiresIsDebugRPCMethod()
        {
            AppsFlyer.enableDebug(true);
            mockRpc.Received(1).ExecuteFire("isDebug",
                Arg.Is<Dictionary<string, object>>(d => (bool)d["isDebug"] == true));
        }

        // ── Net-new / migrated getters (Category C) ─────────────────────────────────

        [Test]
        public void GetAppsFlyerUID_UsesSynchronousExecute()
        {
            mockRpc.Execute("getAppsFlyerUID", Arg.Any<Dictionary<string, object>>()).Returns("uid-123");
            string uid = AppsFlyer.getAppsFlyerUID();
            Assert.AreEqual("uid-123", uid);
        }

        [Test]
        public async Task GetAppsFlyerUIDAsync_UsesExecute()
        {
            mockRpc.Execute("getAppsFlyerUID", Arg.Any<Dictionary<string, object>>()).Returns("uid-123");
            string uid = await AppsFlyer.getAppsFlyerUIDAsync();
            Assert.AreEqual("uid-123", uid);
        }

        [Test]
        public void GetSdkVersion_UsesSynchronousExecute()
        {
            mockRpc.Execute("getSdkVersion", Arg.Any<Dictionary<string, object>>()).Returns("7.0.1");
            Assert.AreEqual("7.0.1", AppsFlyer.getSdkVersion());
        }

        [Test]
        public async Task GetSdkVersionAsync_UsesExecute()
        {
            mockRpc.Execute("getSdkVersion", Arg.Any<Dictionary<string, object>>()).Returns("7.0.1");
            Assert.AreEqual("7.0.1", await AppsFlyer.getSdkVersionAsync());
        }

        [Test]
        public async Task IsSessionReadyAsync_UsesExecute()
        {
            mockRpc.Execute("isSessionReady", Arg.Any<Dictionary<string, object>>()).Returns(true);
            Assert.IsTrue(await AppsFlyer.isSessionReadyAsync());
        }

#if UNITY_ANDROID
        [Test]
        public void GetHostName_Android_NetNew_UsesSynchronousExecute()
        {
            mockRpc.Execute("getHostName", Arg.Any<Dictionary<string, object>>()).Returns("appsflyer.com");
            Assert.AreEqual("appsflyer.com", AppsFlyer.getHostName());
        }

        [Test]
        public async Task GetHostNameAsync_Android_UsesExecute()
        {
            mockRpc.Execute("getHostName", Arg.Any<Dictionary<string, object>>()).Returns("appsflyer.com");
            Assert.AreEqual("appsflyer.com", await AppsFlyer.getHostNameAsync());
        }

        [Test]
        public void GetHostPrefix_Android_NetNew_UsesSynchronousExecute()
        {
            mockRpc.Execute("getHostPrefix", Arg.Any<Dictionary<string, object>>()).Returns("prefix");
            Assert.AreEqual("prefix", AppsFlyer.getHostPrefix());
        }

        [Test]
        public async Task GetHostPrefixAsync_Android_UsesExecute()
        {
            mockRpc.Execute("getHostPrefix", Arg.Any<Dictionary<string, object>>()).Returns("prefix");
            Assert.AreEqual("prefix", await AppsFlyer.getHostPrefixAsync());
        }

        [Test]
        public void IsStopped_Android_UsesSynchronousExecute()
        {
            mockRpc.Execute("isStopped", Arg.Any<Dictionary<string, object>>()).Returns(true);
            Assert.IsTrue(AppsFlyer.isStopped());
        }

        [Test]
        public async Task IsStoppedAsync_Android_UsesExecute()
        {
            mockRpc.Execute("isStopped", Arg.Any<Dictionary<string, object>>()).Returns(true);
            Assert.IsTrue(await AppsFlyer.isStoppedAsync());
        }

        [Test]
        public void GetAttributionId_Android_UsesSynchronousExecute()
        {
            mockRpc.Execute("getAttributionId", Arg.Any<Dictionary<string, object>>()).Returns("attr-id");
            Assert.AreEqual("attr-id", AppsFlyer.getAttributionId());
        }

        [Test]
        public async Task GetAttributionIdAsync_Android_UsesExecute()
        {
            mockRpc.Execute("getAttributionId", Arg.Any<Dictionary<string, object>>()).Returns("attr-id");
            Assert.AreEqual("attr-id", await AppsFlyer.getAttributionIdAsync());
        }

        [Test]
        public void GetOutOfStore_Android_UsesSynchronousExecute()
        {
            mockRpc.Execute("getOutOfStore", Arg.Any<Dictionary<string, object>>()).Returns("google_play");
            Assert.AreEqual("google_play", AppsFlyer.getOutOfStore());
        }

        [Test]
        public async Task GetOutOfStoreAsync_Android_UsesExecute()
        {
            mockRpc.Execute("getOutOfStore", Arg.Any<Dictionary<string, object>>()).Returns("google_play");
            Assert.AreEqual("google_play", await AppsFlyer.getOutOfStoreAsync());
        }

        [Test]
        public void IsPreInstalledApp_Android_UsesSynchronousExecute()
        {
            mockRpc.Execute("isPreInstalledApp", Arg.Any<Dictionary<string, object>>()).Returns(true);
            Assert.IsTrue(AppsFlyer.isPreInstalledApp());
        }

        [Test]
        public async Task IsPreInstalledAppAsync_Android_UsesExecute()
        {
            mockRpc.Execute("isPreInstalledApp", Arg.Any<Dictionary<string, object>>()).Returns(true);
            Assert.IsTrue(await AppsFlyer.isPreInstalledAppAsync());
        }

        [Test]
        public void RegisterDeepLinkListener_Android_SendsSubscribeForDeepLink()
        {
            AppsFlyer.registerDeepLinkListener();
            mockRpc.Received(1).ExecuteFire("subscribeForDeepLink", Arg.Any<Dictionary<string, object>>());
        }

        [Test]
        public void CollectDataFromLauncherActivity_Android_SendsCollectDataFromLauncherActivity()
        {
            AppsFlyer.collectDataFromLauncherActivity();
            mockRpc.Received(1).ExecuteFire("collectDataFromLauncherActivity", Arg.Any<Dictionary<string, object>>());
        }
#endif

#if (UNITY_IOS || UNITY_STANDALONE_OSX) && !UNITY_ANDROID
        [Test]
        public void RegisterDeepLinkListener_iOS_SendsRegisterDeeplinkListener()
        {
            AppsFlyer.registerDeepLinkListener();
            mockRpc.Received(1).ExecuteFire("registerDeeplinkListener", Arg.Any<Dictionary<string, object>>());
        }
#endif
    }
}
