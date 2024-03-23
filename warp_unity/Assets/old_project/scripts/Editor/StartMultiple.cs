using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Diagnostics;

public class StartMultiple
{
    [MenuItem("Start Tools/Build and Start Two")]
    public static void BuildGame()
    {
        // Build player.
        //BuildPipeline.BuildPlayer(levels, path + "/BPBJam.exe", BuildTarget.StandaloneWindows, BuildOptions.None);
        BuildPipeline.BuildPlayer(BuildPlayerWindow.DefaultBuildMethods.GetBuildPlayerOptions(new BuildPlayerOptions()));

        // Copy a file from the project folder to the build folder, alongside the built game.
        //FileUtil.CopyFileOrDirectory("Assets/Templates/Readme.txt", path + "Readme.txt");

        //StartFourGame();

    }

    [MenuItem("Start Tools/Start Two")]
    public static void StartFourGame()
    {

        // Run the game (Process class from System.Diagnostics).
        string path = Application.dataPath;
        path = System.IO.Path.GetDirectoryName(path) + "\\start_four.bat";

        Process proc = new Process();
        proc.StartInfo.FileName = path;
        proc.Start();
    }
}
