package com.appsflyer.unity;

import android.content.Context;
import android.util.Log;

import com.appsflyer.AppsFlyerLib;
import com.appsflyer.pluginbridge.handler.AppsFlyerRpcHandler;
import com.appsflyer.pluginbridge.model.RpcResponse;
import com.appsflyer.pluginbridge.parser.JsonRpcRequestParser;
import com.unity3d.player.UnityPlayer;

import org.json.JSONException;
import org.json.JSONObject;

import kotlin.Unit;
import kotlin.jvm.functions.Function1;

/**
 * RPC-based Unity Android Wrapper for AppsFlyer SDK 7.0.0
 * 
 * Architecture:
 * Unity C# → executeJson(jsonRequest) → RPC Handler → SDK 7.0.0
 * SDK Callbacks → Event Notifier → UnitySendMessage → Unity C#
 */
public class AppsFlyerAndroidWrapperRPC {

    private static final String TAG = "AFUnityRPC";
    
    // RPC Components
    private static AppsFlyerRpcHandler rpcHandler;
    private static JsonRpcRequestParser parser;
    
    // Unity callback object name
    private static String unityObjectName;
    
    // =================================================================================
    // RPC HANDLER INITIALIZATION
    // =================================================================================
    
    /**
     * Initialize RPC handler with event notifier
     * Called automatically on first executeJson() call
     */
    private static void initRpcHandler() {
        if (rpcHandler == null) {
            Context context = UnityPlayer.currentActivity;
            
            // Event notifier lambda - receives SDK callbacks as JSON
            Function1<String, Unit> eventNotifier = new Function1<String, Unit>() {
                @Override
                public Unit invoke(String eventJson) {
                    handleRpcEvent(eventJson);
                    return Unit.INSTANCE;
                }
            };
            
            // JSON parser
            parser = new JsonRpcRequestParser();
            
            // Create RPC handler
            rpcHandler = new AppsFlyerRpcHandler(
                context,                        // Android context
                eventNotifier,                  // Callback lambda
                AppsFlyerLib.getInstance(),     // SDK instance
                parser                          // JSON parser
            );
            
            Log.d(TAG, "RPC Handler initialized");
        }
    }
    
    // =================================================================================
    // EVENT HANDLING (SDK → Unity)
    // =================================================================================
    
    /**
     * Handle RPC events from SDK callbacks
     * Converts RPC event JSON to Unity callbacks
     */
    private static void handleRpcEvent(String eventJson) {
        try {
            Log.d(TAG, "RPC Event received: " + eventJson);
            
            JSONObject event = new JSONObject(eventJson);
            String eventName = event.getString("event");
            JSONObject data = event.optJSONObject("data");
            
            if (unityObjectName != null) {
                String unityCallbackName = mapEventToUnityCallback(eventName);
                if (unityCallbackName != null) {
                    String dataJson = data != null ? data.toString() : "{}";
                    UnityPlayer.UnitySendMessage(unityObjectName, unityCallbackName, dataJson);
                }
            }
            
        } catch (JSONException e) {
            Log.e(TAG, "Failed to parse RPC event: " + e.getMessage());
        }
    }
    
    /**
     * Map RPC event names to Unity callback method names
     */
    private static String mapEventToUnityCallback(String rpcEventName) {
        // TODO: Map RPC events to Unity callbacks
        // Example: "onConversionDataSuccess" → "onConversionDataSuccess"
        Log.d(TAG, "Mapping RPC event: " + rpcEventName);
        return rpcEventName;
    }
    
    // =================================================================================
    // SINGLE RPC ENTRY POINT (Unity → SDK)
    // =================================================================================
    
