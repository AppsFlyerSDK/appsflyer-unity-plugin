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

        public class AppsFlyerPurchaseRevenueBridge : MonoBehaviour
        {
                        #if UNITY_IOS && !UNITY_EDITOR
[DllImport("__Internal")]
private static extern void RegisterUnityPurchaseRevenueParamsCallback(Func<string, string, string> callback);
#endif
                
                private static IAppsFlyerPurchaseRevenueDataSource _dataSource;

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
                                Debug.Log($"[AppsFlyer] productsJson: {productsJson}");
                                Debug.Log($"[AppsFlyer] transactionsJson: {transactionsJson}");

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

        public void OnPurchaseRevenueAdditionalParameters(string jsonParams)
        {
            try
            {
                // Parse the JSON parameters from native
                Dictionary<string, object> parameters = AppsFlyer.CallbackStringToDictionary(jsonParams);
                
                // Get the products and transactions from the parameters
                List<object> products = parameters["products"] as List<object>;
                List<object> transactions = parameters["transactions"] as List<object>;

                // Create custom parameters based on the products and transactions
                Dictionary<string, object> customParams = new Dictionary<string, object>
                {
                    ["additionalParameters"] = new Dictionary<string, object>
                    {
                        ["custom_param1"] = "value1",
                        ["custom_param2"] = "value2",
                        ["product_count"] = products.Count,
                        ["transaction_count"] = transactions.Count
                    }
                };
            }
            catch (Exception e)
            {
                Debug.LogError($"Error in OnPurchaseRevenueAdditionalParameters: {e.Message}");
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

        public static void setPurchaseRevenueDataSource(IAppsFlyerPurchaseRevenueDataSource dataSource) {
#if UNITY_IOS && !UNITY_EDITOR
                _setPurchaseRevenueDataSource(dataSource.GetType().Name);
                AppsFlyerPurchaseRevenueBridge.RegisterDataSource(dataSource);
#elif UNITY_ANDROID && !UNITY_EDITOR
                AppsFlyerPurchaseRevenueBridge.RegisterDataSource(dataSource);
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
    private static extern void _setPurchaseRevenueDataSource(string dataSourceName);
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