using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using Debug = UnityEngine.Debug;

public static class ClientDeploy
{
    const string strBuildDir = "Build/WARP";
    const string strZipPath = "Build/WARP.zip";

    [MenuItem("Warp/Client/Build with Starter")]
    public static void BuildWithStarter()
    {
        if (!Build()) return;
        if (!AddStarter()) return;
        Zip();
    }

    [MenuItem("Warp/Client/Build, Zip and Upload")]
    public static void BuildAndUpload()
    {
        if (!Build()) return;
        if (!AddStarter()) return;
        ZipAndUpload();
    }

    [MenuItem("Warp/Client/Zip Last Build")]
    public static void Zip()
    {
        RunDeployScript(false);
    }

    [MenuItem("Warp/Client/Zip and Upload Last Build")]
    public static void ZipAndUpload()
    {
        RunDeployScript(true);
    }

    static void RunDeployScript(bool _bUpload)
    {
        string strArgs = $"-BuildDir \"{Path.GetFullPath(strBuildDir)}\" -ZipPath \"{Path.GetFullPath(strZipPath)}\"";
        if (_bUpload) strArgs += " -Upload";
        ServerDeploy.RunScript("deploy_client.ps1", strArgs,
            _bUpload ? "Deploying client" : "Zipping client",
            _bUpload ? "Zipping and uploading..." : "Zipping...");
    }

    [MenuItem("Warp/Client/Add Starter to Last Build")]
    public static bool AddStarter()
    {
        // the starter is not rebuilt here; build it in Visual Studio (Release) first
        string strStarterDir = Path.GetFullPath("../warp_starter/warp_starter/bin/Release");
        foreach (string strFile in new[] { "warp_starter.exe", "warp_starter.exe.config" })
        {
            string strSource = Path.Combine(strStarterDir, strFile);
            if (!File.Exists(strSource))
            {
                Debug.LogError($"Starter file not found: {strSource}");
                return false;
            }
            File.Copy(strSource, Path.Combine(strBuildDir, strFile), true);
        }
        Debug.Log($"Copied warp_starter to {Path.GetFullPath(strBuildDir)}");
        return true;
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
