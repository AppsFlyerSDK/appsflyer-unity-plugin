package com.appsflyer.unity

import com.appsflyer.AppsFlyerLib
import com.appsflyer.pluginbridge.handler.AppsFlyerRpcHandler
import com.appsflyer.pluginbridge.model.RpcResponse
import com.appsflyer.pluginbridge.parser.JsonRpcRequestParser
import com.unity3d.player.UnityPlayer
import org.json.JSONException
import org.json.JSONObject

/**
 * Unity-facing RPC bridge for the AppsFlyer Android SDK.
 * Called via AndroidJavaClass from AppsFlyerRPCClient.cs.
 *
 * Must be initialized once (via init) before any fireJson/executeJson calls.
 */
object AppsFlyerRPCBridge {

    @Volatile
    private var sHandler: AppsFlyerRpcHandler? = null

    @Volatile
    private var sCallbackObjectName: String? = null

    /**
     * Initializes the bridge. Must be called once during SDK setup, before any RPC calls.
     *
     * @param callbackObjectName Unity GameObject name that receives "onRPCEvent" messages.
     */
    @JvmStatic
    fun init(callbackObjectName: String) {
        sCallbackObjectName = callbackObjectName
        // af-android-plugin-bridge:7.0.1's AppsFlyerRpcHandler only accepts a fixed Context at
        // construction time (no per-call context-provider overload yet), and sHandler is a
        // process-lifetime singleton — so we must pass applicationContext, not currentActivity,
        // to avoid pinning a since-destroyed Activity for the life of the process.
        val context = UnityPlayer.currentActivity?.applicationContext ?: return
        sHandler = AppsFlyerRpcHandler(context, { eventJson ->
            val obj = sCallbackObjectName
            if (!obj.isNullOrEmpty()) {
                UnityPlayer.UnitySendMessage(obj, "onRPCEvent", eventJson)
            }
            Unit
        }, AppsFlyerLib.getInstance(), JsonRpcRequestParser())
    }

    /**
     * Fire-and-forget: dispatches an RPC request without blocking for a result.
     * Used for setter methods and other void operations.
     */
    @JvmStatic
    fun fireJson(jsonRequest: String) {
        sHandler?.execute(jsonRequest)
    }

    /**
     * Synchronous execute: dispatches an RPC request and returns a JSON response.
     * Used for getter methods that return data to C#.
     */
    @JvmStatic
    fun executeJson(jsonRequest: String): String {
        val handler = sHandler
            ?: return "{\"error\":{\"code\":503,\"message\":\"RPC bridge not initialized — call init() first\"}}"
        return serializeResponse(handler.execute(jsonRequest))
    }

    private fun serializeResponse(response: RpcResponse): String {
        return try {
            val json = JSONObject()
            when (response) {
                is RpcResponse.VoidSuccess -> json.put("result", JSONObject().put("data", JSONObject.NULL))
                is RpcResponse.Success<*> -> json.put("result", JSONObject().put("data", response.result ?: JSONObject.NULL))
                is RpcResponse.Error -> json.put(
                    "error",
                    JSONObject()
                        .put("code", response.code)
                        .put("message", response.message)
                )
                else -> {}
            }
            json.toString()
        } catch (e: JSONException) {
            "{\"error\":{\"code\":500,\"message\":\"Response serialization failed\"}}"
        }
    }
}
