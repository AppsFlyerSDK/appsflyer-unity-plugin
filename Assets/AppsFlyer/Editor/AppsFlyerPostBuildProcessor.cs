#if UNITY_IOS
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;

namespace AppsFlyerSDK
{
    public static class AppsFlyerPostBuildProcessor
    {
        // Run after Unity's own post-process steps (order 100)
        [PostProcessBuild(101)]
        public static void OnPostProcessBuild(BuildTarget buildTarget, string buildPath)
        {
            if (buildTarget != BuildTarget.iOS) return;

            EnsureMinimumDeploymentTarget(buildPath);
            EnsureSwiftStandardLibraries(buildPath);
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
