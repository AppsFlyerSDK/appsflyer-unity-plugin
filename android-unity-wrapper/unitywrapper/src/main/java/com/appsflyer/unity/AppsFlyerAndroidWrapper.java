package com.appsflyer.unity;


import androidx.annotation.NonNull;

import com.appsflyer.AFLogger;
import com.appsflyer.AppsFlyerConversionListener;
import com.appsflyer.AppsFlyerInAppPurchaseValidatorListener;
import com.appsflyer.AppsFlyerLib;
import com.appsflyer.AppsFlyerProperties;
import com.appsflyer.CreateOneLinkHttpTask;
import com.appsflyer.attribution.AppsFlyerRequestListener;
import com.appsflyer.deeplink.DeepLinkListener;
import com.appsflyer.deeplink.DeepLinkResult;
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
    private static final String ON_DEEPLINKING = "onDeepLinking";
    private static final String START_REQUEST_CALLBACK = "requestResponseReceived";
    private static final String IN_APP_RESPONSE_CALLBACK = "inAppResponseReceived";
    private static AppsFlyerConversionListener conversionListener;
    private static String devkey = "";

    public static void initSDK(String devKey, String objectName) {
        if (conversionListener == null && objectName != null){
            conversionListener = getConversionListener(objectName);
        }

        devkey = devKey;

        AppsFlyerLib.getInstance().init(devKey, conversionListener, UnityPlayer.currentActivity);
        AppsFlyerLib.getInstance().setExtension("unity_android_6.2.62");
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

    public static void subscribeForDeepLink(final String objectName){
        AppsFlyerLib.getInstance().subscribeForDeepLink(new DeepLinkListener() {
            @Override
            public void onDeepLinking(@NonNull DeepLinkResult deepLinkResult) {
                if(objectName != null){
                    UnityPlayer.UnitySendMessage(objectName, ON_DEEPLINKING, deepLinkResult.toString());
                }
            }
        });
    }

    public static void addPushNotificationDeepLinkPath(String ... path){
        AppsFlyerLib.getInstance().addPushNotificationDeepLinkPath(path);
    }
}
