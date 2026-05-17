#if UNITY_EDITOR && UNITY_IOS
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;

public static class iOSBuildPostProcess
{
    [PostProcessBuild(100)]
    public static void OnPostProcessBuild(BuildTarget target, string buildPath)
    {
        if (target != BuildTarget.iOS)
            return;

        AddURLScheme(buildPath);
        EnableSimulatorSupport(buildPath);
    }

    static void EnableSimulatorSupport(string buildPath)
    {
        string projPath = PBXProject.GetPBXProjectPath(buildPath);
        var proj = new PBXProject();
        proj.ReadFromFile(projPath);

        string[] guids = new[]
        {
            proj.GetUnityMainTargetGuid(),
            proj.GetUnityFrameworkTargetGuid(),
        };

        foreach (string guid in guids)
        {
            foreach (string config in new[] { "Debug", "Release", "ReleaseForProfiling", "ReleaseForRunning" })
            {
                string configGuid = proj.BuildConfigByName(guid, config);
                if (configGuid == null) continue;
                proj.SetBuildPropertyForConfig(configGuid, "SUPPORTED_PLATFORMS", "iphoneos iphonesimulator");
                proj.SetBuildPropertyForConfig(configGuid, "SUPPORTS_MACCATALYST", "NO");
            }
        }

        proj.WriteToFile(projPath);
    }

    static void AddURLScheme(string buildPath)
    {
        string plistPath = Path.Combine(buildPath, "Info.plist");
        var plist = new PlistDocument();
        plist.ReadFromFile(plistPath);

        PlistElementArray urlTypes = plist.root["CFBundleURLTypes"] as PlistElementArray
            ?? plist.root.CreateArray("CFBundleURLTypes");

        // Check if afqa-unity scheme is already registered
        foreach (var item in urlTypes.values)
        {
            var dict = item as PlistElementDict;
            if (dict == null) continue;
            var schemes = dict["CFBundleURLSchemes"] as PlistElementArray;
            if (schemes == null) continue;
            foreach (var s in schemes.values)
                if (s.AsString() == "afqa-unity") return;
        }

        var entry = urlTypes.AddDict();
        entry.SetString("CFBundleURLName", "com.appsflyer.engagement");
        var schemesArray = entry.CreateArray("CFBundleURLSchemes");
        schemesArray.AddString("afqa-unity");

        plist.WriteToFile(plistPath);
    }
}
#endif
