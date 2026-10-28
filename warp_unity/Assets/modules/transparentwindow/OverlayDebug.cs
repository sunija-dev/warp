using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using Blish_HUD;

// Watchdog for the "WARP turns invisible" bug: checks the real Windows z-order (not just WARP's own visibility flag)
// and dumps the window state plus the recent focus/z-order events when WARP should be visible but isn't above GW2.
public static class OverlayDebug
{
    [DllImport("user32.dll")]
    private static extern uint GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll")]
    private static extern bool IsWindowVisible(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool IsIconic(IntPtr hWnd);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    private const int GWL_EXSTYLE = -20;
    private const uint WS_EX_TOPMOST = 0x00000008;
    private const uint WS_EX_TRANSPARENT = 0x00000020;
    private const uint WS_EX_TOOLWINDOW = 0x00000080;
    private const uint WS_EX_LAYERED = 0x00080000;
    private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
    private const uint SWP_NOSIZE = 0x0001;
    private const uint SWP_NOMOVE = 0x0002;
    private const uint SWP_NOACTIVATE = 0x0010;

    private const int iMAX_EVENTS = 40;
    private const float fRELOG_INTERVAL = 30f;

    private static readonly Queue<string> s_queueEvents = new();
    private static string s_strLastOverlayBranch = "";
    private static bool s_bProblemLast = false;
    private static float s_fLastProblemLog = -999f;

    public static void AddEvent(string _strEvent)
    {
        s_queueEvents.Enqueue($"[{DateTime.Now:HH:mm:ss.fff}] {_strEvent}");
        while (s_queueEvents.Count > iMAX_EVENTS)
            s_queueEvents.Dequeue();
    }

    // called every frame by TransparentWindow, only records when the branch changes
    public static void SetOverlayBranch(string _strBranch)
    {
        if (_strBranch == s_strLastOverlayBranch)
            return;

        s_strLastOverlayBranch = _strBranch;
        AddEvent("UpdateOverlay: " + _strBranch);
    }

    public static void CheckOverlay(IntPtr _hwndWarp, IntPtr _hwndGw2, Process _processGw2)
    {
        if (_hwndWarp == IntPtr.Zero || _hwndGw2 == IntPtr.Zero)
            return;

        string strProblem = "";
        if (!bIsAbove(_hwndWarp, _hwndGw2))
            strProblem += "WARP is not above GW2 in z-order. ";
        if (!IsWindowVisible(_hwndWarp))
            strProblem += "WARP window is not visible. ";
        if (IsIconic(_hwndWarp))
            strProblem += "WARP window is minimized. ";

        if (strProblem == "")
        {
            if (s_bProblemLast)
                UnityEngine.Debug.Log("[OverlayDebug] WARP is visible above GW2 again.");
            s_bProblemLast = false;
            return;
        }

        // log on first detection, then only every few seconds while it persists
        if (!s_bProblemLast || Time.realtimeSinceStartup - s_fLastProblemLog > fRELOG_INTERVAL)
        {
            UnityEngine.Debug.Log(strDumpState(strProblem, _hwndWarp, _hwndGw2, _processGw2));
            s_fLastProblemLog = Time.realtimeSinceStartup;
        }
        s_bProblemLast = true;

        // safety net: push WARP back on top without stealing focus
        bool bOk = SetWindowPos(_hwndWarp, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE);
        AddEvent($"Watchdog re-asserted TOPMOST: {(bOk ? "ok" : "failed, error " + Marshal.GetLastWin32Error())}");
    }

    private static bool bIsAbove(IntPtr _hwndWarp, IntPtr _hwndGw2)
    {
        IntPtr hwnd = WindowUtil.GetWindow(_hwndGw2, WindowUtil.GW.HWNDPREV);
        for (int i = 0; i < 5000 && hwnd != IntPtr.Zero; i++)
        {
            if (hwnd == _hwndWarp)
                return true;
            hwnd = WindowUtil.GetWindow(hwnd, WindowUtil.GW.HWNDPREV);
        }
        return false;
    }

    private static string strDumpState(string _strProblem, IntPtr _hwndWarp, IntPtr _hwndGw2, Process _processGw2)
    {
        StringBuilder sb = new();
        sb.AppendLine("[OverlayDebug] ===== WARP SHOULD BE VISIBLE BUT ISN'T =====");
        sb.AppendLine("Problem: " + _strProblem);

        IntPtr hwndForeground = WindowUtil.GetForegroundWindow();
        sb.AppendLine("Foreground: " + strDescribe(hwndForeground));
        sb.AppendLine($"Foreground == GW2 handle: {hwndForeground == _hwndGw2}, Mumble game focus: {MumbleManager.Instance.bGameHasFocus}");

        sb.AppendLine("GW2 (s_hwndGw2WindowHandle): " + strDescribe(_hwndGw2));
        if (_processGw2 != null)
        {
            try
            {
                sb.AppendLine("GW2 (cached Process.MainWindowHandle, used by FocusGw2): " + strDescribe(_processGw2.MainWindowHandle));
                // new Process object, so the cached one used by FocusGw2 isn't refreshed by debugging
                using (Process processFresh = Process.GetProcessById(_processGw2.Id))
                    sb.AppendLine("GW2 (fresh Process.MainWindowHandle): " + strDescribe(processFresh.MainWindowHandle));
                sb.AppendLine("GW2 (fresh lookup): " + strDescribe(GameIntegration.hwndGetGw2Window(_processGw2)));
            }
            catch (Exception ex)
            {
                sb.AppendLine("GW2 process lookup failed: " + ex.Message);
            }
        }

        sb.AppendLine("WARP (GameIntegration.s_hwndWarpFormHandle): " + strDescribe(_hwndWarp));
        sb.AppendLine("WARP (TransparentWindow.s_hwndWarp, used by SetClickthrough): " + strDescribe(TransparentWindow.s_hwndWarp));
        uint uExStyle = GetWindowLong(_hwndWarp, GWL_EXSTYLE);
        sb.AppendLine($"WARP ex-style: 0x{uExStyle:X8} (TOPMOST: {(uExStyle & WS_EX_TOPMOST) != 0}, LAYERED: {(uExStyle & WS_EX_LAYERED) != 0}, "
            + $"TRANSPARENT: {(uExStyle & WS_EX_TRANSPARENT) != 0}, TOOLWINDOW: {(uExStyle & WS_EX_TOOLWINDOW) != 0}), "
            + $"visible: {IsWindowVisible(_hwndWarp)}, minimized: {IsIconic(_hwndWarp)}");

        sb.AppendLine($"Last UpdateOverlay branch: {s_strLastOverlayBranch}");
        sb.AppendLine($"Last ScaleToGW2 SetWindowPos: {(WindowUtil.LastSetWindowPosResult ? "ok" : "failed, error " + WindowUtil.LastSetWindowPosError)}");
        sb.AppendLine($"s_bWarpIsVisible: {GameIntegration.s_bWarpIsVisible}, s_bWarpHasFocus: {GameIntegration.s_bWarpHasFocus}, "
            + $"clickable: {SuInput.s_bWindowClickable}, hooks: {SuInput.s_bHooksEnabled}");

        sb.AppendLine("Windows directly above GW2 (nearest first):");
        IntPtr hwnd = WindowUtil.GetWindow(_hwndGw2, WindowUtil.GW.HWNDPREV);
        int iListed = 0;
        for (int i = 0; i < 5000 && hwnd != IntPtr.Zero && iListed < 10; i++)
        {
            // skip invisible windows, there are tons of them
            if (IsWindowVisible(hwnd) || hwnd == _hwndWarp)
            {
                sb.AppendLine("  " + strDescribe(hwnd));
                iListed++;
            }
            hwnd = WindowUtil.GetWindow(hwnd, WindowUtil.GW.HWNDPREV);
        }

        sb.AppendLine("Recent events:");
        foreach (string strEvent in s_queueEvents)
            sb.AppendLine("  " + strEvent);

        return sb.ToString();
    }

    private static string strDescribe(IntPtr _hwnd)
    {
        if (_hwnd == IntPtr.Zero)
            return "0";

        StringBuilder sbTitle = new(256);
        GetWindowText(_hwnd, sbTitle, sbTitle.Capacity);
        GetWindowThreadProcessId(_hwnd, out uint uProcessId);
        uint uExStyle = GetWindowLong(_hwnd, GWL_EXSTYLE);
        return $"{_hwnd} class='{WindowUtil.GetClassNameOfWindow(_hwnd)}' title='{sbTitle}' pid={uProcessId} topmost={(uExStyle & WS_EX_TOPMOST) != 0}";
    }
}
