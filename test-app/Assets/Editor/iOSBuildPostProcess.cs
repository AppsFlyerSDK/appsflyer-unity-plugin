#if UNITY_EDITOR && UNITY_IOS
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;

public static class iOSBuildPostProcess
{
    // Universal Links (Associated Domains) for the testunity6 OneLink domain. Requires
    // AppsFlyer's apple-app-site-association file for this domain to list this app's
    // Team ID + bundle ID (managed in the OneLink dashboard, not this repo) — otherwise
    // iOS falls back to opening the link in Safari instead of this app.
    // QA-only: this domain is specific to this test app and must never ship in the plugin.
    static readonly string[] AssociatedDomains = { "applinks:testunity6.onelink.me" };
    const string EntitlementsFileName = "Unity-iPhone.entitlements";

    [PostProcessBuild(100)]
    public static void OnPostProcessBuild(BuildTarget target, string buildPath)
    {
        if (target != BuildTarget.iOS)
            return;

        AddURLScheme(buildPath);
        EnableSimulatorSupport(buildPath);
        AddTrackingUsageDescription(buildPath);
        AddATTFramework(buildPath);
        AddAssociatedDomains(buildPath);
    }

    // Registers the Associated Domains capability and merges our domain into the entitlements
    // file's existing array — additive, so it never clobbers an integrator's own domains.
    static void AddAssociatedDomains(string buildPath)
    {
        string projPath = PBXProject.GetPBXProjectPath(buildPath);
        var proj = new PBXProject();
        proj.ReadFromFile(projPath);

        string mainTarget = proj.GetUnityMainTargetGuid();
        proj.AddCapability(mainTarget, PBXCapabilityType.AssociatedDomains, EntitlementsFileName);
        proj.WriteToFile(projPath);

        string entitlementsPath = Path.Combine(buildPath, EntitlementsFileName);
        var entitlements = new PlistDocument();
        if (File.Exists(entitlementsPath))
            entitlements.ReadFromFile(entitlementsPath);

        PlistElementArray domains = entitlements.root["com.apple.developer.associated-domains"] as PlistElementArray
            ?? entitlements.root.CreateArray("com.apple.developer.associated-domains");

        foreach (string domain in AssociatedDomains)
        {
            bool alreadyPresent = false;
            foreach (var value in domains.values)
                if (value.AsString() == domain) { alreadyPresent = true; break; }
            if (!alreadyPresent)
                domains.AddString(domain);
        }

        entitlements.WriteToFile(entitlementsPath);
    }

    // Required by iOS to show the ATT popup at all — without this key the OS silently
    // skips the prompt and reports .denied.
    static void AddTrackingUsageDescription(string buildPath)
    {
        string plistPath = Path.Combine(buildPath, "Info.plist");
        var plist = new PlistDocument();
        plist.ReadFromFile(plistPath);

        plist.root.SetString("NSUserTrackingUsageDescription",
            "This identifier will be used to test AppsFlyer attribution.");

        plist.WriteToFile(plistPath);
    }

    // Weak-link so the binary still loads on the iOS 13 deployment target the core plugin
    // enforces (AppTrackingTransparency is iOS 14+); the @available guard in
    // ATTPermissionRequest.mm skips the call itself on older OS versions.
    static void AddATTFramework(string buildPath)
    {
        string projPath = PBXProject.GetPBXProjectPath(buildPath);
        var proj = new PBXProject();
        proj.ReadFromFile(projPath);

        // ATTPermissionRequest.mm compiles into UnityFramework (Unity 2019.3+ splits native
        // plugin code out of the thin main app target), so the framework must be linked there,
        // not just on the main target.
        proj.AddFrameworkToProject(proj.GetUnityFrameworkTargetGuid(), "AppTrackingTransparency.framework", true);
        proj.AddFrameworkToProject(proj.GetUnityMainTargetGuid(), "AppTrackingTransparency.framework", true);

        proj.WriteToFile(projPath);
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
