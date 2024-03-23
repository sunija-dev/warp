using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// from: https://forum.unity.com/threads/how-to-change-log-file-path.98859/
public class Logger : MonoBehaviour
{
    public bool m_bLogVerbose = false;

    private string m_strPath = "";


    private void Start()
    {
        m_strPath = System.IO.Path.Combine(ClientManager.Instance.m_strAddonPath, "warp_log.txt");
        try
        {
            System.IO.File.WriteAllText(m_strPath, "");
        }
        catch
        {
            Debug.Log($"ERROR: Could not create log file in {m_strPath}.");
        }
        Application.logMessageReceived += Log;
    }

    void OnApplicationQuit() 
    { 
        Application.logMessageReceived -= Log;
    }

    public void Log(string _strLog, string _strStackTrace, LogType _logType)
    {
        try
        {
            string strLog = string.Format("[{0}] {1}\n", DateTime.UtcNow.ToString("HH:mm:ss"), _strLog);
            if (m_bLogVerbose)
                strLog += _strStackTrace + "\n";
            System.IO.File.AppendAllText(m_strPath, strLog + "\n");
        }
        catch 
        { }
    }
}
