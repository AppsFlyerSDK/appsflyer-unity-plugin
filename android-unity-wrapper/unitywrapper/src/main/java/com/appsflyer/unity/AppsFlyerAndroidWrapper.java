package com.appsflyer.unity;


import androidx.annotation.NonNull;

import com.appsflyer.AFAdRevenueData;
import com.appsflyer.MediationNetwork;
import com.appsflyer.AFLogger;
import com.appsflyer.AFPurchaseDetails;
import com.appsflyer.AFPurchaseType;
import com.appsflyer.AppsFlyerConsent;
import com.appsflyer.AppsFlyerConversionListener;
import com.appsflyer.AppsFlyerInAppPurchaseValidationCallback;
import com.appsflyer.AppsFlyerInAppPurchaseValidatorListener;
import com.appsflyer.AppsFlyerLib;
import com.appsflyer.AppsFlyerProperties;
import com.appsflyer.attribution.AppsFlyerRequestListener;
import com.appsflyer.deeplink.DeepLinkListener;
import com.appsflyer.deeplink.DeepLinkResult;
import com.appsflyer.internal.platform_extension.Plugin;
import com.appsflyer.internal.platform_extension.PluginInfo;
import com.appsflyer.share.CrossPromotionHelper;
import com.appsflyer.share.LinkGenerator;
import com.appsflyer.share.ShareInviteHelper;
import com.unity3d.player.UnityPlayer;

import org.json.JSONObject;

import java.util.HashMap;
import java.util.Map;


public class AppsFlyerAndroidWrapper {

    private static final String VALIDATE_CALLBACK = "didFinishValidateReceipt";
    private static final String VALIDATE_ERROR_CALLBACK = "didFinishValidateReceiptWithError";
    private static final String VALIDATE_AND_LOG_V2_CALLBACK = "onValidateAndLogComplete";
    private static final String VALIDATE_AND_LOG_V2__ERROR_CALLBACK = "onValidateAndLogFailure";
    private static final String GCD_CALLBACK = "onConversionDataSuccess";
    private static final String GCD_ERROR_CALLBACK = "onConversionDataFail";
    private static final String OAOA_CALLBACK = "onAppOpenAttribution";
    private static final String OAOA_ERROR_CALLBACK = "onAppOpenAttributionFailure";
    private static final String GENERATE_LINK_CALLBACK = "onInviteLinkGenerated";
    private static final String GENERATE_LINK_ERROR_CALLBACK = "onInviteLinkGeneratedFailure";
    private static final String ON_DEEPLINKING = "onDeepLinking";
    private static final String START_REQUEST_CALLBACK = "requestResponseReceived";
    private static final String IN_APP_RESPONSE_CALLBACK = "inAppResponseReceived";
    private static final String PLUGIN_VERSION = "6.16.2";
    private static final long DDL_TIMEOUT_DEFAULT = 3000;
    private static AppsFlyerConversionListener conversionListener;
    private static String devkey = "";
    private static long ddlTimeout = DDL_TIMEOUT_DEFAULT;

    public static void initSDK(String devKey, String objectName) {
        if (conversionListener == null && objectName != null){
            conversionListener = getConversionListener(objectName);
        }

        devkey = devKey;
        setPluginInfo();
        AppsFlyerLib.getInstance().init(devKey, conversionListener, UnityPlayer.currentActivity);
    }

    public static void startTracking(final boolean shouldCallback, final String objectName) {
        AppsFlyerLib.getInstance().start(UnityPlayer.currentActivity, devkey, new AppsFlyerRequestListener() {
            @Override
            public void onSuccess() {
                if(shouldCallback && objectName != null){
                    Map<String,Object> map = new HashMap<String,Object>();
                    map.put("statusCode", 200);
                    JSONObject jsonObject = new JSONObject(map);
                    UnityPlayer.UnitySendMessage(objectName, START_REQUEST_CALLBACK, jsonObject.toString());
                }
            }

            @Override
            public void onError(int i, @NonNull String s) {
                if(shouldCallback && objectName != null){
                    Map<String,Object> map = new HashMap<String,Object>();
                    map.put("statusCode", i);
                    map.put("errorDescription", s);
                    JSONObject jsonObject = new JSONObject(map);
                    UnityPlayer.UnitySendMessage(objectName, START_REQUEST_CALLBACK, jsonObject.toString());
                }
            }
        });
    }

