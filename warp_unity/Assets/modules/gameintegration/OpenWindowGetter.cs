using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using HWND = System.IntPtr;

/// <summary>Contains functionality to get all the open windows.</summary>
public static class OpenWindowGetter
{
    /// <summary>Returns a dictionary that contains the handle and title of all the open windows.</summary>
    /// <returns>A dictionary that contains the handle and title of all the open windows.</returns>
    public static IDictionary<HWND, string> GetOpenWindows()
    {
        HWND shellWindow = GetShellWindow();
        Dictionary<HWND, string> windows = new Dictionary<HWND, string>();

        EnumWindows(delegate (HWND hWnd, int lParam)
        {
            if (hWnd == shellWindow) return true;
            if (!IsWindowVisible(hWnd)) return true;

            int length = GetWindowTextLength(hWnd);
            if (length == 0) return true;

            StringBuilder builder = new StringBuilder(length);
            GetWindowText(hWnd, builder, length + 1);

            windows[hWnd] = builder.ToString();
            return true;

        }, 0);

        return windows;
    }

    /// <summary>Returns the first visible top-level window of the process whose class name is in _arClassNames, or IntPtr.Zero.</summary>
    public static HWND hwndFindWindowOfProcess(int _iProcessId, string[] _arClassNames)
    {
        HWND hwndFound = IntPtr.Zero;

        EnumWindows(delegate (HWND hWnd, int lParam)
        {
            if (!IsWindowVisible(hWnd)) return true;

            GetWindowThreadProcessId(hWnd, out uint uProcessId);
            if (uProcessId != (uint)_iProcessId) return true;

            StringBuilder builder = new StringBuilder(256);
            GetClassName(hWnd, builder, builder.Capacity);
            if (Array.IndexOf(_arClassNames, builder.ToString()) < 0) return true;

            hwndFound = hWnd;
            return false;

        }, 0);

        return hwndFound;
    }

    private delegate bool EnumWindowsProc(HWND hWnd, int lParam);

    [DllImport("USER32.DLL")]
    private static extern uint GetWindowThreadProcessId(HWND hWnd, out uint lpdwProcessId);

    [DllImport("USER32.DLL", CharSet = CharSet.Unicode)]
    private static extern int GetClassName(HWND hWnd, StringBuilder lpClassName, int nMaxCount);

    [DllImport("USER32.DLL")]
    private static extern bool EnumWindows(EnumWindowsProc enumFunc, int lParam);

    [DllImport("USER32.DLL")]
    private static extern int GetWindowText(HWND hWnd, StringBuilder lpString, int nMaxCount);

    [DllImport("USER32.DLL")]
    private static extern int GetWindowTextLength(HWND hWnd);

    [DllImport("USER32.DLL")]
    private static extern bool IsWindowVisible(HWND hWnd);

    [DllImport("USER32.DLL")]
    private static extern IntPtr GetShellWindow();
}