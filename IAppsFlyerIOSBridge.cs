using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AppsFlyerSDK
{

	public interface IAppsFlyerIOSBridge : IAppsFlyerNativeBridge
	{
        void setDisableCollectAppleAdSupport(bool disable);
		void setShouldCollectDeviceName(bool shouldCollectDeviceName);
		void setDisableCollectIAd(bool disableCollectIAd);
		void setUseReceiptValidationSandbox(bool useReceiptValidationSandbox);
		void setUseUninstallSandbox(bool useUninstallSandbox);
		void validateAndSendInAppPurchase(string productIdentifier, string price, string currency, string tranactionId, Dictionary<string, string> additionalParameters, MonoBehaviour gameObject);
		void registerUninstall(byte[] deviceToken);
		void handleOpenUrl(string url, string sourceApplication, string annotation);
		void waitForATTUserAuthorizationWithTimeoutInterval(int timeoutInterval);
		void setCurrentDeviceLanguage(string language);
		void disableSKAdNetwork(bool isDisabled);

	}
}