    public static void startTracking() {
        startTracking(false, null);
    }

    public static void stopTracking(boolean isTrackingStopped) {
        AppsFlyerLib.getInstance().stop(isTrackingStopped, UnityPlayer.currentActivity);
    }

    public static String getSdkVersion() {
        return AppsFlyerLib.getInstance().getSdkVersion();
    }

    public static void updateServerUninstallToken(String token) {
        AppsFlyerLib.getInstance().updateServerUninstallToken(UnityPlayer.currentActivity, token);
    }

    public static void setIsDebug(boolean shouldEnable) {
        AppsFlyerLib.getInstance().setDebugLog(shouldEnable);
    }

    public static void setImeiData(String aImei) {
        AppsFlyerLib.getInstance().setImeiData(aImei);

    }

    public static void setAndroidIdData(String aAndroidId) {
        AppsFlyerLib.getInstance().setAndroidIdData(aAndroidId);
    }

    public static void setCustomerUserId(String id) {
        AppsFlyerLib.getInstance().setCustomerUserId(id);
    }

    public static void waitForCustomerUserId(boolean wait) {
        AppsFlyerLib.getInstance().waitForCustomerUserId(true);
    }

    public static void setCustomerIdAndTrack(String id) {
        AppsFlyerLib.getInstance().setCustomerIdAndLogSession(id, UnityPlayer.currentActivity);
    }

    public static void enableTCFDataCollection(boolean shouldCollectTcfData) {
        AppsFlyerLib.getInstance().enableTCFDataCollection(shouldCollectTcfData);
    }

    public static void setConsentData(String isUserSubjectToGDPR, String hasConsentForDataUsage, String hasConsentForAdsPersonalization, String hasConsentForAdStorage) {

        Boolean gdprApplies = parseNullableBoolean(isUserSubjectToGDPR);
        Boolean dataUsage = parseNullableBoolean(hasConsentForDataUsage);
        Boolean adsPersonalization = parseNullableBoolean(hasConsentForAdsPersonalization);
        Boolean adStorage = parseNullableBoolean(hasConsentForAdStorage);

        AppsFlyerLib.getInstance().setConsentData(new AppsFlyerConsent(gdprApplies, dataUsage, adsPersonalization, adStorage));
    }

    public static void logAdRevenue(String monetizationNetwork, MediationNetwork mediationNetwork, String currencyIso4217Code, double revenue, HashMap<String, Object> additionalParameters) {
        AFAdRevenueData adRevenueData = new AFAdRevenueData(monetizationNetwork, mediationNetwork, currencyIso4217Code, revenue);
        AppsFlyerLib.getInstance().logAdRevenue(adRevenueData, additionalParameters);
    }

    public static String getOutOfStore() {
        return AppsFlyerLib.getInstance().getOutOfStore(UnityPlayer.currentActivity);
    }

    public static void setOutOfStore(String sourceName) {
        AppsFlyerLib.getInstance().setOutOfStore(sourceName);
    }

    public static void setAppInviteOneLinkID(String oneLinkId) {
        AppsFlyerLib.getInstance().setAppInviteOneLink(oneLinkId);
    }

    public static void setAdditionalData(HashMap<String, Object> customData) {
        AppsFlyerLib.getInstance().setAdditionalData(customData);
    }

    public static void setUserEmails(String... emails) {
        AppsFlyerLib.getInstance().setUserEmails(emails);
    }

    public static void setUserEmails(AppsFlyerProperties.EmailsCryptType cryptMethod, String... emails) {
        AppsFlyerLib.getInstance().setUserEmails(cryptMethod, emails);
    }

    public static void setCollectAndroidID(boolean isCollect) {
        AppsFlyerLib.getInstance().setCollectAndroidID(isCollect);
    }

    public static void setCollectIMEI(boolean isCollect) {
        AppsFlyerLib.getInstance().setCollectIMEI(isCollect);
    }

    public static void setResolveDeepLinkURLs(String... urls) {
        AppsFlyerLib.getInstance().setResolveDeepLinkURLs(urls);
    }

