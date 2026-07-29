package com.appsflyer.engagement;

import android.content.Intent;
import com.unity3d.player.UnityPlayerActivity;

/**
 * Extends UnityPlayerActivity so a new intent delivered while the app is
 * already running (singleTask bring-to-front) is visible via getIntent().
 *
 * UnityPlayerActivity does not call setIntent() on its own, so without this
 * override getIntent() would keep returning the original launch intent.
 * Resolution itself is left to AppsFlyerLib's own Unified Deep Linking
 * lifecycle hook (triggered on the following onResume()) — calling
 * performDeepLinking() here as well used to race that automatic resolution
 * and drop the callback.
 */
public class AppsFlyerUnityActivity extends UnityPlayerActivity {

    @Override
    protected void onNewIntent(Intent intent) {
        super.onNewIntent(intent);
        setIntent(intent);
    }
}
