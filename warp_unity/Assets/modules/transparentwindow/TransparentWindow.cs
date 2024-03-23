/* 
    ------------------- Code Monkey -------------------

    Thank you for downloading this package
    I hope you find it useful in your projects
    If you have any questions let me know
    Cheers!

               unitycodemonkey.com
    --------------------------------------------------
 */

using System;
using System.Runtime.InteropServices;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Blish_HUD;

public class TransparentWindow : MonoBehaviour 
{
    public Shader m_shaderUIDefault;

    [DllImport("user32.dll")]
    private static extern IntPtr GetActiveWindow();

    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);

    [DllImport("user32.dll", SetLastError = true)]
    static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    [DllImport("user32.dll")]
    static extern int SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);

    private struct MARGINS {
        public int cxLeftWidth;
        public int cxRightWidth;
        public int cyTopHeight;
        public int cyBottomHeight;
    }

    [DllImport("Dwmapi.dll")]
    private static extern uint DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS margins);

    const int GWL_EXSTYLE = -20;

    const uint WS_EX_LAYERED = 0x00080000;
    const uint WS_EX_TRANSPARENT = 0x00000020;
    private const int WS_EX_TOOLWINDOW = 0x0080; // makes taskbar icon disappear
    static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
    const uint LWA_COLORKEY = 0x00000001;
    private static IntPtr hwndWARP;
    private const uint SWP_NOSIZE = 0x0001;
    private const uint SWP_NOMOVE = 0x0002;
    private const int GWL_STYLE = -16;
    private const uint CS_VREDRAW = 0x0001;
    private const uint CS_HREDRAW = 0x0002;


    private bool bInitFinished = false;

    private void Update()
    {
        if (!bInitFinished)
            return;

#if !UNITY_EDITOR
       UpdateOverlay(GameIntegration.s_hwndWarpFormHandle, GameIntegration.s_hwndGw2WindowHandle, GameIntegration.s_bWarpHasFocus);
#endif
    }

    public void UpdateOverlay(IntPtr _hwndWarp, IntPtr _hwndGw2, bool _bWasOnTop)
    {
        IntPtr hwndActive = WindowUtil.GetForegroundWindow();

        // not gw2 focused? -> above gw2
        // gw2 focused? -> just on top (is done in ScaleToGW2)
        if (hwndActive != _hwndGw2 /*&& Form.ActiveForm == null*/) // TODO: and WARP not selected
        {
            _bWasOnTop = true; // DEBUG
            if (_bWasOnTop)
            {
                IntPtr hwndNextAbove = WindowUtil.GetWindow(_hwndGw2, WindowUtil.GW.HWNDPREV);
                if (hwndNextAbove != IntPtr.Zero && hwndNextAbove != _hwndWarp)
                {
                    SetWindowPos(_hwndWarp, hwndNextAbove, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE);
                }
            }
            return;
        }

        // SCALE WINDOW TO WINDOWED MODE
        WindowUtil.ScaleToGW2(_hwndWarp, _hwndGw2, _bWasOnTop); // also sets window on top

        // DEBUG: set default to topmost, as long as scaling doesn't work
        //SetWindowPos(winHandle, HWND_TOPMOST, 0, 0, 0, 0, 0);
    }

    public IEnumerator coSetupOverlay()
    {
        yield return null;

        Debug.Log("Screen sizes: " + Screen.currentResolution.width + ", " + Screen.currentResolution.height);
        Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, FullScreenMode.Windowed, 30);
        Screen.fullScreen = false;
        Screen.fullScreenMode = FullScreenMode.Windowed;
        gameObject.SetActive(true);

        yield return null;

        // NEW SETUP METHOD

#if !UNITY_EDITOR
        SetupOverlay();
#endif

        ClientManager.Instance.eWindowReady.Invoke();
        bInitFinished = true;

        // in case sth went wrong, e.g. after killing warp via task manager
        yield return new WaitForSeconds(10f);
        if (Camera.main.pixelWidth < 100)
        {
            Debug.Log("Screensize error encountered, please restart. Quitting.");
            Application.Quit();
        }

        // =========== OLD METHOD ============

        /*
        Application.runInBackground = true;
        Canvas.GetDefaultCanvasMaterial().shader = m_shaderUIDefault;
        */

    }

    public void SetupOverlay()
    {
        hwndWARP = GameIntegration.hwndGetWarpWindow();
        Debug.Log("Found warp window as: " + WindowUtil.GetClassNameOfWindow(hwndWARP));

        WindowUtil.SetWindowLong(hwndWARP, GWL_STYLE, CS_HREDRAW | CS_VREDRAW);
        WindowUtil.SetShowInTaskbar(hwndWARP, true);
        SetLayeredWindowAttributes(hwndWARP, 0, 255, 2);

//#if !UNITY_EDITOR

        MARGINS margins = new MARGINS { cxLeftWidth = -1 };
        DwmExtendFrameIntoClientArea(hwndWARP, ref margins);
        //SetWindowLong(hwndWARP, GWL_EXSTYLE, WS_EX_LAYERED);
        SetWindowPos(hwndWARP, HWND_TOPMOST, 0, 0, 0, 0, 0);

        // --- SetWindowLong(hwndWARP, GWL_EXSTYLE, WS_EX_LAYERED | WS_EX_TRANSPARENT);
        // --- SetLayeredWindowAttributes(hWnd, 0, 0, LWA_COLORKEY);
//#endif

        Application.runInBackground = true;
        Canvas.GetDefaultCanvasMaterial().shader = m_shaderUIDefault;
    }


    public static void SetClickthrough(bool clickthrough) 
    {
        if (clickthrough) 
        {
            //Debug.Log("Window is CLICKTHROUGH");
            SetWindowLong(hwndWARP, GWL_EXSTYLE, WS_EX_LAYERED | WS_EX_TRANSPARENT | WS_EX_TOOLWINDOW);
        } else 
        {
            //Debug.Log("Window is NOT CLICKTHROUGH");
            SetWindowLong(hwndWARP, GWL_EXSTYLE, WS_EX_LAYERED | WS_EX_TOOLWINDOW);
        }
    }
}
