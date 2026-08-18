using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using AppsFlyerSDK;

// On-screen picker for ad-hoc AppsFlyer API calls, for manual QA on-device.
// Does not touch the automatic init/session flow in QATestScript — this is for
// calling individual APIs after the SDK is already initialized and running.
public class AppsFlyerAPITester : MonoBehaviour
{
    private enum ParamKind { Text, Bool }

    private class Param
    {
        public string Label;
        public ParamKind Kind;
        public string Text;
        public bool Bool;

        public static Param Str(string label, string def = "") => new Param { Label = label, Kind = ParamKind.Text, Text = def };
        public static Param Flag(string label, bool def = false) => new Param { Label = label, Kind = ParamKind.Bool, Bool = def };
    }

    private class ApiEntry
    {
        public string Category;
        public string Name;
        public List<Param> Params = new List<Param>();
        public Action<ApiEntry> Call;
        public string Result = "";
    }

    private List<ApiEntry> _entries;
    private bool _visible;
    private string _filter = "";
    private Vector2 _scroll;
    private bool _dragging;
    private Vector2 _lastDragPos;
    // Matches AppsFlyer.enableDebug(true) already called in QATestScript.InitAsync() — this
    // toggle reflects/overrides that, not a separate initial state.
    private bool _debugEnabled = true;

    void Awake()
    {
        _entries = BuildEntries();
    }

    // ── Parsing helpers for the generic text fields ─────────────────────────────

    private static Dictionary<string, string> KV(string s)
    {
        var d = new Dictionary<string, string>();
        if (string.IsNullOrWhiteSpace(s)) return d;
        foreach (var pair in s.Split(','))
        {
            var kv = pair.Split('=');
            if (kv.Length == 2) d[kv[0].Trim()] = kv[1].Trim();
        }
        return d;
    }

    private static Dictionary<string, object> KVObj(string s)
    {
        var d = new Dictionary<string, object>();
        foreach (var kv in KV(s)) d[kv.Key] = kv.Value;
        return d;
    }

    private static string[] CSV(string s) =>
        string.IsNullOrWhiteSpace(s) ? new string[0] : s.Split(',').Select(x => x.Trim()).ToArray();

    private static T Enum_<T>(string s, T fallback) where T : struct =>
        Enum.TryParse(s, true, out T v) ? v : fallback;

    private static bool? NullableBool(string s) =>
        string.IsNullOrWhiteSpace(s) ? (bool?)null : bool.Parse(s);

    // ── Entry catalogue ──────────────────────────────────────────────────────────

