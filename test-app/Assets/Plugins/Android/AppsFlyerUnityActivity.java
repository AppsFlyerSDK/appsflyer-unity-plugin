package com.appsflyer.engagement;

import android.content.Intent;
import com.appsflyer.AppsFlyerLib;
import com.unity3d.player.UnityPlayerActivity;

/**
 * Extends UnityPlayerActivity to forward onNewIntent to the AppsFlyer SDK
 * so that deep links opened while the app is already running (singleTask
 * bring-to-front) fire UDL onDeepLinking(FOUND).
 *
 * UnityPlayerActivity.onNewIntent only calls setIntent + newIntent on the
 * player; it never notifies AppsFlyerLib. Calling performOnDeepLinking here
 * is the AF SDK v6 equivalent of the older sendDeepLinkData.
 */
public class AppsFlyerUnityActivity extends UnityPlayerActivity {

    @Override
    protected void onNewIntent(Intent intent) {
        super.onNewIntent(intent);
        AppsFlyerLib.getInstance().performOnDeepLinking(intent, this);
    }
}
