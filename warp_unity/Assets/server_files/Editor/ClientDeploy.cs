using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using Debug = UnityEngine.Debug;

public static class ClientDeploy
{
    const string strBuildDir = "Build/client";

    [MenuItem("Warp/Client/Build with Starter")]
    public static void BuildWithStarter()
    {
        if (!Build()) return;
        AddStarter();
    }

    [MenuItem("Warp/Client/Add Starter to Last Build")]
    public static void AddStarter()
    {
        // the starter is not rebuilt here; build it in Visual Studio (Release) first
        string strStarterDir = Path.GetFullPath("../warp_starter/warp_starter/bin/Release");
        foreach (string strFile in new[] { "warp_starter.exe", "warp_starter.exe.config" })
        {
            string strSource = Path.Combine(strStarterDir, strFile);
            if (!File.Exists(strSource))
            {
                Debug.LogError($"Starter file not found: {strSource}");
                return;
            }
            File.Copy(strSource, Path.Combine(strBuildDir, strFile), true);
        }
        Debug.Log($"Copied warp_starter to {Path.GetFullPath(strBuildDir)}");
    }

    static bool Build()
    {
        Directory.CreateDirectory(strBuildDir);
        BuildPlayerOptions buildOptions = new()
        {
            scenes = EditorBuildSettings.scenes.Where(_scene => _scene.enabled).Select(_scene => _scene.path).ToArray(),
            locationPathName = Path.Combine(strBuildDir, "warp.exe"),
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None,
        };
        BuildReport buildReport = BuildPipeline.BuildPlayer(buildOptions);
        if (buildReport.summary.result != BuildResult.Succeeded)
        {
            Debug.LogError($"Client build failed: {buildReport.summary.result}");
            return false;
        }
        return true;
    }
}
