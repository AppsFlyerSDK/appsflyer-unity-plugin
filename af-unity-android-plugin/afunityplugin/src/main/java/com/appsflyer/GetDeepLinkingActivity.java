package com.appsflyer;

import android.app.Activity;
import android.content.Context;
import android.content.Intent;
import android.os.Bundle;
import android.view.Window;
import com.appsflyer.*;
import android.util.Log;
import java.lang.reflect.Method;

public class GetDeepLinkingActivity extends Activity
{
	private static String TAG = "AppsFlyerDeepLinkActivity";

	@Override
	protected void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		requestWindowFeature(Window.FEATURE_NO_TITLE);
		//start main activity
		Intent newIntent = new Intent(this, getMainActivityClass());
		this.startActivity(newIntent);

		AppsFlyerLib.getInstance().setPluginDeepLinkData(getIntent());

		finish();
	}

	private Class<?> getMainActivityClass() {
		String packageName = this.getPackageName();
		Intent launchIntent = this.getPackageManager().getLaunchIntentForPackage(packageName);
		try {
			return Class.forName(launchIntent.getComponent().getClassName());
		} catch (Exception e) {
			Log.e(TAG, "Unable to find Main Activity Class");
			return null;
		}
	}
}