package com.appsflyer.engagement;

import android.app.Activity;
import android.os.Bundle;

/**
 * Momentary, invisible activity that finishes itself the instant it's created.
 *
 * Starting and immediately finishing this on top of AppsFlyerUnityActivity forces a
 * pause/resume pair on it. That resume is what AppsFlyerLib's own (SDK-internal)
 * ActivityLifecycleCallbacks needs to (re-)evaluate session readiness — but that
 * callback is only registered once AppsFlyer.init() runs from Unity's managed layer,
 * which is necessarily after Android's real, launch-triggering onResume() already
 * fired. Without this nudge, session readiness/start() on a cold launch would stay
 * unevaluated until the user genuinely backgrounds and foregrounds the app.
 *
 * See AppsFlyerUnityActivity.triggerLifecycleNudge(). No AppsFlyerLib/RPC API is
 * called here — this is pure Android Activity-lifecycle plumbing.
 */
public class AppsFlyerLifecycleNudgeActivity extends Activity {

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        finish();
        overridePendingTransition(0, 0);
    }
}
