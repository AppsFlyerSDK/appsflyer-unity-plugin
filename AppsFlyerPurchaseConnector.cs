using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using System;

namespace AppsFlyerSDK
{

        public interface IAppsFlyerPurchaseRevenueDataSource
        {
                Dictionary<string, object> PurchaseRevenueAdditionalParametersForProducts(HashSet<object> products, HashSet<object> transactions);
        }

        public interface IAppsFlyerPurchaseRevenueDataSourceStoreKit2
        {
                Dictionary<string, object> PurchaseRevenueAdditionalParametersStoreKit2ForProducts(HashSet<object> products, HashSet<object> transactions);
        }

        public class AppsFlyerPurchaseRevenueBridge : MonoBehaviour
        {
                        #if UNITY_IOS && !UNITY_EDITOR
[DllImport("__Internal")]
private static extern void RegisterUnityPurchaseRevenueParamsCallback(Func<string, string, string> callback);

[DllImport("__Internal")]
private static extern void RegisterUnityPurchaseRevenueParamsCallbackSK2(Func<string, string, string> callback);
#endif
                
                private static IAppsFlyerPurchaseRevenueDataSource _dataSource;
                private static IAppsFlyerPurchaseRevenueDataSourceStoreKit2 _dataSourceSK2;

                public static void RegisterDataSource(IAppsFlyerPurchaseRevenueDataSource dataSource)
                {
                        _dataSource = dataSource;
        #if UNITY_IOS && !UNITY_EDITOR
                        RegisterUnityPurchaseRevenueParamsCallback(GetAdditionalParameters);
        #elif UNITY_ANDROID && !UNITY_EDITOR
                        using (AndroidJavaClass jc = new AndroidJavaClass("com.appsflyer.unity.PurchaseRevenueBridge"))
                        {
                                jc.CallStatic("setUnityBridge", new UnityPurchaseRevenueBridgeProxy());
                        }
        #endif
                }

                public static void RegisterDataSourceStoreKit2(IAppsFlyerPurchaseRevenueDataSourceStoreKit2 dataSource)
                {
                #if UNITY_IOS && !UNITY_EDITOR
                        _dataSourceSK2 = dataSource;
                        RegisterUnityPurchaseRevenueParamsCallbackSK2(GetAdditionalParametersSK2);
                #endif
                }

                public static Dictionary<string, object> GetAdditionalParametersForAndroid(HashSet<object> products, HashSet<object> transactions)
                {
                        return _dataSource?.PurchaseRevenueAdditionalParametersForProducts(products, transactions)
                        ?? new Dictionary<string, object>();
                }

        #if UNITY_IOS && !UNITY_EDITOR
                [AOT.MonoPInvokeCallback(typeof(Func<string, string, string>))]
                public static string GetAdditionalParameters(string productsJson, string transactionsJson)
                {
                        try
                        {
                                HashSet<object> products = new HashSet<object>();
                                HashSet<object> transactions = new HashSet<object>();

                                if (!string.IsNullOrEmpty(productsJson))
                                {
                                var dict = AFMiniJSON.Json.Deserialize(productsJson) as Dictionary<string, object>;
                                if (dict != null)
                                {
                                        if (dict.TryGetValue("products", out var productsObj) && productsObj is List<object> productList)
                                        products = new HashSet<object>(productList);

                                        if (dict.TryGetValue("transactions", out var transactionsObj) && transactionsObj is List<object> transactionList)
                                        transactions = new HashSet<object>(transactionList);
                                }
                                }

                                var parameters = _dataSource?.PurchaseRevenueAdditionalParametersForProducts(products, transactions)
                                                ?? new Dictionary<string, object>();
                                return AFMiniJSON.Json.Serialize(parameters);
                        }
                        catch (Exception e)
                        {
                                Debug.LogError($"[AppsFlyer] Exception in GetAdditionalParameters: {e}");
                                return "{}";
                        }
                }
        #endif

        #if UNITY_IOS && !UNITY_EDITOR
                [AOT.MonoPInvokeCallback(typeof(Func<string, string, string>))]
                public static string GetAdditionalParametersSK2(string productsJson, string transactionsJson)
                {
                        try
                        {
                                HashSet<object> products = new HashSet<object>();
                                HashSet<object> transactions = new HashSet<object>();

                                if (!string.IsNullOrEmpty(productsJson))
                                {
                                        var dict = AFMiniJSON.Json.Deserialize(productsJson) as Dictionary<string, object>;
                                        if (dict != null && dict.TryGetValue("products", out var productsObj) && productsObj is List<object> productList)
                                                products = new HashSet<object>(productList);
                                }
                                if (!string.IsNullOrEmpty(transactionsJson))
                                {
                                        var dict = AFMiniJSON.Json.Deserialize(transactionsJson) as Dictionary<string, object>;
                                        if (dict != null && dict.TryGetValue("transactions", out var transactionsObj) && transactionsObj is List<object> transactionList)
                                                transactions = new HashSet<object>(transactionList);
                                }

                                var parameters = _dataSourceSK2?.PurchaseRevenueAdditionalParametersStoreKit2ForProducts(products, transactions)
                                                ?? new Dictionary<string, object>();
                                return AFMiniJSON.Json.Serialize(parameters);
                        }
                        catch (Exception e)
                        {
                                Debug.LogError($"[AppsFlyer] Exception in GetAdditionalParametersSK2: {e}");
                                return "{}";
                        }
                }
        #endif
        }

        public class UnityPurchaseRevenueBridgeProxy : AndroidJavaProxy
        {
                public UnityPurchaseRevenueBridgeProxy() : base("com.appsflyer.unity.PurchaseRevenueBridge$UnityPurchaseRevenueBridge") { }