    public static void setOneLinkCustomDomain(String... domains) {
        AppsFlyerLib.getInstance().setOneLinkCustomDomain(domains);
    }

    public static void setIsUpdate(boolean isUpdate) {
        AppsFlyerLib.getInstance().setIsUpdate(isUpdate);
    }

    public static void setCurrencyCode(String currencyCode) {
        AppsFlyerLib.getInstance().setCurrencyCode(currencyCode);
    }

    public static void trackLocation(double latitude, double longitude) {
        AppsFlyerLib.getInstance().logLocation(UnityPlayer.currentActivity, latitude, longitude);
    }

    public static void trackEvent(String eventName, HashMap<String, Object> eventValues, final boolean shouldCallback, final String objectName) {
        AppsFlyerLib.getInstance().logEvent(UnityPlayer.currentActivity, eventName, eventValues, new AppsFlyerRequestListener() {
            @Override
            public void onSuccess() {
                if(shouldCallback && objectName != null){
                    Map<String,Object> map = new HashMap<String,Object>();
                    map.put("statusCode", 200);
                    JSONObject jsonObject = new JSONObject(map);
                    UnityPlayer.UnitySendMessage(objectName, IN_APP_RESPONSE_CALLBACK, jsonObject.toString());
                }
            }

            @Override
            public void onError(int i, @NonNull String s) {
                if(shouldCallback && objectName != null){
                    Map<String,Object> map = new HashMap<String,Object>();
                    map.put("statusCode", i);
                    map.put("errorDescription", s);
                    JSONObject jsonObject = new JSONObject(map);
                    UnityPlayer.UnitySendMessage(objectName, IN_APP_RESPONSE_CALLBACK, jsonObject.toString());
                }
            }
        });
    }

    public static void trackEvent(String eventName, HashMap<String, Object> eventValues) {
        trackEvent(eventName, eventValues, false, null);
    }

    public static void setDeviceTrackingDisabled(boolean isDisabled) {
        AppsFlyerLib.getInstance().anonymizeUser(isDisabled);
    }

    public static void enableFacebookDeferredApplinks(boolean isEnabled) {
        AppsFlyerLib.getInstance().enableFacebookDeferredApplinks(isEnabled);
    }

    public static void setConsumeAFDeepLinks(boolean doConsume) {
        //AppsFlyerLib.getInstance().setConsumeAFDeepLinks(doConsume);
    }

    public static void setPreinstallAttribution(String mediaSource, String campaign, String siteId) {
        AppsFlyerLib.getInstance().setPreinstallAttribution(mediaSource, campaign, siteId);
    }

    public static boolean isPreInstalledApp() {
        return AppsFlyerLib.getInstance().isPreInstalledApp(UnityPlayer.currentActivity);
    }

    public static String getAttributionId() {
        return AppsFlyerLib.getInstance().getAttributionId(UnityPlayer.currentActivity);
    }

    public static String getAppsFlyerId() {
        return AppsFlyerLib.getInstance().getAppsFlyerUID(UnityPlayer.currentActivity);
    }

    public static void validateAndTrackInAppPurchase(String publicKey, String signature, String purchaseData, String price, String currency, HashMap<String, String> additionalParameters, String objectName) {
        AppsFlyerLib.getInstance().validateAndLogInAppPurchase(UnityPlayer.currentActivity, publicKey, signature, purchaseData, price, currency, additionalParameters);
        if (objectName != null){
            initInAppPurchaseValidatorListener(objectName);
        }
    }

    public static void validateAndTrackInAppPurchaseV2(int purchaseType, String purchaseToken, String productId, String price, String currency, HashMap<String, String> additionalParameters, final String objectName) {
        AFPurchaseType type = purchaseType == 0 ? AFPurchaseType.SUBSCRIPTION : AFPurchaseType.ONE_TIME_PURCHASE;
        AFPurchaseDetails details = new AFPurchaseDetails(type, purchaseToken, productId, price, currency);

        if (objectName != null){
            AppsFlyerInAppPurchaseValidationCallback listener = initInAppPurchaseValidatorV2Listener(objectName);
            AppsFlyerLib.getInstance().validateAndLogInAppPurchase(details, additionalParameters, listener);
        }
    }

