package com.appsflyer.unity;

import android.annotation.SuppressLint;
import android.os.Build;
import android.util.Log;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import androidx.annotation.RequiresApi;

import com.appsflyer.api.PurchaseClient;
import com.appsflyer.api.Store;
import com.appsflyer.internal.models.InAppPurchaseValidationResult;
import com.appsflyer.internal.models.ProductPurchase;
import com.appsflyer.internal.models.SubscriptionPurchase;
import com.appsflyer.internal.models.SubscriptionValidationResult;
import com.appsflyer.internal.models.ValidationFailureData;

import com.unity3d.player.UnityPlayer;

import org.json.JSONObject;

import java.util.HashMap;
import java.util.Map;

/**
 * Bridge for {@link com.appsflyer.share.PurchaseConnector} (AppsFlyerPurchaseConnector.cs).
 * The RPC bridge (AppsFlyerRPCBridge.kt / AppsFlyerRpcHandler) now covers the rest of the
 * legacy AppsFlyerAndroidWrapper surface; only the Purchase Connector methods below still
 * have a caller (AppsFlyerPurchaseConnector.cs, via AndroidJavaClass reflection).
 */
public class AppsFlyerAndroidWrapper {

    private static final String VALIDATION_CALLBACK = "didReceivePurchaseRevenueValidationInfo";
    private static final String ERROR_CALLBACK = "didReceivePurchaseRevenueError";

    private static PurchaseClient purchaseClientInstance;
    private static PurchaseClient.Builder builder;

    private static String unityObjectName;

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
