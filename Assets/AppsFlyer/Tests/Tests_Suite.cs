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
            // BuildRequest-generated id back — this exercises the BuildRequest -> Dispatch ->
            // ParseResponse id round trip end to end, on the real AppsFlyerRPCClient, not a mocked
            // IAppsFlyerRPCClient. Note this only covers the Editor stub branch, not the Android/iOS
            // native bridge, so it cannot by itself catch a native-side id-echo mismatch.
            Assert.DoesNotThrow(() => rpc.Execute("getSdkVersion"));
        }

        [Test]
        public void ExecuteFire_EndToEnd_RealDefaultInstance_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => rpc.ExecuteFire("start"));
        }

        // --- onRPCEvent routing tests ---

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
        public void OnRPCEvent_OnSessionReadyEvent_FiresOnSessionReady()
        {
            bool fired = false;
            EventHandler handler = (s, e) => { fired = true; };
            AppsFlyer.OnSessionReady += handler;
            var af = NewAppsFlyerComponent();
            af.onRPCEvent("{\"event\":\"onSessionReady\",\"data\":{}}");
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

        private static readonly string[] StaticCallbackFieldNames =
        {
            "onConversionDataSuccessCallback", "onConversionDataFailCallback", "onDeepLinkListenerCallback"
        };

        [TearDown]
        public void TearDown()
        {
            AppsFlyerRPCClient.instance = AppsFlyerRPCClient.DefaultInstance;
            AppsFlyer.CallBackObjectName = null;
            foreach (var name in StaticCallbackFieldNames)
            {
                typeof(AppsFlyer).GetField(name, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
                    ?.SetValue(null, null);
            }
            foreach (var go in _spawned) UnityEngine.Object.DestroyImmediate(go);
            _spawned.Clear();
        }

        // ── Init / lifecycle ───────────────────────────────────────────────────────

        [Test]
        [Timeout(10000)]
        public async Task Start_FiresStartWithNoParams()
        {
            await AppsFlyer.start();
            mockRpc.Received(1).ExecuteFire("start", Arg.Any<Dictionary<string, object>>());
        }

        [Test]
        [Timeout(10000)]
        public async Task Stop_FiresStopWithShouldStop()
        {
            await AppsFlyer.stop(true);
            mockRpc.Received(1).ExecuteFire("stop",
                Arg.Is<Dictionary<string, object>>(d => (bool)d["shouldStop"] == true));
        }

        [Test]
        [Timeout(10000)]
        public async Task LogEvent_FiresLogEventWithNameAndValues()
        {
            var values = new Dictionary<string, string> { { "key", "value" } };
            await AppsFlyer.logEvent("testevent", values);
            mockRpc.Received(1).ExecuteFire("logEvent",
                Arg.Is<Dictionary<string, object>>(d => (string)d["eventName"] == "testevent" && d["eventValues"] == values));
        }

#if UNITY_ANDROID
        [Test]
        [Timeout(10000)]
        public async Task Init_Android_SendsInitWithDevKeyOnly()
        {
            await AppsFlyer.init("key123", "appId456");
            mockRpc.Received(1).ExecuteFire("init",
                Arg.Is<Dictionary<string, object>>(d => (string)d["devKey"] == "key123" && !d.ContainsKey("appId")));
        }

        [Test]
        [Timeout(10000)]
        public async Task Init_Android_CallsInitBridgeBeforeFiringInit()
        {
            await AppsFlyer.init("key123", "appId456");
            Received.InOrder(() =>
            {
                mockRpc.InitBridge(Arg.Any<string>());
                mockRpc.ExecuteFire("init", Arg.Any<Dictionary<string, object>>());
            });
        }
#endif

#if (UNITY_IOS || UNITY_STANDALONE_OSX) && !UNITY_ANDROID
        [Test]
        [Timeout(10000)]
        public async Task Init_iOS_SendsInitializeWithDevKeyAndAppId()
        {
            await AppsFlyer.init("key123", "appId456");
            mockRpc.Received(1).Execute("initialize",
                Arg.Is<Dictionary<string, object>>(d => (string)d["devKey"] == "key123" && (string)d["appId"] == "appId456"));
        }

        [Test]
        [Timeout(10000)]
        public async Task Init_iOS_CallsInitBridgeBeforeInitializeRpc()
        {
            await AppsFlyer.init("key123", "appId456");
            Received.InOrder(() =>
            {
                mockRpc.InitBridge(Arg.Any<string>());
                mockRpc.Execute("initialize", Arg.Any<Dictionary<string, object>>());
            });
        }
#endif

        // ── Renamed / fixed methods (Category B rewrites) ───────────────────────────

        [Test]
        [Timeout(10000)]
        public async Task SetUserEmail_SendsSingularMethodWithEmailKey()
        {
            await AppsFlyer.setUserEmail("a@b.com");
            mockRpc.Received(1).ExecuteFire("setUserEmail",
                Arg.Is<Dictionary<string, object>>(d => (string)d["email"] == "a@b.com"));
            mockRpc.DidNotReceive().ExecuteFire("setUserEmails", Arg.Any<Dictionary<string, object>>());
        }

        [Test]
        [Timeout(10000)]
        public async Task SetUserPhone_SendsCountryCodeAndPhoneNumber()
        {
            await AppsFlyer.setUserPhone("1", "0501234567");
            mockRpc.Received(1).ExecuteFire("setUserPhone",
                Arg.Is<Dictionary<string, object>>(d =>
                    (string)d["countryCode"] == "1" && (string)d["phoneNumber"] == "0501234567"));
        }

        [Test]
        [Timeout(10000)]
        public async Task LogAndOpenStore_SendsPromotedAppIdNotAppId()
        {
            await AppsFlyer.logAndOpenStore("appid", "campaign", null);
            mockRpc.Received(1).ExecuteFire("logAndOpenStore",
                Arg.Is<Dictionary<string, object>>(d => (string)d["promotedAppId"] == "appid" && !d.ContainsKey("appId")));
        }

        [Test]
        [Timeout(10000)]
        public async Task LogCrossPromoteImpression_SendsAppId()
        {
            await AppsFlyer.logCrossPromoteImpression("appid", "campaign", null);
            mockRpc.Received(1).ExecuteFire("logCrossPromoteImpression",
                Arg.Is<Dictionary<string, object>>(d => (string)d["appId"] == "appid"));
        }

        [Test]
        [Timeout(10000)]
        public async Task LogAdRevenue_SendsMediationNetworkAsString_NotInt()
        {
            var adRevenue = new AFAdRevenueData("network", MediationNetwork.GoogleAdMob, "USD", 1.0);
            await AppsFlyer.logAdRevenue(adRevenue, null);
            mockRpc.Received(1).ExecuteFire("logAdRevenue",
                Arg.Is<Dictionary<string, object>>(d => (string)d["mediationNetwork"] == "google_admob"));
        }

        [Test]
        [Timeout(10000)]
        public async Task LogAdRevenue_MapsEachMediationNetworkToItsCanonicalWireName()
        {
            var expected = new Dictionary<MediationNetwork, string>
            {
                { MediationNetwork.GoogleAdMob, "google_admob" },
                { MediationNetwork.IronSource, "ironsource" },
                { MediationNetwork.ApplovinMax, "applovin_max" },
                { MediationNetwork.Fyber, "fyber" },
                { MediationNetwork.Appodeal, "appodeal" },
                { MediationNetwork.Admost, "admost" },
                { MediationNetwork.Topon, "topon" },
                { MediationNetwork.Tradplus, "tradplus" },
                { MediationNetwork.Yandex, "yandex" },
                { MediationNetwork.ChartBoost, "chartboost" },
                { MediationNetwork.Unity, "unity" },
                { MediationNetwork.ToponPte, "topon_pte" },
                { MediationNetwork.Custom, "custom_mediation" },
                { MediationNetwork.DirectMonetization, "direct_monetization_network" }
            };

            foreach (var pair in expected)
            {
                mockRpc.ClearReceivedCalls();
                var adRevenue = new AFAdRevenueData("network", pair.Key, "USD", 1.0);
                await AppsFlyer.logAdRevenue(adRevenue, null);
                mockRpc.Received(1).ExecuteFire("logAdRevenue",
                    Arg.Is<Dictionary<string, object>>(d => (string)d["mediationNetwork"] == pair.Value));
            }
        }

        [Test]
        [Timeout(10000)]
        public async Task GenerateInviteLink_SpreadsKeysTopLevel_NotNestedUnderParameters()
        {
            var parameters = new Dictionary<string, string> { { "channel", "sms" }, { "campaign", "referral" } };
            await AppsFlyer.generateInviteLink(parameters);
            mockRpc.Received(1).Execute("generateInviteLink",
                Arg.Is<Dictionary<string, object>>(d =>
                    (string)d["channel"] == "sms" && (string)d["campaign"] == "referral" && !d.ContainsKey("parameters")));
        }

        [Test]
        [Timeout(10000)]
        public async Task GenerateInviteLinkAsync_RpcException_ReturnsNull()
        {
            // Matches Query/QueryAsync/QueryValidateAndLogAsync's convention: swallow
            // AppsFlyerRPCException into a safe default rather than throwing out of an
            // Awaitable that may never be observed by the caller.
            mockRpc.Execute("generateInviteLink", Arg.Any<Dictionary<string, object>>())
                .Returns(_ => throw new AppsFlyerRPCException(422, "invalid channel"));
            var result = await AppsFlyer.generateInviteLink(new Dictionary<string, string> { { "channel", "sms" } });
            Assert.IsNull(result);
        }

#if UNITY_ANDROID
        [Test]
        [Timeout(10000)]
        public async Task GenerateInviteLink_Android_RemapsReferrerCustomerIdToCustomerId()
        {
            var parameters = new Dictionary<string, string> { { "referrerCustomerId", "cust-1" } };
            await AppsFlyer.generateInviteLink(parameters);
            mockRpc.Received(1).Execute("generateInviteLink",
                Arg.Is<Dictionary<string, object>>(d =>
                    (string)d["customerId"] == "cust-1" && !d.ContainsKey("referrerCustomerId")));
        }
#else
        [Test]
        [Timeout(10000)]
        public async Task GenerateInviteLink_NonAndroid_PassesReferrerCustomerIdThrough()
        {
            var parameters = new Dictionary<string, string> { { "referrerCustomerId", "cust-1" } };
            await AppsFlyer.generateInviteLink(parameters);
            mockRpc.Received(1).Execute("generateInviteLink",
                Arg.Is<Dictionary<string, object>>(d =>
                    (string)d["referrerCustomerId"] == "cust-1" && !d.ContainsKey("customerId")));
        }
#endif

        [Test]
        [Timeout(10000)]
        public async Task RegisterConversionListener_FiresWithNoParams()
        {
            // Per schema: zero declared params on both platforms.
            await AppsFlyer.registerConversionListener(_ => { }, _ => { });
            // null and an empty dict are equivalent on the wire (BuildRequest normalizes null
            // params to "{}" — see BuildRequest_NullParams_ProducesEmptyParamsObject).
            mockRpc.Received(1).ExecuteFire("registerConversionListener",
                Arg.Is<Dictionary<string, object>>(d => d == null || d.Count == 0));
        }

        [Test]
        [Timeout(10000)]
        public async Task RegisterConversionListener_RoutesConversionEventsToSuppliedCallbacks()
        {
            string success = null, fail = null;
            await AppsFlyer.registerConversionListener(d => success = d, e => fail = e);
            var af = NewAppsFlyerComponent();

            af.onRPCEvent("{\"event\":\"onConversionDataSuccess\",\"data\":{\"af_status\":\"Organic\"}}");
            Assert.IsNotNull(success);
            Assert.IsNull(fail);

            af.onRPCEvent("{\"event\":\"onConversionDataFail\",\"data\":{\"error\":\"boom\"}}");
            Assert.IsNotNull(fail);
        }

        [Test]
        [Timeout(10000)]
        public async Task RegisterDeepLinkListener_RoutesOnDeepLinkingToSuppliedCallback()
        {
            DeepLinkEventsArgs received = null;
            await AppsFlyer.registerDeepLinkListener(args => received = args);
            var af = NewAppsFlyerComponent();

            af.onRPCEvent("{\"event\":\"onDeepLinking\",\"data\":{\"status\":\"FOUND\"}}");
            Assert.IsNotNull(received);
        }

        [Test]
        [Timeout(10000)]
        public async Task RegisterDeepLinkListener_RoutesOnDeepLinkReceivedToSuppliedCallback()
        {
            // "onDeepLinkReceived" is iOS's real native event name per the schema's callbackMappings —
            // the "onDeepLinking" case above covers the generic/Android alias.
            DeepLinkEventsArgs received = null;
            await AppsFlyer.registerDeepLinkListener(args => received = args);
            var af = NewAppsFlyerComponent();

            af.onRPCEvent("{\"event\":\"onDeepLinkReceived\",\"data\":{\"status\":\"FOUND\"}}");
            Assert.IsNotNull(received);
        }

        [Test]
        public void WaitForATT_NoLongerFiresAnyRPCCall()
        {
            // waitForATTUserAuthorizationWithTimeoutInterval is deprecated (confirmed out of scope) —
            // removed entirely, no longer a public method. Asserted via reflection (rather than just
            // calling nothing and checking DidNotReceive, which passes trivially regardless of whether
            // the method still exists) so a future re-add is actually caught by this test.
            var method = typeof(AppsFlyer).GetMethod("waitForATTUserAuthorizationWithTimeoutInterval");
            Assert.IsNull(method, "waitForATTUserAuthorizationWithTimeoutInterval should not exist as a public API");
            mockRpc.DidNotReceive().Execute("waitForATT", Arg.Any<Dictionary<string, object>>());
        }

#if UNITY_ANDROID
        [Test]
        [Timeout(10000)]
        public async Task SetDisableAdvertisingIdentifiers_Android_SendsIsDisableKey()
        {
            await AppsFlyer.setDisableAdvertisingIdentifiers(true);
            mockRpc.Received(1).ExecuteFire("setDisableAdvertisingIdentifiers",
                Arg.Is<Dictionary<string, object>>(d => d.ContainsKey("isDisable") && !d.ContainsKey("disable")));
        }

        [Test]
        [Timeout(10000)]
        public async Task ValidateAndLogInAppPurchase_Android_FiresWithStringPurchaseType()
        {
            var details = new AFPurchaseDetailsAndroid(AFPurchaseType.Subscription, "token123", "product1");
            await AppsFlyer.validateAndLogInAppPurchase(details, null);
            mockRpc.Received(1).Execute("validateAndLogInAppPurchase",
                Arg.Is<Dictionary<string, object>>(d =>
                    (string)d["purchaseType"] == "subscription" &&
                    (string)d["purchaseToken"] == "token123" &&
                    (string)d["productId"] == "product1"));
        }

        [Test]
        [Timeout(10000)]
        public async Task ValidateAndLogInAppPurchase_Android_RpcException_ReturnsErrorResult()
        {
            mockRpc.Execute("validateAndLogInAppPurchase", Arg.Any<Dictionary<string, object>>())
                .Returns(_ => throw new AppsFlyerRPCException(422, "invalid receipt"));
            var details = new AFPurchaseDetailsAndroid(AFPurchaseType.Subscription, "token123", "product1");
            var result = await AppsFlyer.validateAndLogInAppPurchase(details, null);
            Assert.AreEqual(AFSDKValidateAndLogStatus.AFSDKValidateAndLogStatusError, result.status);
            Assert.AreEqual("invalid receipt", result.error);
        }

        [Test]
        [Timeout(10000)]
        public async Task ClearUserPii_Android_Fires()
        {
            await AppsFlyer.clearUserPii();
            mockRpc.Received(1).ExecuteFire("clearUserPii", Arg.Any<Dictionary<string, object>>());
        }

        [Test]
        [Timeout(10000)]
        public async Task UpdateServerUninstallToken_Android_SendsTokenKey()
        {
            await AppsFlyer.updateServerUninstallToken("fcmtoken");
            mockRpc.Received(1).ExecuteFire("updateServerUninstallToken",
                Arg.Is<Dictionary<string, object>>(d => (string)d["token"] == "fcmtoken"));
        }
#endif

#if (UNITY_IOS || UNITY_STANDALONE_OSX) && !UNITY_ANDROID
        [Test]
        [Timeout(10000)]
        public async Task SetDisableAdvertisingIdentifiers_iOS_SendsDisableKey()
        {
            await AppsFlyer.setDisableAdvertisingIdentifiers(true);
            mockRpc.Received(1).ExecuteFire("setDisableAdvertisingIdentifiers",
                Arg.Is<Dictionary<string, object>>(d => d.ContainsKey("disable") && !d.ContainsKey("isDisable")));
        }

        [Test]
        [Timeout(10000)]
        public async Task ValidateAndLogInAppPurchase_iOS_SendsNestedProductAndTransaction()
        {
            var details = AFSDKPurchaseDetailsIOS.Init("product1", "txn123", AFSDKPurchaseType.OneTimePurchase);
            await AppsFlyer.validateAndLogInAppPurchase(details, null);
            mockRpc.Received(1).Execute("validateAndLogInAppPurchase",
                Arg.Is<Dictionary<string, object>>(d =>
                    (d["product"] as Dictionary<string, object>) != null &&
                    (string)(d["product"] as Dictionary<string, object>)["productId"] == "product1" &&
                    (d["transaction"] as Dictionary<string, object>) != null &&
                    (string)(d["transaction"] as Dictionary<string, object>)["transactionId"] == "txn123" &&
                    (string)(d["transaction"] as Dictionary<string, object>)["purchaseType"] == "oneTimePurchase"));
        }

        [Test]
        [Timeout(10000)]
        public async Task ValidateAndLogInAppPurchase_iOS_RpcException_ReturnsErrorResult()
        {
            mockRpc.Execute("validateAndLogInAppPurchase", Arg.Any<Dictionary<string, object>>())
                .Returns(_ => throw new AppsFlyerRPCException(422, "invalid receipt"));
            var details = AFSDKPurchaseDetailsIOS.Init("product1", "txn123", AFSDKPurchaseType.OneTimePurchase);
            var result = await AppsFlyer.validateAndLogInAppPurchase(details, null);
            Assert.AreEqual(AFSDKValidateAndLogStatus.AFSDKValidateAndLogStatusError, result.status);
            Assert.AreEqual("invalid receipt", result.error);
        }

        [Test]
        [Timeout(10000)]
        public async Task UpdateServerUninstallToken_iOS_SendsDeviceTokenKey_NotToken()
        {
            var token = System.Text.Encoding.UTF8.GetBytes("740f4707bebcf74f");
            await AppsFlyer.updateServerUninstallToken(token);
            mockRpc.Received(1).ExecuteFire("registerUninstall",
                Arg.Is<Dictionary<string, object>>(d => d.ContainsKey("deviceToken") && !d.ContainsKey("token")));
        }

        [Test]
        [Timeout(10000)]
        public async Task UpdateServerUninstallToken_iOS_EncodesBytesAsHexString()
        {
            var token = new byte[] { 0x74, 0x0F, 0x47, 0x07 };
            await AppsFlyer.updateServerUninstallToken(token);
            mockRpc.Received(1).ExecuteFire("registerUninstall",
                Arg.Is<Dictionary<string, object>>(d => (string)d["deviceToken"] == "740F4707"));
        }

        [Test]
        [Timeout(10000)]
        public async Task UpdateServerUninstallToken_iOS_NullToken_SendsNullDeviceToken()
        {
            await AppsFlyer.updateServerUninstallToken((byte[])null);
            mockRpc.Received(1).ExecuteFire("registerUninstall",
                Arg.Is<Dictionary<string, object>>(d => d["deviceToken"] == null));
        }

        [Test]
        [Timeout(10000)]
        public async Task UpdateServerUninstallToken_iOS_EmptyToken_SendsEmptyDeviceTokenString()
        {
            await AppsFlyer.updateServerUninstallToken(new byte[0]);
            mockRpc.Received(1).ExecuteFire("registerUninstall",
                Arg.Is<Dictionary<string, object>>(d => (string)d["deviceToken"] == ""));
        }

        [Test]
        [Timeout(10000)]
        public async Task HandlePushNotifications_iOS_SendsPushPayload()
        {
            var payload = new Dictionary<string, object> { { "aps", new Dictionary<string, object>() } };
            await AppsFlyer.handlePushNotifications(payload);
            mockRpc.Received(1).ExecuteFire("handlePushNotification",
                Arg.Is<Dictionary<string, object>>(d => d.ContainsKey("pushPayload")));
        }

        [Test]
        [Timeout(10000)]
        public async Task ClearUserPii_iOS_NowFires()
        {
            // Platform-gap fix: schema defines clearUserPii on both platforms; previously Android-only.
            await AppsFlyer.clearUserPii();
            mockRpc.Received(1).ExecuteFire("clearUserPii", Arg.Any<Dictionary<string, object>>());
        }

        [Test]
        [Timeout(10000)]
        public async Task SetUserFirstName_iOS_NowFires()
        {
            await AppsFlyer.setUserFirstName("Jane");
            mockRpc.Received(1).ExecuteFire("setUserFirstName",
                Arg.Is<Dictionary<string, object>>(d => (string)d["firstName"] == "Jane"));
        }

        [Test]
        [Timeout(10000)]
        public async Task SetUserLastName_iOS_NowFires()
        {
            await AppsFlyer.setUserLastName("Doe");
            mockRpc.Received(1).ExecuteFire("setUserLastName",
                Arg.Is<Dictionary<string, object>>(d => (string)d["lastName"] == "Doe"));
        }

        [Test]
        [Timeout(10000)]
        public async Task SetUserFbLoginId_iOS_NowFires()
        {
            await AppsFlyer.setUserFbLoginId(12345L);
            mockRpc.Received(1).ExecuteFire("setUserFbLoginId",
                Arg.Is<Dictionary<string, object>>(d => (long)d["fbLoginId"] == 12345L));
        }

        [Test]
        [Timeout(10000)]
        public async Task SetUserPhone_iOS_NowFires()
        {
            await AppsFlyer.setUserPhone("1", "0501234567");
            mockRpc.Received(1).ExecuteFire("setUserPhone",
                Arg.Is<Dictionary<string, object>>(d => (string)d["phoneNumber"] == "0501234567"));
        }

        [Test]
        [Timeout(10000)]
        public async Task HandleOpenUrl_iOS_SendsUrlAndOptions()
        {
            // TODO (blocking — see Notion doc): exact shape of `options` unconfirmed against native.
            var options = new Dictionary<string, object>();
            await AppsFlyer.handleOpenUrl("www.test.com", options);
            mockRpc.Received(1).ExecuteFire("handleOpenUrl",
                Arg.Is<Dictionary<string, object>>(d => (string)d["url"] == "www.test.com" && d.ContainsKey("options")));
        }

        [Test]
        [Timeout(10000)]
        public async Task ContinueUserActivity_iOS_Fires()
        {
            await AppsFlyer.continueUserActivity("www.test.com", "NSUserActivityTypeBrowsingWeb");
            mockRpc.Received(1).ExecuteFire("continueUserActivity",
                Arg.Is<Dictionary<string, object>>(d => (string)d["url"] == "www.test.com"));
        }

        [Test]
        [Timeout(10000)]
        public async Task SetDisableCollectASA_iOS_Fires()
        {
            await AppsFlyer.setDisableCollectASA(true);
            mockRpc.Received(1).ExecuteFire("setDisableCollectASA", Arg.Any<Dictionary<string, object>>());
        }

        [Test]
        [Timeout(10000)]
        public async Task SetDisableAppleAdsAttribution_iOS_Fires()
        {
            await AppsFlyer.setDisableAppleAdsAttribution(true);
            mockRpc.Received(1).ExecuteFire("setDisableAppleAdsAttribution", Arg.Any<Dictionary<string, object>>());
        }

        [Test]
        [Timeout(10000)]
        public async Task SetDisableSKAdNetwork_iOS_Fires()
        {
            await AppsFlyer.setDisableSKAdNetwork(true);
            mockRpc.Received(1).ExecuteFire("setDisableSKAdNetwork", Arg.Any<Dictionary<string, object>>());
        }

        [Test]
        [Timeout(10000)]
        public async Task SetDisableIDFVCollection_iOS_Fires()
        {
            await AppsFlyer.setDisableIDFVCollection(true);
            mockRpc.Received(1).ExecuteFire("setDisableIDFVCollection", Arg.Any<Dictionary<string, object>>());
        }
#endif

        // ── Already-correct behavior carried over unchanged ─────────────────────────

        [Test]
        [Timeout(10000)]
        public async Task AnonymizeUser_SendsAnonymizeUser()
        {
            await AppsFlyer.anonymizeUser(true);
            mockRpc.Received(1).ExecuteFire("anonymizeUser", Arg.Any<Dictionary<string, object>>());
        }

        [Test]
        [Timeout(10000)]
        public async Task SetAppInviteOneLink_SendsSetAppInviteOneLink()
        {
            await AppsFlyer.setAppInviteOneLink("2f36");
            mockRpc.Received(1).ExecuteFire("setAppInviteOneLink", Arg.Any<Dictionary<string, object>>());
        }

        [Test]
        [Timeout(10000)]
        public async Task SetOneLinkCustomDomain_SendsSingularNotPlural()
        {
            await AppsFlyer.setOneLinkCustomDomain("domain1", "domain2");
            mockRpc.Received(1).ExecuteFire("setOneLinkCustomDomain", Arg.Any<Dictionary<string, object>>());
            mockRpc.DidNotReceive().ExecuteFire("setOneLinkCustomDomains", Arg.Any<Dictionary<string, object>>());
        }

        [Test]
        [Timeout(10000)]
        public async Task SetHost_UsesHostPrefixNameParam()
        {
            await AppsFlyer.setHost("myprefix", "myhost");
            mockRpc.Received(1).ExecuteFire("setHost",
                Arg.Is<Dictionary<string, object>>(d => d.ContainsKey("hostPrefixName") && !d.ContainsKey("prefix")));
        }

        [Test]
        [Timeout(10000)]
        public async Task EnableDebug_FiresIsDebugRPCMethod()
        {
            await AppsFlyer.enableDebug(true);
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
        [Timeout(10000)]
        public async Task GetAppsFlyerUIDAsync_UsesExecute()
        {
            mockRpc.Execute("getAppsFlyerUID", Arg.Any<Dictionary<string, object>>()).Returns("uid-123");
            string uid = await AppsFlyer.getAppsFlyerUIDAsync();
            Assert.AreEqual("uid-123", uid);
        }

        [Test]
        [Timeout(10000)]
        public async Task GetAppsFlyerUIDAsync_RpcException_ReturnsEmptyString()
        {
            mockRpc.Execute("getAppsFlyerUID", Arg.Any<Dictionary<string, object>>())
                .Returns(_ => throw new AppsFlyerRPCException(-1, "boom"));
            // getAppsFlyerUIDAsync never returns null (QueryAsync's null fallback is coalesced to
            // string.Empty), matching the non-null-guarantee pattern used by the other getters.
            string uid = await AppsFlyer.getAppsFlyerUIDAsync();
            Assert.AreEqual(string.Empty, uid);
        }

        [Test]
        public void GetSdkVersion_UsesSynchronousExecute()
        {
            mockRpc.Execute("getSdkVersion", Arg.Any<Dictionary<string, object>>()).Returns("7.0.1");
            Assert.AreEqual("7.0.1", AppsFlyer.getSdkVersion());
        }

        [Test]
        public void IsRPCBridgeAvailable_DelegatesToRPCClient()
        {
            mockRpc.IsBridgeAvailable.Returns(false);
            Assert.IsFalse(AppsFlyer.isRPCBridgeAvailable());

            mockRpc.IsBridgeAvailable.Returns(true);
            Assert.IsTrue(AppsFlyer.isRPCBridgeAvailable());
        }

        [Test]
        [Timeout(10000)]
        public async Task GetSdkVersionAsync_UsesExecute()
        {
            mockRpc.Execute("getSdkVersion", Arg.Any<Dictionary<string, object>>()).Returns("7.0.1");
            Assert.AreEqual("7.0.1", await AppsFlyer.getSdkVersionAsync());
        }

        [Test]
        [Timeout(10000)]
        public async Task GetSdkVersionAsync_RpcException_ReturnsEmptyString()
        {
            mockRpc.Execute("getSdkVersion", Arg.Any<Dictionary<string, object>>())
                .Returns(_ => throw new AppsFlyerRPCException(-1, "boom"));
            Assert.AreEqual(string.Empty, await AppsFlyer.getSdkVersionAsync());
        }

        [Test]
        [Timeout(10000)]
        public async Task IsSessionReadyAsync_UsesExecute()
        {
            mockRpc.Execute("isSessionReady", Arg.Any<Dictionary<string, object>>()).Returns(true);
            Assert.IsTrue(await AppsFlyer.isSessionReadyAsync());
        }

        [Test]
        [Timeout(10000)]
        public async Task IsSessionReadyAsync_RpcException_ReturnsFalse()
        {
            mockRpc.Execute("isSessionReady", Arg.Any<Dictionary<string, object>>())
                .Returns(_ => throw new AppsFlyerRPCException(-1, "boom"));
            Assert.IsFalse(await AppsFlyer.isSessionReadyAsync());
        }

#if UNITY_ANDROID
        [Test]
        public void GetHostName_Android_NetNew_UsesSynchronousExecute()
        {
            mockRpc.Execute("getHostName", Arg.Any<Dictionary<string, object>>()).Returns("appsflyer.com");
            Assert.AreEqual("appsflyer.com", AppsFlyer.getHostName());
        }

        [Test]
        [Timeout(10000)]
        public async Task GetHostNameAsync_Android_UsesExecute()
        {
            mockRpc.Execute("getHostName", Arg.Any<Dictionary<string, object>>()).Returns("appsflyer.com");
            Assert.AreEqual("appsflyer.com", await AppsFlyer.getHostNameAsync());
        }

        [Test]
        [Timeout(10000)]
        public async Task GetHostNameAsync_Android_RpcException_ReturnsEmptyString()
        {
            mockRpc.Execute("getHostName", Arg.Any<Dictionary<string, object>>())
                .Returns(_ => throw new AppsFlyerRPCException(-1, "boom"));
            Assert.AreEqual(string.Empty, await AppsFlyer.getHostNameAsync());
        }

        [Test]
        public void GetHostPrefix_Android_NetNew_UsesSynchronousExecute()
        {
            mockRpc.Execute("getHostPrefix", Arg.Any<Dictionary<string, object>>()).Returns("prefix");
            Assert.AreEqual("prefix", AppsFlyer.getHostPrefix());
        }

        [Test]
        [Timeout(10000)]
        public async Task GetHostPrefixAsync_Android_UsesExecute()
        {
            mockRpc.Execute("getHostPrefix", Arg.Any<Dictionary<string, object>>()).Returns("prefix");
            Assert.AreEqual("prefix", await AppsFlyer.getHostPrefixAsync());
        }

        [Test]
        [Timeout(10000)]
        public async Task GetHostPrefixAsync_Android_RpcException_ReturnsEmptyString()
        {
            mockRpc.Execute("getHostPrefix", Arg.Any<Dictionary<string, object>>())
                .Returns(_ => throw new AppsFlyerRPCException(-1, "boom"));
            Assert.AreEqual(string.Empty, await AppsFlyer.getHostPrefixAsync());
        }

        [Test]
        public void IsStopped_Android_UsesSynchronousExecute()
        {
            mockRpc.Execute("isStopped", Arg.Any<Dictionary<string, object>>()).Returns(true);
            Assert.IsTrue(AppsFlyer.isStopped());
        }

        [Test]
        [Timeout(10000)]
        public async Task IsStoppedAsync_Android_UsesExecute()
        {
            mockRpc.Execute("isStopped", Arg.Any<Dictionary<string, object>>()).Returns(true);
            Assert.IsTrue(await AppsFlyer.isStoppedAsync());
        }

        [Test]
        [Timeout(10000)]
        public async Task IsStoppedAsync_Android_RpcException_ReturnsFalse()
        {
            mockRpc.Execute("isStopped", Arg.Any<Dictionary<string, object>>())
                .Returns(_ => throw new AppsFlyerRPCException(-1, "boom"));
            Assert.IsFalse(await AppsFlyer.isStoppedAsync());
        }

        [Test]
        public void GetAttributionId_Android_UsesSynchronousExecute()
        {
            mockRpc.Execute("getAttributionId", Arg.Any<Dictionary<string, object>>()).Returns("attr-id");
            Assert.AreEqual("attr-id", AppsFlyer.getAttributionId());
        }

        [Test]
        [Timeout(10000)]
        public async Task GetAttributionIdAsync_Android_UsesExecute()
        {
            mockRpc.Execute("getAttributionId", Arg.Any<Dictionary<string, object>>()).Returns("attr-id");
            Assert.AreEqual("attr-id", await AppsFlyer.getAttributionIdAsync());
        }

        [Test]
        [Timeout(10000)]
        public async Task GetAttributionIdAsync_Android_RpcException_ReturnsEmptyString()
        {
            mockRpc.Execute("getAttributionId", Arg.Any<Dictionary<string, object>>())
                .Returns(_ => throw new AppsFlyerRPCException(-1, "boom"));
            Assert.AreEqual(string.Empty, await AppsFlyer.getAttributionIdAsync());
        }

        [Test]
        public void GetOutOfStore_Android_UsesSynchronousExecute()
        {
            mockRpc.Execute("getOutOfStore", Arg.Any<Dictionary<string, object>>()).Returns("google_play");
            Assert.AreEqual("google_play", AppsFlyer.getOutOfStore());
        }

        [Test]
        [Timeout(10000)]
        public async Task GetOutOfStoreAsync_Android_UsesExecute()
        {
            mockRpc.Execute("getOutOfStore", Arg.Any<Dictionary<string, object>>()).Returns("google_play");
            Assert.AreEqual("google_play", await AppsFlyer.getOutOfStoreAsync());
        }

        [Test]
        [Timeout(10000)]
        public async Task GetOutOfStoreAsync_Android_RpcException_ReturnsEmptyString()
        {
            mockRpc.Execute("getOutOfStore", Arg.Any<Dictionary<string, object>>())
                .Returns(_ => throw new AppsFlyerRPCException(-1, "boom"));
            Assert.AreEqual(string.Empty, await AppsFlyer.getOutOfStoreAsync());
        }

        [Test]
        public void IsPreInstalledApp_Android_UsesSynchronousExecute()
        {
            mockRpc.Execute("isPreInstalledApp", Arg.Any<Dictionary<string, object>>()).Returns(true);
            Assert.IsTrue(AppsFlyer.isPreInstalledApp());
        }

        [Test]
        [Timeout(10000)]
        public async Task IsPreInstalledAppAsync_Android_UsesExecute()
        {
            mockRpc.Execute("isPreInstalledApp", Arg.Any<Dictionary<string, object>>()).Returns(true);
            Assert.IsTrue(await AppsFlyer.isPreInstalledAppAsync());
        }

        [Test]
        [Timeout(10000)]
        public async Task IsPreInstalledAppAsync_Android_RpcException_ReturnsFalse()
        {
            mockRpc.Execute("isPreInstalledApp", Arg.Any<Dictionary<string, object>>())
                .Returns(_ => throw new AppsFlyerRPCException(-1, "boom"));
            Assert.IsFalse(await AppsFlyer.isPreInstalledAppAsync());
        }

        [Test]
        [Timeout(10000)]
        public async Task RegisterDeepLinkListener_Android_SendsSubscribeForDeepLink()
        {
            await AppsFlyer.registerDeepLinkListener(_ => { });
            mockRpc.Received(1).ExecuteFire("subscribeForDeepLink", Arg.Any<Dictionary<string, object>>());
        }

        [Test]
        [Timeout(10000)]
        public async Task CollectDataFromLauncherActivity_Android_SendsCollectDataFromLauncherActivity()
        {
            await AppsFlyer.collectDataFromLauncherActivity();
            mockRpc.Received(1).ExecuteFire("collectDataFromLauncherActivity", Arg.Any<Dictionary<string, object>>());
        }
#endif

#if (UNITY_IOS || UNITY_STANDALONE_OSX) && !UNITY_ANDROID
        [Test]
        [Timeout(10000)]
        public async Task RegisterDeepLinkListener_iOS_SendsRegisterDeeplinkListener()
        {
            await AppsFlyer.registerDeepLinkListener(_ => { });
            mockRpc.Received(1).ExecuteFire("registerDeeplinkListener", Arg.Any<Dictionary<string, object>>());
        }
#endif
    }
}
