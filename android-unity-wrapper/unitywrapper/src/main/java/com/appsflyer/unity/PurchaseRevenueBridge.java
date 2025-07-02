package com.appsflyer.unity;

import android.util.Log;
import com.appsflyer.api.InAppPurchaseEvent;
import com.appsflyer.api.SubscriptionPurchaseEvent;
import com.appsflyer.api.PurchaseClient;
import com.appsflyer.api.Store;
import com.appsflyer.internal.models.InAppPurchaseValidationResult;
import com.appsflyer.internal.models.SubscriptionValidationResult;
import com.appsflyer.internal.models.ValidationFailureData;
import com.unity3d.player.UnityPlayer;

import org.json.JSONObject;
import org.json.JSONException;
import org.json.JSONArray;

import java.util.Collections;
import java.util.HashMap;
import java.util.List;
import java.util.Map;
import java.util.Iterator;

public class PurchaseRevenueBridge {
    private static final String TAG = "AppsFlyerUnity";

    public interface UnityPurchaseRevenueBridge {
        String getAdditionalParameters(String productsJson, String transactionsJson);
    }

    private static UnityPurchaseRevenueBridge unityBridge;

    public static void setUnityBridge(UnityPurchaseRevenueBridge bridge) {
        unityBridge = bridge;
    }

    public static PurchaseClient.Builder configurePurchaseClient(PurchaseClient.Builder builder) {
        return builder
            .setInAppPurchaseEventDataSource(purchaseEvents -> {
                try {
                    String eventsJson = new JSONObject(Collections.singletonMap("events", purchaseEvents)).toString();
                    String response = unityBridge != null ? unityBridge.getAdditionalParameters(eventsJson, "") : null;
                    if (response != null) {
                        JSONObject json = new JSONObject(response);
                        Map<String, Object> map = new HashMap<>();
                        Iterator<String> keys = json.keys();
                        while (keys.hasNext()) {
                            String key = keys.next();
                            map.put(key, json.get(key));
                        }
                        return map;
                    }
                } catch (JSONException e) {
                    Log.e(TAG, "Failed to parse additional params from Unity", e);
                }
                return Collections.emptyMap();
            })
            .setSubscriptionPurchaseEventDataSource(purchaseEvents -> {
                try {
                    String eventsJson = new JSONObject(Collections.singletonMap("events", purchaseEvents)).toString();
                    String response = unityBridge != null ? unityBridge.getAdditionalParameters("", eventsJson) : null;
                    if (response != null) {
                        JSONObject json = new JSONObject(response);
                        Map<String, Object> map = new HashMap<>();
                        Iterator<String> keys = json.keys();
                        while (keys.hasNext()) {
                            String key = keys.next();
                            map.put(key, json.get(key));
                        }
                        return map;
                    }
                } catch (JSONException e) {
                    Log.e(TAG, "Failed to parse additional params from Unity", e);
                }
                return Collections.emptyMap();
            });
    }
}
