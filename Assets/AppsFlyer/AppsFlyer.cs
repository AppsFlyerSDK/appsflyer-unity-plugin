using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using AFMiniJSON;

namespace AppsFlyerSDK
{
    /// <summary>
    /// 100% RPC-schema-aligned implementation (see Assets/AppsFlyer/appsflyer-plugins-rpc-schema.json).
    /// Every public method name and RPC parameter matches the schema's canonical name/keys/types on
    /// both platforms. Pure RPC — no legacy AndroidJavaClass/DllImport bridge involved at all.
    /// See plans/03-rpc-full-schema-alignment-testing.md for the migration/testing plan.
    /// </summary>
    public class AppsFlyer : MonoBehaviour
    {
        public static readonly string kAppsFlyerPluginVersion = "7.0.1";
        public static string CallBackObjectName = null;
        private static EventHandler onRequestResponse;
        private static EventHandler onInAppResponse;
        private static EventHandler onDeepLinkReceived;
        private static EventHandler onSessionReady;
        public delegate void unityCallBack(string message);

        private static void Fire(string method, Dictionary<string, object> parameters = null)
        {
            try { AppsFlyerRPCClient.instance.ExecuteFire(method, parameters); }
            catch (AppsFlyerRPCException e) { AFLog(method, "RPC error: " + e.Message); }
        }

        private static object Query(string method, Dictionary<string, object> parameters = null)
        {
            try { return AppsFlyerRPCClient.instance.Execute(method, parameters); }
            catch (AppsFlyerRPCException e) { AFLog(method, "RPC error: " + e.Message); return null; }
        }

        // On iOS, Execute() blocks on _afExecuteJson's semaphore, which can only be signaled once
        // the main thread is free — calling Query() directly from Unity's main thread deadlocks.
        // QueryAsync hops to a background thread first (matching generateInviteLinkAsync), so every
        // *Async getter below is safe to call from the main thread; the synchronous getters are not.
        private static async Awaitable<object> QueryAsync(string method, Dictionary<string, object> parameters = null)
        {
            await Awaitable.BackgroundThreadAsync();
            try
            {
                return AppsFlyerRPCClient.instance.Execute(method, parameters);
            }
            catch (AppsFlyerRPCException e)
            {
                AFLog(method, "RPC error: " + e.Message);
                return null;
            }
            finally
            {
                await Awaitable.MainThreadAsync();
            }
        }

        // ── Initialization ──────────────────────────────────────────────────────

        /// <summary>
        /// Initialize the AppsFlyer SDK. devKey is required on all platforms; appID is required for iOS
        /// (pass null on Android-only apps).
        /// </summary>
        public static void init(string devKey, string appID, MonoBehaviour gameObject = null)
        {
            if (gameObject != null)
            {
#if UNITY_STANDALONE_OSX
                CallBackObjectName = gameObject.GetType().ToString();
#else
                CallBackObjectName = gameObject.name;
#endif
            }

#if UNITY_ANDROID
            AppsFlyerRPCClient.instance.InitBridge(CallBackObjectName ?? "");
            Fire("init", new Dictionary<string, object> { { "devKey", devKey } });
#elif UNITY_IOS || UNITY_STANDALONE_OSX
            // Wire the RPC -> Unity event channel before initializing, so onRPCEvent (conversion
            // data, deep links, sessionReady) has somewhere to route to as soon as native starts
            // firing events. No-op on macOS standalone, matching Fire/Dispatch's stub behavior there.
            AppsFlyerRPCClient.instance.InitBridge(CallBackObjectName ?? "");
            // Blocking, not Fire: iOS's fire-and-forget path (_afFireJson) dispatches async and
            // returns before native has necessarily set devKey/appId, which can race with an
            // immediately-following registerSessionReadyListener()/start() and crash with
            // "devKey and appleAppID must be set before calling registerSessionReadyListener:".
            // Query blocks (via _afExecuteJson's semaphore) until native actually finishes init.
            Query("initialize", new Dictionary<string, object> { { "devKey", devKey }, { "appId", appID } });
#elif UNITY_WSA_10_0
            AppsFlyerWindows.InitSDK(devKey, appID, gameObject);
#endif
            Fire("setPluginInfo", new Dictionary<string, object>
            {
                { "plugin", "unity" },
                { "pluginVersion", kAppsFlyerPluginVersion }
            });
        }

        /// <summary>Starts the SDK. A session is sent immediately, and on every foreground transition.</summary>
        public static void start()
        {
#if UNITY_WSA_10_0
            AppsFlyerWindows.Start();
#else
            Fire("start");
#endif
        }

        /// <summary>Stops/resumes all SDK activity.</summary>
        public static void stop(bool shouldStop)
        {
            Fire("stop", new Dictionary<string, object> { { "shouldStop", shouldStop } });
        }

        /// <summary>Synchronous RPC query — matches the schema's canonical isSessionReady contract.
        /// On iOS, calling this from Unity's main thread deadlocks (see QueryAsync); prefer
        /// <see cref="isSessionReadyAsync"/>.</summary>
        public static bool isSessionReady()
        {
            return (Query("isSessionReady") as bool?) ?? false;
        }

