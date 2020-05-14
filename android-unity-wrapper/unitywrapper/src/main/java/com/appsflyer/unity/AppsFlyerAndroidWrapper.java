package com.appsflyer.unity;


import com.appsflyer.AFLogger;
import com.appsflyer.AppsFlyerConversionListener;
import com.appsflyer.AppsFlyerInAppPurchaseValidatorListener;
import com.appsflyer.AppsFlyerLib;
import com.appsflyer.AppsFlyerProperties;
import com.appsflyer.CreateOneLinkHttpTask;
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
    private static final String GCD_CALLBACK = "onConversionDataSuccess";
    private static final String GCD_ERROR_CALLBACK = "onConversionDataFail";
    private static final String OAOA_CALLBACK = "onAppOpenAttribution";
    private static final String OAOA_ERROR_CALLBACK = "onAppOpenAttributionFailure";
    private static final String GENERATE_LINK_CALLBACK = "onInviteLinkGenerated";
    private static final String GENERATE_LINK_ERROR_CALLBACK = "onInviteLinkGeneratedFailure";
    private static AppsFlyerConversionListener conversionListener;

    public static void initSDK(String devKey, String objectName) {
        if (conversionListener == null && objectName != null){
            conversionListener = getConversionListener(objectName);
        }

        AppsFlyerLib.getInstance().init(devKey, conversionListener, UnityPlayer.currentActivity);
        AppsFlyerLib.getInstance().setExtension("unity_android_5.3.1");
    }

    public static void startTracking() {
        AppsFlyerLib.getInstance().startTracking(UnityPlayer.currentActivity);
    }

    public static void stopTracking(boolean isTrackingStopped) {
        AppsFlyerLib.getInstance().stopTracking(isTrackingStopped, UnityPlayer.currentActivity);
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
        AppsFlyerLib.getInstance().setCustomerIdAndTrack(id, UnityPlayer.currentActivity);
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
        AppsFlyerLib.getInstance().trackLocation(UnityPlayer.currentActivity, latitude, longitude);
    }

    public static void trackEvent(String eventName, HashMap<String, Object> eventValues) {
        AppsFlyerLib.getInstance().trackEvent(UnityPlayer.currentActivity, eventName, eventValues);
    }

    public static void setDeviceTrackingDisabled(boolean isDisabled) {
        AppsFlyerLib.getInstance().setDeviceTrackingDisabled(isDisabled);
    }

    public static void enableFacebookDeferredApplinks(boolean isEnabled) {
        AppsFlyerLib.getInstance().enableFacebookDeferredApplinks(isEnabled);
    }

    public static void setConsumeAFDeepLinks(boolean doConsume) {
        AppsFlyerLib.getInstance().setConsumeAFDeepLinks(doConsume);
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
        AppsFlyerLib.getInstance().validateAndTrackInAppPurchase(UnityPlayer.currentActivity, publicKey, signature, purchaseData, price, currency, additionalParameters);
        if (objectName != null){
            initInAppPurchaseValidatorListener(objectName);
        }
    }

    public static boolean isTrackingStopped() {
        return AppsFlyerLib.getInstance().isTrackingStopped();
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

    public static void handlePushNotifications(){
        AppsFlyerLib.getInstance().sendPushNotificationData(UnityPlayer.currentActivity);
    }

    public static void attributeAndOpenStore(String promoted_app_id, String campaign, Map<String, String> userParams) {
        CrossPromotionHelper.trackAndOpenStore(UnityPlayer.currentActivity, promoted_app_id, campaign, userParams);
    }

    public static void recordCrossPromoteImpression(String appID, String campaign){
        CrossPromotionHelper.trackCrossPromoteImpression(UnityPlayer.currentActivity, appID, campaign);
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

        linkGenerator.generateLink(UnityPlayer.currentActivity, new CreateOneLinkHttpTask.ResponseListener() {
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
}
