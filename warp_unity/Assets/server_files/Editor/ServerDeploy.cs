using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using Debug = UnityEngine.Debug;

public static class ServerDeploy
{
    const string strBuildDir = "Build/Server";

    [MenuItem("Warp/Server/Build Linux Server")]
    public static bool Build()
    {
        Directory.CreateDirectory(strBuildDir);
        BuildPlayerOptions buildOptions = new()
        {
            scenes = GetEnabledScenes(),
            locationPathName = Path.Combine(strBuildDir, "warp_server.x86_64"),
            target = BuildTarget.StandaloneLinux64,
            subtarget = (int)StandaloneBuildSubtarget.Server,
            options = BuildOptions.None,
        };
        // building the server can leave the Editor on the Server subtarget, which makes Mirror auto-start a server in play mode
        BuildTarget buildTargetPrevious = EditorUserBuildSettings.activeBuildTarget;
        BuildTargetGroup buildTargetGroupPrevious = BuildPipeline.GetBuildTargetGroup(buildTargetPrevious);
        StandaloneBuildSubtarget subtargetPrevious = EditorUserBuildSettings.standaloneBuildSubtarget;
        BuildReport buildReport;
        try
        {
            buildReport = BuildPipeline.BuildPlayer(buildOptions);
        }
        finally
        {
            EditorUserBuildSettings.standaloneBuildSubtarget = subtargetPrevious;
            if (EditorUserBuildSettings.activeBuildTarget != buildTargetPrevious)
                EditorUserBuildSettings.SwitchActiveBuildTarget(buildTargetGroupPrevious, buildTargetPrevious);
        }
        if (buildReport.summary.result != BuildResult.Succeeded)
        {
            Debug.LogError($"Server build failed: {buildReport.summary.result}");
            return false;
        }
        return true;
    }

    [MenuItem("Warp/Server/Build and Deploy")]
    public static void BuildAndDeploy()
    {
        if (!Build()) return;
        Deploy();
    }

    [MenuItem("Warp/Server/Deploy Last Build")]
    public static void Deploy()
    {
        string strBuildDirFull = Path.GetFullPath(strBuildDir);
        RunScript("deploy_server.ps1", $"-BuildDir \"{strBuildDirFull}\"", "Deploying server", "Uploading and restarting...");
    }

    [MenuItem("Warp/Server/Restart Remote Server")]
    public static void Restart()
    {
        RunScript("restart_server.ps1", "", "Restarting server", "Killing the remote server process...");
    }

    public static void RunScript(string _strScriptName, string _strScriptArgs, string _strProgressTitle, string _strProgressInfo)
    {
        string strScript = Path.GetFullPath("Tools/" + _strScriptName);
        ProcessStartInfo startInfo = new("powershell.exe",
            $"-NoProfile -ExecutionPolicy Bypass -File \"{strScript}\" {_strScriptArgs}")
        {
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
        };

        EditorUtility.DisplayProgressBar(_strProgressTitle, _strProgressInfo, 0.5f);
        try
        {
            using Process process = Process.Start(startInfo);
            string strOutput = process.StandardOutput.ReadToEnd();
            string strError = process.StandardError.ReadToEnd();
            process.WaitForExit();
            if (process.ExitCode == 0) Debug.Log(strOutput);
            else Debug.LogError($"{_strProgressTitle} failed:\n{strOutput}\n{strError}");
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }
    }

    static string[] GetEnabledScenes()
    {
        return EditorBuildSettings.scenes.Where(_scene => _scene.enabled).Select(_scene => _scene.path).ToArray();
    }
}