        /// <summary>Awaitable counterpart of <see cref="isSessionReady"/> — safe to call from the main thread.</summary>
        public static async Awaitable<bool> isSessionReadyAsync()
        {
            return (await QueryAsync("isSessionReady") as bool?) ?? false;
        }

        /// <summary>Gets the AppsFlyer SDK version used by native, via a synchronous RPC query.
        /// On iOS, calling this from Unity's main thread deadlocks (see QueryAsync); prefer
        /// <see cref="getSdkVersionAsync"/>.</summary>
        public static string getSdkVersion()
        {
            return Query("getSdkVersion") as string ?? string.Empty;
        }

        /// <summary>Awaitable counterpart of <see cref="getSdkVersion"/> — safe to call from the main thread.</summary>
        public static async Awaitable<string> getSdkVersionAsync()
        {
            return await QueryAsync("getSdkVersion") as string ?? string.Empty;
        }

        /// <summary>Gets AppsFlyer's unique device ID, via a synchronous RPC query.
        /// On iOS, calling this from Unity's main thread deadlocks (see QueryAsync); prefer
        /// <see cref="getAppsFlyerUIDAsync"/>.</summary>
        public static string getAppsFlyerUID()
        {
#if UNITY_WSA_10_0
            return AppsFlyerWindows.GetAppsFlyerId();
#else
            return Query("getAppsFlyerUID") as string ?? string.Empty;
#endif
        }

        /// <summary>Awaitable counterpart of <see cref="getAppsFlyerUID"/> — safe to call from the main thread.</summary>
        public static async Awaitable<string> getAppsFlyerUIDAsync()
        {
#if UNITY_WSA_10_0
            return AppsFlyerWindows.GetAppsFlyerId();
#else
            return await QueryAsync("getAppsFlyerUID") as string ?? string.Empty;
#endif
        }

        // ── Events ───────────────────────────────────────────────────────────────

        public static void logEvent(string eventName, Dictionary<string, string> eventValues)
        {
#if UNITY_WSA_10_0
            AppsFlyerWindows.LogEvent(eventName, eventValues);
#else
            Fire("logEvent", new Dictionary<string, object> { { "eventName", eventName }, { "eventValues", eventValues } });
#endif
        }

        public static void logAdRevenue(AFAdRevenueData adRevenueData, Dictionary<string, string> additionalParameters)
        {
            Fire("logAdRevenue", new Dictionary<string, object>
            {
                { "monetizationNetwork", adRevenueData?.monetizationNetwork },
                { "mediationNetwork", adRevenueData != null ? adRevenueData.mediationNetwork.ToString() : null },
                { "currencyIso4217Code", adRevenueData?.currencyIso4217Code },
                { "revenue", adRevenueData?.eventRevenue },
                { "additionalParameters", additionalParameters }
            });
        }

        public static void logLocation(double latitude, double longitude)
        {
            Fire("logLocation", new Dictionary<string, object> { { "latitude", latitude }, { "longitude", longitude } });
        }

        /// <summary>Logs a store-open event and has native open the promoted app's store page.</summary>
        public static void logAndOpenStore(string promotedAppId, string campaign, Dictionary<string, string> userParams)
        {
            Fire("logAndOpenStore", new Dictionary<string, object>
            {
                { "promotedAppId", promotedAppId }, { "campaign", campaign }, { "userParams", userParams }
            });
        }

        public static void logCrossPromoteImpression(string appId, string campaign, Dictionary<string, string> userParams)
        {
            Fire("logCrossPromoteImpression", new Dictionary<string, object>
            {
                { "appId", appId }, { "campaign", campaign }, { "userParams", userParams }
            });
        }

        public static void logInvite(string channel, Dictionary<string, string> eventParameters)
        {
            Fire("logInvite", new Dictionary<string, object> { { "channel", channel }, { "eventParameters", eventParameters } });
        }

        /// <summary>Manually records a session. Android only.</summary>
        public static void logSession()
        {
#if UNITY_ANDROID
            Fire("logSession");
#endif
        }

        /// <summary>Collects attribution data from the launcher Activity. Android only.</summary>
        public static void collectDataFromLauncherActivity()
        {
#if UNITY_ANDROID
            Fire("collectDataFromLauncherActivity");
#endif
        }

        /// <summary>
        /// Manually triggers deep-link attribution for the given URL. Manual/advanced-integration escape
        /// hatch only — native already resolves real deep links automatically (Android via AppsFlyerLib's
        /// onResume() UDL hook, iOS via the plugin's AppDelegateListener/swizzle). Calling this alongside
        /// that automatic path has previously caused a real race condition that dropped callbacks
        /// (see commit 2de97096) — do not call if relying on the default automatic integration.
        /// <paramref name="shouldTriggerSession"/> is Android-only; iOS's native SDK has no
        /// equivalent capability, so this parameter has no effect on iOS/macOS.
        /// </summary>
        public static void performDeepLinking(string url, bool shouldTriggerSession = false)
        {
#if UNITY_ANDROID
            Fire("performDeepLinking", new Dictionary<string, object> { { "url", url }, { "shouldTriggerSession", shouldTriggerSession } });
#elif UNITY_IOS || UNITY_STANDALONE_OSX
            Fire("performDeepLinking", new Dictionary<string, object> { { "url", url } });
#endif
        }

