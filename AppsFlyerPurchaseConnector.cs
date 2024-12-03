using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using System;

namespace AppsFlyerSDK
{
    
    public class AppsFlyerPurchaseConnector : MonoBehaviour {

        public static readonly string kAppsFlyerPurchaseConnectorVersion = "2.0.4";

#if UNITY_ANDROID && !UNITY_EDITOR
        private static AndroidJavaClass appsFlyerAndroidConnector = new AndroidJavaClass("com.appsflyer.unity.afunitypurchaseconnector.AppsFlyerAndroidWrapper");
#endif

        public static void init(MonoBehaviour unityObject, Store s) {
#if UNITY_IOS && !UNITY_EDITOR
                _initPurchaseConnector(unityObject.name);
#elif UNITY_ANDROID && !UNITY_EDITOR
                int store = mapStoreToInt(s);
                appsFlyerAndroidConnector.CallStatic("initPurchaseConnector", unityObject ? unityObject.name : null, store);
#endif
        }

        public static void build() {
#if UNITY_IOS && !UNITY_EDITOR
        //not for iOS
#elif UNITY_ANDROID && !UNITY_EDITOR
                appsFlyerAndroidConnector.CallStatic("build");

#else
#endif
        }

        public static void startObservingTransactions() {
#if UNITY_IOS && !UNITY_EDITOR
                _startObservingTransactions();
#elif UNITY_ANDROID && !UNITY_EDITOR
                appsFlyerAndroidConnector.CallStatic("startObservingTransactions");
#else 
#endif
        }

        public static void stopObservingTransactions() {
#if UNITY_IOS && !UNITY_EDITOR
                _stopObservingTransactions();
#elif UNITY_ANDROID && !UNITY_EDITOR
                appsFlyerAndroidConnector.CallStatic("stopObservingTransactions");
#else
#endif
        }

        public static void setIsSandbox(bool isSandbox) {
#if UNITY_IOS && !UNITY_EDITOR
                _setIsSandbox(isSandbox);
#elif UNITY_ANDROID && !UNITY_EDITOR
                appsFlyerAndroidConnector.CallStatic("setIsSandbox", isSandbox);
#else
#endif
        }

        public static void setPurchaseRevenueValidationListeners(bool enableCallbacks) {
#if UNITY_IOS && !UNITY_EDITOR
                _setPurchaseRevenueDelegate();
#elif UNITY_ANDROID && !UNITY_EDITOR
                appsFlyerAndroidConnector.CallStatic("setPurchaseRevenueValidationListeners", enableCallbacks);
#else
#endif
        }

        public static void setAutoLogPurchaseRevenue(params AppsFlyerAutoLogPurchaseRevenueOptions[] autoLogPurchaseRevenueOptions) {
#if UNITY_IOS && !UNITY_EDITOR
                int option = 0;
                foreach (AppsFlyerAutoLogPurchaseRevenueOptions op in autoLogPurchaseRevenueOptions) {
                        option = option | (int)op;
                }
                _setAutoLogPurchaseRevenue(option);
#elif UNITY_ANDROID && !UNITY_EDITOR
                if (autoLogPurchaseRevenueOptions.Length == 0) {
                        return;
                }
                foreach (AppsFlyerAutoLogPurchaseRevenueOptions op in autoLogPurchaseRevenueOptions) {
                        switch(op) {
                                case AppsFlyerAutoLogPurchaseRevenueOptions.AppsFlyerAutoLogPurchaseRevenueOptionsDisabled:
                                        break;
                                case AppsFlyerAutoLogPurchaseRevenueOptions.AppsFlyerAutoLogPurchaseRevenueOptionsAutoRenewableSubscriptions:
                                        appsFlyerAndroidConnector.CallStatic("setAutoLogSubscriptions", true);
                                        break;
                                case AppsFlyerAutoLogPurchaseRevenueOptions.AppsFlyerAutoLogPurchaseRevenueOptionsInAppPurchases:
                                        appsFlyerAndroidConnector.CallStatic("setAutoLogInApps", true);
                                        break;
                                default:
                                        break;
                        }
                }
#else
#endif
        }

        private static int mapStoreToInt(Store s) {
                switch(s) {
                        case(Store.GOOGLE):
                                return 0;
                        default:
                                return -1;
                }
        }

#if UNITY_IOS && !UNITY_EDITOR

    [DllImport("__Internal")]
    private static extern void _startObservingTransactions();
    [DllImport("__Internal")]
    private static extern void _stopObservingTransactions();
    [DllImport("__Internal")]
    private static extern void _setIsSandbox(bool isSandbox);
    [DllImport("__Internal")]
    private static extern void _setPurchaseRevenueDelegate();
    [DllImport("__Internal")]
    private static extern void _setAutoLogPurchaseRevenue(int option);
    [DllImport("__Internal")]
    private static extern void _initPurchaseConnector(string objectName);

#endif
    }
    public enum Store {
    GOOGLE = 0
    }
    public enum AppsFlyerAutoLogPurchaseRevenueOptions
    {
        AppsFlyerAutoLogPurchaseRevenueOptionsDisabled = 0,
        AppsFlyerAutoLogPurchaseRevenueOptionsAutoRenewableSubscriptions = 1 << 0,
        AppsFlyerAutoLogPurchaseRevenueOptionsInAppPurchases = 1 << 1
    }

}