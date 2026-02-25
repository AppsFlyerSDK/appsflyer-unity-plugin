package com.appsflyer.unity.rpc;

import android.app.Activity;
import android.util.Log;

import com.appsflyer.AppsFlyerLib;
import com.appsflyer.pluginbridge.handler.AppsFlyerRpcHandler;
import com.appsflyer.pluginbridge.model.RpcResponse;
import com.appsflyer.pluginbridge.parser.JsonRpcRequestParser;
import com.appsflyer.share.platform_extension.Plugin;
import com.appsflyer.share.platform_extension.PluginInfo;
import com.unity3d.player.UnityPlayer;

import org.json.JSONObject;

import java.util.Map;

import kotlin.Unit;
import kotlin.jvm.functions.Function1;

public class AppsFlyerRPCAndroidWrapper {

    private static final String TAG = "AppsFlyer_Unity_RPC";
    private static final String PLUGIN_VERSION = "6.17.81";

    private static AppsFlyerRpcHandler rpcHandler;
    private static String callbackObjectName;

    public static void init(String objectName) {
        callbackObjectName = objectName;
        Activity activity = UnityPlayer.currentActivity;

        rpcHandler = new AppsFlyerRpcHandler(
            activity,
            createEventNotifier(),
            AppsFlyerLib.getInstance(),
            new JsonRpcRequestParser()
        );

        setPluginInfo();
        registerSessionReadyListener();

        Log.d(TAG, "RPC handler initialized");
    }

    public static String execute(String jsonRequest) {
        if (rpcHandler == null) {
            Log.e(TAG, "RPC handler not initialized. Call init() first.");
            return "{\"error\":{\"code\":503,\"message\":\"Not initialized\"}}";
        }

        try {
            RpcResponse response = rpcHandler.execute(jsonRequest);
            return serializeResponse(response);
        } catch (Exception e) {
            Log.e(TAG, "execute failed: " + e.getMessage(), e);
            JSONObject error = new JSONObject();
            try {
                JSONObject inner = new JSONObject();
                inner.put("code", 500);
                inner.put("message", e.getMessage());
                error.put("error", inner);
            } catch (Exception ignored) {}
            return error.toString();
        }
    }

    private static void setPluginInfo() {
        try {
            PluginInfo pluginInfo = new PluginInfo(Plugin.UNITY, PLUGIN_VERSION);
            AppsFlyerLib.getInstance().setPluginInfo(pluginInfo);
        } catch (Exception e) {
            Log.e(TAG, "setPluginInfo failed: " + e.getMessage(), e);
        }
    }

    private static void registerSessionReadyListener() {
        try {
            AppsFlyerLib.getInstance().registerSessionReadyListener(() -> {
                Log.d(TAG, "Session ready");
                if (callbackObjectName != null && !callbackObjectName.isEmpty()) {
                    UnityPlayer.UnitySendMessage(callbackObjectName, "requestResponseReceived",
                        "{\"statusCode\":200}");
                }
            });
        } catch (Exception e) {
            Log.e(TAG, "registerSessionReadyListener failed: " + e.getMessage(), e);
        }
    }

    private static Function1<String, Unit> createEventNotifier() {
        return eventJson -> {
            handleEvent(eventJson);
            return Unit.INSTANCE;
        };
    }

    private static String serializeResponse(RpcResponse response) {
        try {
            JSONObject json = new JSONObject();
            if (response instanceof RpcResponse.VoidSuccess) {
                json.put("result", JSONObject.NULL);
            } else if (response instanceof RpcResponse.Success) {
                Object result = ((RpcResponse.Success<?>) response).getResult();
                if (result instanceof Map) {
                    json.put("result", new JSONObject((Map) result));
                } else {
                    json.put("result", result != null ? result : JSONObject.NULL);
                }
            } else if (response instanceof RpcResponse.Error) {
                RpcResponse.Error err = (RpcResponse.Error) response;
                JSONObject errorObj = new JSONObject();
                errorObj.put("code", err.getCode());
                errorObj.put("message", err.getMessage());
                json.put("error", errorObj);
            }
            return json.toString();
        } catch (Exception e) {
            return "{\"error\":{\"code\":500,\"message\":\"Serialization failed\"}}";
        }
    }

    private static void handleEvent(String eventJson) {
        if (callbackObjectName == null || callbackObjectName.isEmpty()) return;

        Log.d(TAG, "Event received: " + eventJson);

        try {
            JSONObject event = new JSONObject(eventJson);
            String eventType = event.optString("event", "");
            Object data = event.opt("data");

            String callbackMethod = null;
            switch (eventType) {
                case "onConversionDataSuccess":
                    callbackMethod = "onConversionDataSuccess";
                    break;
                case "onConversionDataFail":
                    callbackMethod = "onConversionDataFail";
                    break;
                case "onAppOpenAttribution":
                    callbackMethod = "onAppOpenAttribution";
                    break;
                case "onAppOpenAttributionFailure":
                    callbackMethod = "onAppOpenAttributionFailure";
                    break;
                case "onDeepLinkReceived":
                    callbackMethod = "onDeepLinking";
                    break;
            }

            if (callbackMethod != null) {
                String payload = (data != null && data != JSONObject.NULL) ? data.toString() : "";
                Log.d(TAG, "Event -> " + callbackMethod + ": " + payload);
                UnityPlayer.UnitySendMessage(callbackObjectName, callbackMethod, payload);
            }
        } catch (Exception e) {
            Log.e(TAG, "Event handling failed: " + e.getMessage(), e);
        }
    }
}