        // ── Identity & configuration ──────────────────────────────────────────────

        public static void setCustomerUserId(string customerId)
        {
#if UNITY_WSA_10_0
            AppsFlyerWindows.SetCustomerUserId(customerId);
#else
            Fire("setCustomerUserId", new Dictionary<string, object> { { "customerId", customerId } });
#endif
        }

        public static void setAppInviteOneLink(string oneLinkId)
        {
            Fire("setAppInviteOneLink", new Dictionary<string, object> { { "oneLinkId", oneLinkId } });
        }

        public static void setDeepLinkTimeout(long timeout)
        {
            Fire("setDeepLinkTimeout", new Dictionary<string, object> { { "timeout", timeout } });
        }

        public static void setAdditionalData(Dictionary<string, string> customData)
        {
            Fire("setAdditionalData", new Dictionary<string, object> { { "customData", customData } });
        }

        public static void setResolveDeepLinkURLs(params string[] urls)
        {
            Fire("setResolveDeepLinkURLs", new Dictionary<string, object> { { "urls", urls } });
        }

        public static void setOneLinkCustomDomain(params string[] domains)
        {
            Fire("setOneLinkCustomDomain", new Dictionary<string, object> { { "domains", domains } });
        }

        public static void setCurrencyCode(string currencyCode)
        {
            Fire("setCurrencyCode", new Dictionary<string, object> { { "currencyCode", currencyCode } });
        }

        public static void setConsentData(AppsFlyerConsent appsFlyerConsent)
        {
            Fire("setConsentData", new Dictionary<string, object>
            {
                { "isUserSubjectToGDPR", appsFlyerConsent?.isUserSubjectToGDPR },
                { "hasConsentForDataUsage", appsFlyerConsent?.hasConsentForDataUsage },
                { "hasConsentForAdsPersonalization", appsFlyerConsent?.hasConsentForAdsPersonalization },
                { "hasConsentForAdStorage", appsFlyerConsent?.hasConsentForAdStorage }
            });
        }

        public static void anonymizeUser(bool shouldAnonymizeUser)
        {
            Fire("anonymizeUser", new Dictionary<string, object> { { "shouldAnonymize", shouldAnonymizeUser } });
        }

        public static void enableTCFDataCollection(bool shouldCollectTcfData)
        {
            Fire("enableTCFDataCollection", new Dictionary<string, object> { { "shouldCollect", shouldCollectTcfData } });
        }

        public static void setMinTimeBetweenSessions(int seconds)
        {
            Fire("setMinTimeBetweenSessions", new Dictionary<string, object> { { "seconds", seconds } });
        }

        public static void setHost(string hostPrefixName, string hostName)
        {
            Fire("setHost", new Dictionary<string, object> { { "hostPrefixName", hostPrefixName }, { "hostName", hostName } });
        }

        public static void setInstallId(string installId)
        {
            Fire("setInstallId", new Dictionary<string, object> { { "installId", installId } });
        }

        /// <summary>Enables SDK debug logs. Public name and parameter follow the schema's canonical
        /// "enableDebug(enabled)"; the wire RPC method both platforms actually implement is "isDebug".</summary>
        public static void enableDebug(bool enabled)
        {
            Fire("isDebug", new Dictionary<string, object> { { "isDebug", enabled } });
        }

        public static void setPartnerData(string partnerId, Dictionary<string, string> data)
        {
            Fire("setPartnerData", new Dictionary<string, object> { { "partnerId", partnerId }, { "data", data } });
        }

        public static void appendParametersToDeepLinkingURL(string contains, Dictionary<string, string> parameters)
        {
            Fire("appendParametersToDeepLinkingURL", new Dictionary<string, object> { { "contains", contains }, { "parameters", parameters } });
        }

        public static void enableFacebookDeferredApplinks(bool isEnabled)
        {
            Fire("enableFacebookDeferredApplinks", new Dictionary<string, object> { { "isEnabled", isEnabled } });
        }

        /// <summary>Sets the user's email (single address — the schema does not support multiple
        /// emails or a crypt-type parameter; those existed in the old off-schema "setUserEmails" call).</summary>
        public static void setUserEmail(string email)
        {
            Fire("setUserEmail", new Dictionary<string, object> { { "email", email } });
        }

        public static void setUserFirstName(string firstName)
        {
            Fire("setUserFirstName", new Dictionary<string, object> { { "firstName", firstName } });
        }

        public static void setUserLastName(string lastName)
        {
            Fire("setUserLastName", new Dictionary<string, object> { { "lastName", lastName } });
        }

        public static void setUserFbLoginId(long fbLoginId)
        {
            Fire("setUserFbLoginId", new Dictionary<string, object> { { "fbLoginId", fbLoginId } });
        }

        public static void setUserPhone(string countryCode, string phoneNumber)
        {
            Fire("setUserPhone", new Dictionary<string, object> { { "countryCode", countryCode }, { "phoneNumber", phoneNumber } });
        }

        public static void clearUserPii()
        {
            Fire("clearUserPii");
        }

        /// <summary>Sets the SDK log level. Accepted values (case-insensitive): none/error/warning/info/debug/verbose.
        /// Android only.</summary>
        public static void setLogLevel(string logLevel)
        {
#if UNITY_ANDROID
            Fire("setLogLevel", new Dictionary<string, object> { { "logLevel", logLevel?.ToUpperInvariant() } });
#endif
        }

