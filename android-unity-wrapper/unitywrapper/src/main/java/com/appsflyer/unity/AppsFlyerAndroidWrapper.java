package com.appsflyer.unity;

import android.annotation.SuppressLint;
import android.os.Build;
import android.util.Log;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import androidx.annotation.RequiresApi;

import com.appsflyer.api.InAppPurchaseEvent;
import com.appsflyer.api.PurchaseClient;
import com.appsflyer.api.Store;
import com.appsflyer.api.SubscriptionPurchaseEvent;
import com.appsflyer.internal.models.InAppPurchaseValidationResult;
import com.appsflyer.internal.models.ProductPurchase;
import com.appsflyer.internal.models.SubscriptionPurchase;
import com.appsflyer.internal.models.SubscriptionValidationResult;
import com.appsflyer.internal.models.ValidationFailureData;

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
    private static final String VALIDATION_CALLBACK = "didReceivePurchaseRevenueValidationInfo";
    private static final String ERROR_CALLBACK = "didReceivePurchaseRevenueError";
    private static final String PLUGIN_VERSION = "6.17.72";

    private static final long DDL_TIMEOUT_DEFAULT = 3000;
    private static AppsFlyerConversionListener conversionListener;
    private static String devkey = "";
    private static long ddlTimeout = DDL_TIMEOUT_DEFAULT;

    private static PurchaseClient purchaseClientInstance;
    private static PurchaseClient.Builder builder;

    private static String unityObjectName;

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

    public static void validateAndTrackInAppPurchaseV2(int purchaseType, String purchaseToken, String productId, HashMap<String, String> purchaseAdditionalDetails, final String objectName) {
        AFPurchaseType type = purchaseType == 0 ? AFPurchaseType.SUBSCRIPTION : AFPurchaseType.ONE_TIME_PURCHASE;
        AFPurchaseDetails details = new AFPurchaseDetails(type, purchaseToken, productId);

        if (objectName != null){
            AppsFlyerInAppPurchaseValidationCallback listener = initInAppPurchaseValidatorV2Listener(objectName);
            AppsFlyerLib.getInstance().validateAndLogInAppPurchase(details, purchaseAdditionalDetails, listener);
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


    //Purchase Connector
    public static void initPurchaseConnector(String objectName, int store) {
        unityObjectName = objectName;
        Store s = mappingEnum(store);
        if (s != null) {
            builder = new PurchaseClient.Builder(UnityPlayer.currentActivity, s);
            builder = PurchaseRevenueBridge.configurePurchaseClient(builder);
        } else {
            Log.w("AppsFlyer_Connector", "[PurchaseConnector]: Please choose a valid store.");
        }
    }

    public static void build() {
        if (builder != null) {
            purchaseClientInstance = builder.build();
        } else {
            Log.w("AppsFlyer_Connector", "[PurchaseConnector]: Initialization is required prior to building.");
        }
    }


    public static void setIsSandbox(boolean isSandbox) {
        if (builder != null) {
            builder.setSandbox(isSandbox);
        }
    }

    public static void setAutoLogSubscriptions(boolean logSubscriptions) {
        if (builder != null) {
            builder.logSubscriptions(logSubscriptions);
        }
    }

    public static void setAutoLogInApps(boolean autoLogInApps) {
        if (builder != null) {
            builder.autoLogInApps(autoLogInApps);
        }
    }

    public static void setPurchaseRevenueValidationListeners(boolean enableCallbacks) {
        if (builder != null && enableCallbacks) {
            builder.setSubscriptionValidationResultListener(new PurchaseClient.SubscriptionPurchaseValidationResultListener() {
                @RequiresApi(api = Build.VERSION_CODES.N)
                @Override
                public void onResponse(@Nullable Map<String, ? extends SubscriptionValidationResult> result) {
                    if (unityObjectName != null) {
                        if (result == null) {
                            return;
                        }
                        result.forEach((k, v) -> {
                            Map<String, Object> map = new HashMap<>();
                            Map<String, Object> mapSubscription = new HashMap<>();

                            map.put("productId", k);
                            map.put("success", v.getSuccess() ? "true" : "false");
                            if (v.getSuccess()) {
                                SubscriptionPurchase subscriptionPurchase = v.getSubscriptionPurchase();
                                Map<String, Object> mapCancelSurveyResult = new HashMap<>();
                                if (subscriptionPurchase.getCanceledStateContext() != null) {
                                    Map<String, Object> mapCanceledStateContext = new HashMap<>();
                                    Map<String, Object> mapUserInitiatedCancellation = new HashMap<>();
                                    if (subscriptionPurchase.getCanceledStateContext().
                                            getUserInitiatedCancellation() != null) {
                                        if (subscriptionPurchase.getCanceledStateContext().
                                                getUserInitiatedCancellation().
                                                getCancelSurveyResult() != null) {
                                            mapCancelSurveyResult.put("reason",
                                                    subscriptionPurchase.getCanceledStateContext().
                                                            getUserInitiatedCancellation().
                                                            getCancelSurveyResult().getReason());
                                            mapCancelSurveyResult.put("reasonUserInput",
                                                    subscriptionPurchase.getCanceledStateContext().
                                                            getUserInitiatedCancellation().
                                                            getCancelSurveyResult().getReasonUserInput());
                                            mapUserInitiatedCancellation.put("cancelSurveyResult",
                                                    mapCancelSurveyResult);
                                        }
                                        mapUserInitiatedCancellation.put("cancelTime",
                                                subscriptionPurchase.getCanceledStateContext().
                                                        getUserInitiatedCancellation().getCancelTime());
                                    }
                                    mapCanceledStateContext.put("developerInitiatedCancellation",
                                            null);
                                    mapCanceledStateContext.put("replacementCancellation",
                                            null);
                                    mapCanceledStateContext.put("systemInitiatedCancellation",
                                            null);
                                    mapCanceledStateContext.put("userInitiatedCancellation",
                                            mapUserInitiatedCancellation);
                                }
                                if (subscriptionPurchase.getExternalAccountIdentifiers() != null) {
                                    Map<String, Object> mapExternalAccountIdentifiers = new HashMap<>();
                                    mapExternalAccountIdentifiers.put("externalAccountId",
                                            subscriptionPurchase.getExternalAccountIdentifiers().
                                                    getExternalAccountId());
                                    mapExternalAccountIdentifiers.put("obfuscatedExternalAccountId",
                                            subscriptionPurchase.getExternalAccountIdentifiers().
                                                    getObfuscatedExternalAccountId());
                                    mapExternalAccountIdentifiers.put("obfuscatedExternalProfileId",
                                            subscriptionPurchase.getExternalAccountIdentifiers().
                                                    getObfuscatedExternalProfileId());
                                    mapSubscription.put("externalAccountIdentifiers",
                                            mapExternalAccountIdentifiers);
                                }
                                if (subscriptionPurchase.getPausedStateContext() != null) {
                                    Map<String, Object> mapPausedStateContext = new HashMap<>();
                                    mapPausedStateContext.put("autoResumeTime",
                                            subscriptionPurchase.getPausedStateContext().
                                                    getAutoResumeTime());
                                    mapSubscription.put("pausedStateContext", mapPausedStateContext);


                                }
                                if (subscriptionPurchase.getSubscribeWithGoogleInfo() != null) {
                                    Map<String, Object> mapSubscribeWithGoogleInfo = new HashMap<>();
                                    mapSubscribeWithGoogleInfo.put("emailAddress",
                                            subscriptionPurchase.getSubscribeWithGoogleInfo().getEmailAddress());
                                    mapSubscribeWithGoogleInfo.put("familyName",
                                            subscriptionPurchase.getSubscribeWithGoogleInfo().getFamilyName());
                                    mapSubscribeWithGoogleInfo.put("givenName",
                                            subscriptionPurchase.getSubscribeWithGoogleInfo().getGivenName());
                                    mapSubscribeWithGoogleInfo.put("profileId",
                                            subscriptionPurchase.getSubscribeWithGoogleInfo().getProfileId());
                                    mapSubscribeWithGoogleInfo.put("profileName",
                                            subscriptionPurchase.getSubscribeWithGoogleInfo().getProfileName());
                                    mapSubscription.put("subscribeWithGoogleInfo",
                                            mapSubscribeWithGoogleInfo);
                                }
                                int sizeItems = subscriptionPurchase.getLineItems().size();
                                Map<String, Object>[] lineItems = new Map[sizeItems];
                                for (int i = 0; i < sizeItems; i++) {
                                    Map<String, Object> mapSubscriptionPurchaseLineItem = new HashMap<>();
                                    mapSubscriptionPurchaseLineItem.put("expiryTime",
                                            subscriptionPurchase.getLineItems().get(i).getExpiryTime());
                                    mapSubscriptionPurchaseLineItem.put("productId",
                                            subscriptionPurchase.getLineItems().get(i).getProductId());
                                    if (subscriptionPurchase.getLineItems().get(i).getAutoRenewingPlan()
                                            != null) {
                                        Map<String, Object> mapAutoRenewingPlan = new HashMap<>();
                                        if (subscriptionPurchase.getLineItems().get(i).
                                                getAutoRenewingPlan().getAutoRenewEnabled() != null) {
                                            mapAutoRenewingPlan.put("autoRenewEnabled",
                                                    subscriptionPurchase.getLineItems().get(i).getAutoRenewingPlan().
                                                            getAutoRenewEnabled() ? "true" : "false");
                                        }
                                        if (subscriptionPurchase.getLineItems().get(i).getAutoRenewingPlan().
                                                getPriceChangeDetails() != null) {
                                            Map<String, Object> mapPriceChangeDetails = new HashMap<>();
                                            mapPriceChangeDetails.put("expectedNewPriceChargeTime",
                                                    subscriptionPurchase.getLineItems().get(i).
                                                            getAutoRenewingPlan().getPriceChangeDetails().
                                                            getExpectedNewPriceChargeTime());
                                            mapPriceChangeDetails.put("priceChangeMode",
                                                    subscriptionPurchase.getLineItems().get(i).
                                                            getAutoRenewingPlan().
                                                            getPriceChangeDetails().getPriceChangeMode());
                                            mapPriceChangeDetails.put("priceChangeState",
                                                    subscriptionPurchase.getLineItems().get(i).
                                                            getAutoRenewingPlan().
                                                            getPriceChangeDetails().getPriceChangeState());
                                            mapAutoRenewingPlan.put("priceChangeDetails", mapPriceChangeDetails);
                                            if (subscriptionPurchase.getLineItems().get(i).
                                                    getAutoRenewingPlan().getPriceChangeDetails().
                                                    getNewPrice() != null) {
                                                Map<String, Object> mapMoney = new HashMap<>();
                                                mapMoney.put("currencyCode",
                                                        subscriptionPurchase.getLineItems().get(i).
                                                                getAutoRenewingPlan().
                                                                getPriceChangeDetails().getNewPrice().
                                                                getCurrencyCode());
                                                mapMoney.put("nanos",
                                                        subscriptionPurchase.getLineItems().get(i).
                                                                getAutoRenewingPlan().
                                                                getPriceChangeDetails().getNewPrice().getNanos());
                                                mapMoney.put("units",
                                                        subscriptionPurchase.getLineItems().get(i).
                                                                getAutoRenewingPlan().
                                                                getPriceChangeDetails().getNewPrice().getUnits());
                                                mapPriceChangeDetails.put("newPrice", mapMoney);
                                            }
                                        }
                                        mapSubscriptionPurchaseLineItem.put("autoRenewingPlan", mapAutoRenewingPlan);
                                    }
                                    if (subscriptionPurchase.getLineItems().get(i).getOfferDetails() != null) {
                                        Map<String, Object> mapOfferDetails = new HashMap<>();
                                        mapOfferDetails.put("basePlanId",
                                                subscriptionPurchase.getLineItems().get(i).
                                                        getOfferDetails().getBasePlanId());
                                        if (subscriptionPurchase.getLineItems().get(i).
                                                getOfferDetails().getOfferId() != null) {
                                            mapOfferDetails.put("offerId",
                                                    subscriptionPurchase.getLineItems().get(i).
                                                            getOfferDetails().getOfferId());
                                        }
                                        mapSubscriptionPurchaseLineItem.put("offerDetails", mapOfferDetails);

                                    }
                                    if (subscriptionPurchase.getLineItems().get(i).getDeferredItemReplacement() != null) {
                                        Map<String, Object> mapDeferredItemReplacement = new HashMap<>();
                                        mapDeferredItemReplacement.put("productId",
                                                subscriptionPurchase.getLineItems().get(i).
                                                        getDeferredItemReplacement().getProductId());
                                        mapSubscriptionPurchaseLineItem.put("deferredItemReplacement",
                                                mapDeferredItemReplacement);

                                    }
                                    if (subscriptionPurchase.getLineItems().get(i).getPrepaidPlan() != null
                                            && subscriptionPurchase.getLineItems().get(i).
                                            getPrepaidPlan().getAllowExtendAfterTime() != null) {
                                        Map<String, Object> mapPrepaidPlan = new HashMap<>();
                                        mapPrepaidPlan.put("allowExtendAfterTime",
                                                subscriptionPurchase.getLineItems().get(i).
                                                        getPrepaidPlan().getAllowExtendAfterTime());
                                        mapSubscriptionPurchaseLineItem.put("prepaidPlan", mapPrepaidPlan);
                                    }
                                    lineItems[i] = mapSubscriptionPurchaseLineItem;
                                }
                                mapSubscription.put("lineItems", lineItems);
                                mapSubscription.put("acknowledgementState",
                                        subscriptionPurchase.getAcknowledgementState());
                                mapSubscription.put("canceledStateContext",
                                        subscriptionPurchase.getCanceledStateContext());
                                mapSubscription.put("kind",
                                        subscriptionPurchase.getKind());
                                mapSubscription.put("latestOrderId",
                                        subscriptionPurchase.getLatestOrderId());
                                mapSubscription.put("linkedPurchaseToken",
                                        subscriptionPurchase.getLinkedPurchaseToken());
                                mapSubscription.put("regionCode",
                                        subscriptionPurchase.getRegionCode());
                                mapSubscription.put("subscriptionState",
                                        subscriptionPurchase.getSubscriptionState());
                                mapSubscription.put("testPurchase", null);
                                mapSubscription.put("startTime",
                                        subscriptionPurchase.getStartTime());
                                map.put("subscriptionPurchase", mapSubscription);
                            } else {
                                ValidationFailureData failureData = v.getFailureData();
                                Map<String, Object> mapValidationFailureData = new HashMap<>();
                                mapValidationFailureData.put("status", failureData.getStatus());
                                mapValidationFailureData.put("description", failureData.getDescription());
                                map.put("failureData", mapValidationFailureData);
                            }
                            JSONObject resultObject = new JSONObject(map);
                            UnityPlayer.UnitySendMessage(unityObjectName, VALIDATION_CALLBACK,
                                    resultObject.toString());
                        });
                    }
                }

                @Override
                public void onFailure(@NonNull String result, @Nullable Throwable error) {
                    if (unityObjectName != null) {
                        UnityPlayer.UnitySendMessage(unityObjectName, ERROR_CALLBACK, result);
                    }
                }
            });

            builder.setInAppValidationResultListener(new PurchaseClient.InAppPurchaseValidationResultListener() {
                @SuppressLint("LongLogTag")
                @RequiresApi(api = Build.VERSION_CODES.N)
                @Override
                public void onResponse(@Nullable Map<String, ? extends InAppPurchaseValidationResult> result) {
                    if (unityObjectName != null) {
                        if (result == null) {
                            return;
                        }
                        result.forEach((k, v) -> {
                            Map<String, Object> map = new HashMap<>();
                            Map<String, Object> mapIAP = new HashMap<>();
//                            JSONObject jsonObject = new JSONObject(map);
                            map.put("token", k);
                            map.put("success", v.getSuccess() ? "true" : "false");
                            if (v.getSuccess()) {
                                ProductPurchase productPurchase = v.getProductPurchase();

                                mapIAP.put("productId", productPurchase.getProductId());
                                mapIAP.put("purchaseState", productPurchase.getPurchaseState());
                                mapIAP.put("kind", productPurchase.getKind());
                                mapIAP.put("purchaseTimeMillis", productPurchase.getPurchaseTimeMillis());
                                mapIAP.put("consumptionState", productPurchase.getConsumptionState());
                                mapIAP.put("developerPayload", productPurchase.getDeveloperPayload());
                                mapIAP.put("orderId", productPurchase.getOrderId());
                                mapIAP.put("purchaseType", productPurchase.getPurchaseType());
                                mapIAP.put("acknowledgementState", productPurchase.getAcknowledgementState());
                                mapIAP.put("purchaseToken", productPurchase.getPurchaseToken());
                                mapIAP.put("quantity", productPurchase.getQuantity());
                                mapIAP.put("obfuscatedExternalAccountId", productPurchase.getObfuscatedExternalAccountId());
                                mapIAP.put("obfuscatedExternalProfileId", productPurchase.getObfuscatedExternalProfileId());
                                mapIAP.put("regionCode", productPurchase.getRegionCode());


                                map.put("productPurchase", mapIAP);
                            } else {
                                ValidationFailureData failureData = v.getFailureData();
                                Map<String, Object> mapValidationFailureData = new HashMap<>();
                                mapValidationFailureData.put("status", failureData.getStatus());
                                mapValidationFailureData.put("description", failureData.getDescription());
                                map.put("failureData", mapValidationFailureData);
                            }

                            JSONObject resultObject = new JSONObject(map);
                            UnityPlayer.UnitySendMessage(unityObjectName, VALIDATION_CALLBACK, resultObject.toString());
                        });
                    }
                }

                @Override
                public void onFailure(@NonNull String result, @Nullable Throwable error) {
                    if (unityObjectName != null) {
                        UnityPlayer.UnitySendMessage(unityObjectName, ERROR_CALLBACK, result);
                    }
                }
            });
        }
    }


    public static void startObservingTransactions() {
        if (purchaseClientInstance != null) {
            purchaseClientInstance.startObservingTransactions();
        } else {
            Log.w("AppsFlyer_Connector", "[PurchaseConnector]: startObservingTransactions was not called because the purchase client instance is null, please call build() prior to this function.");
        }
    }

    public static void stopObservingTransactions() {
        if (purchaseClientInstance != null) {
            purchaseClientInstance.stopObservingTransactions();
        } else {
            Log.w("AppsFlyer_Connector", "[PurchaseConnector]: stopObservingTransactions was not called because the purchase client instance is null, please call build() prior to this function.");
        }
    }

    private static Store mappingEnum(int storeEnum) {
        switch (storeEnum) {
            case 0:
                return Store.GOOGLE;
            default:
                return null;
        }
    }
}
