#if UNITY_IOS
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using UnityEditor.iOS.Xcode.Extensions;

namespace AppsFlyerSDK
{
    public static class AppsFlyerPostBuildProcessor
    {
        // Real built framework binary names for the *dynamic* SPM package products declared in
        // AppsFlyerDependencies.xml. The SPM product name (e.g. "AppsFlyerLib-Dynamic") is not
        // the name of the framework binary it produces (e.g. "AppsFlyerLib.framework") - EDM4U
        // links these into whichever target actually imports them (UnityFramework), but never
        // embeds the resulting dynamic framework into the app bundle, so dyld can't find it at
        // launch ("Library not loaded"). AppsFlyerRPC is intentionally excluded here: it's a
        // static xcframework, statically linked, with no separate binary to embed.
        private static readonly string[] DynamicFrameworksToEmbed =
        {
            "AppsFlyerLib.framework",
            "PurchaseConnector.framework",
        };

        // Run after Unity's own post-process steps (order 100)
        [PostProcessBuild(101)]
        public static void OnPostProcessBuild(BuildTarget buildTarget, string buildPath)
        {
            if (buildTarget != BuildTarget.iOS) return;

            EnsureMinimumDeploymentTarget(buildPath);
            EnsureSwiftStandardLibraries(buildPath);
            EmbedDynamicFrameworkDependencies(buildPath);
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

        // Embeds the dynamic frameworks EDM4U's remoteSwiftPackage support links but never
        // embeds. Uses a plain BUILT_PRODUCTS_DIR file reference - the same mechanism Unity
        // itself uses to embed UnityFramework.framework/UnityRuntime.framework in this same
        // phase - rather than referencing the SPM package product directly (productRef): the
        // package product's name doesn't match its built framework's binary name, so Xcode's
        // own build system cannot resolve a productRef-based copy step for these packages.
        // FindFileGuidByProjectPath()/AddFileToEmbedFrameworks() are each documented as
        // returning null / no-oping when the file/embed entry already exists, so repeat
        // builds stay idempotent without scanning the raw project text.
        private static void EmbedDynamicFrameworkDependencies(string buildPath)
        {
            string projPath = PBXProject.GetPBXProjectPath(buildPath);
            PBXProject proj = new PBXProject();
            proj.ReadFromFile(projPath);
            string mainTarget = proj.GetUnityMainTargetGuid();
            proj.AddCopyFilesBuildPhaseBeforeTargetPostprocess(mainTarget, "Embed Frameworks", "", "10");

            bool changed = false;
            foreach (string frameworkFileName in DynamicFrameworksToEmbed)
            {
                string projectPath = "Frameworks/" + frameworkFileName;
                string fileGuid = proj.FindFileGuidByProjectPath(projectPath);
                if (fileGuid == null)
                {
                    fileGuid = proj.AddFile(frameworkFileName, projectPath, PBXSourceTree.Build);
                    changed = true;
                }

                proj.AddFileToEmbedFrameworks(mainTarget, fileGuid);
            }

            if (changed) proj.WriteToFile(projPath);
        }
    }
}
#endif