        // ── Deep linking & conversion data ─────────────────────────────────────────

        /// <summary>
        /// Registers a conversion-data listener. Callback delivery is routed through the unified
        /// onRPCEvent envelope to CallBackObjectName (set in init), not by an RPC parameter.
        /// Resolved: schema declares zero params (maxProperties: 0) on both platforms. Confirmed there
        /// is no undeclared callbackObjectName side channel — AppsFlyerRPCBridge.init() (Android) and
        /// _setRPCEventHandler (iOS) each wire one generic event handler at SDK init that routes every
        /// RPC event, including conversion callbacks, through onRPCEvent to CallBackObjectName.
        /// </summary>
        public static void registerConversionListener()
        {
#if UNITY_WSA_10_0
            AppsFlyerWindows.GetConversionData("");
#else
            Fire("registerConversionListener");
#endif
        }

        /// <summary>Android only.</summary>
        public static void unregisterConversionListener()
        {
#if UNITY_ANDROID
            Fire("unregisterConversionListener");
#endif
        }

        private static bool _androidDeepLinkLoggerAttached = false;

        /// <summary>
        /// Subscribes for the unified deep-link event. Manual/advanced-integration escape hatch — native
        /// already resolves deep links automatically on both platforms (see performDeepLinking doc comment
        /// for the same race-condition warning). This is called automatically from OnDeepLinkReceived's
        /// add accessor.
        /// </summary>
        public static void registerDeepLinkListener()
        {
#if UNITY_ANDROID
            Fire("subscribeForDeepLink");

            // Guarantee onDeepLinking is observable even if the caller registers via
            // registerDeepLinkListener() directly instead of OnDeepLinkReceived +=, since
            // onDeepLinking() only invokes onDeepLinkReceived when it has a subscriber.
            if (!_androidDeepLinkLoggerAttached)
            {
                onDeepLinkReceived += (sender, args) =>
                {
                    var dlArgs = args as DeepLinkEventsArgs;
                    AFLog("onDeepLinking", dlArgs != null ? dlArgs.getDeepLinkValue() : "received");
                };
                _androidDeepLinkLoggerAttached = true;
            }
#elif UNITY_IOS || UNITY_STANDALONE_OSX
            Fire("registerDeeplinkListener");
#endif
        }

        /// <summary>Android only.</summary>
        public static void unregisterDeeplinkListener()
        {
#if UNITY_ANDROID
            Fire("unsubscribeForDeepLink");
#endif
        }

        public static void registerSessionReadyListener()
        {
            Fire("registerSessionReadyListener");
        }

        public static void unregisterSessionReadyListener()
        {
            Fire("unregisterSessionReadyListener");
        }

        /// <summary>
        /// Handles a URL open (iOS-only capability in the schema).
        /// Resolved: schema declares `options` as free-form (additionalProperties: true, no fixed shape),
        /// matching iOS's native UIApplicationOpenURLOptionsKey dictionary, which has no fixed shape
        /// either. An open Dictionary&lt;string, object&gt; is the correct signature, not a placeholder.
        /// </summary>
        public static void handleOpenUrl(string url, Dictionary<string, object> options = null)
        {
#if UNITY_IOS || UNITY_STANDALONE_OSX
            Fire("handleOpenUrl", new Dictionary<string, object> { { "url", url }, { "options", options } });
#endif
        }

        /// <summary>Passes launch options to the SDK for cold-start attribution. iOS only. Manual/advanced
        /// escape hatch — no native caller found for this capability anywhere in this repo; verify it's
        /// actually needed before relying on it.</summary>
        public static void handleLaunchOptions(Dictionary<string, object> launchOptions)
        {
#if UNITY_IOS || UNITY_STANDALONE_OSX
            Fire("handleLaunchOptions", new Dictionary<string, object> { { "launchOptions", launchOptions } });
#endif
        }

        /// <summary>Handles a Universal Link for deep-link attribution. iOS only. Manual/advanced
        /// escape hatch — native's AppDelegateListener/swizzle already forwards this automatically;
        /// do not call if relying on the default automatic integration.</summary>
        public static void continueUserActivity(string url, string activityType = null)
        {
#if UNITY_IOS || UNITY_STANDALONE_OSX
            Fire("continueUserActivity", new Dictionary<string, object> { { "url", url }, { "activityType", activityType } });
#endif
        }

        /// <summary>Forwards a push payload to native for attribution. iOS only — the schema declares no
        /// Android RPC method for this; Android push handling happens natively without a Unity call.</summary>
        public static void handlePushNotifications(Dictionary<string, object> pushPayload)
        {
#if UNITY_IOS || UNITY_STANDALONE_OSX
            Fire("handlePushNotification", new Dictionary<string, object> { { "pushPayload", pushPayload } });
#endif
        }

        /// <summary>Android only.</summary>
        public static void sendPushNotificationData(string campaign, string pid, bool isRetargeting, Dictionary<string, string> additionalParameters = null)
        {
#if UNITY_ANDROID
            Fire("sendPushNotificationData", new Dictionary<string, object>
            {
                { "campaign", campaign }, { "pid", pid }, { "isRetargeting", isRetargeting }, { "additionalParameters", additionalParameters }
            });
#endif
        }

