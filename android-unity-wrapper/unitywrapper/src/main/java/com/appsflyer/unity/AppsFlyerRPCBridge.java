package com.appsflyer.unity;

import android.content.Context;

import com.appsflyer.AppsFlyerLib;
import kotlin.Unit;
import com.appsflyer.pluginbridge.handler.AppsFlyerRpcHandler;
import com.appsflyer.pluginbridge.model.RpcResponse;
import com.appsflyer.pluginbridge.parser.JsonRpcRequestParser;
import com.unity3d.player.UnityPlayer;

import org.json.JSONException;
import org.json.JSONObject;

/**
 * Unity-facing RPC bridge for the AppsFlyer Android SDK.
 * Called via AndroidJavaClass from AppsFlyerRPCClient.cs.
 *
 * Must be initialized once (via init) before any fireJson/executeJson calls.
 */
public class AppsFlyerRPCBridge {

    private static volatile AppsFlyerRpcHandler sHandler;
    private static volatile String sCallbackObjectName;

    /**
     * Initializes the bridge. Must be called once during SDK setup, before any RPC calls.
     *
     * @param callbackObjectName Unity GameObject name that receives "onRPCEvent" messages.
     */
    public static void init(String callbackObjectName) {
        sCallbackObjectName = callbackObjectName;
        Context context = UnityPlayer.currentActivity;
        sHandler = new AppsFlyerRpcHandler(context, eventJson -> {
            String obj = sCallbackObjectName;
            if (obj != null && !obj.isEmpty()) {
                UnityPlayer.UnitySendMessage(obj, "onRPCEvent", eventJson);
            }
            return Unit.INSTANCE;
        }, AppsFlyerLib.getInstance(), new JsonRpcRequestParser());
    }

    /**
     * Fire-and-forget: dispatches an RPC request without blocking for a result.
     * Used for setter methods and other void operations.
     */
    public static void fireJson(String jsonRequest) {
        AppsFlyerRpcHandler handler = sHandler;
        if (handler != null) {
            handler.execute(jsonRequest);
        }
    }

    /**
     * Synchronous execute: dispatches an RPC request and returns a JSON response.
     * Used for getter methods that return data to C#.
     */
    public static String executeJson(String jsonRequest) {
        AppsFlyerRpcHandler handler = sHandler;
        if (handler == null) {
            return "{\"error\":{\"code\":503,\"message\":\"RPC bridge not initialized — call init() first\"}}";
        }
        return serializeResponse(handler.execute(jsonRequest));
    }

    private static String serializeResponse(RpcResponse response) {
        try {
            JSONObject json = new JSONObject();
            if (response instanceof RpcResponse.VoidSuccess) {
                json.put("result", new JSONObject().put("data", JSONObject.NULL));
            } else if (response instanceof RpcResponse.Success) {
                Object value = ((RpcResponse.Success<?>) response).getResult();
                json.put("result", new JSONObject().put("data", value != null ? value : JSONObject.NULL));
            } else if (response instanceof RpcResponse.Error) {
                RpcResponse.Error error = (RpcResponse.Error) response;
                json.put("error", new JSONObject()
                        .put("code", error.getCode())
                        .put("message", error.getMessage()));
            }
            return json.toString();
        } catch (JSONException e) {
            return "{\"error\":{\"code\":500,\"message\":\"Response serialization failed\"}}";
        }
    }
}
