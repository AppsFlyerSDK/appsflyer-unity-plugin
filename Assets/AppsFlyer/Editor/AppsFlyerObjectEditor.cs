using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AppsFlyerObjectScript))]
[CanEditMultipleObjects]
public class AppsFlyerObjectEditor : Editor
{
    SerializedProperty iOSDevKey;
    SerializedProperty iOSAppID;
    SerializedProperty androidDevKey;
    SerializedProperty androidAppID;
    SerializedProperty UWPAppID;
    SerializedProperty macOSAppID;
    SerializedProperty isDebug;
    SerializedProperty getConversionData;
    SerializedProperty enableRPC;

    void OnEnable()
    {
        iOSDevKey = serializedObject.FindProperty("iOSDevKey");
        iOSAppID = serializedObject.FindProperty("iOSAppID");
        androidDevKey = serializedObject.FindProperty("androidDevKey");
        androidAppID = serializedObject.FindProperty("androidAppID");
        UWPAppID = serializedObject.FindProperty("UWPAppID");
        macOSAppID = serializedObject.FindProperty("macOSAppID");
        isDebug = serializedObject.FindProperty("isDebug");
        getConversionData = serializedObject.FindProperty("getConversionData");
        enableRPC = serializedObject.FindProperty("enableRPC");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawLogo();
        DrawSDKSettings();
        DrawRPCSection();
        DrawDocLinks();

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawLogo()
    {
        Texture logo = (Texture)AssetDatabase.LoadAssetAtPath(
            AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("appsflyer_logo")[0]),
            typeof(Texture));

        if (logo == null) return;

        float maxWidth = Mathf.Min(200, EditorGUIUtility.currentViewWidth - 40);
        float aspect = (float)logo.height / logo.width;
        float height = maxWidth * aspect;

        Rect rect = GUILayoutUtility.GetRect(maxWidth, height, GUILayout.ExpandWidth(false));
        rect.x = (EditorGUIUtility.currentViewWidth - maxWidth) * 0.5f;
        rect.width = maxWidth;
        GUI.DrawTexture(rect, logo, ScaleMode.ScaleToFit);
    }

    private void DrawSDKSettings()
    {
        EditorGUILayout.Separator();
        EditorGUILayout.HelpBox(
            "Set your devKey and appID per platform to init the AppsFlyer SDK and start tracking.\n" +
            "iOS devKey / appId - Credentials for iOS.\n" +
            "Android devKey / appId - Credentials for Android.\n" +
            "UWP app id - For UWP only.\n" +
            "Mac OS app id - For MacOS only.", MessageType.Info);

        EditorGUILayout.PropertyField(iOSDevKey, new GUIContent("iOS Dev Key"));
        EditorGUILayout.PropertyField(iOSAppID, new GUIContent("iOS App ID"));
        EditorGUILayout.PropertyField(androidDevKey, new GUIContent("Android Dev Key"));
        EditorGUILayout.PropertyField(androidAppID, new GUIContent("Android App ID"));
        EditorGUILayout.PropertyField(UWPAppID);
        EditorGUILayout.PropertyField(macOSAppID);

        EditorGUILayout.Separator();
        EditorGUILayout.HelpBox("Enable get conversion data to allow your app to recive deeplinking callbacks", MessageType.None);
        EditorGUILayout.PropertyField(getConversionData);

        EditorGUILayout.Separator();
        EditorGUILayout.HelpBox("Debugging should be restricted to development phase only.\n Do not distribute the app to app stores with debugging enabled", MessageType.Warning);
        EditorGUILayout.PropertyField(isDebug);
        EditorGUILayout.Separator();
    }

    private void DrawRPCSection()
    {
        EditorGUILayout.HelpBox(
            "Enable RPC to route SDK calls (init, start, logEvent, isDebug) through the AppsFlyerRPC bridge layer.\n" +
            "iOS: AppsFlyerRPC pod\n" +
            "Android: af-android-plugin-bridge",
            MessageType.None);

        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(enableRPC, new GUIContent("Enable AppsFlyerRPC"));

        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
            AppsFlyerRPCConfig.SetEnabled(enableRPC.boolValue);
        }

        EditorGUILayout.Separator();
    }

    private static void DrawDocLinks()
    {
        EditorGUILayout.HelpBox("For more information on setting up AppsFlyer check out our relevant docs.", MessageType.None);

        DocButton("AppsFlyer Unity Docs",      "https://support.appsflyer.com/hc/en-us/articles/213766183-Unity-SDK-integration-for-developers");
        DocButton("AppsFlyer Android Docs",    "https://support.appsflyer.com/hc/en-us/articles/207032126-Android-SDK-integration-for-developers");
        DocButton("AppsFlyer iOS Docs",        "https://support.appsflyer.com/hc/en-us/articles/207032066-AppsFlyer-SDK-Integration-iOS");
        DocButton("AppsFlyer Deeplinking Docs","https://support.appsflyer.com/hc/en-us/articles/208874366-OneLink-deep-linking-guide#Setups");
        DocButton("AppsFlyer Windows Docs",    "https://support.appsflyer.com/hc/en-us/articles/207032026-Windows-and-Xbox-SDK-integration-for-developers");
    }

    private static void DocButton(string label, string url)
    {
        if (GUILayout.Button(label, GUILayout.Width(200)))
        {
            Application.OpenURL(url);
        }
    }
}
