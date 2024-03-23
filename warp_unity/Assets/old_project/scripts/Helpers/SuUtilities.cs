using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.ComponentModel;
using System.Linq;
using System.Text;
using UnityEngine.Events;

public static class SuUtility 
{
    public static Vector2 v2MousePos = Vector2.zero;


    public static int StringDistanceLevenshtein(string s, string t)
    {
        if (string.IsNullOrEmpty(s))
        {
            if (string.IsNullOrEmpty(t))
                return 0;
            return t.Length;
        }

        if (string.IsNullOrEmpty(t))
        {
            return s.Length;
        }

        int n = s.Length;
        int m = t.Length;
        int[,] d = new int[n + 1, m + 1];

        // initialize the top and right of the table to 0, 1, 2, ...
        for (int i = 0; i <= n; d[i, 0] = i++) ;
        for (int j = 1; j <= m; d[0, j] = j++) ;

        for (int i = 1; i <= n; i++)
        {
            for (int j = 1; j <= m; j++)
            {
                int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;
                int min1 = d[i - 1, j] + 1;
                int min2 = d[i, j - 1] + 1;
                int min3 = d[i - 1, j - 1] + cost;
                d[i, j] = Math.Min(Math.Min(min1, min2), min3);
            }
        }
        return d[n, m];
    }

    public static T StrToEnum<T>(string _str)
    {
        return (T)Enum.Parse(typeof(T), _str);
    }

    // from https://stackoverflow.com/questions/972307/how-to-loop-through-all-enum-values-in-c
    public static IEnumerable<T> GetEnumValues<T>()
    {
        return Enum.GetValues(typeof(T)).Cast<T>();
    }

    public static List<string> GetEnumStrings<T>()
    {
        return Enum.GetValues(typeof(T)).Cast<T>().Select(x => x.ToString()).ToList();
    }

    public static float SqrMagnitude(Vector3 _v3)
    {
        return _v3.x * _v3.x + _v3.y * _v3.y + _v3.z * _v3.z;
    }

    public static string strGetActiveWindow()
    {
        const int nChars = 256;
        IntPtr handle;
        StringBuilder Buff = new StringBuilder(nChars);

        handle = GetForegroundWindow();

        if (GetWindowText(handle, Buff, nChars) > 0)
        {
            return Buff.ToString();
        }

        return "";
    }

    public static Vector2 v2GetMousePosition(bool _bInvertY = true)
    {
        // mouse position
        float[] arMousePos = SuSpecialUtilities.v2GetMousePosition();
        v2MousePos.x = arMousePos[0];
        if (_bInvertY)
            v2MousePos.y = Screen.height - arMousePos[1];
        else
            v2MousePos.y = arMousePos[1];

        return v2MousePos;
    }

    public static IntPtr FindWindowThatContains(string _strNameContains)
    {
        IntPtr? handle = Process
            .GetProcesses()
            .SingleOrDefault(x => x.MainWindowTitle.Contains(_strNameContains))
            ?.Handle;
        return handle.HasValue ? handle.Value : IntPtr.Zero;
    }

    public static List<Process> liFindProcessesThatContain(string _strNameContains)
    {
        /*
        Process process = Process
            .GetProcesses()
            .SingleOrDefault(x => x.MainWindowTitle.Contains(_strNameContains));
        */

        List<Process> liProcesses = Process.GetProcesses().Where(x => x.ProcessName.Contains(_strNameContains)).ToList();
        return liProcesses;
    }

    // Rename Window
    public static void RenameWindow(string _strOldName, string _strNewName)
    {
        IntPtr h = FindWindow(null, _strOldName);
        //IntPtr h = FindWindowThatContains(_strOldName);
        SetWindowText(h, _strNewName);
    }

    public static void FindAndCloseWindow(string _strWindowName)
    {
        IntPtr h = FindWindow(null, _strWindowName);
        CloseWindow(h);
    }

    public static bool Gw2IsRunning()
    {
        Process[] processes = Process.GetProcessesByName("Gw2-64");
        if (processes.Length > 0)
            return true;
        return false;
    }

    public static void FocusGw2()
    {
        Process[] processes = Process.GetProcessesByName("Gw2-64");
        /*
        if (processes.Length > 0)
        {
            Debug.Log("Gw2 process found");

            if (processes[0].MainWindowHandle == IntPtr.Zero)
            {
                // the window is hidden so try to restore it before setting focus.
                ShowWindow(processes[0].Handle, ShowWindowEnum.Restore);
            }
            SetForegroundWindow((int)processes[0].MainWindowHandle);
        }
        */

    }

    public static void SetClipboard(string _strText)
    {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR
        GUIUtility.systemCopyBuffer = _strText;
        //System.Windows.Forms.Clipboard.SetText(_strText);
#endif
    }

