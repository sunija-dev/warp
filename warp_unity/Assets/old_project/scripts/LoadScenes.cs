using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadScenes : MonoBehaviour
{
    void Start()
    {
        string[] args = System.Environment.GetCommandLineArgs();

        bool bIsSceneView = false;
        for (int i = 0; i < args.Length; i++)
        {
            Debug.Log("ARG " + i + ": " + args[i]);
            if (args[i] == "-sceneview")
            {
                //input = args[i + 1];
                bIsSceneView = true;
            }
        }

#if UNITY_STANDALONE_WIN || UNITY_EDITOR
        if (!bIsSceneView)
        {
            // cleanup old processes
            SuUtility.FindAndCloseWindow(GlobalConstants.c_strMainWindowName);
        }
#endif

        if (!bIsSceneView)
            SceneManager.LoadScene(1);
        else
            SceneManager.LoadScene(2);
    }

}
