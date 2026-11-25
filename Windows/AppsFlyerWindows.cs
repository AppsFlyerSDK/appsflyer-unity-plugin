//#define AFSDK_WIN_DEBUG
//#define UNITY_WSA_10_0
//#define ENABLE_WINMD_SUPPORT

#if UNITY_WSA_10_0
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.ComponentModel;
using UnityEngine;
using System.Threading.Tasks;

#if ENABLE_WINMD_SUPPORT
using AppsFlyerLib;
#endif

namespace AppsFlyerSDK
{
    public class AppsFlyerWindows
    {
#if ENABLE_WINMD_SUPPORT
        static private MonoBehaviour _gameObject = null;
#endif

        public static void InitSDK(string devKey, string appId, MonoBehaviour gameObject)
        {
#if ENABLE_WINMD_SUPPORT

#if AFSDK_WIN_DEBUG
            // Remove callstack
            Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
#endif
            Log("[InitSDK]: devKey: {0}, appId: {1}, gameObject: {2}", devKey, appId, gameObject == null ? "null" : gameObject.ToString());
            AppsFlyerTracker tracker = AppsFlyerTracker.GetAppsFlyerTracker();
            tracker.devKey = devKey;
            tracker.appId = appId;
            // Interface
            _gameObject = gameObject;
#endif
        }

        public static string GetAppsFlyerId()
        {
#if ENABLE_WINMD_SUPPORT
            Log("[GetAppsFlyerId]");
            return AppsFlyerTracker.GetAppsFlyerTracker().GetAppsFlyerUID();
#else
            return "";
#endif
        }

        public static void SetCustomerUserId(string customerUserId)
        {
#if ENABLE_WINMD_SUPPORT
            Log("[SetCustomerUserId] customerUserId: {0}", customerUserId);
            if (customerUserId.Contains("test_device:"))
            {
                string testDeviceId = customerUserId.Substring(12);
                AppsFlyerTracker.GetAppsFlyerTracker().testDeviceId = testDeviceId;
            }
            AppsFlyerTracker.GetAppsFlyerTracker().customerUserId = customerUserId;
#endif
        }

        public static void Start()
        {
#if ENABLE_WINMD_SUPPORT
            Log("[Start]");
            AppsFlyerTracker.GetAppsFlyerTracker().TrackAppLaunchAsync(Callback);
#endif
        }

#if ENABLE_WINMD_SUPPORT
        public static void Callback(AppsFlyerLib.ServerStatusCode code)
        {
            Log("[Callback]: {0}", code.ToString());

            AppsFlyerRequestEventArgs eventArgs = new AppsFlyerRequestEventArgs((int)code, code.ToString());
            if (_gameObject != null) {
                var method = _gameObject.GetType().GetMethod("AppsFlyerOnRequestResponse");
                if (method != null) {
                    method.Invoke(_gameObject, new object[] { AppsFlyerTracker.GetAppsFlyerTracker(), eventArgs });
                }
            }
        }
#endif

        public static void LogEvent(string eventName, Dictionary<string, string> eventValues)
        {
#if ENABLE_WINMD_SUPPORT
            if (eventValues == null)
            {
                    eventValues = new Dictionary<string, string>();
            }
            IDictionary<string, object> result = new Dictionary<string, object>();
            foreach (KeyValuePair<string, string> kvp in eventValues)
            {
                result.Add(kvp.Key.ToString(), kvp.Value);
            }

            Log("[LogEvent]: eventName: {0} result: {1}", eventName, result.ToString());

            AppsFlyerTracker tracker = AppsFlyerTracker.GetAppsFlyerTracker();
            tracker.TrackEvent(eventName, result);

#endif
        }


        public static void GetConversionData(string _reserved)
        {
#if ENABLE_WINMD_SUPPORT
            Task.Run(async () =>
            {
                AppsFlyerLib.AppsFlyerTracker tracker = AppsFlyerLib.AppsFlyerTracker.GetAppsFlyerTracker();
                string conversionData = await tracker.GetConversionDataAsync();

                IAppsFlyerConversionData conversionDataHandler = _gameObject as IAppsFlyerConversionData;

                if (conversionDataHandler != null)
                {
                    Log("[GetConversionData] Will call `onConversionDataSuccess` with: {0}", conversionData);
                    conversionDataHandler.onConversionDataSuccess(conversionData);
                } else {
                    Log("[GetConversionData] Object with `IAppsFlyerConversionData` interface not found! Check `InitSDK` implementation");
                }
                // _gameObject.GetType().GetMethod("onConversionDataSuccess").Invoke(_gameObject, new[] { conversionData });
            });
#endif
        }

        private static void Log(string format, params string[] args)
        {
#if AFSDK_WIN_DEBUG
#if ENABLE_WINMD_SUPPORT
            Debug.Log("AF_UNITY_WSA_10_0" + String.Format(format, args));
#endif
#endif
        }

    }

}
#endif
