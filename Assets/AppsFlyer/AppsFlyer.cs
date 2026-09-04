using System;
using System.Collections.Generic;
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
    // CS1998: many methods below are platform-only (#if UNITY_ANDROID / UNITY_IOS) and their only
    // `await` lives inside the guarded branch, so compiling for the other platform yields a body
    // with no `await` at all. That's an intentional no-op, not a bug - suppressed for the whole class.
#pragma warning disable CS1998
    public class AppsFlyer : MonoBehaviour
    {
        public static readonly string kAppsFlyerPluginVersion = "7.0.2-rc7";
        public static string CallBackObjectName = null;
        private static EventHandler onSessionReady;
        private static Action<string> onConversionDataSuccessCallback;
        private static Action<string> onConversionDataFailCallback;
        private static Action<DeepLinkEventsArgs> onDeepLinkListenerCallback;
        public delegate void unityCallBack(string message);

        // Dispatches via ExecuteFire() on the calling thread, in place - no BackgroundThreadAsync hop -
        // so call-site ordering across multiple non-awaited calls is preserved exactly like the
        // fire-and-forget Fire() this replaced. Logs here so a caller who doesn't await still gets
        // the failure logged instead of an unobserved fault, then rethrows so a caller who does
        // await still sees the exception - swallowing it unconditionally would silence failures for
        // awaited callers too.
        private static async Awaitable FireAsync(string method, Dictionary<string, object> parameters = null)
        {
            try
            {
                AppsFlyerRPCClient.instance.ExecuteFire(method, parameters);
            }
            catch (Exception e)
            {
                AFLog(method, "RPC error: " + e.Message);
                throw;
            }
        }

        private static object Query(string method, Dictionary<string, object> parameters = null)
        {
            try { return AppsFlyerRPCClient.instance.Execute(method, parameters); }
            catch (AppsFlyerRPCException e) { AFLog(method, "RPC error: " + e.Message); return null; }
            catch (Exception e) { AFLog(method, "Unexpected error dispatching RPC: " + e.Message); return null; }
        }

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
            catch (Exception e)
            {
                AFLog(method, "Unexpected error dispatching RPC: " + e.Message);
                return null;
            }
            finally
            {
                await Awaitable.MainThreadAsync();
            }
        }

        // ── Initialization ──────────────────────────────────────────────────────

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitBridgeOnLoad()
        {
#if UNITY_ANDROID || UNITY_IOS || UNITY_STANDALONE_OSX
            AppsFlyerRPCClient.instance.InitBridge(CallBackObjectName ?? "");
#endif
        }

        /// <summary>
        /// True unless the native RPC bridge failed to load (Android-only failure mode: the
        /// AppsFlyerRPCBridge class isn't present in the built APK). Every fire-and-forget call
        /// (the majority of the public API) completes its Awaitable "successfully" even when this is
        /// false, since native never gets a chance to signal the failure back — assert on this during
        /// smoke tests rather than relying on an awaitable completing.
        /// </summary>
        public static bool isRPCBridgeAvailable()
        {
            return AppsFlyerRPCClient.instance.IsBridgeAvailable;
        }

        /// <summary>
        /// Initialize the AppsFlyer SDK. devKey is required on all platforms; appID is required for iOS
        /// (pass null on Android-only apps).
        /// </summary>
        public static async Awaitable init(string devKey, string appID, MonoBehaviour gameObject = null)
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
            await FireAsync("init", new Dictionary<string, object> { { "devKey", devKey } });
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
            await FireAsync("setPluginInfo", new Dictionary<string, object>
            {
                { "plugin", "unity" },
                { "pluginVersion", kAppsFlyerPluginVersion }
            });
        }

        /// <summary>Starts the SDK. A session is sent immediately, and on every foreground transition.
        /// Pass <paramref name="awaitResponse"/> to wait for the server round trip (session request
        /// completed) instead of just the fire-and-forget dispatch to native.</summary>
        public static async Awaitable start(bool awaitResponse = false)
        {
#if UNITY_WSA_10_0
            AppsFlyerWindows.Start();
#else
            var parameters = new Dictionary<string, object> { { "awaitResponse", awaitResponse } };
            if (awaitResponse)
                await QueryAsync("start", parameters);
            else
                await FireAsync("start", parameters);
#endif
        }

        /// <summary>Stops/resumes all SDK activity.</summary>
        public static async Awaitable stop(bool shouldStop)
        {
            await FireAsync("stop", new Dictionary<string, object> { { "shouldStop", shouldStop } });
        }

        /// <summary>Matches the schema's canonical isSessionReady contract. Safe to call from the main thread.</summary>
        public static async Awaitable<bool> isSessionReady()
        {
            return (await QueryAsync("isSessionReady") as bool?) ?? false;
        }

        /// <summary>Gets the AppsFlyer SDK version used by native. Safe to call from the main thread.</summary>
        public static async Awaitable<string> getSdkVersion()
        {
            return await QueryAsync("getSdkVersion") as string ?? string.Empty;
        }

        /// <summary>Gets AppsFlyer's unique device ID. Safe to call from the main thread.</summary>
        public static async Awaitable<string> getAppsFlyerUID()
        {
#if UNITY_WSA_10_0
            return AppsFlyerWindows.GetAppsFlyerId();
#else
            return await QueryAsync("getAppsFlyerUID") as string ?? string.Empty;
#endif
        }

        // ── Events ───────────────────────────────────────────────────────────────

        /// <summary>Logs an in-app event. Pass <paramref name="awaitResponse"/> to wait for the server
        /// round trip (event request completed) instead of just the fire-and-forget dispatch to native.</summary>
        public static async Awaitable logEvent(string eventName, Dictionary<string, string> eventValues, bool awaitResponse = false)
        {
#if UNITY_WSA_10_0
            AppsFlyerWindows.LogEvent(eventName, eventValues);
#else
            var parameters = new Dictionary<string, object>
            {
                { "eventName", eventName }, { "eventValues", eventValues }, { "awaitResponse", awaitResponse }
            };
            if (awaitResponse)
                await QueryAsync("logEvent", parameters);
            else
                await FireAsync("logEvent", parameters);
#endif
        }

        private static readonly Dictionary<MediationNetwork, string> MediationNetworkWireNames = new Dictionary<MediationNetwork, string>
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

        public static async Awaitable logAdRevenue(AFAdRevenueData adRevenueData, Dictionary<string, string> additionalParameters)
        {
            string mediationNetworkWireName = "none";
            if (adRevenueData != null && MediationNetworkWireNames.TryGetValue(adRevenueData.mediationNetwork, out var wireName))
            {
                mediationNetworkWireName = wireName;
            }

            await FireAsync("logAdRevenue", new Dictionary<string, object>
            {
                { "monetizationNetwork", adRevenueData?.monetizationNetwork },
                { "mediationNetwork", mediationNetworkWireName },
                { "currencyIso4217Code", adRevenueData?.currencyIso4217Code },
                { "revenue", adRevenueData?.eventRevenue },
                { "additionalParameters", additionalParameters }
            });
        }

        public static async Awaitable logLocation(double latitude, double longitude)
        {
            await FireAsync("logLocation", new Dictionary<string, object> { { "latitude", latitude }, { "longitude", longitude } });
        }

        /// <summary>Logs a store-open event and has native open the promoted app's store page.</summary>
        public static async Awaitable logAndOpenStore(string promotedAppId, string campaign, Dictionary<string, string> userParams)
        {
            await FireAsync("logAndOpenStore", new Dictionary<string, object>
            {
                { "promotedAppId", promotedAppId }, { "campaign", campaign }, { "userParams", userParams }
            });
        }

        public static async Awaitable logCrossPromoteImpression(string appId, string campaign, Dictionary<string, string> userParams)
        {
            await FireAsync("logCrossPromoteImpression", new Dictionary<string, object>
            {
                { "appId", appId }, { "campaign", campaign }, { "userParams", userParams }
            });
        }

        public static async Awaitable logInvite(string channel, Dictionary<string, string> eventParameters)
        {
            await FireAsync("logInvite", new Dictionary<string, object> { { "channel", channel }, { "eventParameters", eventParameters } });
        }

        /// <summary>Manually records a session. Android only.</summary>
        public static async Awaitable logSession()
        {
#if UNITY_ANDROID
            await FireAsync("logSession");
#endif
        }

        /// <summary>Collects attribution data from the launcher Activity. Android only.</summary>
        public static async Awaitable collectDataFromLauncherActivity()
        {
#if UNITY_ANDROID
            await FireAsync("collectDataFromLauncherActivity");
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
        public static async Awaitable performDeepLinking(string url, bool shouldTriggerSession = false)
        {
#if UNITY_ANDROID
            await FireAsync("performDeepLinking", new Dictionary<string, object> { { "url", url }, { "shouldTriggerSession", shouldTriggerSession } });
#elif UNITY_IOS || UNITY_STANDALONE_OSX
            await FireAsync("performDeepLinking", new Dictionary<string, object> { { "url", url } });
#endif
        }

        // ── Identity & configuration ──────────────────────────────────────────────

        public static async Awaitable setCustomerUserId(string customerId)
        {
#if UNITY_WSA_10_0
            AppsFlyerWindows.SetCustomerUserId(customerId);
#else
            await FireAsync("setCustomerUserId", new Dictionary<string, object> { { "customerId", customerId } });
#endif
        }

        public static async Awaitable setAppInviteOneLink(string oneLinkId)
        {
            await FireAsync("setAppInviteOneLink", new Dictionary<string, object> { { "oneLinkId", oneLinkId } });
        }

        public static async Awaitable setDeepLinkTimeout(long timeout)
        {
            await FireAsync("setDeepLinkTimeout", new Dictionary<string, object> { { "timeout", timeout } });
        }

        public static async Awaitable setAdditionalData(Dictionary<string, string> customData)
        {
            await FireAsync("setAdditionalData", new Dictionary<string, object> { { "customData", customData } });
        }

        public static async Awaitable setResolveDeepLinkURLs(params string[] urls)
        {
            await FireAsync("setResolveDeepLinkURLs", new Dictionary<string, object> { { "urls", urls } });
        }

        public static async Awaitable setOneLinkCustomDomain(params string[] domains)
        {
            await FireAsync("setOneLinkCustomDomain", new Dictionary<string, object> { { "domains", domains } });
        }

        public static async Awaitable setCurrencyCode(string currencyCode)
        {
            await FireAsync("setCurrencyCode", new Dictionary<string, object> { { "currencyCode", currencyCode } });
        }

        public static async Awaitable setConsentData(AppsFlyerConsent appsFlyerConsent)
        {
            await FireAsync("setConsentData", new Dictionary<string, object>
            {
                { "isUserSubjectToGDPR", appsFlyerConsent?.isUserSubjectToGDPR },
                { "hasConsentForDataUsage", appsFlyerConsent?.hasConsentForDataUsage },
                { "hasConsentForAdsPersonalization", appsFlyerConsent?.hasConsentForAdsPersonalization },
                { "hasConsentForAdStorage", appsFlyerConsent?.hasConsentForAdStorage }
            });
        }

        public static async Awaitable anonymizeUser(bool shouldAnonymizeUser)
        {
            await FireAsync("anonymizeUser", new Dictionary<string, object> { { "shouldAnonymize", shouldAnonymizeUser } });
        }

        public static async Awaitable enableTCFDataCollection(bool shouldCollectTcfData)
        {
            await FireAsync("enableTCFDataCollection", new Dictionary<string, object> { { "shouldCollect", shouldCollectTcfData } });
        }

        public static async Awaitable setMinTimeBetweenSessions(int seconds)
        {
            await FireAsync("setMinTimeBetweenSessions", new Dictionary<string, object> { { "seconds", seconds } });
        }

        public static async Awaitable setHost(string hostPrefixName, string hostName)
        {
            await FireAsync("setHost", new Dictionary<string, object> { { "hostPrefixName", hostPrefixName }, { "hostName", hostName } });
        }

        public static async Awaitable setInstallId(string installId)
        {
            await FireAsync("setInstallId", new Dictionary<string, object> { { "installId", installId } });
        }

        /// <summary>Enables SDK debug logs. Public name and parameter follow the schema's canonical
        /// "enableDebug(enabled)"; the wire RPC method both platforms actually implement is "isDebug".</summary>
        public static async Awaitable enableDebug(bool enabled)
        {
            await FireAsync("isDebug", new Dictionary<string, object> { { "isDebug", enabled } });
        }

        public static async Awaitable setPartnerData(string partnerId, Dictionary<string, string> data)
        {
            await FireAsync("setPartnerData", new Dictionary<string, object> { { "partnerId", partnerId }, { "data", data } });
        }

        public static async Awaitable appendParametersToDeepLinkingURL(string contains, Dictionary<string, string> parameters)
        {
            await FireAsync("appendParametersToDeepLinkingURL", new Dictionary<string, object> { { "contains", contains }, { "parameters", parameters } });
        }

        public static async Awaitable enableFacebookDeferredApplinks(bool isEnabled)
        {
            await FireAsync("enableFacebookDeferredApplinks", new Dictionary<string, object> { { "isEnabled", isEnabled } });
        }

        /// <summary>Sets the user's email (single address — the schema does not support multiple
        /// emails or a crypt-type parameter; those existed in the old off-schema "setUserEmails" call).</summary>
        public static async Awaitable setUserEmail(string email)
        {
            await FireAsync("setUserEmail", new Dictionary<string, object> { { "email", email } });
        }

        public static async Awaitable setUserFirstName(string firstName)
        {
            await FireAsync("setUserFirstName", new Dictionary<string, object> { { "firstName", firstName } });
        }

        public static async Awaitable setUserLastName(string lastName)
        {
            await FireAsync("setUserLastName", new Dictionary<string, object> { { "lastName", lastName } });
        }

        public static async Awaitable setUserFbLoginId(long fbLoginId)
        {
            await FireAsync("setUserFbLoginId", new Dictionary<string, object> { { "fbLoginId", fbLoginId } });
        }

        public static async Awaitable setUserPhone(string countryCode, string phoneNumber)
        {
            await FireAsync("setUserPhone", new Dictionary<string, object> { { "countryCode", countryCode }, { "phoneNumber", phoneNumber } });
        }

        public static async Awaitable clearUserPii()
        {
            await FireAsync("clearUserPii");
        }

        /// <summary>Sets the SDK log level. Accepted values (case-insensitive): none/error/warning/info/debug/verbose.
        /// Android only.</summary>
        public static async Awaitable setLogLevel(string logLevel)
        {
#if UNITY_ANDROID
            await FireAsync("setLogLevel", new Dictionary<string, object> { { "logLevel", logLevel?.ToUpperInvariant() } });
#endif
        }

        // ── Deep linking & conversion data ─────────────────────────────────────────

        /// <summary>
        /// Registers a conversion-data listener. Callback delivery is routed through the unified
        /// onRPCEvent envelope to CallBackObjectName (set in init), not by an RPC parameter — but the
        /// register function itself is the API surface: it takes the listeners directly as parameters,
        /// matching the long-lived/recurring-result register-function shape (see e.g. the Flutter
        /// plugin's registerConversionListener), rather than a separate += event with a side effect.
        /// Resolved: schema declares zero params (maxProperties: 0) on both platforms. Confirmed there
        /// is no undeclared callbackObjectName side channel — AppsFlyerRPCBridge.init() (Android) and
        /// _setRPCEventHandler (iOS) each wire one generic event handler at SDK init that routes every
        /// RPC event, including conversion callbacks, through onRPCEvent to CallBackObjectName.
        /// </summary>
        public static async Awaitable registerConversionListener(Action<string> onConversionDataSuccess, Action<string> onConversionDataFail)
        {
            onConversionDataSuccessCallback = onConversionDataSuccess;
            onConversionDataFailCallback = onConversionDataFail;
#if UNITY_WSA_10_0
            AppsFlyerWindows.GetConversionData("");
#else
            await FireAsync("registerConversionListener");
#endif
        }

        /// <summary>Android only.</summary>
        public static async Awaitable unregisterConversionListener()
        {
#if UNITY_ANDROID
            await FireAsync("unregisterConversionListener");
#endif
            onConversionDataSuccessCallback = null;
            onConversionDataFailCallback = null;
        }

        /// <summary>
        /// Subscribes for the unified deep-link event. Manual/advanced-integration escape hatch — native
        /// already resolves deep links automatically on both platforms (see performDeepLinking doc comment
        /// for the same race-condition warning). The listener is supplied directly as a parameter here —
        /// the register function is the API surface, not a += event with a side-effecting accessor.
        /// </summary>
        public static async Awaitable registerDeepLinkListener(Action<DeepLinkEventsArgs> callback)
        {
            onDeepLinkListenerCallback = callback;
#if UNITY_ANDROID
            await FireAsync("subscribeForDeepLink");
#elif UNITY_IOS || UNITY_STANDALONE_OSX
            await FireAsync("registerDeeplinkListener");
#endif
        }

        /// <summary>Android only.</summary>
        public static async Awaitable unregisterDeeplinkListener()
        {
#if UNITY_ANDROID
            await FireAsync("unsubscribeForDeepLink");
#endif
            onDeepLinkListenerCallback = null;
        }

        public static async Awaitable registerSessionReadyListener()
        {
            await FireAsync("registerSessionReadyListener");
        }

        public static async Awaitable unregisterSessionReadyListener()
        {
            await FireAsync("unregisterSessionReadyListener");
        }

        /// <summary>
        /// Handles a URL open (iOS-only capability in the schema).
        /// Resolved: schema declares `options` as free-form (additionalProperties: true, no fixed shape),
        /// matching iOS's native UIApplicationOpenURLOptionsKey dictionary, which has no fixed shape
        /// either. An open Dictionary&lt;string, object&gt; is the correct signature, not a placeholder.
        /// </summary>
        public static async Awaitable handleOpenUrl(string url, Dictionary<string, object> options = null)
        {
#if UNITY_IOS || UNITY_STANDALONE_OSX
            await FireAsync("handleOpenUrl", new Dictionary<string, object> { { "url", url }, { "options", options } });
#endif
        }

        /// <summary>Passes launch options to the SDK for cold-start attribution. iOS only. Manual/advanced
        /// escape hatch — no native caller found for this capability anywhere in this repo; verify it's
        /// actually needed before relying on it.</summary>
        public static async Awaitable handleLaunchOptions(Dictionary<string, object> launchOptions)
        {
#if UNITY_IOS || UNITY_STANDALONE_OSX
            await FireAsync("handleLaunchOptions", new Dictionary<string, object> { { "launchOptions", launchOptions } });
#endif
        }

        /// <summary>Handles a Universal Link for deep-link attribution. iOS only. Manual/advanced
        /// escape hatch — native's AppDelegateListener/swizzle already forwards this automatically;
        /// do not call if relying on the default automatic integration.</summary>
        public static async Awaitable continueUserActivity(string url, string activityType = null)
        {
#if UNITY_IOS || UNITY_STANDALONE_OSX
            await FireAsync("continueUserActivity", new Dictionary<string, object> { { "url", url }, { "activityType", activityType } });
#endif
        }

        /// <summary>Forwards a push payload to native for attribution. iOS only — the schema declares no
        /// Android RPC method for this; Android push handling happens natively without a Unity call.</summary>
        public static async Awaitable handlePushNotifications(Dictionary<string, object> pushPayload)
        {
#if UNITY_IOS || UNITY_STANDALONE_OSX
            await FireAsync("handlePushNotification", new Dictionary<string, object> { { "pushPayload", pushPayload } });
#endif
        }

        /// <summary>Android only.</summary>
        public static async Awaitable sendPushNotificationData(string campaign, string pid, bool isRetargeting, Dictionary<string, string> additionalParameters = null)
        {
#if UNITY_ANDROID
            await FireAsync("sendPushNotificationData", new Dictionary<string, object>
            {
                { "campaign", campaign }, { "pid", pid }, { "isRetargeting", isRetargeting }, { "additionalParameters", additionalParameters }
            });
#endif
        }

        public static async Awaitable addPushNotificationDeepLinkPath(params string[] paths)
        {
            await FireAsync("addPushNotificationDeepLinkPath", new Dictionary<string, object> { { "deepLinkPath", paths } });
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
        public static async Awaitable<string> generateInviteLink(Dictionary<string, string> parameters)
        {
            var payload = BuildInviteLinkPayload(parameters);
            await Awaitable.BackgroundThreadAsync();
            try
            {
                return AppsFlyerRPCClient.instance.Execute("generateInviteLink", payload) as string;
            }
            catch (AppsFlyerRPCException e)
            {
                AFLog("generateInviteLink", "Failed to generate invite link: " + e.Message + " (code " + e.Code + "): " + e.Details);
                return null;
            }
            catch (Exception e)
            {
                AFLog("generateInviteLink", "Failed to generate invite link: " + e.Message);
                return null;
            }
            finally
            {
                await Awaitable.MainThreadAsync();
            }
        }

        // ── Advertising identifiers & privacy ─────────────────────────────────────

        /// <summary>Android's RPC parameter key is "isDisable"; iOS's is "disable" — the schema declares
        /// different key names per platform for this capability.</summary>
        public static async Awaitable setDisableAdvertisingIdentifiers(bool disable)
        {
#if UNITY_ANDROID
            await FireAsync("setDisableAdvertisingIdentifiers", new Dictionary<string, object> { { "isDisable", disable } });
#elif UNITY_IOS || UNITY_STANDALONE_OSX
            await FireAsync("setDisableAdvertisingIdentifiers", new Dictionary<string, object> { { "disable", disable } });
#endif
        }

        /// <summary>iOS only.</summary>
        public static async Awaitable setDisableAppleAdsAttribution(bool disable)
        {
#if UNITY_IOS || UNITY_STANDALONE_OSX
            await FireAsync("setDisableAppleAdsAttribution", new Dictionary<string, object> { { "disable", disable } });
#endif
        }

        /// <summary>iOS only.</summary>
        public static async Awaitable setDisableCollectASA(bool disable)
        {
#if UNITY_IOS || UNITY_STANDALONE_OSX
            await FireAsync("setDisableCollectASA", new Dictionary<string, object> { { "disable", disable } });
#endif
        }

        /// <summary>iOS only.</summary>
        public static async Awaitable setDisableIDFVCollection(bool disable)
        {
#if UNITY_IOS || UNITY_STANDALONE_OSX
            await FireAsync("setDisableIDFVCollection", new Dictionary<string, object> { { "disable", disable } });
#endif
        }

        /// <summary>Android only.</summary>
        public static async Awaitable setDisableNetworkData(bool isDisable)
        {
#if UNITY_ANDROID
            await FireAsync("setDisableNetworkData", new Dictionary<string, object> { { "isDisable", isDisable } });
#endif
        }

        /// <summary>iOS only.</summary>
        public static async Awaitable setDisableSKAdNetwork(bool disable)
        {
#if UNITY_IOS || UNITY_STANDALONE_OSX
            await FireAsync("setDisableSKAdNetwork", new Dictionary<string, object> { { "disable", disable } });
#endif
        }

        /// <summary>iOS only.</summary>
        public static async Awaitable setFacebookDeferredAppLink(string url)
        {
#if UNITY_IOS || UNITY_STANDALONE_OSX
            await FireAsync("setFacebookDeferredAppLink", new Dictionary<string, object> { { "url", url } });
#endif
        }

        /// <summary>iOS only.</summary>
        public static async Awaitable setShouldCollectDeviceName(bool collect)
        {
#if UNITY_IOS || UNITY_STANDALONE_OSX
            await FireAsync("setShouldCollectDeviceName", new Dictionary<string, object> { { "collect", collect } });
#endif
        }

        /// <summary>iOS only.</summary>
        public static async Awaitable setUseReceiptValidationSandbox(bool sandbox)
        {
#if UNITY_IOS || UNITY_STANDALONE_OSX
            await FireAsync("setUseReceiptValidationSandbox", new Dictionary<string, object> { { "sandbox", sandbox } });
#endif
        }

        /// <summary>iOS only.</summary>
        public static async Awaitable setUseUninstallSandbox(bool sandbox)
        {
#if UNITY_IOS || UNITY_STANDALONE_OSX
            await FireAsync("setUseUninstallSandbox", new Dictionary<string, object> { { "sandbox", sandbox } });
#endif
        }

        /// <summary>iOS only.</summary>
        public static async Awaitable setCurrentDeviceLanguage(string language)
        {
#if UNITY_IOS || UNITY_STANDALONE_OSX
            await FireAsync("setCurrentDeviceLanguage", new Dictionary<string, object> { { "language", language } });
#endif
        }

        public static async Awaitable setSharingFilterForPartners(params string[] partners)
        {
            await FireAsync("setSharingFilterForPartners", new Dictionary<string, object> { { "partners", partners } });
        }

        // ── Android-only ─────────────────────────────────────────────────────────

        public static async Awaitable setAppId(string appId)
        {
#if UNITY_ANDROID
            await FireAsync("setAppId", new Dictionary<string, object> { { "appId", appId } });
#endif
        }

        public static async Awaitable setCollectAndroidID(bool isCollect)
        {
#if UNITY_ANDROID
            await FireAsync("setCollectAndroidID", new Dictionary<string, object> { { "isCollect", isCollect } });
#endif
        }

        public static async Awaitable setIsUpdate(bool isUpdate)
        {
#if UNITY_ANDROID
            await FireAsync("setIsUpdate", new Dictionary<string, object> { { "isUpdate", isUpdate } });
#endif
        }

        public static async Awaitable setOutOfStore(string sourceName)
        {
#if UNITY_ANDROID
            await FireAsync("setOutOfStore", new Dictionary<string, object> { { "sourceName", sourceName } });
#endif
        }

        /// <summary>Android only. Safe to call from the main thread.</summary>
        public static async Awaitable<string> getOutOfStore()
        {
#if UNITY_ANDROID
            return await QueryAsync("getOutOfStore") as string ?? string.Empty;
#else
            return string.Empty;
#endif
        }

        public static async Awaitable setPreinstallAttribution(string mediaSource, string campaign, string siteId)
        {
#if UNITY_ANDROID
            await FireAsync("setPreinstallAttribution", new Dictionary<string, object>
            {
                { "mediaSource", mediaSource }, { "campaign", campaign }, { "siteId", siteId }
            });
#endif
        }

        /// <summary>Android only. Safe to call from the main thread.</summary>
        public static async Awaitable<bool> isPreInstalledApp()
        {
#if UNITY_ANDROID
            return (await QueryAsync("isPreInstalledApp") as bool?) ?? false;
#else
            return false;
#endif
        }

        /// <summary>Android only. Safe to call from the main thread.</summary>
        public static async Awaitable<string> getAttributionId()
        {
#if UNITY_ANDROID
            return await QueryAsync("getAttributionId") as string ?? string.Empty;
#else
            return string.Empty;
#endif
        }

        /// <summary>Android only. Net-new — not exposed prior to this migration. Safe to call from the main thread.</summary>
        public static async Awaitable<string> getHostName()
        {
#if UNITY_ANDROID
            return await QueryAsync("getHostName") as string ?? string.Empty;
#else
            return string.Empty;
#endif
        }

        /// <summary>Android only. Net-new — not exposed prior to this migration. Safe to call from the main thread.</summary>
        public static async Awaitable<string> getHostPrefix()
        {
#if UNITY_ANDROID
            return await QueryAsync("getHostPrefix") as string ?? string.Empty;
#else
            return string.Empty;
#endif
        }

        /// <summary>
        /// Android only per the schema — note this is a capability reduction from the old isSDKStopped(),
        /// which also worked on iOS via the legacy bridge (no iOS RPC method for "isStopped" is declared
        /// in the schema). Safe to call from the main thread.
        /// </summary>
        public static async Awaitable<bool> isStopped()
        {
#if UNITY_ANDROID
            return (await QueryAsync("isStopped") as bool?) ?? false;
#else
            return false;
#endif
        }

        public static async Awaitable disableAppSetId()
        {
#if UNITY_ANDROID
            await FireAsync("disableAppSetId");
#endif
        }

        // ── Server-side uninstall tracking ────────────────────────────────────────

        /// <summary>Android: pass the FCM token.</summary>
        public static async Awaitable updateServerUninstallToken(string token)
        {
#if UNITY_ANDROID
            await FireAsync("updateServerUninstallToken", new Dictionary<string, object> { { "token", token } });
#endif
        }

        /// <summary>iOS: pass the raw APNs device token bytes. Encoded as a hex string on the wire
        /// (schema requires deviceToken to match ^(?:[0-9A-Fa-f]{2})+$ — not Base64).</summary>
        public static async Awaitable updateServerUninstallToken(byte[] deviceToken)
        {
#if UNITY_IOS || UNITY_STANDALONE_OSX
            await FireAsync("registerUninstall", new Dictionary<string, object>
            {
                { "deviceToken", deviceToken != null ? BitConverter.ToString(deviceToken).Replace("-", "") : null }
            });
#endif
        }

        // ── In-app purchase validation ────────────────────────────────────────────

        private static async Awaitable<IAFValidateAndLogResult> QueryValidateAndLogAsync(Dictionary<string, object> payload)
        {
            await Awaitable.BackgroundThreadAsync();
            try
            {
                var result = AppsFlyerRPCClient.instance.Execute("validateAndLogInAppPurchase", payload) as Dictionary<string, object>;
                return AFSDKValidateAndLogResult.Init(AFSDKValidateAndLogStatus.AFSDKValidateAndLogStatusSuccess, result, null, null);
            }
            catch (AppsFlyerRPCException e)
            {
                return AFSDKValidateAndLogResult.Init(AFSDKValidateAndLogStatus.AFSDKValidateAndLogStatusError, null, e.Details as Dictionary<string, object>, e.Message);
            }
            catch (Exception e)
            {
                return AFSDKValidateAndLogResult.Init(AFSDKValidateAndLogStatus.AFSDKValidateAndLogStatusError, null, null, e.Message);
            }
            finally
            {
                await Awaitable.MainThreadAsync();
            }
        }

        /// <summary>
        /// Validates an in-app purchase and logs it to AppsFlyer. Net-new RPC integration on Android —
        /// the pre-migration implementation had no RPC call for this at all (legacy bridge only); fixed
        /// on iOS to nest the payload under product/transaction per schema (the pre-migration
        /// implementation sent a flat, incorrectly-shaped payload).
        /// <paramref name="details"/> is <see cref="AFPurchaseDetailsAndroid"/> on Android or
        /// <see cref="AFSDKPurchaseDetailsIOS"/> on iOS/macOS — each builds its own RPC payload shape via
        /// <see cref="IAFPurchaseDetails.ToRpcPayload"/>, so adding a platform means implementing the
        /// interface once, not adding another overload here.
        /// </summary>
        public static async Awaitable<IAFValidateAndLogResult> validateAndLogInAppPurchase(IAFPurchaseDetails details, Dictionary<string, string> additionalParameters)
        {
#if UNITY_ANDROID || UNITY_IOS || UNITY_STANDALONE_OSX
            var payload = details?.ToRpcPayload() ?? new Dictionary<string, object>();
            payload["additionalParameters"] = additionalParameters;
            return await QueryValidateAndLogAsync(payload);
#else
            return null;
#endif
        }

        // ── Callback plumbing ────────────────────────────────────────────────────

        public static event EventHandler OnSessionReady
        {
            add { onSessionReady += value; }
            remove { onSessionReady -= value; }
        }

        public void onSessionReadyReceived(string response)
        {
            if (onSessionReady != null) onSessionReady.Invoke(null, new AppsFlyerRequestEventArgs(0, response));
        }

        public void onDeepLinking(string response)
        {
            DeepLinkEventsArgs args = new DeepLinkEventsArgs(response);
            onDeepLinkListenerCallback?.Invoke(args);
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
                    case "onDeepLinking":
                    case "onDeepLinkReceived":
                        onDeepLinking(dataStr);
                        break;
                    case "onConversionDataSuccess":
                        onConversionDataSuccessCallback?.Invoke(dataStr);
                        break;
                    case "onConversionDataFail":
                        onConversionDataFailCallback?.Invoke(dataStr);
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

        public static Dictionary<string, object> CallbackStringToDictionary(string str)
        {
            return AFMiniJSON.Json.Deserialize(str) as Dictionary<string, object>;
        }

        public static void AFLog(string methodName, string str)
        {
            Debug.Log(string.Format("AppsFlyer_Unity_v{0} {1} called with {2}", kAppsFlyerPluginVersion, methodName, str));
        }
    }
#pragma warning restore CS1998
}