                public string getAdditionalParameters(string productsJson, string transactionsJson)
                {
                        try
                        {
                                // Create empty sets if JSON is null or empty
                                HashSet<object> products = new HashSet<object>();
                                HashSet<object> transactions = new HashSet<object>();

                                // Only try to parse if we have valid JSON
                                if (!string.IsNullOrEmpty(productsJson))
                                {
                                        try
                                        {
                                                // First try to parse as a simple array
                                                var parsedProducts = AFMiniJSON.Json.Deserialize(productsJson);
                                                if (parsedProducts is List<object> productList)
                                                {
                                                        products = new HashSet<object>(productList);
                                                }
                                                else if (parsedProducts is Dictionary<string, object> dict)
                                                {
                                                        if (dict.ContainsKey("events") && dict["events"] is List<object> eventsList)
                                                        {
                                                                products = new HashSet<object>(eventsList);
                                                        }
                                                        else
                                                        {
                                                                // If it's a dictionary but doesn't have events, add the whole dict
                                                                products.Add(dict);
                                                        }
                                                }
                                        }
                                        catch (Exception e)
                                        {
                                                Debug.LogError($"Error parsing products JSON: {e.Message}\nJSON: {productsJson}");
                                        }
                                }

                                if (!string.IsNullOrEmpty(transactionsJson))
                                {
                                        try
                                        {
                                                // First try to parse as a simple array
                                                var parsedTransactions = AFMiniJSON.Json.Deserialize(transactionsJson);
                                                if (parsedTransactions is List<object> transactionList)
                                                {
                                                        transactions = new HashSet<object>(transactionList);
                                                }
                                                else if (parsedTransactions is Dictionary<string, object> dict)
                                                {
                                                        if (dict.ContainsKey("events") && dict["events"] is List<object> eventsList)
                                                        {
                                                                transactions = new HashSet<object>(eventsList);
                                                        }
                                                        else
                                                        {
                                                                // If it's a dictionary but doesn't have events, add the whole dict
                                                                transactions.Add(dict);
                                                        }
                                                }
                                        }
                                        catch (Exception e)
                                        {
                                                Debug.LogError($"Error parsing transactions JSON: {e.Message}\nJSON: {transactionsJson}");
                                        }
                                }

                                var parameters = AppsFlyerPurchaseRevenueBridge.GetAdditionalParametersForAndroid(products, transactions);
                                return AFMiniJSON.Json.Serialize(parameters);
                        }
                        catch (Exception e)
                        {
                                Debug.LogError($"Error in getAdditionalParameters: {e.Message}\nProducts JSON: {productsJson}\nTransactions JSON: {transactionsJson}");
                                return "{}";
                        }
                }
        }

    
        public class AppsFlyerPurchaseConnector : MonoBehaviour {

        private static AppsFlyerPurchaseConnector instance;
        private Dictionary<string, object> pendingParameters;
        private Action<Dictionary<string, object>> pendingCallback;

        public static AppsFlyerPurchaseConnector Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject go = new GameObject("AppsFlyerPurchaseConnector");
                    instance = go.AddComponent<AppsFlyerPurchaseConnector>();
                    DontDestroyOnLoad(go);
                }
                return instance;
            }
        }

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        private static AndroidJavaClass appsFlyerAndroidConnector = new AndroidJavaClass("com.appsflyer.unity.AppsFlyerAndroidWrapper");
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

        public static void setPurchaseRevenueDataSource(IAppsFlyerPurchaseRevenueDataSource dataSource)
        {
#if UNITY_IOS && !UNITY_EDITOR

                if (dataSource != null)
                {
                        _setPurchaseRevenueDataSource(dataSource.GetType().Name);
                        AppsFlyerPurchaseRevenueBridge.RegisterDataSource(dataSource);
                }
#elif UNITY_ANDROID && !UNITY_EDITOR
                if (dataSource != null)
                {
                        AppsFlyerPurchaseRevenueBridge.RegisterDataSource(dataSource);
                }
#endif
        }


        public static void setPurchaseRevenueDataSourceStoreKit2(IAppsFlyerPurchaseRevenueDataSourceStoreKit2 dataSourceSK2)
        {
#if UNITY_IOS && !UNITY_EDITOR
                if (dataSourceSK2 != null)
                {
                        AppsFlyerPurchaseRevenueBridge.RegisterDataSourceStoreKit2(dataSourceSK2);
                        _setPurchaseRevenueDataSource("AppsFlyerObjectScript_StoreKit2");
                }
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

        public static void setStoreKitVersion(StoreKitVersion storeKitVersion) {
#if UNITY_IOS && !UNITY_EDITOR
                _setStoreKitVersion((int)storeKitVersion);
#elif UNITY_ANDROID && !UNITY_EDITOR
                // Android doesn't use StoreKit
#else
#endif
        }

        public static void logConsumableTransaction(string transactionJson) {
#if UNITY_IOS && !UNITY_EDITOR
                _logConsumableTransaction(transactionJson);
#elif UNITY_ANDROID && !UNITY_EDITOR
                // Android doesn't use StoreKit
#else
#endif
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
    private static extern void _setPurchaseRevenueDataSource(string dataSourceName);
    [DllImport("__Internal")]
    private static extern void _setAutoLogPurchaseRevenue(int option);
    [DllImport("__Internal")]
    private static extern void _initPurchaseConnector(string objectName);
    [DllImport("__Internal")]
    private static extern void _setStoreKitVersion(int storeKitVersion);
    [DllImport("__Internal")]
    private static extern void _logConsumableTransaction(string transactionJson);

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

    public enum StoreKitVersion {
        SK1 = 0,
        SK2 = 1
    }
}