        public static void addPushNotificationDeepLinkPath(params string[] paths)
        {
            Fire("addPushNotificationDeepLinkPath", new Dictionary<string, object> { { "deepLinkPath", paths } });
        }

        /// <summary>
        /// `parameters` is spread as top-level RPC keys (channel, campaign, referrerName,
        /// referrerImageUrl, referrerCustomerId, baseDeepLink, brandDomain, etc.), not nested under a
        /// single "parameters" key. Use the canonical "referrerCustomerId" key on both platforms —
        /// Android's wire remap to "customerId" is handled internally. A raw "customerId" key (Android's
        /// old wire key) is still passed through as-is for backward compatibility.
        /// </summary>
        private static Dictionary<string, object> BuildInviteLinkPayload(Dictionary<string, string> parameters)
        {
            var payload = new Dictionary<string, object>();
            if (parameters != null)
            {
                foreach (var kv in parameters)
                {
#if UNITY_ANDROID
                    var key = kv.Key == "referrerCustomerId" ? "customerId" : kv.Key;
#else
                    var key = kv.Key;
#endif
                    payload[key] = kv.Value;
                }
            }
            return payload;
        }

        /// <summary>
        /// Generates a OneLink user-invite link and returns it directly to the caller. The schema
        /// declares a string "result" for this RPC method on both platforms — native blocks internally
        /// until the link is generated over the network — so the Execute() call itself happens off the
        /// main thread via Awaitable.BackgroundThreadAsync(), instead of blocking Unity's player loop for
        /// the round trip.
        /// </summary>
        public static async Awaitable<string> generateInviteLinkAsync(Dictionary<string, string> parameters)
        {
            var payload = BuildInviteLinkPayload(parameters);
            await Awaitable.BackgroundThreadAsync();
            try
            {
                return AppsFlyerRPCClient.instance.Execute("generateInviteLink", payload) as string;
            }
            finally
            {
                await Awaitable.MainThreadAsync();
            }
        }

        /// <summary>
        /// Builds and fires a OneLink user-invite request; delivers the outcome to CallBackObjectName via
        /// the pre-RPC IAppsFlyerUserInvite callback names (onInviteLinkGenerated /
        /// onInviteLinkGeneratedFailure), since onRPCEvent has no routing for this call. Thin wrapper
        /// around <see cref="generateInviteLinkAsync"/>; prefer calling that directly to get the link
        /// without going through CallBackObjectName.
        /// </summary>
        public static async void generateInviteLink(Dictionary<string, string> parameters)
        {
            await DeliverInviteLinkAsync(parameters);
        }

        // Task-returning so tests can await the exact delivery logic generateInviteLink fires-and-forgets.
        internal static async Task DeliverInviteLinkAsync(Dictionary<string, string> parameters)
        {
            var go = string.IsNullOrEmpty(CallBackObjectName) ? null : GameObject.Find(CallBackObjectName);
            try
            {
                var link = await generateInviteLinkAsync(parameters);
                go?.SendMessage("onInviteLinkGenerated", link, SendMessageOptions.DontRequireReceiver);
            }
            catch (AppsFlyerRPCException e)
            {
                AFLog("generateInviteLink", "RPC error: " + e.Message);
                go?.SendMessage("onInviteLinkGeneratedFailure", e.Message, SendMessageOptions.DontRequireReceiver);
            }
        }

        // ── Advertising identifiers & privacy ─────────────────────────────────────

        /// <summary>Android's RPC parameter key is "isDisable"; iOS's is "disable" — the schema declares
        /// different key names per platform for this capability.</summary>
        public static void setDisableAdvertisingIdentifiers(bool disable)
        {
#if UNITY_ANDROID
            Fire("setDisableAdvertisingIdentifiers", new Dictionary<string, object> { { "isDisable", disable } });
#elif UNITY_IOS || UNITY_STANDALONE_OSX
            Fire("setDisableAdvertisingIdentifiers", new Dictionary<string, object> { { "disable", disable } });
#endif
        }

        /// <summary>iOS only.</summary>
        public static void setDisableAppleAdsAttribution(bool disable)
        {
#if UNITY_IOS || UNITY_STANDALONE_OSX
            Fire("setDisableAppleAdsAttribution", new Dictionary<string, object> { { "disable", disable } });
#endif
        }

        /// <summary>iOS only.</summary>
        public static void setDisableCollectASA(bool disable)
        {
#if UNITY_IOS || UNITY_STANDALONE_OSX
            Fire("setDisableCollectASA", new Dictionary<string, object> { { "disable", disable } });
#endif
        }

        /// <summary>iOS only.</summary>
        public static void setDisableIDFVCollection(bool disable)
        {
#if UNITY_IOS || UNITY_STANDALONE_OSX
            Fire("setDisableIDFVCollection", new Dictionary<string, object> { { "disable", disable } });
#endif
        }

        /// <summary>Android only.</summary>
        public static void setDisableNetworkData(bool isDisable)
        {
#if UNITY_ANDROID
            Fire("setDisableNetworkData", new Dictionary<string, object> { { "isDisable", isDisable } });
#endif
        }

