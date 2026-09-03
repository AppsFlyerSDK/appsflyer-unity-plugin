package com.appsflyer.engagement;

import android.content.Intent;
import com.unity3d.player.UnityPlayerGameActivity;

/**
 * Extends UnityPlayerGameActivity so a new intent delivered while the app is
 * already running (singleTask bring-to-front) is visible via getIntent().
 *
 * UnityPlayerGameActivity does not call setIntent() on its own, so without this
 * override getIntent() would keep returning the original launch intent.
 * Resolution itself is left to AppsFlyerLib's own Unified Deep Linking
 * lifecycle hook (triggered on the following onResume()) — calling
 * performDeepLinking() here as well used to race that automatic resolution
 * and drop the callback.
 */
public class AppsFlyerUnityActivity extends UnityPlayerGameActivity {

    @Override
    protected void onNewIntent(Intent intent) {
        super.onNewIntent(intent);
        setIntent(intent);
    }

    /**
     * Called from QATestScript.cs right after AppsFlyer.registerSessionReadyListener().
     * Forces a synthetic pause/resume on this Activity via AppsFlyerLifecycleNudgeActivity
     * — see that class for why. Pure Android lifecycle plumbing; no AppsFlyerLib/RPC call here.
     */
    public void triggerLifecycleNudge() {
        startActivity(new Intent(this, AppsFlyerLifecycleNudgeActivity.class));
        overridePendingTransition(0, 0);
    }
}
