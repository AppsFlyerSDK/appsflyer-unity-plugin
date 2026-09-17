package com.appsflyer.unity

import android.util.Log
import com.appsflyer.api.PurchaseClient
import com.unity3d.player.UnityPlayer
import org.json.JSONObject

object PurchaseRevenueBridge {
    private const val TAG = "AppsFlyerUnity"

    fun interface UnityPurchaseRevenueBridge {
        fun getAdditionalParameters(productsJson: String, transactionsJson: String): String
    }

    @Volatile
    private var unityBridge: UnityPurchaseRevenueBridge? = null

    @JvmStatic
    fun setUnityBridge(bridge: UnityPurchaseRevenueBridge) {
        unityBridge = bridge
    }

    @JvmStatic
    fun configurePurchaseClient(builder: PurchaseClient.Builder): PurchaseClient.Builder {
        return builder
            .setInAppPurchaseEventDataSource { purchaseEvents ->
                try {
                    val eventsJson = JSONObject(mapOf("events" to purchaseEvents)).toString()
                    val response = unityBridge?.getAdditionalParameters(eventsJson, "")
                    if (response != null) {
                        return@setInAppPurchaseEventDataSource jsonToMap(JSONObject(response))
                    }
                } catch (e: Exception) {
                    Log.e(TAG, "Failed to get additional params from Unity", e)
                }
                emptyMap()
            }
            .setSubscriptionPurchaseEventDataSource { purchaseEvents ->
                try {
                    val eventsJson = JSONObject(mapOf("events" to purchaseEvents)).toString()
                    val response = unityBridge?.getAdditionalParameters("", eventsJson)
                    if (response != null) {
                        return@setSubscriptionPurchaseEventDataSource jsonToMap(JSONObject(response))
                    }
                } catch (e: Exception) {
                    Log.e(TAG, "Failed to get additional params from Unity", e)
                }
                emptyMap()
            }
    }

    private fun jsonToMap(json: JSONObject): Map<String, Any> {
        val map = HashMap<String, Any>()
        val keys = json.keys()
        while (keys.hasNext()) {
            val key = keys.next()
            map[key] = json.get(key)
        }
        return map
    }
}
