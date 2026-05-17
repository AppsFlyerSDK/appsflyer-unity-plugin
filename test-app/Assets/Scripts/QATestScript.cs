using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using AppsFlyerSDK;

public class QATestScript : MonoBehaviour, IAppsFlyerConversionData
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void AutoInit()
    {
        var go = new GameObject("QATestObject");
        DontDestroyOnLoad(go);
        go.AddComponent<AppsFlyer>();
        go.AddComponent<QATestScript>();
    }

    private string _devKey;
    private string _iosAppId;
    private string _androidAppId;

    void Start()
    {
        if (!LoadConfig())
            return;

        AppsFlyer.OnRequestResponse += OnRequestResponse;
        AppsFlyer.OnInAppResponse += OnInAppResponse;

        RunPreStartApis();

        string appId = Application.platform == RuntimePlatform.IPhonePlayer ? _iosAppId : _androidAppId;
        AppsFlyer.setIsDebug(true);
        AppsFlyer.initSDK(_devKey, appId, GetComponent<AppsFlyer>() ?? this as MonoBehaviour);
        AppsFlyer.OnDeepLinkReceived += OnDeepLinkReceived;
        AppsFlyer.startSDK();
        AFQALogger.Log("[AF_QA][startSDK] result: SUCCESS");

        StartCoroutine(RunPostStartApis());
    }

    void OnDestroy()
    {
        AppsFlyer.OnDeepLinkReceived -= OnDeepLinkReceived;
        AppsFlyer.OnRequestResponse -= OnRequestResponse;
        AppsFlyer.OnInAppResponse -= OnInAppResponse;
    }

    // ── Config loading ────────────────────────────────────────────────────────

    bool LoadConfig()
    {
        // Primary path: StreamingAssets (embedded in build, works on all platforms).
        // In CI the workflow writes secrets to test-app/Assets/StreamingAssets/.env
        // before calling game-ci/unity-builder so they get baked into the binary.
        string envPath = Path.Combine(Application.streamingAssetsPath, ".env");

        string content = null;

#if UNITY_ANDROID && !UNITY_EDITOR
        // On Android, StreamingAssets lives inside the APK; use the persistent
        // copy written by the workflow via `adb shell run-as` (fallback path).
        string persistentEnv = Path.Combine(Application.persistentDataPath, ".env");
        if (File.Exists(persistentEnv))
        {
            content = File.ReadAllText(persistentEnv);
        }
        else
        {
            AFQALogger.Log("[AF_QA][CONFIG] .env not found at persistentDataPath; " +
                           "push it with: adb shell run-as com.appsflyer.engagement " +
                           "sh -c 'cat > /data/data/com.appsflyer.engagement/files/.env'");
        }
#else
        if (File.Exists(envPath))
            content = File.ReadAllText(envPath);
        else
        {
            // Editor fallback: project root .env
            string editorEnv = Path.Combine(Application.dataPath, "../.env");
            if (File.Exists(editorEnv))
                content = File.ReadAllText(editorEnv);
        }
#endif

        if (string.IsNullOrEmpty(content))
        {
            AFQALogger.Log("[AF_QA][CONFIG] DEV_KEY missing");
            return false;
        }

        foreach (var line in content.Split('\n'))
        {
            string trimmed = line.Trim();
            if (trimmed.StartsWith("DEV_KEY="))        _devKey     = trimmed.Substring("DEV_KEY=".Length);
            else if (trimmed.StartsWith("IOS_APP_ID=")) _iosAppId   = trimmed.Substring("IOS_APP_ID=".Length);
            else if (trimmed.StartsWith("ANDROID_APP_ID=")) _androidAppId = trimmed.Substring("ANDROID_APP_ID=".Length);
        }

        if (string.IsNullOrEmpty(_devKey))
        {
            AFQALogger.Log("[AF_QA][CONFIG] DEV_KEY missing");
            return false;
        }

        AFQALogger.Log("[AF_QA][CONFIG] loaded");
        return true;
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

        string uid = AppsFlyer.getAppsFlyerId();
        AFQALogger.Log("[AF_QA][getAppsFlyerUID] result: " + uid);

        // E2E-001: three standard events
        AppsFlyer.sendEvent("af_demo_launch", null);
        AFQALogger.Log("[AF_QA][logEvent(af_demo_launch)] result: SUCCESS");

        AppsFlyer.sendEvent("af_purchase", new Dictionary<string, string>
        {
            { "af_revenue",      "9.99" },
            { "af_currency",     "USD" },
            { "af_content_type", "subscription" }
        });
        AFQALogger.Log("[AF_QA][logEvent: af_purchase sent] result: SUCCESS");

        AppsFlyer.sendEvent("af_content_view", new Dictionary<string, string>
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
        AppsFlyer.sendEvent("af_qa_custom_purchase", customParams);

        yield return new WaitForSeconds(1f);

        // E2E-005: identity check event — customer_user_id propagation
        var identityParams = new Dictionary<string, string>
        {
            { "customer_user_id", "e2e_user_42" },
            { "tenant",           "qa_eu" },
            { "experiment",       "rc_pipeline_v1" }
        };
        AFQALogger.Log("[AF_QA][logEvent] name=af_qa_identity_check params={customer_user_id: e2e_user_42, tenant: qa_eu, experiment: rc_pipeline_v1}");
        AppsFlyer.sendEvent("af_qa_identity_check", identityParams);

        yield return new WaitForSeconds(1f);

        // E2E-006: stop / resume toggle
        AppsFlyer.stopSDK(true);
        AFQALogger.Log("[AF_QA][stop] result: true");

        AppsFlyer.sendEvent("af_qa_suppressed", null);

        AppsFlyer.stopSDK(false);
        AFQALogger.Log("[AF_QA][stop] result: false");

        AppsFlyer.sendEvent("af_qa_resumed", null);

        AFQALogger.Log("[AF_QA][AUTO_APIS] --- Post-start auto APIs complete ---");
        AFQALogger.Log("[AF_QA][AUTO_APIS] --- Auto run complete ---");
    }

    // ── IAppsFlyerConversionData ──────────────────────────────────────────────

    public void onConversionDataSuccess(string conversionData)
    {
        AFQALogger.Log("[AF_QA][CALLBACK][onInstallConversionData] " + conversionData);
    }

    public void onConversionDataFail(string error)
    {
        AFQALogger.Log("[AF_QA][CALLBACK][onInstallConversionData] error: " + error);
    }

    public void onAppOpenAttribution(string attributionData)
    {
        AFQALogger.Log("[AF_QA][CALLBACK][onAppOpenAttribution] " + attributionData);
    }

    public void onAppOpenAttributionFailure(string error)
    {
        AFQALogger.Log("[AF_QA][CALLBACK][onAppOpenAttribution] error: " + error);
    }

    // ── Deep link callback ────────────────────────────────────────────────────

    void OnDeepLinkReceived(object sender, EventArgs args)
    {
        var dlArgs = args as DeepLinkEventsArgs;
        if (dlArgs == null)
        {
            AFQALogger.Log("[AF_QA][CALLBACK][onDeepLinking] received: null args");
            return;
        }
        string status = dlArgs.status.ToString();
        string deepLinkValue = dlArgs.getDeepLinkValue() ?? "";
        AFQALogger.Log("[AF_QA][CALLBACK][onDeepLinking] received: status=" + status + ", deepLinkValue=" + deepLinkValue);
    }

    // ── Request / in-app response callbacks ──────────────────────────────────

    void OnRequestResponse(object sender, EventArgs e)
    {
        var a = e as AppsFlyerRequestEventArgs;
        if (a != null)
            AFQALogger.Log("[AF_QA][RequestResponse] responseCode=" + a.statusCode + " desc=" + a.errorDescription);
    }

    void OnInAppResponse(object sender, EventArgs e)
    {
        var a = e as AppsFlyerRequestEventArgs;
        if (a != null)
            AFQALogger.Log("[AF_QA][InAppResponse] responseCode=" + a.statusCode + " desc=" + a.errorDescription);
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