    public static boolean isTrackingStopped() {
        return AppsFlyerLib.getInstance().isStopped();
    }

    public static void setMinTimeBetweenSessions(int seconds) {
        AppsFlyerLib.getInstance().setMinTimeBetweenSessions(seconds);
    }

    public static void setLogLevel(AFLogger.LogLevel logLevel) {
        AppsFlyerLib.getInstance().setLogLevel(logLevel);
    }

    public static void setHost(String hostPrefixName, String hostName) {
        AppsFlyerLib.getInstance().setHost(hostPrefixName, hostName);
    }

    public static String getHostName() {
        return AppsFlyerLib.getInstance().getHostName();
    }

    public static String getHostPrefix() {
        return AppsFlyerLib.getInstance().getHostPrefix();
    }

    public static void setCollectOaid(boolean isCollect) {
        AppsFlyerLib.getInstance().setCollectOaid(isCollect);
    }

    public static void setSharingFilterForAllPartners() {
        AppsFlyerLib.getInstance().setSharingFilterForAllPartners();
    }

    public static void setSharingFilter(String ... partners) {
        AppsFlyerLib.getInstance().setSharingFilter(partners);
    }

    public static void getConversionData(final String objectName){
        if (conversionListener == null){
            conversionListener = getConversionListener(objectName);
        }

        AppsFlyerLib.getInstance().registerConversionListener(UnityPlayer.currentActivity, conversionListener);
    }

    private static AppsFlyerConversionListener getConversionListener(final String objectName){

        return new AppsFlyerConversionListener() {
            @Override
            public void onConversionDataSuccess(Map<String, Object> map) {
                if(objectName != null){
                    JSONObject jsonObject = new JSONObject(map);
                    UnityPlayer.UnitySendMessage(objectName, GCD_CALLBACK, jsonObject.toString());
                }
            }

            @Override
            public void onConversionDataFail(String s) {
                if(objectName != null){
                    UnityPlayer.UnitySendMessage(objectName, GCD_ERROR_CALLBACK, s);
                }
            }

            @Override
            public void onAppOpenAttribution(Map<String, String> map) {
                if(objectName != null){
                    JSONObject jsonObject = new JSONObject(map);
                    UnityPlayer.UnitySendMessage(objectName, OAOA_CALLBACK, jsonObject.toString());
                }
            }

            @Override
            public void onAttributionFailure(String s) {
                if(objectName != null){
                    UnityPlayer.UnitySendMessage(objectName, OAOA_ERROR_CALLBACK, s);
                }
            }
        };
    }

    private static Boolean parseNullableBoolean(String value) {
        if (value == null) return null;
        if (value.equalsIgnoreCase("true")) return true;
        if (value.equalsIgnoreCase("false")) return false;
        return null;
    }

    public static void initInAppPurchaseValidatorListener(final String objectName) {
        AppsFlyerLib.getInstance().registerValidatorListener(UnityPlayer.currentActivity, new AppsFlyerInAppPurchaseValidatorListener() {
            @Override
            public void onValidateInApp() {
                if(objectName != null){
                    UnityPlayer.UnitySendMessage(objectName, VALIDATE_CALLBACK, "Validate success");
                }
            }

            @Override
            public void onValidateInAppFailure(String error) {
                if(objectName != null){
                    UnityPlayer.UnitySendMessage(objectName, VALIDATE_ERROR_CALLBACK, error);
                }
            }
        });
    }

    public static AppsFlyerInAppPurchaseValidationCallback initInAppPurchaseValidatorV2Listener(final String objectName) {
        return new AppsFlyerInAppPurchaseValidationCallback() {
            @Override
            public void onInAppPurchaseValidationFinished(@NonNull Map<String, ?> map) {
                if (objectName != null) {
                    JSONObject jsonObject = new JSONObject(map);
                    UnityPlayer.UnitySendMessage(objectName, VALIDATE_AND_LOG_V2_CALLBACK, jsonObject.toString());
                }
            }

            @Override
            public void onInAppPurchaseValidationError(@NonNull Map<String, ?> map) {
                if (objectName != null) {
                    JSONObject jsonObject = new JSONObject(map);
                    UnityPlayer.UnitySendMessage(objectName, VALIDATE_AND_LOG_V2__ERROR_CALLBACK, jsonObject.toString());
                }
            }
        };
    }