    /**
     * Main RPC execution entry point
     * 
     * @param jsonRequest JSON-RPC request string
     * @return JSON-RPC response string
     * 
     * Request format:
     * {
     *   "method": "init",
     *   "params": {"devKey": "xxx", "hasConversionListener": true}
     * }
     * 
     * Response format:
     * {
     *   "success": true,
     *   "result": {...}
     * }
     * OR
     * {
     *   "success": false,
     *   "error": {"code": 500, "message": "..."}
     * }
     */
    public static String executeJson(String jsonRequest) {
        try {
            // Initialize RPC handler on first call
            initRpcHandler();
            
            Log.d(TAG, "Executing RPC: " + jsonRequest);
            
            // Execute RPC request
            RpcResponse response = rpcHandler.execute(jsonRequest);
            
            // Convert response to JSON
            String jsonResponse = rpcResponseToJson(response);
            
            Log.d(TAG, "RPC Response: " + jsonResponse);
            
            return jsonResponse;
            
        } catch (Exception e) {
            Log.e(TAG, "RPC execution failed", e);
            return createErrorResponse(500, e.getMessage());
        }
    }
    
    // =================================================================================
    // RESPONSE CONVERSION
    // =================================================================================
    
    /**
     * Convert RpcResponse to JSON string
     */
    private static String rpcResponseToJson(RpcResponse response) {
        try {
            JSONObject json = new JSONObject();
            
            if (response instanceof RpcResponse.Success) {
                json.put("success", true);
                Object result = ((RpcResponse.Success<?>) response).component1();
                json.put("result", result != null ? result : JSONObject.NULL);
                
            } else if (response instanceof RpcResponse.VoidSuccess) {
                json.put("success", true);
                json.put("result", JSONObject.NULL);
                
            } else if (response instanceof RpcResponse.Error) {
                json.put("success", false);
                JSONObject error = new JSONObject();
                error.put("code", ((RpcResponse.Error) response).component1());
                error.put("message", ((RpcResponse.Error) response).component2());
                json.put("error", error);
            }
            
            return json.toString();
            
        } catch (JSONException e) {
            return createErrorResponse(500, "Failed to serialize response");
        }
    }
    
    /**
     * Create error response JSON
     */
    private static String createErrorResponse(int code, String message) {
        try {
            JSONObject json = new JSONObject();
            json.put("success", false);
            JSONObject error = new JSONObject();
            error.put("code", code);
            error.put("message", message != null ? message : "Unknown error");
            json.put("error", error);
            return json.toString();
        } catch (JSONException e) {
            return "{\"success\":false,\"error\":{\"code\":500,\"message\":\"Failed to create error\"}}";
        }
    }
    
    // =================================================================================
    // UNITY OBJECT NAME (for callbacks)
    // =================================================================================
    
    /**
     * Set Unity object name for callbacks
     * Called from C# before any RPC calls
     */
    public static void setUnityObjectName(String objectName) {
        unityObjectName = objectName;
        Log.d(TAG, "Unity object name set: " + objectName);
    }
}


// =================================================================================
// OLD APPSFLYER CODE (COMMENTED OUT FOR REFERENCE)
// =================================================================================

/*
 * OLD IMPLEMENTATION - Direct SDK calls
 * 
 * public static void initSDK(String devKey, String objectName) {
 *     if (conversionListener == null && objectName != null){
 *         conversionListener = getConversionListener(objectName);
 *     }
 *     devkey = devKey;
 *     setPluginInfo();
 *     AppsFlyerLib.getInstance().init(devKey, conversionListener, UnityPlayer.currentActivity);
 * }
 * 
 * public static void startTracking(final boolean shouldCallback, final String objectName) {
 *     AppsFlyerLib.getInstance().start(UnityPlayer.currentActivity, devkey, new AppsFlyerRequestListener() {
 *         @Override
 *         public void onSuccess() {
 *             if(shouldCallback && objectName != null){
 *                 Map<String,Object> map = new HashMap<String,Object>();
 *                 map.put("statusCode", 200);
 *                 JSONObject jsonObject = new JSONObject(map);
 *                 UnityPlayer.UnitySendMessage(objectName, START_REQUEST_CALLBACK, jsonObject.toString());
 *             }
 *         }
 *         
 *         @Override
 *         public void onError(int i, @NonNull String s) {
 *             if(shouldCallback && objectName != null){
 *                 Map<String,Object> map = new HashMap<String,Object>();
 *                 map.put("statusCode", i);
 *                 map.put("errorDescription", s);
 *                 JSONObject jsonObject = new JSONObject(map);
 *                 UnityPlayer.UnitySendMessage(objectName, START_REQUEST_CALLBACK, jsonObject.toString());
 *             }
 *         }
 *     });
 * }
 */
