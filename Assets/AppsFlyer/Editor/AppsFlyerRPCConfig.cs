using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Linq;

public static class AppsFlyerRPCConfig
{
    private const string RPC_DEFINE = "APPSFLYER_RPC";

    private const string DEPS_FILENAME     = "AppsFlyerDependencies.xml";
    private const string DEPS_DEFAULT_FILE = "AppsFlyerDepsTemplate_Default.xml";
    private const string DEPS_RPC_FILE     = "AppsFlyerDepsTemplate_RPC.xml";

    public static void SetEnabled(bool enabled)
    {
        ManageDefine(enabled);
        ManageDependencies(enabled);
    }

    private static void ManageDefine(bool enabled)
    {
        BuildTargetGroup[] targetGroups = new BuildTargetGroup[]
        {
            BuildTargetGroup.iOS,
            BuildTargetGroup.Android,
            EditorUserBuildSettings.selectedBuildTargetGroup
        };

        foreach (var targetGroup in targetGroups)
        {
            if (targetGroup == BuildTargetGroup.Unknown)
                continue;

            string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);
            var defineList = new List<string>(defines.Split(';').Where(d => !string.IsNullOrWhiteSpace(d)));

            bool changed = false;
            if (enabled && !defineList.Contains(RPC_DEFINE))
            {
                defineList.Add(RPC_DEFINE);
                changed = true;
            }
            else if (!enabled && defineList.Contains(RPC_DEFINE))
            {
                defineList.Remove(RPC_DEFINE);
                changed = true;
            }

            if (changed)
            {
                PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, string.Join(";", defineList));
                Debug.Log("[AppsFlyer] " + (enabled ? "Added" : "Removed") + " scripting define: " + RPC_DEFINE + " for " + targetGroup);
            }
        }
    }

    private static void ManageDependencies(bool enabled)
    {
        string templateName = enabled ? DEPS_RPC_FILE : DEPS_DEFAULT_FILE;
        string templatePath = FindEditorAsset(templateName);

        if (string.IsNullOrEmpty(templatePath))
        {
            Debug.LogError("[AppsFlyer] Template not found: " + templateName);
            return;
        }

        string targetPath = FindEditorAsset(DEPS_FILENAME);
        if (string.IsNullOrEmpty(targetPath))
        {
            targetPath = Path.Combine(Path.GetDirectoryName(templatePath), DEPS_FILENAME);
        }

        File.Copy(templatePath, targetPath, true);
        AssetDatabase.Refresh();
        Debug.Log("[AppsFlyer] Dependencies switched to " + (enabled ? "RPC" : "default") + " template.");
    }

    private static string FindEditorAsset(string filename)
    {
        string nameWithoutExt = Path.GetFileNameWithoutExtension(filename);
        string[] guids = AssetDatabase.FindAssets(nameWithoutExt);
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (path.EndsWith(filename))
                return path;
        }
        return null;
    }
}