    private List<ApiEntry> BuildEntries()
    {
        var e = new List<ApiEntry>();
        void Add(string category, string name, Action<ApiEntry> call, params Param[] paramsList)
        {
            var entry = new ApiEntry { Category = category, Name = name, Call = call };
            entry.Params.AddRange(paramsList);
            e.Add(entry);
        }
        string P(ApiEntry en, int i) => en.Params[i].Text;
        bool B(ApiEntry en, int i) => en.Params[i].Bool;

        // Lifecycle
        Add("Lifecycle", "start()", en => AppsFlyer.start());
        Add("Lifecycle", "stop(shouldStop)", en => AppsFlyer.stop(B(en, 0)), Param.Flag("shouldStop"));
        Add("Lifecycle", "isSessionReady()", en => en.Result = AppsFlyer.isSessionReady().ToString());
        Add("Lifecycle", "getSdkVersion()", en => en.Result = AppsFlyer.getSdkVersion());
        Add("Lifecycle", "getAppsFlyerUID()", en => en.Result = AppsFlyer.getAppsFlyerUID());

        // Events
        Add("Events", "logEvent(eventName, eventValues)",
            en => AppsFlyer.logEvent(P(en, 0), KV(P(en, 1))),
            Param.Str("eventName", "af_tester_event"), Param.Str("eventValues (k=v,k=v)", "af_content_id=sku_1,af_price=9.99"));

        Add("Events", "logAdRevenue(adRevenueData, additionalParameters)",
            en => AppsFlyer.logAdRevenue(
                new AFAdRevenueData(P(en, 0), Enum_(P(en, 1), MediationNetwork.Custom), P(en, 2), double.Parse(P(en, 3))),
                KV(P(en, 4))),
            Param.Str("monetizationNetwork", "admob"), Param.Str("mediationNetwork (enum name)", "GoogleAdMob"),
            Param.Str("currencyIso4217Code", "USD"), Param.Str("eventRevenue", "1.5"), Param.Str("additionalParameters (k=v,k=v)"));

        Add("Events", "logLocation(latitude, longitude)",
            en => AppsFlyer.logLocation(double.Parse(P(en, 0)), double.Parse(P(en, 1))),
            Param.Str("latitude", "32.0853"), Param.Str("longitude", "34.7818"));

        Add("Events", "logAndOpenStore(promotedAppId, campaign, userParams)",
            en => AppsFlyer.logAndOpenStore(P(en, 0), P(en, 1), KV(P(en, 2))),
            Param.Str("promotedAppId", "com.example.promoted"), Param.Str("campaign", "af_tester_campaign"), Param.Str("userParams (k=v,k=v)"));

        Add("Events", "logCrossPromoteImpression(appId, campaign, userParams)",
            en => AppsFlyer.logCrossPromoteImpression(P(en, 0), P(en, 1), KV(P(en, 2))),
            Param.Str("appId", "com.example.promoted"), Param.Str("campaign", "af_tester_campaign"), Param.Str("userParams (k=v,k=v)"));

        Add("Events", "logInvite(channel, eventParameters)",
            en => AppsFlyer.logInvite(P(en, 0), KV(P(en, 1))),
            Param.Str("channel", "sms"), Param.Str("eventParameters (k=v,k=v)"));

        Add("Events", "logSession()", en => AppsFlyer.logSession());

        // Deep linking
        Add("Deep linking", "performDeepLinking(url, shouldTriggerSession)",
            en => AppsFlyer.performDeepLinking(P(en, 0), B(en, 1)),
            Param.Str("url", "https://example.onelink.me/abcd?deep_link_value=test"), Param.Flag("shouldTriggerSession"));

        Add("Deep linking", "registerDeepLinkListener()", en => AppsFlyer.registerDeepLinkListener());
        Add("Deep linking", "unregisterDeeplinkListener()", en => AppsFlyer.unregisterDeeplinkListener());

        Add("Deep linking", "handleOpenUrl(url, options)",
            en => AppsFlyer.handleOpenUrl(P(en, 0), KVObj(P(en, 1))),
            Param.Str("url", "myapp://open?ref=123"), Param.Str("options (k=v,k=v)"));

        Add("Deep linking", "handleLaunchOptions(launchOptions)",
            en => AppsFlyer.handleLaunchOptions(KVObj(P(en, 0))),
            Param.Str("launchOptions (k=v,k=v)", "UIApplicationLaunchOptionsURLKey=myapp://open"));

        Add("Deep linking", "continueUserActivity(url, activityType)",
            en => AppsFlyer.continueUserActivity(P(en, 0), string.IsNullOrEmpty(P(en, 1)) ? null : P(en, 1)),
            Param.Str("url", "https://example.onelink.me/abcd"), Param.Str("activityType (optional)"));

        Add("Deep linking", "appendParametersToDeepLinkingURL(contains, parameters)",
            en => AppsFlyer.appendParametersToDeepLinkingURL(P(en, 0), KV(P(en, 1))),
            Param.Str("contains", "example.onelink.me"), Param.Str("parameters (k=v,k=v)", "extra=1"));

        Add("Deep linking", "generateInviteLink(parameters)",
            en => AppsFlyer.generateInviteLink(KV(P(en, 0))),
            Param.Str("parameters (k=v,k=v)", "channel=sms"));

        Add("Deep linking", "setResolveDeepLinkURLs(urls)",
            en => AppsFlyer.setResolveDeepLinkURLs(CSV(P(en, 0))),
            Param.Str("urls (comma-separated)", "example.com,example2.com"));

        Add("Deep linking", "setOneLinkCustomDomain(domains)",
            en => AppsFlyer.setOneLinkCustomDomain(CSV(P(en, 0))),
            Param.Str("domains (comma-separated)", "custom.onelink.me"));

        Add("Deep linking", "setDeepLinkTimeout(timeout)",
            en => AppsFlyer.setDeepLinkTimeout(long.Parse(P(en, 0))),
            Param.Str("timeout (ms)", "3000"));

        Add("Deep linking", "setAppInviteOneLink(oneLinkId)",
            en => AppsFlyer.setAppInviteOneLink(P(en, 0)),
            Param.Str("oneLinkId", "abcd"));

        // Conversion / session
        Add("Conversion & session", "registerConversionListener()", en => AppsFlyer.registerConversionListener());
        Add("Conversion & session", "unregisterConversionListener()", en => AppsFlyer.unregisterConversionListener());
        Add("Conversion & session", "registerSessionReadyListener()", en => AppsFlyer.registerSessionReadyListener());
        Add("Conversion & session", "unregisterSessionReadyListener()", en => AppsFlyer.unregisterSessionReadyListener());

        // Identity
        Add("Identity", "setCustomerUserId(customerId)",
            en => AppsFlyer.setCustomerUserId(P(en, 0)),
            Param.Str("customerId", "af_tester_user"));

        Add("Identity", "setUserEmail(email)",
            en => AppsFlyer.setUserEmail(P(en, 0)),
            Param.Str("email", "tester@example.com"));

        Add("Identity", "setUserFirstName(firstName)",
            en => AppsFlyer.setUserFirstName(P(en, 0)),
            Param.Str("firstName", "Ada"));

        Add("Identity", "setUserLastName(lastName)",
            en => AppsFlyer.setUserLastName(P(en, 0)),
            Param.Str("lastName", "Lovelace"));

        Add("Identity", "setUserFbLoginId(fbLoginId)",
            en => AppsFlyer.setUserFbLoginId(long.Parse(P(en, 0))),
            Param.Str("fbLoginId", "1234567890"));

        Add("Identity", "setUserPhone(countryCode, phoneNumber)",
            en => AppsFlyer.setUserPhone(P(en, 0), P(en, 1)),
            Param.Str("countryCode", "1"), Param.Str("phoneNumber", "5551234567"));

        Add("Identity", "clearUserPii()", en => AppsFlyer.clearUserPii());

        Add("Identity", "setInstallId(installId)",
            en => AppsFlyer.setInstallId(P(en, 0)),
            Param.Str("installId", "af_tester_install_id"));

        // Configuration
        Add("Configuration", "setAdditionalData(customData)",
            en => AppsFlyer.setAdditionalData(KV(P(en, 0))),
            Param.Str("customData (k=v,k=v)", "abtest=variantA"));

        Add("Configuration", "setCurrencyCode(currencyCode)",
            en => AppsFlyer.setCurrencyCode(P(en, 0)),
            Param.Str("currencyCode", "USD"));

        Add("Configuration", "setConsentData(GDPR, dataUsage, adsPersonalization, adStorage)",
            en => AppsFlyer.setConsentData(new AppsFlyerConsent(
                NullableBool(P(en, 0)), NullableBool(P(en, 1)), NullableBool(P(en, 2)), NullableBool(P(en, 3)))),
            Param.Str("isUserSubjectToGDPR (true/false/blank)", "true"),
            Param.Str("hasConsentForDataUsage (true/false/blank)", "true"),
            Param.Str("hasConsentForAdsPersonalization (true/false/blank)", "false"),
            Param.Str("hasConsentForAdStorage (true/false/blank)", "true"));

        Add("Configuration", "anonymizeUser(shouldAnonymizeUser)",
            en => AppsFlyer.anonymizeUser(B(en, 0)),
            Param.Flag("shouldAnonymizeUser"));

        Add("Configuration", "enableTCFDataCollection(shouldCollectTcfData)",
            en => AppsFlyer.enableTCFDataCollection(B(en, 0)),
            Param.Flag("shouldCollectTcfData"));

        Add("Configuration", "setMinTimeBetweenSessions(seconds)",
            en => AppsFlyer.setMinTimeBetweenSessions(int.Parse(P(en, 0))),
            Param.Str("seconds", "5"));

        Add("Configuration", "setHost(hostPrefixName, hostName)",
            en => AppsFlyer.setHost(P(en, 0), P(en, 1)),
            Param.Str("hostPrefixName", "prefix"), Param.Str("hostName", "example.com"));

        Add("Configuration", "setPartnerData(partnerId, data)",
            en => AppsFlyer.setPartnerData(P(en, 0), KV(P(en, 1))),
            Param.Str("partnerId", "partner_1"), Param.Str("data (k=v,k=v)", "key=value"));

        Add("Configuration", "enableFacebookDeferredApplinks(isEnabled)",
            en => AppsFlyer.enableFacebookDeferredApplinks(B(en, 0)),
            Param.Flag("isEnabled"));

        Add("Configuration", "setFacebookDeferredAppLink(url)",
            en => AppsFlyer.setFacebookDeferredAppLink(P(en, 0)),
            Param.Str("url", "https://example.com/deferred"));

        Add("Configuration", "setLogLevel(logLevel) [Android]",
            en => AppsFlyer.setLogLevel(P(en, 0)),
            Param.Str("logLevel", "DEBUG"));

        Add("Configuration", "setCurrentDeviceLanguage(language)",
            en => AppsFlyer.setCurrentDeviceLanguage(P(en, 0)),
            Param.Str("language", "en"));

        Add("Configuration", "setAppId(appId) [Android]",
            en => AppsFlyer.setAppId(P(en, 0)),
            Param.Str("appId", "com.example.app"));

        Add("Configuration", "setCollectAndroidID(isCollect) [Android]",
            en => AppsFlyer.setCollectAndroidID(B(en, 0)),
            Param.Flag("isCollect"));

        Add("Configuration", "setIsUpdate(isUpdate)",
            en => AppsFlyer.setIsUpdate(B(en, 0)),
            Param.Flag("isUpdate"));

        Add("Configuration", "setOutOfStore(sourceName)",
            en => AppsFlyer.setOutOfStore(P(en, 0)),
            Param.Str("sourceName", "my_store"));

        Add("Configuration", "getOutOfStore()", en => en.Result = AppsFlyer.getOutOfStore());

        Add("Configuration", "setPreinstallAttribution(mediaSource, campaign, siteId)",
            en => AppsFlyer.setPreinstallAttribution(P(en, 0), P(en, 1), P(en, 2)),
            Param.Str("mediaSource", "preload"), Param.Str("campaign", "preinstall_campaign"), Param.Str("siteId", "site_1"));

        Add("Configuration", "isPreInstalledApp()", en => en.Result = AppsFlyer.isPreInstalledApp().ToString());
        Add("Configuration", "getAttributionId()", en => en.Result = AppsFlyer.getAttributionId());
        Add("Configuration", "getHostName() [Android]", en => en.Result = AppsFlyer.getHostName());
        Add("Configuration", "getHostPrefix() [Android]", en => en.Result = AppsFlyer.getHostPrefix());
        Add("Configuration", "isStopped() [Android]", en => en.Result = AppsFlyer.isStopped().ToString());
        Add("Configuration", "disableAppSetId() [Android]", en => AppsFlyer.disableAppSetId());

        Add("Configuration", "updateServerUninstallToken(token) [Android]",
            en => AppsFlyer.updateServerUninstallToken(P(en, 0)),
            Param.Str("token", "af_tester_push_token"));

        Add("Configuration", "updateServerUninstallToken(deviceToken) [iOS]",
            en => AppsFlyer.updateServerUninstallToken(System.Text.Encoding.UTF8.GetBytes(P(en, 0))),
            Param.Str("deviceToken (raw text, UTF8-encoded)", "af_tester_device_token"));

        // Privacy / disable flags
        Add("Privacy", "setDisableAdvertisingIdentifiers(disable)",
            en => AppsFlyer.setDisableAdvertisingIdentifiers(B(en, 0)), Param.Flag("disable"));
        Add("Privacy", "setDisableAppleAdsAttribution(disable) [iOS]",
            en => AppsFlyer.setDisableAppleAdsAttribution(B(en, 0)), Param.Flag("disable"));
        Add("Privacy", "setDisableCollectASA(disable) [iOS]",
            en => AppsFlyer.setDisableCollectASA(B(en, 0)), Param.Flag("disable"));
        Add("Privacy", "setDisableIDFVCollection(disable) [iOS]",
            en => AppsFlyer.setDisableIDFVCollection(B(en, 0)), Param.Flag("disable"));
        Add("Privacy", "setDisableNetworkData(disable) [Android]",
            en => AppsFlyer.setDisableNetworkData(B(en, 0)), Param.Flag("disable"));
        Add("Privacy", "setDisableSKAdNetwork(disable) [iOS]",
            en => AppsFlyer.setDisableSKAdNetwork(B(en, 0)), Param.Flag("disable"));
        Add("Privacy", "setShouldCollectDeviceName(collect) [iOS]",
            en => AppsFlyer.setShouldCollectDeviceName(B(en, 0)), Param.Flag("collect"));
        Add("Privacy", "setUseReceiptValidationSandbox(sandbox) [iOS]",
            en => AppsFlyer.setUseReceiptValidationSandbox(B(en, 0)), Param.Flag("sandbox"));
        Add("Privacy", "setUseUninstallSandbox(sandbox) [iOS]",
            en => AppsFlyer.setUseUninstallSandbox(B(en, 0)), Param.Flag("sandbox"));

        Add("Privacy", "setSharingFilterForPartners(partners)",
            en => AppsFlyer.setSharingFilterForPartners(CSV(P(en, 0))),
            Param.Str("partners (comma-separated)", "partner_a,partner_b"));
        Add("Privacy", "setSharingFilterForAllPartners()", en => AppsFlyer.setSharingFilterForAllPartners());
        Add("Privacy", "setSharingFilter(partners)",
            en => AppsFlyer.setSharingFilter(CSV(P(en, 0))),
            Param.Str("partners (comma-separated)", "partner_a,partner_b"));

        // Push notifications
        Add("Push", "handlePushNotifications(pushPayload)",
            en => AppsFlyer.handlePushNotifications(KVObj(P(en, 0))),
            Param.Str("pushPayload (k=v,k=v)", "af=1,pid=push_campaign"));

        Add("Push", "sendPushNotificationData(campaign, pid, isRetargeting, additionalParameters)",
            en => AppsFlyer.sendPushNotificationData(P(en, 0), P(en, 1), B(en, 2), KV(P(en, 3))),
            Param.Str("campaign", "push_campaign"), Param.Str("pid", "push_pid"),
            Param.Flag("isRetargeting"), Param.Str("additionalParameters (k=v,k=v)"));

        Add("Push", "addPushNotificationDeepLinkPath(paths)",
            en => AppsFlyer.addPushNotificationDeepLinkPath(CSV(P(en, 0))),
            Param.Str("paths (comma-separated)", "data,deep_link"));

        // In-app purchase validation
        Add("Purchases", "validateAndLogInAppPurchase(details, additionalParameters) [Android]",
            en => AppsFlyer.validateAndLogInAppPurchase(
                new AFPurchaseDetailsAndroid(Enum_(P(en, 0), AFPurchaseType.OneTimePurchase), P(en, 1), P(en, 2)),
                KV(P(en, 3))),
            Param.Str("purchaseType (Subscription/OneTimePurchase)", "OneTimePurchase"),
            Param.Str("purchaseToken", "af_tester_purchase_token"), Param.Str("productId", "product_1"),
            Param.Str("additionalParameters (k=v,k=v)"));

        Add("Purchases", "validateAndLogInAppPurchase(details, additionalParameters) [iOS]",
            en => AppsFlyer.validateAndLogInAppPurchase(
                AFSDKPurchaseDetailsIOS.Init(P(en, 0), P(en, 1), Enum_(P(en, 2), AFSDKPurchaseType.OneTimePurchase)),
                KV(P(en, 3))),
            Param.Str("productId", "product_1"), Param.Str("transactionId", "txn_123"),
            Param.Str("purchaseType (Subscription/OneTimePurchase)", "OneTimePurchase"),
            Param.Str("additionalParameters (k=v,k=v)"));

        return e;
    }