        /// <summary>iOS only.</summary>
        public static void setDisableSKAdNetwork(bool disable)
        {
#if UNITY_IOS || UNITY_STANDALONE_OSX
            Fire("setDisableSKAdNetwork", new Dictionary<string, object> { { "disable", disable } });
#endif
        }

        /// <summary>iOS only.</summary>
        public static void setFacebookDeferredAppLink(string url)
        {
#if UNITY_IOS || UNITY_STANDALONE_OSX
            Fire("setFacebookDeferredAppLink", new Dictionary<string, object> { { "url", url } });
#endif
        }

        /// <summary>iOS only.</summary>
        public static void setShouldCollectDeviceName(bool collect)
        {
#if UNITY_IOS || UNITY_STANDALONE_OSX
            Fire("setShouldCollectDeviceName", new Dictionary<string, object> { { "collect", collect } });
#endif
        }

        /// <summary>iOS only.</summary>
        public static void setUseReceiptValidationSandbox(bool sandbox)
        {
#if UNITY_IOS || UNITY_STANDALONE_OSX
            Fire("setUseReceiptValidationSandbox", new Dictionary<string, object> { { "sandbox", sandbox } });
#endif
        }

        /// <summary>iOS only.</summary>
        public static void setUseUninstallSandbox(bool sandbox)
        {
#if UNITY_IOS || UNITY_STANDALONE_OSX
            Fire("setUseUninstallSandbox", new Dictionary<string, object> { { "sandbox", sandbox } });
#endif
        }

        /// <summary>iOS only.</summary>
        public static void setCurrentDeviceLanguage(string language)
        {
#if UNITY_IOS || UNITY_STANDALONE_OSX
            Fire("setCurrentDeviceLanguage", new Dictionary<string, object> { { "language", language } });
#endif
        }

        public static void setSharingFilterForPartners(params string[] partners)
        {
            Fire("setSharingFilterForPartners", new Dictionary<string, object> { { "partners", partners } });
        }

        [Obsolete("Please use setSharingFilterForPartners")]
        public static void setSharingFilterForAllPartners() => setSharingFilterForPartners("all");

        [Obsolete("Please use setSharingFilterForPartners")]
        public static void setSharingFilter(params string[] partners) => setSharingFilterForPartners(partners);

        // ── Android-only ─────────────────────────────────────────────────────────

        public static void setAppId(string appId)
        {
#if UNITY_ANDROID
            Fire("setAppId", new Dictionary<string, object> { { "appId", appId } });
#endif
        }

        public static void setCollectAndroidID(bool isCollect)
        {
#if UNITY_ANDROID
            Fire("setCollectAndroidID", new Dictionary<string, object> { { "isCollect", isCollect } });
#endif
        }

        public static void setIsUpdate(bool isUpdate)
        {
#if UNITY_ANDROID
            Fire("setIsUpdate", new Dictionary<string, object> { { "isUpdate", isUpdate } });
#endif
        }

        public static void setOutOfStore(string sourceName)
        {
#if UNITY_ANDROID
            Fire("setOutOfStore", new Dictionary<string, object> { { "sourceName", sourceName } });
#endif
        }

        /// <summary>Synchronous RPC query. Android only. Prefer <see cref="getOutOfStoreAsync"/> to avoid
        /// blocking the calling thread.</summary>
        public static string getOutOfStore()
        {
#if UNITY_ANDROID
            return Query("getOutOfStore") as string ?? string.Empty;
#else
            return string.Empty;
#endif
        }

        /// <summary>Awaitable counterpart of <see cref="getOutOfStore"/> — safe to call from the main thread.</summary>
        public static async Awaitable<string> getOutOfStoreAsync()
        {
#if UNITY_ANDROID
            return await QueryAsync("getOutOfStore") as string ?? string.Empty;
#else
            return string.Empty;
#endif
        }

        public static void setPreinstallAttribution(string mediaSource, string campaign, string siteId)
        {
#if UNITY_ANDROID
            Fire("setPreinstallAttribution", new Dictionary<string, object>
            {
                { "mediaSource", mediaSource }, { "campaign", campaign }, { "siteId", siteId }
            });
#endif
        }

        /// <summary>Synchronous RPC query. Android only. Prefer <see cref="isPreInstalledAppAsync"/> to
        /// avoid blocking the calling thread.</summary>
        public static bool isPreInstalledApp()
        {
#if UNITY_ANDROID
            return (Query("isPreInstalledApp") as bool?) ?? false;
#else
            return false;
#endif
        }

        /// <summary>Awaitable counterpart of <see cref="isPreInstalledApp"/> — safe to call from the main thread.</summary>
        public static async Awaitable<bool> isPreInstalledAppAsync()
        {
#if UNITY_ANDROID
            return (await QueryAsync("isPreInstalledApp") as bool?) ?? false;
#else
            return false;
#endif
        }

        /// <summary>Synchronous RPC query. Android only. Prefer <see cref="getAttributionIdAsync"/> to
        /// avoid blocking the calling thread.</summary>
        public static string getAttributionId()
        {
#if UNITY_ANDROID
            return Query("getAttributionId") as string ?? string.Empty;
#else
            return string.Empty;
#endif
        }

