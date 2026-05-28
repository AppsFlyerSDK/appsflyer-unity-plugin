package com.appsflyer.engagement;

import android.content.Intent;
import android.util.Log;
import com.appsflyer.AppsFlyerLib;
import com.unity3d.player.UnityPlayerActivity;

/**
 * Extends UnityPlayerActivity to forward onNewIntent to the AppsFlyer SDK
 * so that deep links opened while the app is already running (singleTask
 * bring-to-front) fire UDL onDeepLinking(FOUND).
 *
 * UnityPlayerActivity does not notify AppsFlyerLib. Calling
 * performOnDeepLinking here is the AF SDK v6 equivalent of the older
 * sendDeepLinkData. The explicit setIntent + ACTION_VIEW guard mirrors the
 * Capacitor QA app's stable Android E2E deep-link path.
 */
public class AppsFlyerUnityActivity extends UnityPlayerActivity {

    @Override
    protected void onNewIntent(Intent intent) {
        super.onNewIntent(intent);
        setIntent(intent);
        if (intent != null
                && Intent.ACTION_VIEW.equals(intent.getAction())
                && intent.getData() != null) {
            Log.i("AF_QA", "[AF_QA][AndroidDeepLink] onNewIntent data=" + intent.getDataString());
            AppsFlyerLib.getInstance().performOnDeepLinking(intent, getApplication());
        }
    }
}