    public static void handlePushNotifications(){
        AppsFlyerLib.getInstance().sendPushNotificationData(UnityPlayer.currentActivity);
    }

    public static void setPhoneNumber(String phoneNumber){
        AppsFlyerLib.getInstance().setPhoneNumber(phoneNumber);
    }

    public static void attributeAndOpenStore(String promoted_app_id, String campaign, Map<String, String> userParams) {
        CrossPromotionHelper.logAndOpenStore(UnityPlayer.currentActivity, promoted_app_id, campaign, userParams);
    }

    public static void recordCrossPromoteImpression(String appID, String campaign, Map<String,String> params){
        CrossPromotionHelper.logCrossPromoteImpression(UnityPlayer.currentActivity, appID, campaign, params);
    }

    public static void createOneLinkInviteListener(Map<String,String> params, final String objectName){

        LinkGenerator linkGenerator = ShareInviteHelper.generateInviteUrl(UnityPlayer.currentActivity);

        linkGenerator.setChannel(params.get("channel"));
        linkGenerator.setCampaign(params.get("campaign"));
        linkGenerator.setReferrerName(params.get("referrerName"));
        linkGenerator.setReferrerImageURL(params.get("referrerImageUrl"));
        linkGenerator.setReferrerCustomerId(params.get("customerID"));
        linkGenerator.setBaseDeeplink(params.get("baseDeepLink"));
        linkGenerator.setBrandDomain(params.get("brandDomain"));

        params.remove("channel");
        params.remove("campaign");
        params.remove("referrerName");
        params.remove("referrerImageUrl");
        params.remove("customerID");
        params.remove("baseDeepLink");
        params.remove("brandDomain");

        linkGenerator.addParameters(params);

        linkGenerator.generateLink(UnityPlayer.currentActivity, new LinkGenerator.ResponseListener() {
            @Override
            public void onResponse(String link) {
                if(objectName != null){
                    UnityPlayer.UnitySendMessage(objectName, GENERATE_LINK_CALLBACK, link);
                }
            }

            @Override
            public void onResponseError(String error) {
                if(objectName != null){
                    UnityPlayer.UnitySendMessage(objectName, GENERATE_LINK_ERROR_CALLBACK, error);
                }
            }
        });

    }

    public static void subscribeForDeepLink(final String objectName){
        if (ddlTimeout != DDL_TIMEOUT_DEFAULT) {
            AppsFlyerLib.getInstance().subscribeForDeepLink(new DeepLinkListener() {
                @Override
                public void onDeepLinking(@NonNull DeepLinkResult deepLinkResult) {
                    if(objectName != null){
                        UnityPlayer.UnitySendMessage(objectName, ON_DEEPLINKING, deepLinkResult.toString());
                    }
                }
            }, ddlTimeout);
        } else
        {
            AppsFlyerLib.getInstance().subscribeForDeepLink(new DeepLinkListener() {
                @Override
                public void onDeepLinking(@NonNull DeepLinkResult deepLinkResult) {
                    if(objectName != null){
                        UnityPlayer.UnitySendMessage(objectName, ON_DEEPLINKING, deepLinkResult.toString());
                    }
                }
            });
        }
    }

    public static void addPushNotificationDeepLinkPath(String ... path){
        AppsFlyerLib.getInstance().addPushNotificationDeepLinkPath(path);
    }

    public static void setDisableAdvertisingIdentifiers(boolean disable){
        AppsFlyerLib.getInstance().setDisableAdvertisingIdentifiers(disable);
    }

    public static void setSharingFilterForPartners(java.lang.String... partners){
        AppsFlyerLib.getInstance().setSharingFilterForPartners(partners);
    }

    public static void setDisableNetworkData(boolean disable){
        AppsFlyerLib.getInstance().setDisableNetworkData(disable);
    }

    public static void setPluginInfo() {
        PluginInfo pluginInfo = new PluginInfo(Plugin.UNITY, PLUGIN_VERSION);
        AppsFlyerLib.getInstance().setPluginInfo(pluginInfo);
    }

    public static void setDeepLinkTimeout(long deepLinkTimeout) {
        ddlTimeout = deepLinkTimeout;
    }
}