    // ── UI ────────────────────────────────────────────────────────────────────

    // Legacy IMGUI has no DPI awareness — default skin font/control sizes read as
    // near-invisible on a high-res phone. Styles below are sized directly off actual
    // Screen.width (real pixels, same space OnGUI draws in) so they stay touch-usable
    // and readable regardless of device resolution.
    private bool _stylesBuilt;
    private int _fontSize, _headerFontSize;
    private float _controlHeight, _rowGap, _pad;
    private GUIStyle _labelStyle, _headerStyle, _buttonStyle, _fieldStyle, _toggleStyle, _resultStyle, _boxStyle, _entryBoxStyle, _paramLabelStyle;

    private void BuildStyles()
    {
        if (_stylesBuilt) return;
        _stylesBuilt = true;

        _fontSize = Mathf.RoundToInt(Mathf.Clamp(Screen.width * 0.05f, 30f, 64f));
        _headerFontSize = Mathf.RoundToInt(_fontSize * 1.2f);
        _controlHeight = Mathf.Clamp(Screen.width * 0.16f, 90f, 170f);
        _rowGap = _controlHeight * 0.2f;
        _pad = _controlHeight * 0.3f;

        _labelStyle = new GUIStyle(GUI.skin.label) { fontSize = _fontSize, wordWrap = true };
        _paramLabelStyle = new GUIStyle(GUI.skin.label) { fontSize = Mathf.RoundToInt(_fontSize * 0.85f), wordWrap = true };
        _headerStyle = new GUIStyle(GUI.skin.label) { fontSize = _headerFontSize, fontStyle = FontStyle.Bold, wordWrap = true };
        _buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = _fontSize, fixedHeight = _controlHeight };
        _fieldStyle = new GUIStyle(GUI.skin.textField) { fontSize = _fontSize, fixedHeight = _controlHeight };
        _toggleStyle = new GUIStyle(GUI.skin.toggle) { fontSize = _fontSize };
        _resultStyle = new GUIStyle(GUI.skin.label) { fontSize = _fontSize, fontStyle = FontStyle.Italic, wordWrap = true };
        _boxStyle = new GUIStyle(GUI.skin.box) { padding = new RectOffset((int)_pad, (int)_pad, (int)_pad, (int)_pad) };
        _entryBoxStyle = new GUIStyle(GUI.skin.box)
        {
            padding = new RectOffset((int)_pad, (int)_pad, (int)_pad, (int)_pad),
            margin = new RectOffset(0, 0, 0, (int)_rowGap)
        };
    }

    // Legacy IMGUI's ScrollView only scrolls via its thin scrollbar thumb — no touch/click-drag
    // support anywhere in the content, unlike every native mobile scroll view. Track mouse/touch
    // drag deltas ourselves (mobile touch synthesizes mouse events by default) and feed them into
    // the scroll position so the whole panel is swipeable, not just the scrollbar strip.
    private void HandleDragScroll(Rect scrollArea)
    {
        Event ev = Event.current;
        switch (ev.type)
        {
            case EventType.MouseDown:
                if (scrollArea.Contains(ev.mousePosition))
                {
                    _dragging = true;
                    _lastDragPos = ev.mousePosition;
                }
                break;
            case EventType.MouseDrag:
                if (_dragging)
                {
                    Vector2 delta = ev.mousePosition - _lastDragPos;
                    _scroll.y -= delta.y;
                    _lastDragPos = ev.mousePosition;
                }
                break;
            case EventType.MouseUp:
                _dragging = false;
                break;
        }
    }

    // IMGUI word-wrap only breaks at existing whitespace, so a long identifier like
    // "validateAndLogInAppPurchase(details,...)" is one unbreakable token that can be wider
    // than the screen. Insert spaces before "(" and at camelCase boundaries (display-only —
    // logging/filtering still use the original entry.Name) so every entry can wrap and never
    // forces the row — and with it the whole ScrollView — wider than the panel.
    private static string WrapFriendly(string s)
    {
        var sb = new System.Text.StringBuilder(s.Length + 8);
        for (int i = 0; i < s.Length; i++)
        {
            char c = s[i];
            char prev = i > 0 ? s[i - 1] : ' ';
            bool needsSpace = (c == '(' && prev != ' ') ||
                               (char.IsUpper(c) && !char.IsUpper(prev) && prev != ' ' && prev != '(');
            if (needsSpace) sb.Append(' ');
            sb.Append(c);
        }
        return sb.ToString();
    }

    private Rect GetSafeArea()
    {
        Rect safe = Screen.safeArea;
        // Screen.safeArea is bottom-left-origin; GUI space is top-left-origin.
        return new Rect(safe.x, Screen.height - safe.y - safe.height, safe.width, safe.height);
    }

    void OnGUI()
    {
        BuildStyles();
        Rect safeArea = GetSafeArea();

        float margin = _controlHeight * 0.15f;
        var toggleRect = new Rect(safeArea.x + margin, safeArea.y + margin, Mathf.Min(safeArea.width * 0.5f, 500f), _controlHeight);
        if (GUI.Button(toggleRect, _visible ? "Close API Tester" : "API Tester", _buttonStyle))
            _visible = !_visible;

        if (!_visible) return;

        var panel = new Rect(
            safeArea.x + margin,
            toggleRect.yMax + margin,
            safeArea.width - 2 * margin,
            safeArea.yMax - toggleRect.yMax - 2 * margin);
        GUI.Box(panel, "", _boxStyle);

        var inner = new Rect(panel.x + _pad, panel.y + _pad, panel.width - 2 * _pad, panel.height - 2 * _pad);
        GUILayout.BeginArea(inner);

        GUILayout.BeginHorizontal(GUILayout.Height(_controlHeight));
        GUILayout.Label("Debug Mode:", _paramLabelStyle, GUILayout.ExpandWidth(true));
        bool newDebug = GUILayout.Toggle(_debugEnabled, _debugEnabled ? "ON" : "OFF", _toggleStyle, GUILayout.Width(_controlHeight * 1.6f));
        if (newDebug != _debugEnabled)
        {
            _debugEnabled = newDebug;
            AppsFlyer.enableDebug(_debugEnabled);
            AFQALogger.Log("[AF_QA][TESTER] enableDebug -> " + _debugEnabled);
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(_rowGap * 0.5f);
        GUILayout.Label("Filter:", _paramLabelStyle);
        GUILayout.BeginHorizontal();
        _filter = GUILayout.TextField(_filter, _fieldStyle, GUILayout.ExpandWidth(true));
        if (GUILayout.Button("Clear", _buttonStyle, GUILayout.Width(_controlHeight * 1.4f))) _filter = "";
        GUILayout.EndHorizontal();

        GUILayout.Space(_rowGap * 0.5f);
        // Local to the BeginArea(inner) we're already inside — GUILayout.BeginArea remaps
        // Event.current.mousePosition to area-local coordinates, so this rect must be too.
        float headerHeight = _controlHeight * 2 + _rowGap * 2.5f;
        var scrollViewRect = new Rect(0, headerHeight, inner.width, inner.height - headerHeight);
        HandleDragScroll(scrollViewRect);
        // Horizontal scrollbar explicitly off: with WrapFriendly() in place nothing should need
        // it, and this is the safety net — any stray overflow clips at the edge instead of
        // growing the view or popping a horizontal scrollbar.
        _scroll = GUILayout.BeginScrollView(_scroll, GUIStyle.none, GUI.skin.verticalScrollbar);

        string lastCategory = null;
        foreach (var entry in _entries)
        {
            if (!string.IsNullOrEmpty(_filter) &&
                entry.Name.IndexOf(_filter, StringComparison.OrdinalIgnoreCase) < 0 &&
                entry.Category.IndexOf(_filter, StringComparison.OrdinalIgnoreCase) < 0)
                continue;

            if (entry.Category != lastCategory)
            {
                GUILayout.Space(_rowGap * 2);
                GUILayout.Label("── " + entry.Category + " ──", _headerStyle);
                lastCategory = entry.Category;
            }

            GUILayout.BeginVertical(_entryBoxStyle);

            GUILayout.Label(WrapFriendly(entry.Name), _labelStyle);
            if (GUILayout.Button("Call", _buttonStyle))
            {
                try
                {
                    entry.Result = "";
                    entry.Call(entry);
                    if (string.IsNullOrEmpty(entry.Result)) entry.Result = "OK";
                    AFQALogger.Log("[AF_QA][TESTER] " + entry.Name + " -> " + entry.Result);
                }
                catch (Exception ex)
                {
                    entry.Result = "ERROR: " + ex.Message;
                    AFQALogger.Log("[AF_QA][TESTER] " + entry.Name + " -> " + entry.Result);
                }
            }

            foreach (var param in entry.Params)
            {
                GUILayout.Space(_rowGap * 0.5f);
                GUILayout.Label(param.Label, _paramLabelStyle);
                if (param.Kind == ParamKind.Bool)
                    param.Bool = GUILayout.Toggle(param.Bool, param.Bool ? "true" : "false", _toggleStyle);
                else
                    param.Text = GUILayout.TextField(param.Text, _fieldStyle);
            }

            if (!string.IsNullOrEmpty(entry.Result))
            {
                GUILayout.Space(_rowGap * 0.5f);
                GUILayout.Label("Result: " + entry.Result, _resultStyle);
            }

            GUILayout.EndVertical();
        }

        GUILayout.EndScrollView();
        GUILayout.EndArea();
    }
}
