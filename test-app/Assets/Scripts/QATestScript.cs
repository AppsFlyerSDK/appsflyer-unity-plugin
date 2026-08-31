using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Networking;
using AppsFlyerSDK;
using System.Text;

public class QATestScript : MonoBehaviour, IAppsFlyerConversionData
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void AutoInit()
    {
        var go = new GameObject("QATestObject");
        DontDestroyOnLoad(go);
        go.AddComponent<AppsFlyer>();
        go.AddComponent<QATestScript>();
        go.AddComponent<AppsFlyerAPITester>();
    }

#if UNITY_IOS
    [DllImport("__Internal")]
    private static extern void _afqaRequestTrackingAuthorization();
#endif

    private string _devKey;
    private string _iosAppId;
    private string _androidAppId;
    private bool _conversionDataReceived = false;

    void Start()
    {
        StartCoroutine(InitAsync());
    }

    void OnDestroy()
    {
        AppsFlyer.OnSessionReady -= OnSessionReadyHandler;
    }

    // ── Initialisation ────────────────────────────────────────────────────────

    IEnumerator InitAsync()
    {
        yield return StartCoroutine(LoadConfig());

        if (string.IsNullOrEmpty(_devKey))
            yield break;

        // Subscribed before registerSessionReadyListener() below so the event can't fire
        // before we're listening. start() is called from inside OnSessionReadyHandler,
        // matching the native contract: call start inside the session-ready block, not
        // unconditionally right after registering it.
        AppsFlyer.OnSessionReady += OnSessionReadyHandler;

        string appId = Application.platform == RuntimePlatform.IPhonePlayer ? _iosAppId : _androidAppId;

#if UNITY_ANDROID
        // Android: registerDeepLinkListener() before init() — the RPC bridge handler isn't
        // wired until AppsFlyer.init() runs (AppsFlyerRPCClient.instance.InitBridge ->
        // AppsFlyerRPCBridge.init()), so this call no-ops natively until init() executes,
        // then the delegate is already in place once the bridge comes up.
        AppsFlyer.registerDeepLinkListener(OnDeepLinkReceived);
        AFQALogger.Log("[AF_QA][registerDeepLinkListener] registered");
#endif

        // init() must come first — on Android it's what wires up the native RPC bridge
        // (AppsFlyerRPCClient.instance.InitBridge -> AppsFlyerRPCBridge.init(), which creates the
        // handler every other Fire()/Query() call needs). Any RPC call issued before this silently
        // no-ops on Android (the bridge's fireJson/executeJson just skip when the handler is null),
        // which is exactly what caused the native "SessionReadyListener is not registered!" warning
        // when registerSessionReadyListener() was called before init().
        AppsFlyer.init(_devKey, appId, GetComponent<AppsFlyer>() ?? this as MonoBehaviour);
        AppsFlyer.enableDebug(true);

#if UNITY_IOS
        // iOS: registerDeepLinkListener() after init() — devKey/appleAppID must already be set
        // natively before setDeepLinkDelegate: runs its one-shot resolve attempt, otherwise that
        // attempt fires with an empty devKey and is never retried (see AppsFlyerLib.m
        // setDeepLinkDelegate:'s dispatch_once).
        AppsFlyer.registerDeepLinkListener(OnDeepLinkReceived);
        AFQALogger.Log("[AF_QA][registerDeepLinkListener] registered");
#endif

#if UNITY_IOS
        // TEMP WORKAROUND (sample app only, pending a real fix in the RPC wrapper's init
        // flow): AppsFlyer.init()'s blocking Query("initialize", ...) call runs on Unity's
        // main thread, which deadlocks against AppsFlyerRPCBridge's @MainActor-isolated
        // completion until the 5s semaphore timeout fires and frees the main thread — but
        // by then this same call stack has already unwound back to Unity's main loop
        // without giving the native run loop a tick to actually run the queued main-actor
        // work that sets devKey/appleAppID. Yielding here lets that pending work run before
        // we call registerSessionReadyListener(), avoiding the native
        // "devKey and appleAppID must be set before calling registerSessionReadyListener:" crash.
        yield return new WaitForSeconds(1f);
#endif

        // SDK 7 flow: session readiness gates start().
        AppsFlyer.registerSessionReadyListener();
        AppsFlyer.registerConversionListener(onConversionDataSuccess, onConversionDataFail);

        RunPreStartApis();

        AFQALogger.Log("[AF_QA][registerSessionReadyListener] registered");

#if UNITY_IOS
        // ATT popup disabled for now (see _afqaRequestTrackingAuthorization in ATTPermissionRequest.mm).
        // Requested here, after init()/registerConversionListener(), so the ATT system prompt's
        // resign/become-active cycle can't race AppsFlyer's own applicationDidBecomeActive:
        // swizzle before devKey/appID are set natively.
        // _afqaRequestTrackingAuthorization();
#endif
    }

    // ── Config loading ────────────────────────────────────────────────────────

    IEnumerator LoadConfig()
    {
        string content = null;

#if UNITY_ANDROID && !UNITY_EDITOR
        // On Android, StreamingAssets are inside the APK — use UnityWebRequest.
        // The CI workflow bakes .env into StreamingAssets before calling unity-builder.
        string url = Path.Combine(Application.streamingAssetsPath, ".env");
        using var req = UnityWebRequest.Get(url);
        yield return req.SendWebRequest();
        if (req.result == UnityWebRequest.Result.Success)
            content = req.downloadHandler.text;
        else
            AFQALogger.Log("[AF_QA][CONFIG] .env read failed: " + req.error);
#else
        // iOS / Editor: StreamingAssets are on the regular filesystem.
        string envPath = Path.Combine(Application.streamingAssetsPath, ".env");
        if (File.Exists(envPath))
            content = File.ReadAllText(envPath);
        else
        {
            string editorEnv = Path.Combine(Application.dataPath, "../.env");
            if (File.Exists(editorEnv))
                content = File.ReadAllText(editorEnv);
        }
        yield return null;
#endif

        if (string.IsNullOrEmpty(content))
        {
            AFQALogger.Log("[AF_QA][CONFIG] DEV_KEY missing");
            yield break;
        }

        foreach (var line in content.Split('\n'))
        {
            string trimmed = line.Trim();
            if (trimmed.StartsWith("DEV_KEY="))             _devKey       = trimmed.Substring("DEV_KEY=".Length);
            else if (trimmed.StartsWith("IOS_APP_ID="))     _iosAppId     = trimmed.Substring("IOS_APP_ID=".Length);
            else if (trimmed.StartsWith("ANDROID_APP_ID=")) _androidAppId = trimmed.Substring("ANDROID_APP_ID=".Length);
        }

        if (string.IsNullOrEmpty(_devKey))
        {
            AFQALogger.Log("[AF_QA][CONFIG] DEV_KEY missing");
            yield break;
        }

        AFQALogger.Log("[AF_QA][CONFIG] loaded");
    }

    // ── Session ready handler (SDK 7) ─────────────────────────────────────────

    void OnSessionReadyHandler(object sender, EventArgs args)
    {
        AppsFlyer.OnSessionReady -= OnSessionReadyHandler;
        AFQALogger.Log("[AF_QA][SESSION_READY] received");
        AppsFlyer.start();
        AFQALogger.Log("[AF_QA][start] result: SUCCESS");
        StartCoroutine(RunPostStartApis());
    }

    // ── Pre-start APIs ────────────────────────────────────────────────────────

    void RunPreStartApis()
    {
        AppsFlyer.setCustomerUserId("e2e_user_42");
        AFQALogger.Log("[AF_QA][setCustomerUserId] result: e2e_user_42");

        AppsFlyer.setCurrencyCode("EUR");
        AFQALogger.Log("[AF_QA][setCurrencyCode] result: EUR");

        var additionalData = new Dictionary<string, string>
        {
            { "tenant",     "qa_eu" },
            { "experiment", "rc_pipeline_v1" }
        };
        AppsFlyer.setAdditionalData(additionalData);
        AFQALogger.Log("[AF_QA][setAdditionalData] tenant=qa_eu experiment=rc_pipeline_v1");

        AFQALogger.Log("[AF_QA][AUTO_APIS] --- Pre-start auto APIs complete ---");
    }

    // ── Post-start APIs ───────────────────────────────────────────────────────

    IEnumerator RunPostStartApis()
    {
        yield return new WaitForSeconds(1f);

        string sdkVersion = AppsFlyer.getSdkVersion();
        AFQALogger.Log("[AF_QA][getSDKVersion] result: " + sdkVersion);

        string uid = AppsFlyer.getAppsFlyerUID();
        AFQALogger.Log("[AF_QA][getAppsFlyerUID] result: " + uid);

        // E2E-001: three standard events
        AppsFlyer.logEvent("af_demo_launch", null);
        AFQALogger.Log("[AF_QA][logEvent(af_demo_launch)] result: SUCCESS");

        AppsFlyer.logEvent("af_purchase", new Dictionary<string, string>
        {
            { "af_revenue",      "9.99" },
            { "af_currency",     "USD" },
            { "af_content_type", "subscription" }
        });
        AFQALogger.Log("[AF_QA][logEvent: af_purchase sent] result: SUCCESS");

        AppsFlyer.logEvent("af_content_view", new Dictionary<string, string>
        {
            { "af_content_id", "qa_content_1" }
        });
        AFQALogger.Log("[AF_QA][logEvent: af_content_view sent] result: SUCCESS");

        // E2E-004: custom event with revenue, currency, and nested metadata
        var customParams = new Dictionary<string, string>
        {
            { "af_revenue", "19.99" },
            { "af_currency", "EUR" },
            { "metadata", "{\"source\":\"qa\",\"variant\":\"A\"}" }
        };
        AFQALogger.Log("[AF_QA][logEvent] name=af_qa_custom_purchase params=" + DictToJson(customParams));
        AppsFlyer.logEvent("af_qa_custom_purchase", customParams);

        yield return new WaitForSeconds(1f);

        // E2E-005: identity check event — customer_user_id propagation
        var identityParams = new Dictionary<string, string>
        {
            { "customer_user_id", "e2e_user_42" },
            { "tenant",           "qa_eu" },
            { "experiment",       "rc_pipeline_v1" }
        };
        AFQALogger.Log("[AF_QA][logEvent] name=af_qa_identity_check params={customer_user_id: e2e_user_42, tenant: qa_eu, experiment: rc_pipeline_v1}");
        AppsFlyer.logEvent("af_qa_identity_check", identityParams);

        // Wait for conversion data before stopping — prevents ClearCache from evicting the
        // in-flight conversion request. Falls back after 120s so the test can still complete.
        float _conversionWaitTimeout = 120f;
        while (!_conversionDataReceived && _conversionWaitTimeout > 0f)
        {
            yield return new WaitForSeconds(1f);
            _conversionWaitTimeout -= 1f;
        }

        // E2E-006: stop / resume toggle
        AppsFlyer.stop(true);
        AFQALogger.Log("[AF_QA][stop] result: true");

        AppsFlyer.logEvent("af_qa_suppressed", null);

        AppsFlyer.stop(false);
        AFQALogger.Log("[AF_QA][stop] result: false");

        AppsFlyer.logEvent("af_qa_resumed", null);

        AFQALogger.Log("[AF_QA][AUTO_APIS] --- Post-start auto APIs complete ---");

        yield return StartCoroutine(RunRPCCoverageApis());

        AFQALogger.Log("[AF_QA][AUTO_APIS] --- Auto run complete ---");
    }

    // ── RPC payload coverage ──────────────────────────────────────────────────

    IEnumerator RunRPCCoverageApis()
    {
        AFQALogger.Log("[AF_QA][RPC_COVERAGE] --- start ---");

        // Configuration
        AppsFlyer.setAppInviteOneLink("rpc_onelink_id");
        AFQALogger.Log("[AF_QA][RPC_COVERAGE] setAppInviteOneLink oneLinkID=rpc_onelink_id");

        AppsFlyer.setDeepLinkTimeout(1500);
        AFQALogger.Log("[AF_QA][RPC_COVERAGE] setDeepLinkTimeout timeout=1500");

        AppsFlyer.setResolveDeepLinkURLs("rpc_url_1", "rpc_url_2", "testunity6.onelink.me");
        AFQALogger.Log("[AF_QA][RPC_COVERAGE] setResolveDeepLinkURLs urls=rpc_url_1,rpc_url_2,testunity6.onelink.me");

        AppsFlyer.setOneLinkCustomDomain("rpc_domain_1", "rpc_domain_2");
        AFQALogger.Log("[AF_QA][RPC_COVERAGE] setOneLinkCustomDomain domains=rpc_domain_1,rpc_domain_2");

        AppsFlyer.setMinTimeBetweenSessions(7);
        AFQALogger.Log("[AF_QA][RPC_COVERAGE] setMinTimeBetweenSessions seconds=7");

        // setHost intentionally not exercised here — it already has unit-test coverage
        // (Tests_Suite.cs, against a mocked RPC client), and calling it here with a real,
        // running SDK would redirect every subsequent network call in this session to a
        // fake domain (see AF_QA logs from earlier "rpc_hostname.com" runs).

        AppsFlyer.setCurrentDeviceLanguage("rpc_lang");
        AFQALogger.Log("[AF_QA][RPC_COVERAGE] setCurrentDeviceLanguage lang=rpc_lang");

        AppsFlyer.setUserPhone("1", "rpc_phone_123");
        AFQALogger.Log("[AF_QA][RPC_COVERAGE] setUserPhone countryCode=1 phone=rpc_phone_123");

        AppsFlyer.setUserEmail("rpc_email@test.com");
        AFQALogger.Log("[AF_QA][RPC_COVERAGE] setUserEmail email=rpc_email@test.com");

        AppsFlyer.setPartnerData("rpc_partner_id", new Dictionary<string, string> { { "rpc_key", "rpc_val" } });
        AFQALogger.Log("[AF_QA][RPC_COVERAGE] setPartnerData partnerId=rpc_partner_id key=rpc_key val=rpc_val");

        AppsFlyer.setAdditionalData(new Dictionary<string, string> { { "rpc_data_key", "rpc_data_val" } });
        AFQALogger.Log("[AF_QA][RPC_COVERAGE] setAdditionalData key=rpc_data_key val=rpc_data_val");

        // Privacy / consent
        AppsFlyer.anonymizeUser(false);
        AFQALogger.Log("[AF_QA][RPC_COVERAGE] anonymizeUser anonymize=false");

        AppsFlyer.enableTCFDataCollection(true);
        AFQALogger.Log("[AF_QA][RPC_COVERAGE] enableTCFDataCollection enabled=true");

        var consent = AppsFlyerConsent.ForGDPRUser(true, true);
        AppsFlyer.setConsentData(consent);
        AFQALogger.Log("[AF_QA][RPC_COVERAGE] setConsentData gdpr=true hasConsent=true hasDataUsageConsent=true");

        // Sharing filters
        AppsFlyer.setSharingFilterForPartners("rpc_partner_a", "rpc_partner_b");
        AFQALogger.Log("[AF_QA][RPC_COVERAGE] setSharingFilterForPartners partners=rpc_partner_a,rpc_partner_b");

        // iOS-specific flags
        AppsFlyer.setDisableCollectASA(false);
        AFQALogger.Log("[AF_QA][RPC_COVERAGE] setDisableCollectASA disabled=false");

        AppsFlyer.setShouldCollectDeviceName(false);
        AFQALogger.Log("[AF_QA][RPC_COVERAGE] setShouldCollectDeviceName shouldCollect=false");

        AppsFlyer.setDisableAppleAdsAttribution(false);
        AFQALogger.Log("[AF_QA][RPC_COVERAGE] setDisableAppleAdsAttribution disabled=false");

        AppsFlyer.setUseReceiptValidationSandbox(true);
        AFQALogger.Log("[AF_QA][RPC_COVERAGE] setUseReceiptValidationSandbox useSandbox=true");

        AppsFlyer.setUseUninstallSandbox(true);
        AFQALogger.Log("[AF_QA][RPC_COVERAGE] setUseUninstallSandbox useSandbox=true");

        AppsFlyer.setDisableSKAdNetwork(false);
        AFQALogger.Log("[AF_QA][RPC_COVERAGE] setDisableSKAdNetwork disabled=false");

        AppsFlyer.setDisableIDFVCollection(false);
        AFQALogger.Log("[AF_QA][RPC_COVERAGE] setDisableIDFVCollection disabled=false");

        // Location / cross-promo
        AppsFlyer.logLocation(37.7749, -122.4194);
        AFQALogger.Log("[AF_QA][RPC_COVERAGE] logLocation lat=37.7749 lon=-122.4194");

        AppsFlyer.logCrossPromoteImpression("rpc_app_id", "rpc_campaign", new Dictionary<string, string> { { "rpc_imp_key", "rpc_imp_val" } });
        AFQALogger.Log("[AF_QA][RPC_COVERAGE] logCrossPromoteImpression appId=rpc_app_id campaign=rpc_campaign key=rpc_imp_key val=rpc_imp_val");

        AppsFlyer.logAndOpenStore("rpc_store_app", "rpc_store_campaign", new Dictionary<string, string> { { "rpc_store_key", "rpc_store_val" } });
        AFQALogger.Log("[AF_QA][RPC_COVERAGE] logAndOpenStore appId=rpc_store_app campaign=rpc_store_campaign key=rpc_store_key val=rpc_store_val");

        // Push / deeplink paths
        AppsFlyer.addPushNotificationDeepLinkPath("rpc_path_root", "rpc_path_child");
        AFQALogger.Log("[AF_QA][RPC_COVERAGE] addPushNotificationDeepLinkPath path=rpc_path_root,rpc_path_child");

        // Ad revenue
        var adRevenue = new AFAdRevenueData("rpc_network", MediationNetwork.GoogleAdMob, "USD", 0.42);
        AppsFlyer.logAdRevenue(adRevenue, new Dictionary<string, string> { { "rpc_rev_key", "rpc_rev_val" } });
        AFQALogger.Log("[AF_QA][RPC_COVERAGE] logAdRevenue network=rpc_network currency=USD amount=0.42 key=rpc_rev_key val=rpc_rev_val");

        // Uninstall token (dummy bytes)
        AppsFlyer.updateServerUninstallToken(Encoding.UTF8.GetBytes("rpc_dummy_token"));
        AFQALogger.Log("[AF_QA][RPC_COVERAGE] updateServerUninstallToken token=rpc_dummy_token");

        // Identifiers
        AppsFlyer.setDisableAdvertisingIdentifiers(false);
        AFQALogger.Log("[AF_QA][RPC_COVERAGE] setDisableAdvertisingIdentifiers disabled=false");

        yield return new WaitForSeconds(1f);

        AFQALogger.Log("[AF_QA][RPC_COVERAGE] --- end ---");
    }

    // ── IAppsFlyerConversionData ──────────────────────────────────────────────

    public void onConversionDataSuccess(string conversionData)
    {
        _conversionDataReceived = true;
        AFQALogger.Log("[AF_QA][CALLBACK][onInstallConversionData] " + conversionData);
    }

    public void onConversionDataFail(string error)
    {
        _conversionDataReceived = true;
        AFQALogger.Log("[AF_QA][CALLBACK][onInstallConversionData] error: " + error);
    }

    // ── Deep link callback ────────────────────────────────────────────────────

    void OnDeepLinkReceived(DeepLinkEventsArgs dlArgs)
    {
        if (dlArgs == null)
        {
            AFQALogger.Log("[AF_QA][CALLBACK][onDeepLinking] received: null args");
            return;
        }
        string status = dlArgs.status.ToString();
        string deepLinkValue = dlArgs.getDeepLinkValue() ?? "";
        AFQALogger.Log("[AF_QA][CALLBACK][onDeepLinking] received: status=" + status + ", deepLinkValue=" + deepLinkValue);
    }

    // ── Utilities ─────────────────────────────────────────────────────────────

    static string DictToJson(Dictionary<string, string> d)
    {
        var parts = new List<string>();
        foreach (var kv in d)
            parts.Add("\"" + kv.Key + "\":\"" + kv.Value + "\"");
        return "{" + string.Join(",", parts) + "}";
    }
}
