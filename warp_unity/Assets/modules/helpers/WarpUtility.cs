using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blish_HUD.Input;
using Blish_HUD;
using System;
using Warp.Utility;
using TMPro;

public class WarpUtility : MonoBehaviour
{
    public static WarpUtility instance;
    public TMP_Text m_textDebugOutput;

    public void Awake()
    {
        instance = this;
        Application.logMessageReceived += OutputDebugLog;
    }

    public void OutputDebugLog(string _strLogString, string _strStackTrace, LogType type)
    {
        List<string> liLines = new List<string>(m_textDebugOutput.text.Split(new[] { '\r', '\n' }));

        if (liLines.Count > 10)
            liLines.RemoveAt(0);

        string strOutput = "";
        liLines.ForEach(line => strOutput += line + "\n");

        m_textDebugOutput.text = strOutput +  _strLogString;
    }
}
