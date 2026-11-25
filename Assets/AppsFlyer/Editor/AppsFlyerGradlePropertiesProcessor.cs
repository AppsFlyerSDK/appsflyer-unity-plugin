// Not guarded by UNITY_ANDROID: a scripted Android build started while the Editor's active platform
// is iOS/Standalone would otherwise compile without this hook. IPostGenerateGradleAndroidProject
// lives in UnityEditor.CoreModule, so this compiles without the Android module, and Unity only
// invokes it for Android builds.
#if !APPSFLYER_DISABLE_UNIQUE_PACKAGE_NAMES_FIX
using System.IO;
using UnityEditor.Android;
using UnityEngine;

namespace AppsFlyerSDK
{
    // com.appsflyer:purchase-connector and com.appsflyer:af-android-sdk share the manifest
    // package "com.appsflyer". AGP 8.x only warns about it, but AGP 9.0 changed the default of
    // android.uniquePackageNames to true, which turns it into a build error. This adds
    // android.uniquePackageNames=false to the generated gradle.properties so that apps building
    // with AGP 9+ (Unity 6000.0.79f1+, 6000.3.17f1+, ...) don't have to set it by hand.
    //
    // The property is only added when the project doesn't already set it, so an explicit
    // value in gradleTemplate.properties always wins. Define the scripting symbol
    // APPSFLYER_DISABLE_UNIQUE_PACKAGE_NAMES_FIX to turn this off entirely.
    public class AppsFlyerGradlePropertiesProcessor : IPostGenerateGradleAndroidProject
    {
        private const string PropertyKey = "android.uniquePackageNames";

        public int callbackOrder => 0;

        public void OnPostGenerateGradleAndroidProject(string path)
        {
            // path points to the unityLibrary module; gradle.properties lives in the root project.
            string propertiesPath = Path.GetFullPath(Path.Combine(path, "..", "gradle.properties"));
            if (!File.Exists(propertiesPath))
            {
                Debug.LogWarning("[AppsFlyer] gradle.properties not found at " + propertiesPath +
                    ". If the build fails with \"Namespace 'com.appsflyer' is used in multiple modules\", add " +
                    PropertyKey + "=false to Assets/Plugins/Android/gradleTemplate.properties.");
                return;
            }

            string contents = File.ReadAllText(propertiesPath);
            if (DefinesProperty(contents, PropertyKey)) return;

            string separator = contents.Length == 0 || contents.EndsWith("\n") ? "" : "\n";
            File.AppendAllText(propertiesPath, separator +
                "# Added by AppsFlyer: purchase-connector and af-android-sdk share the namespace com.appsflyer (AGP 9+).\n" +
                PropertyKey + "=false\n");
        }

        private static bool DefinesProperty(string contents, string key)
        {
            foreach (string rawLine in contents.Split('\n'))
            {
                string line = rawLine.Trim();
                if (line.Length == 0 || line[0] == '#' || line[0] == '!') continue;

                int end = line.IndexOfAny(new[] { '=', ':', ' ', '\t' });
                string lineKey = end < 0 ? line : line.Substring(0, end);
                if (lineKey == key) return true;
            }
            return false;
        }
    }
}
#endif
