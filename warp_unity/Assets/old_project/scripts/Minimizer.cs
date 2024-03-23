using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System;
using System.Linq;
using System.Text;

public class Minimizer : MonoBehaviour
{
    private float fSwitchDelay = 0.15f;
    private bool bShowWindows = true;
    private bool bGW2InFocus = true;
    private bool bGW2WasOpenend = false;

    private const int SW_HIDE = 0;
    private const int SW_SHOWNORMAL = 1;
    private const int SW_SHOW = 5;

    private float fDelayTimer = 0f;

    private void Awake()
    {
        if (Application.isBatchMode)
            gameObject.SetActive(false);
    }

    void Update()
    {
        // minimize if necessary
        string strActiveWindowName = SuUtility.strGetActiveWindow();
        bool bGW2InFocusCurr = strActiveWindowName == "Guild Wars 2" || strActiveWindowName == GlobalConstants.c_strMainWindowName;
        if (bGW2InFocusCurr)
            bGW2WasOpenend = true;

        bool bShowWindowsCurr = !bGW2WasOpenend || bGW2InFocusCurr;

        if (bShowWindowsCurr != bShowWindows)
            fDelayTimer += Time.deltaTime;
        else
            fDelayTimer = 0f;
            
        if (fDelayTimer > fSwitchDelay)
        {
            bShowWindows = bShowWindowsCurr;

            if (bShowWindows)
            {
                Debug.Log("Showed GW2.");
                ShowRPWindow(GlobalConstants.c_strMainWindowName);
                ShowRPWindow(GlobalConstants.c_strSecondWindowName);
            }
            else
            {
                Debug.Log("Minimized GW2.");
                HideRPWindow(GlobalConstants.c_strMainWindowName);
                HideRPWindow(GlobalConstants.c_strSecondWindowName);
            }
        }
    }

    private void ShowRPWindow(string _strName)
    {
        Application.targetFrameRate = Settings.iRead(Settings.OptionKey.iMaxFPS);
        //IntPtr h = FindWindow(null, _strName);
        IntPtr h = SuUtility.FindWindow(_strName);
        if (h == IntPtr.Zero)
            return;

        ShowWindow(h, SW_SHOW);
        EnableWindow(h, true);
        
    }

    private void HideRPWindow(string _strName)
    {
        if (!Application.isEditor)
            Application.targetFrameRate = 1;
        IntPtr h = SuUtility.FindWindow(_strName);
        if (h == IntPtr.Zero)
            return;

        ShowWindow(h, SW_HIDE);
        EnableWindow(h, false);
    }

    

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern bool ShowWindow(IntPtr hwnd, int nCmdShow);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern bool EnableWindow(IntPtr hwnd, bool enabled);

}
