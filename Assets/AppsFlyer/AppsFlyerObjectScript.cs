using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AppsFlyerSDK;

// This class is intended to be used the the AppsFlyerObject.prefab

public class AppsFlyerObjectScript : MonoBehaviour , IAppsFlyerConversionData
{

    // These fields are set from the editor so do not modify!
    //******************************//
    public string devKey;
    public string appID;
    public string UWPAppID;
    public string macOSAppID;
    public bool isDebug;
    public bool getConversionData;
    //******************************//


    void Start()
    {
        // These fields are set from the editor so do not modify!
        //******************************//
        AppsFlyer.enableDebug(isDebug);
#if UNITY_STANDALONE_OSX && !UNITY_EDITOR
        AppsFlyer.init(devKey, macOSAppID, this);
#elif UNITY_WSA_10_0 && !UNITY_EDITOR
        AppsFlyer.init(devKey, UWPAppID, this);
#else
        AppsFlyer.init(devKey, appID, this);
#endif
        if (getConversionData) AppsFlyer.registerConversionListener(onConversionDataSuccess, onConversionDataFail);
        //******************************/

        AppsFlyer.start();
    }


    void Update()
    {

    }

    // Mark AppsFlyer CallBacks
    public void onConversionDataSuccess(string conversionData)
    {
        AppsFlyer.AFLog("didReceiveConversionData", conversionData);
        Dictionary<string, object> conversionDataDictionary = AppsFlyer.CallbackStringToDictionary(conversionData);
        // add deferred deeplink logic here
    }

    public void onConversionDataFail(string error)
    {
        AppsFlyer.AFLog("didReceiveConversionDataWithError", error);
    }

}