        /// <summary>Awaitable counterpart of <see cref="getAttributionId"/> — safe to call from the main thread.</summary>
        public static async Awaitable<string> getAttributionIdAsync()
        {
#if UNITY_ANDROID
            return await QueryAsync("getAttributionId") as string ?? string.Empty;
#else
            return string.Empty;
#endif
        }

        /// <summary>Synchronous RPC query. Android only. Net-new — not exposed prior to this migration.
        /// Prefer <see cref="getHostNameAsync"/> to avoid blocking the calling thread.</summary>
        public static string getHostName()
        {
#if UNITY_ANDROID
            return Query("getHostName") as string ?? string.Empty;
#else
            return string.Empty;
#endif
        }

        /// <summary>Awaitable counterpart of <see cref="getHostName"/> — safe to call from the main thread.</summary>
        public static async Awaitable<string> getHostNameAsync()
        {
#if UNITY_ANDROID
            return await QueryAsync("getHostName") as string ?? string.Empty;
#else
            return string.Empty;
#endif
        }

        /// <summary>Synchronous RPC query. Android only. Net-new — not exposed prior to this migration.
        /// Prefer <see cref="getHostPrefixAsync"/> to avoid blocking the calling thread.</summary>
        public static string getHostPrefix()
        {
#if UNITY_ANDROID
            return Query("getHostPrefix") as string ?? string.Empty;
#else
            return string.Empty;
#endif
        }

        /// <summary>Awaitable counterpart of <see cref="getHostPrefix"/> — safe to call from the main thread.</summary>
        public static async Awaitable<string> getHostPrefixAsync()
        {
#if UNITY_ANDROID
            return await QueryAsync("getHostPrefix") as string ?? string.Empty;
#else
            return string.Empty;
#endif
        }

        /// <summary>
        /// Synchronous RPC query. Android only per the schema — note this is a capability reduction from
        /// the old isSDKStopped(), which also worked on iOS via the legacy bridge (no iOS RPC method for
        /// "isStopped" is declared in the schema). Prefer <see cref="isStoppedAsync"/> to avoid blocking
        /// the calling thread.
        /// </summary>
        public static bool isStopped()
        {
#if UNITY_ANDROID
            return (Query("isStopped") as bool?) ?? false;
#else
            return false;
#endif
        }

        /// <summary>Awaitable counterpart of <see cref="isStopped"/> — safe to call from the main thread.</summary>
        public static async Awaitable<bool> isStoppedAsync()
        {
#if UNITY_ANDROID
            return (await QueryAsync("isStopped") as bool?) ?? false;
#else
            return false;
#endif
        }

        public static void disableAppSetId()
        {
#if UNITY_ANDROID
            Fire("disableAppSetId");
#endif
        }

        /// <summary>
        /// Unity engine callback, invoked automatically when the app is paused/resumed. Kept unchanged
        /// from the pre-migration implementation — the native SDK documents this as the required
        /// workaround for plugin bridges since Unity doesn't reliably deliver Android Activity
        /// foreground/background transitions otherwise.
        /// </summary>
        void OnApplicationPause(bool pauseStatus)
        {
#if UNITY_ANDROID
            if (!pauseStatus) return;
            Fire("onPause");
#endif
        }

        // ── Server-side uninstall tracking ────────────────────────────────────────

        /// <summary>Android: pass the FCM token.</summary>
        public static void updateServerUninstallToken(string token)
        {
#if UNITY_ANDROID
            Fire("updateServerUninstallToken", new Dictionary<string, object> { { "token", token } });
#endif
        }

        /// <summary>iOS: pass the raw APNs device token bytes. Encoded as a hex string on the wire
        /// (schema requires deviceToken to match ^(?:[0-9A-Fa-f]{2})+$ — not Base64).</summary>
        public static void updateServerUninstallToken(byte[] deviceToken)
        {
#if UNITY_IOS || UNITY_STANDALONE_OSX
            Fire("registerUninstall", new Dictionary<string, object>
            {
                { "deviceToken", deviceToken != null ? BitConverter.ToString(deviceToken).Replace("-", "") : null }
            });
#endif
        }

        // ── In-app purchase validation ────────────────────────────────────────────

        private static string PurchaseTypeToAndroidString(AFPurchaseType t) =>
            t == AFPurchaseType.Subscription ? "subscription" : "one_time_purchase";

        private static string PurchaseTypeToIOSString(AFSDKPurchaseType t) =>
            t == AFSDKPurchaseType.Subscription ? "subscription" : "oneTimePurchase";

        /// <summary>
        /// Validates an Android in-app purchase. Net-new RPC integration — the pre-migration
        /// implementation had no RPC call for this at all (legacy bridge only).
        /// Resolved: purchaseType casing ("subscription"/"one_time_purchase") matches the schema's
        /// declared Android enum exactly. Recommend one on-device validation pass before final sign-off,
        /// but this is no longer a blocking unknown.
        /// </summary>
        public static void validateAndLogInAppPurchase(AFPurchaseDetailsAndroid details, Dictionary<string, string> additionalParameters)
        {
#if UNITY_ANDROID
            Fire("validateAndLogInAppPurchase", new Dictionary<string, object>
            {
                { "purchaseType", details != null ? PurchaseTypeToAndroidString(details.purchaseType) : null },
                { "purchaseToken", details?.purchaseToken },
                { "productId", details?.productId },
                { "additionalParameters", additionalParameters }
            });
#endif
        }

