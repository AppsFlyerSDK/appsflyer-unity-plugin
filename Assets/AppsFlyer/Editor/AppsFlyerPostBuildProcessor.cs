#if UNITY_IOS
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;

namespace AppsFlyerSDK
{
    public static class AppsFlyerPostBuildProcessor
    {
        // Universal Links (Associated Domains) for the testunity6 OneLink domain. Requires
        // AppsFlyer's apple-app-site-association file for this domain to list this app's
        // Team ID + bundle ID (managed in the OneLink dashboard, not this repo) — otherwise
        // iOS falls back to opening the link in Safari instead of this app.
        private static readonly string[] AssociatedDomains = { "applinks:testunity6.onelink.me" };
        private const string EntitlementsFileName = "Unity-iPhone.entitlements";

        // Run after Unity's own post-process steps (order 100)
        [PostProcessBuild(101)]
        public static void OnPostProcessBuild(BuildTarget buildTarget, string buildPath)
        {
            if (buildTarget != BuildTarget.iOS) return;

            EnsureMinimumDeploymentTarget(buildPath);
            EnsureSwiftStandardLibraries(buildPath);
            EnsureAssociatedDomains(buildPath);
        }

        // Registers the Associated Domains capability and writes the actual domain list into
        // the entitlements file — AddCapability alone only wires up the capability/entitlements
        // file plumbing, it doesn't know which domains we want.
        private static void EnsureAssociatedDomains(string buildPath)
        {
            string projPath = PBXProject.GetPBXProjectPath(buildPath);
            PBXProject proj = new PBXProject();
            proj.ReadFromFile(projPath);

            string mainTarget = proj.GetUnityMainTargetGuid();
            proj.AddCapability(mainTarget, PBXCapabilityType.AssociatedDomains, EntitlementsFileName);
            proj.WriteToFile(projPath);

            string entitlementsPath = Path.Combine(buildPath, EntitlementsFileName);
            PlistDocument entitlements = new PlistDocument();
            if (File.Exists(entitlementsPath))
                entitlements.ReadFromFile(entitlementsPath);

            PlistElementArray domains = entitlements.root.CreateArray("com.apple.developer.associated-domains");
            foreach (string domain in AssociatedDomains)
                domains.AddString(domain);

            entitlements.WriteToFile(entitlementsPath);
        }

        // AppsFlyerRPC.xcframework is Swift — the runtime must be embedded.
        private static void EnsureSwiftStandardLibraries(string buildPath)
        {
            string projPath = PBXProject.GetPBXProjectPath(buildPath);
            PBXProject proj = new PBXProject();
            proj.ReadFromFile(projPath);

            string mainTarget = proj.GetUnityMainTargetGuid();
            proj.SetBuildProperty(mainTarget, "ALWAYS_EMBED_SWIFT_STANDARD_LIBRARIES", "YES");
            proj.WriteToFile(projPath);
        }

        // AppsFlyerRPC requires iOS 13.0+. Bump the deployment target if it is lower.
        private static void EnsureMinimumDeploymentTarget(string buildPath)
        {
            string projPath = PBXProject.GetPBXProjectPath(buildPath);
            PBXProject proj = new PBXProject();
            proj.ReadFromFile(projPath);

            string mainTarget = proj.GetUnityMainTargetGuid();
            const float required = 13.0f;

            string current = proj.GetBuildPropertyForAnyConfig(mainTarget, "IPHONEOS_DEPLOYMENT_TARGET") ?? "";
            float currentVer = 0f;
            float.TryParse(current, out currentVer);

            if (currentVer < required)
            {
                proj.SetBuildProperty(mainTarget, "IPHONEOS_DEPLOYMENT_TARGET", "13.0");
                proj.WriteToFile(projPath);
            }
        }
    }
}
#endif
