#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// CI build entry point called by game-ci/unity-builder via -executeMethod.
/// Reads output path from the UNITY_BUILD_PATH env var (set by game-ci).
/// </summary>
public static class BuildScript
{
    public static void BuildAndroid()
    {
        string outputPath = GetBuildPath("Build/Android/com.appsflyer.engagement.apk");
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);

        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.appsflyer.engagement");
        PlayerSettings.productName = "UnityQATest";
        PlayerSettings.Android.bundleVersionCode = 1;
        PlayerSettings.bundleVersion = "1.0";
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.X86_64 | AndroidArchitecture.ARM64;

        var options = new BuildPlayerOptions
        {
            scenes          = new[] { "Assets/Scenes/QATestScene.unity" },
            locationPathName = outputPath,
            target          = BuildTarget.Android,
            options         = BuildOptions.Development | BuildOptions.AllowDebugging
        };

        Build(options);
    }

    public static void BuildIOS()
    {
        BuildIOSInternal(simulator: false);
    }

    public static void BuildIOSSimulator()
    {
        BuildIOSInternal(simulator: true);
    }

    static void BuildIOSInternal(bool simulator)
    {
        string outputPath = GetBuildPath(simulator ? "Build/iOS-Simulator" : "Build/iOS");
        Directory.CreateDirectory(outputPath);

        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.iOS, "com.appsflyer.engagement");
        PlayerSettings.productName = "UnityQATest";
        PlayerSettings.bundleVersion = "1.0";
        PlayerSettings.iOS.buildNumber = "1";
        PlayerSettings.iOS.sdkVersion = simulator
            ? iOSSdkVersion.SimulatorSDK
            : iOSSdkVersion.DeviceSDK;

        var options = new BuildPlayerOptions
        {
            scenes          = new[] { "Assets/Scenes/QATestScene.unity" },
            locationPathName = outputPath,
            target          = BuildTarget.iOS,
            options         = BuildOptions.Development | BuildOptions.AllowDebugging
        };

        Build(options);
    }

    static void Build(BuildPlayerOptions options)
    {
        BuildReport report = BuildPipeline.BuildPlayer(options);
        if (report.summary.result == BuildResult.Succeeded)
        {
            Debug.Log("[BuildScript] Build succeeded: " + options.locationPathName);
        }
        else
        {
            Debug.LogError("[BuildScript] Build FAILED. Result: " + report.summary.result);
            EditorApplication.Exit(1);
        }
    }

    static string GetBuildPath(string fallback)
    {
        string env = Environment.GetEnvironmentVariable("UNITY_BUILD_PATH");
        return string.IsNullOrEmpty(env) ? fallback : env;
    }
}
#endif