    public static string strGetClipboardText()
    {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR
        return GUIUtility.systemCopyBuffer;
        //return System.Windows.Forms.Clipboard.GetText();
#else
        return "";
#endif
    }

    public static IntPtr FindWindow(string _strName)
    {
        return FindWindow(null, _strName);
    }

    private const int SW_HIDE = 0;
    private const int SW_SHOWNORMAL = 1;
    private const int SW_SHOW = 5;

    [DllImport("user32.dll")]
    public static extern int SetForegroundWindow(int hwnd);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

    [DllImport("user32.dll")]
    static extern bool SetWindowText(IntPtr hWnd, string text);

    [DllImport("user32.dll")]
    static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

    [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool ShowWindow(IntPtr hWnd, ShowWindowEnum flags);

    private enum ShowWindowEnum
    {
        Hide = 0,
        ShowNormal = 1, ShowMinimized = 2, ShowMaximized = 3,
        Maximize = 3, ShowNormalNoActivate = 4, Show = 5,
        Minimize = 6, ShowMinNoActivate = 7, ShowNoActivate = 8,
        Restore = 9, ShowDefault = 10, ForceMinimized = 11
    };

    private const UInt32 WM_CLOSE = 0x0010;

    static void CloseWindow(IntPtr hwnd)
    {
        SendMessage(hwnd, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
    }

    // from: https://stackoverflow.com/questions/33199868/waiting-for-event-inside-unity-coroutine
    public static IEnumerator coWaitUntilEvent(UnityEvent unityEvent)
    {
        var trigger = false;
        Action action = () => trigger = true;
        unityEvent.AddListener(action.Invoke);
        yield return new WaitUntil(() => trigger);
        unityEvent.RemoveListener(action.Invoke);
    }

    public static Vector2 v2ClampToWindow(RectTransform _rtransPanel, float _fAllowedPercentageOffscreen)
    {
        RectTransform rtransCanvas = MainWindow.Instance.GetComponentInParent<Canvas>().transform as RectTransform;
        Vector3[] v3CanvasCorners = new Vector3[4];
        rtransCanvas.GetWorldCorners(v3CanvasCorners);

        Vector3[] v3PanelCorners = new Vector3[4];
        _rtransPanel.GetWorldCorners(v3PanelCorners);

        Vector2 v2ClampedPosition = _rtransPanel.localPosition;
        float fWidthTolerance = Mathf.Abs(v3PanelCorners[2].x - v3PanelCorners[0].x) * _fAllowedPercentageOffscreen;
        float fHeightTolerance = Mathf.Abs(v3PanelCorners[2].y - v3PanelCorners[0].y) * _fAllowedPercentageOffscreen;

        if (v3PanelCorners[2].x > v3CanvasCorners[2].x + fWidthTolerance) // right
            v2ClampedPosition.x = (rtransCanvas.rect.width * 0.5f) - (_rtransPanel.rect.width * (1f - _rtransPanel.pivot.x)) + (_rtransPanel.rect.width * _fAllowedPercentageOffscreen);
        else if (v3PanelCorners[0].x < v3CanvasCorners[0].x - fWidthTolerance) // left
            v2ClampedPosition.x = (-rtransCanvas.rect.width * 0.5f) + (_rtransPanel.rect.width * _rtransPanel.pivot.x) - (_rtransPanel.rect.width * _fAllowedPercentageOffscreen);

        if (v3PanelCorners[2].y > v3CanvasCorners[2].y + fHeightTolerance) // top
            v2ClampedPosition.y = (rtransCanvas.rect.height * 0.5f) - (_rtransPanel.rect.height * (1f - _rtransPanel.pivot.y)) + (_rtransPanel.rect.height * _fAllowedPercentageOffscreen);
        else if (v3PanelCorners[0].y < v3CanvasCorners[0].y - fHeightTolerance) // bottom
            v2ClampedPosition.y = (-rtransCanvas.rect.height * 0.5f) + (_rtransPanel.rect.height * _rtransPanel.pivot.y) - (_rtransPanel.rect.height * _fAllowedPercentageOffscreen);

        return v2ClampedPosition;
    }
}

public class CoroutineWithData
{
    public Coroutine coroutine { get; private set; }
    public object result;
    private IEnumerator target;
    public CoroutineWithData(MonoBehaviour owner, IEnumerator target)
    {
        this.target = target;
        this.coroutine = owner.StartCoroutine(Run());
    }

    private IEnumerator Run()
    {
        while (target.MoveNext())
        {
            result = target.Current;
            yield return result;
        }
    }
}

public class CoroutineFeedback
{
    public bool bFinished = false;
    public bool bSuccessfull = false;
    public Exception exception = null;
}

// from: https://forum.unity.com/threads/read-from-textasset-line-by-line.327422/
public static class TextAssetExtensionMethods
{
    public static List<string> ToList(this TextAsset ta)
    {
        return new List<string>(ta.text.Split('\n'));
    }
}
