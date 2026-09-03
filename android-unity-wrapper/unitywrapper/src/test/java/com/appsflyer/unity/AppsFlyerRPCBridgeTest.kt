package com.appsflyer.unity

import com.appsflyer.pluginbridge.model.RpcResponse
import org.json.JSONObject
import org.junit.Assert.assertEquals
import org.junit.Assert.assertNull
import org.junit.Test

// Exercises the real Kotlin serializer AppsFlyerRPCBridge.executeJson() ultimately calls, rather
// than a mocked IAppsFlyerRPCClient — AppsFlyerRPCClient.cs's ParseResponse rejects any response
// whose "id" doesn't echo the request's, so every branch here must round-trip it.
class AppsFlyerRPCBridgeTest {

    @Test
    fun serializeResponse_success_echoesRequestId() {
        val json = AppsFlyerRPCBridge.serializeResponse(RpcResponse.Success("hello"), "req-1")
        val root = JSONObject(json)
        assertEquals("req-1", root.getString("id"))
        assertEquals("hello", root.getJSONObject("result").getString("data"))
    }

    @Test
    fun serializeResponse_voidSuccess_echoesRequestId() {
        val json = AppsFlyerRPCBridge.serializeResponse(RpcResponse.VoidSuccess, "req-2")
        val root = JSONObject(json)
        assertEquals("req-2", root.getString("id"))
        assertEquals(JSONObject.NULL, root.getJSONObject("result").get("data"))
    }

    @Test
    fun serializeResponse_error_echoesRequestId() {
        val json = AppsFlyerRPCBridge.serializeResponse(RpcResponse.Error(42, "boom"), "req-3")
        val root = JSONObject(json)
        assertEquals("req-3", root.getString("id"))
        assertEquals(42, root.getJSONObject("error").getInt("code"))
        assertEquals("boom", root.getJSONObject("error").getString("message"))
    }

    @Test
    fun serializeResponse_nullRequestId_omitsId() {
        val json = AppsFlyerRPCBridge.serializeResponse(RpcResponse.VoidSuccess, null)
        assertNull(JSONObject(json).opt("id"))
    }

    @Test
    fun serializeError_echoesRequestId() {
        val json = AppsFlyerRPCBridge.serializeError("req-4", 503, "not initialized")
        val root = JSONObject(json)
        assertEquals("req-4", root.getString("id"))
        assertEquals(503, root.getJSONObject("error").getInt("code"))
    }

    @Test
    fun extractRequestId_parsesIdFromRequest() {
        val id = AppsFlyerRPCBridge.extractRequestId("{\"id\":\"getSdkVersion-1\",\"method\":\"getSdkVersion\",\"params\":{}}")
        assertEquals("getSdkVersion-1", id)
    }

    @Test
    fun extractRequestId_missingId_returnsNull() {
        assertNull(AppsFlyerRPCBridge.extractRequestId("{\"method\":\"getSdkVersion\",\"params\":{}}"))
    }

    @Test
    fun extractRequestId_malformedJson_returnsNull() {
        assertNull(AppsFlyerRPCBridge.extractRequestId("not json"))
    }

    @Test
    fun executeJson_notInitialized_returns503Error() {
        val json = AppsFlyerRPCBridge.executeJson("{\"id\":\"req-5\",\"method\":\"getSdkVersion\",\"params\":{}}")
        val root = JSONObject(json)
        assertEquals("req-5", root.getString("id"))
        assertEquals(503, root.getJSONObject("error").getInt("code"))
    }

    @Test
    fun fireJson_notInitialized_doesNotThrow() {
        AppsFlyerRPCBridge.fireJson("{\"id\":\"req-6\",\"method\":\"start\",\"params\":{}}")
    }
}
