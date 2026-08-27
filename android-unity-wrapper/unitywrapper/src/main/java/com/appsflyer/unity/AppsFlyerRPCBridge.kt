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
        // AppsFlyerRpcHandler wraps this provider in its own `by lazy` and only calls it on the
        // first actual RPC execution, not at construction time - so the handler can be built here
        // immediately without needing UnityPlayer.currentActivity to be set yet. Re-querying
        // currentActivity inside the lambda (instead of resolving it once up front) also means we
        // always read the current Activity's applicationContext rather than a value memoized
        // before it existed; applicationContext (not currentActivity itself) avoids pinning a
        // since-destroyed Activity for the life of this process-lifetime singleton.
        sHandler = AppsFlyerRpcHandler({ UnityPlayer.currentActivity!!.applicationContext }, { eventJson ->
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
        val requestId = extractRequestId(jsonRequest)
        val handler = sHandler
            ?: return serializeError(requestId, 503, "RPC bridge not initialized — call init() first")
        return serializeResponse(handler.execute(jsonRequest), requestId)
    }

    // Internal (not private) so AppsFlyerRPCBridgeTest can exercise the real serializer directly.
    internal fun serializeError(requestId: String?, code: Int, message: String): String {
        val json = JSONObject()
        if (requestId != null) json.put("id", requestId)
        json.put("error", JSONObject().put("code", code).put("message", message))
        return json.toString()
    }

    // AppsFlyerRPCClient.cs's ParseResponse rejects any response whose "id" doesn't match the
    // request it sent, so every response — success or error — must echo it back.
    internal fun extractRequestId(jsonRequest: String): String? {
        return try {
            val request = JSONObject(jsonRequest)
            if (request.has("id")) request.getString("id") else null
        } catch (e: JSONException) {
            null
        }
    }

    internal fun serializeResponse(response: RpcResponse, requestId: String?): String {
        return try {
            val json = JSONObject()
            if (requestId != null) json.put("id", requestId)
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
            serializeError(requestId, 500, "Response serialization failed")
        }
    }
}