        /// <summary>
        /// Validates an iOS in-app purchase. Fixed to nest under product/transaction per schema (the
        /// pre-migration implementation sent a flat, incorrectly-shaped payload).
        /// Resolved: purchaseType casing ("subscription"/"oneTimePurchase") matches the schema's declared
        /// iOS enum exactly. Recommend one on-device validation pass before final sign-off, but this is
        /// no longer a blocking unknown.
        /// </summary>
        public static void validateAndLogInAppPurchase(AFSDKPurchaseDetailsIOS details, Dictionary<string, string> additionalParameters)
        {
#if UNITY_IOS || UNITY_STANDALONE_OSX
            Fire("validateAndLogInAppPurchase", new Dictionary<string, object>
            {
                { "product", new Dictionary<string, object> { { "productId", details?.productId } } },
                { "transaction", new Dictionary<string, object>
                    {
                        { "transactionId", details?.transactionId },
                        { "purchaseType", details != null ? PurchaseTypeToIOSString(details.purchaseType) : null }
                    }
                },
                { "additionalParameters", additionalParameters }
            });
#endif
        }

        // ── Callback plumbing (unchanged — already pure RPC/UnitySendMessage, no legacy bridge) ────────

        public static event EventHandler OnRequestResponse
        {
            add { onRequestResponse += value; }
            remove { onRequestResponse -= value; }
        }

        public static event EventHandler OnInAppResponse
        {
            add { onInAppResponse += value; }
            remove { onInAppResponse -= value; }
        }

        public static event EventHandler OnDeepLinkReceived
        {
            add { onDeepLinkReceived += value; registerDeepLinkListener(); }
            remove { onDeepLinkReceived -= value; }
        }

        public static event EventHandler OnSessionReady
        {
            add { onSessionReady += value; }
            remove { onSessionReady -= value; }
        }

        public void inAppResponseReceived(string response)
        {
            if (onInAppResponse != null) onInAppResponse.Invoke(null, parseRequestCallback(response));
        }

        public void requestResponseReceived(string response)
        {
            if (onRequestResponse != null) onRequestResponse.Invoke(null, parseRequestCallback(response));
        }

        public void onSessionReadyReceived(string response)
        {
            if (onSessionReady != null) onSessionReady.Invoke(null, new AppsFlyerRequestEventArgs(0, response));
        }

        public void onDeepLinking(string response)
        {
            DeepLinkEventsArgs args = new DeepLinkEventsArgs(response);
            if (onDeepLinkReceived != null) onDeepLinkReceived.Invoke(null, args);
        }

        /// <summary>
        /// Receives unified RPC event envelopes from native via UnitySendMessage.
        /// Format: {"event": "onConversionDataSuccess", "data": {...}}
        /// </summary>
        public void onRPCEvent(string jsonEvent)
        {
            try
            {
                var envelope = CallbackStringToDictionary(jsonEvent);
                if (envelope == null || !envelope.ContainsKey("event")) return;

                string eventType = envelope["event"] as string;
                var data = envelope.ContainsKey("data") ? envelope["data"] : null;
                string dataStr = data != null ? Json.Serialize(data) : jsonEvent;

                switch (eventType)
                {
                    case "start":
                    case "onRequestResponse":
                        requestResponseReceived(dataStr);
                        break;
                    case "logEvent":
                    case "onInAppResponse":
                        inAppResponseReceived(dataStr);
                        break;
                    case "onDeepLinking":
                    case "onDeepLinkReceived":
                        onDeepLinking(dataStr);
                        break;
                    case "onConversionDataSuccess":
                    case "onConversionDataFail":
                        var go = GameObject.Find(CallBackObjectName);
                        go?.SendMessage(eventType, dataStr, SendMessageOptions.DontRequireReceiver);
                        break;
                    case "sessionReady":
                    case "onSessionReady":
                        onSessionReadyReceived(dataStr);
                        break;
                    default:
                        AFLog("onRPCEvent", "Unhandled event type: " + eventType);
                        break;
                }
            }
            catch (Exception e)
            {
                AFLog("onRPCEvent", "Exception: " + e.Message);
            }
        }

        private static AppsFlyerRequestEventArgs parseRequestCallback(string response)
        {
            int responseCode = 0;
            string errorDescription = "";
            try
            {
                Dictionary<string, object> dictionary = CallbackStringToDictionary(response);
                var errorResponse = dictionary.ContainsKey("errorDescription") ? dictionary["errorDescription"] : "";
                errorDescription = (string)errorResponse;
                responseCode = (int)(long)dictionary["statusCode"];
            }
            catch (Exception e)
            {
                AFLog("parseRequestCallback", String.Format("{0} Exception caught.", e));
            }
            return new AppsFlyerRequestEventArgs(responseCode, errorDescription);
        }

        public static Dictionary<string, object> CallbackStringToDictionary(string str)
        {
            return AFMiniJSON.Json.Deserialize(str) as Dictionary<string, object>;
        }

        public static void AFLog(string methodName, string str)
        {
            Debug.Log(string.Format("AppsFlyer_Unity_v{0} {1} called with {2}", kAppsFlyerPluginVersion, methodName, str));
        }
    }
}
