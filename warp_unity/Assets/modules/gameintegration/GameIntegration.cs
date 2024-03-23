using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Security.AccessControl;
using System.Threading;
using System.Threading.Tasks;
using Blish_HUD.Controls.Extern;
using Blish_HUD.Controls.Intern;
using Blish_HUD.GameIntegration;
//using Blish_HUD.Settings;
using Microsoft.Win32;
using Blish_HUD;
using Microsoft.Xna.Framework;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class GameIntegration : MonoBehaviour
{
    //private static readonly Logger Logger = Logger.GetLogger<GameIntegrationService>();

    public TMP_Text m_textFocusWarp;
    public TMP_Text m_textFocusGW2;
    public TMP_Text m_textFocusGW2Window;
    public TMP_Text m_textFocusGW2Mumble;
    public TMP_Text m_textGw2Window;
    public TMP_Text m_textForegroundWindow;

    public static GameIntegration Instance;

    public static float s_fCanvasScaling = 1f;
    public List<Canvas> liCanvases;
    public float m_fWarpCloseDelay = 1f; // when warp is closed after gw2 closed

    public event EventHandler<EventArgs> eGw2Closed;
    public event EventHandler<EventArgs> eGw2Started;

    public event EventHandler<EventArgs> Gw2AcquiredFocus;
    public event EventHandler<EventArgs> Gw2LostFocus;
    public event EventHandler<EventArgs> eWarpBecomesHidden;
    public event EventHandler<EventArgs> eWarpBecomesVisible;
    public event EventHandler<EventArgs> eScreenSizeChanged;

    public event EventHandler<ValueEventArgs<bool>> eIsInGameChanged;

    // How long, in seconds, between each
    // check to see if GW2 is running
    private const float GW2_EXE_CHECKRATE = 5f;

    private const string GW2_REGISTRY_KEY = @"SOFTWARE\ArenaNet\Guild Wars 2";
    private const string GW2_REGISTRY_PATH_SV = "Path";

    private const string GW2_PATCHWINDOW_NAME = "ArenaNet";
    private static string[] GW2_GAMEWINDOW_NAMES = { "ArenaNet_Dx_Window_Class", "ArenaNet_Gr_Window_Class" };

    public WinFormsIntegration WinForms { get; private set; }
    //public ClientTypeIntegration ClientType { get; private set; }

    private readonly string[] m_arProcessNames = { "Gw2-64", "Gw2", "KZW" };

    public IGameChat Chat { get; private set; }
    private bool bIsInGame;
    public bool m_bIsInGame
    {
        get => bIsInGame;
        private set
        {
            if (bIsInGame == value) return;

            bIsInGame = value;

            eIsInGameChanged?.Invoke(this, new ValueEventArgs<bool>(bIsInGame));
        }
    }
    private bool bGw2HasFocus = false;
    public bool m_bGw2HasFocus
    {
        get => bGw2HasFocus;
        private set
        {
            if (bGw2HasFocus == value) return;

            bGw2HasFocus = value;

            if (bGw2HasFocus)
                Gw2AcquiredFocus?.Invoke(this, EventArgs.Empty);
            else
                Gw2LostFocus?.Invoke(this, EventArgs.Empty);
        }
    }
    public static bool s_bWarpHasFocus = false;
    private bool bGw2IsRunning = false;
    public bool m_bGw2IsRunning
    {
        get => bGw2IsRunning;
        private set
        {
            if (bGw2IsRunning == value) return;

            bGw2IsRunning = value;

            if (bGw2IsRunning)
                eGw2Started?.Invoke(this, EventArgs.Empty);
            else
                eGw2Closed?.Invoke(this, EventArgs.Empty);
        }
    }

    public static IntPtr s_hwndGw2WindowHandle = IntPtr.Zero;
    public static IntPtr s_hwndWarpFormHandle;

    public static bool s_bWarpIsVisible = true;

    private Process processGw2;
    public Process m_processGw2
    {
        get => processGw2;
        set
        {
            if (processGw2 == value) return;

            processGw2 = value;

            // TODO? Make warp invisible
            /*
            if (value == null || _gw2Process.MainWindowHandle == IntPtr.Zero) {
                BlishHud.Form.Invoke((MethodInvoker) (() => { BlishHud.Form.Visible = false; } 
                ));
                _gw2Process = null;
            } 
            else 
            {
                if (_gw2Process.MainModule != null) 
                {
                    _gw2ExecutablePath.Value = _gw2Process.MainModule.FileName;
                }
            }
            */

            if (value != null && processGw2.MainModule != null) // && _gw2Process.MainWindowHandle != IntPtr.Zero 
                    strGw2ExecutablePath = processGw2.MainModule.FileName;

            // GW2 is running if the "_gw2Process" isn't null and the class name of process' 
            // window is the game window name (so we know we are passed the login screen)
            IntPtr hwndGw2 = hwndGetGw2Window();
            s_hwndGw2WindowHandle = (IntPtr)hwndGw2.ToInt32();
            Mouse.SetGw2WindowHandle(s_hwndGw2WindowHandle);
            Keyboard.SetGw2WindowHandle(s_hwndGw2WindowHandle);

            string strWindowName = WindowUtil.GetClassNameOfWindow(hwndGw2);
            bool bRightWindowName = GW2_GAMEWINDOW_NAMES.Contains(strWindowName);
            UnityEngine.Debug.Log($"Class name of window {strWindowName}"); 
            this.m_bGw2IsRunning = processGw2 != null && bRightWindowName;
        }
    }

    private string strGw2ExecutablePath;

    public string m_strGw2ExecutablePath => strGw2ExecutablePath;

    private bool m_bWarpHasFocusLast = false; // HACKFIX

    private Camera m_cameraMain;
    private int m_iCameraCullingMask = 0;
    private float m_fGw2ClosedSince = 0f;
    private bool m_bGw2WasOpenOnce = false;

    public static Rect s_rectGW2 = new Rect();

    private int iScreenHeightLast = 0;
    private int iScreenWidthLast = 0;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        Chat = new GameChat();

        m_cameraMain = Camera.main;
        m_iCameraCullingMask = m_cameraMain.cullingMask;

        eWarpBecomesHidden += (s, e) => OnWarpHidden();
        eWarpBecomesVisible += (s, e) => OnWarpVisible();
        MumbleManager.Instance.eventUiSizeChanged.AddListener(OnUiScalingChanged);
        eScreenSizeChanged += (s, e) => UpdateScaleOfCanvases();

        strGw2ExecutablePath = GetGw2PathFromRegistry();
        s_hwndWarpFormHandle = hwndGetWarpWindow();
        TryAttachToGw2();
    }

    public void Update()
    {
        // Determine if we are in game or not
        //this.IsInGame = Gw2Mumble.TimeSinceTick.TotalSeconds <= 0.5;

        // Hackfix: only switch state to true if it changed for two frames
        bool bWarpHasFocusRaw = WindowUtil.GetForegroundWindow() == s_hwndWarpFormHandle; // TODO: randomly sets to true sometimes
        s_bWarpHasFocus = m_bWarpHasFocusLast && bWarpHasFocusRaw;
        m_bWarpHasFocusLast = bWarpHasFocusRaw;

        if (m_bGw2IsRunning) 
        {
            IntPtr hwndActiveWindowHandle = WindowUtil.GetForegroundWindow();
            m_bGw2HasFocus = (hwndActiveWindowHandle == s_hwndGw2WindowHandle) || MumbleManager.Instance.bGameHasFocus;

            m_bGw2WasOpenOnce = true;
            m_fGw2ClosedSince = 0f;
        }
        else
        {
            // close warp with gw2
            if (m_bGw2WasOpenOnce)
                m_fGw2ClosedSince += Time.unscaledDeltaTime;
            if (m_fGw2ClosedSince > m_fWarpCloseDelay)
            {
                UnityEngine.Debug.Log("GW2 was closed, closing WARP too.");
                Application.Quit();
            }
                

            m_fLastGw2Check += Time.unscaledDeltaTime;

            if (m_fLastGw2Check > GW2_EXE_CHECKRATE)
            {
                TryAttachToGw2();
                //UnityEngine.Debug.Log("Try attaching");
                m_fLastGw2Check = 0f;
            }
        }

        // check if warp should be visible (= gw2 or warp focused)
        bool bWarpisVisible = m_bGw2HasFocus || m_bWarpHasFocusLast;
        //|| !Settings.bRead(Settings.OptionKey.bFinishedSetup);


        if (bWarpisVisible != s_bWarpIsVisible)
        { 
            if (bWarpisVisible)
                eWarpBecomesVisible.Invoke(this, null);
            else
                eWarpBecomesHidden.Invoke(this, null);
        }
        s_bWarpIsVisible = bWarpisVisible;

        (int, int, int, int) arRect = WindowUtil.rectGetGW2Window(s_hwndGw2WindowHandle); // also sets window on top
        if (arRect.Item1 > -1)
        {
            s_rectGW2.x = arRect.Item1;
            s_rectGW2.y = arRect.Item2;
            s_rectGW2.width = arRect.Item3;
            s_rectGW2.height = arRect.Item4;
        }
        

        m_textFocusWarp.text = s_bWarpHasFocus ? "FOCUS: WARP" : "";
        m_textFocusGW2.text = bGw2HasFocus ? "FOCUS: GW2" : "";

        m_textFocusGW2Window.text = WindowUtil.GetForegroundWindow() == s_hwndGw2WindowHandle ? "Focus Window: FOUND" : "Focus Window: --";
        m_textFocusGW2Mumble.text = MumbleManager.Instance.bGameHasFocus ? "Focus Mumble: FOUND" : "Focus Mumble: --";
        m_textGw2Window.text = "Window Gw2: " + s_hwndGw2WindowHandle.ToString();
        m_textForegroundWindow.text = "Window Fore: " + WindowUtil.GetForegroundWindow().ToString();

        if (Screen.height != iScreenHeightLast || Screen.width != iScreenWidthLast)
            eScreenSizeChanged.Invoke(this, null);

        iScreenHeightLast = Screen.height;
        iScreenWidthLast = Screen.width;
    }

    private void OnWarpVisible()
    {
        Application.targetFrameRate = Settings.iRead(Settings.OptionKey.iMaxFPS);
        /*
         * Time.timeScale = 1f;
        m_cameraMain.cullingMask = m_iCameraCullingMask;
        foreach (Canvas canvas in liCanvases)
            canvas.enabled = true;
        */
    }

    private void OnWarpHidden()
    {
        /*
        m_cameraMain.cullingMask = 0;
        //Application.targetFrameRate = 10; // TODO: Maybe add again?
        //Time.timeScale = 0f;

        foreach (Canvas canvas in liCanvases)
            canvas.enabled = false;
        */

        SuInput.SetWindowClickable(false);
        SuInput.DisableHooks();
    }


    private string GetGw2PathFromRegistry()
    {
        try
        {
            using (var regKeyGw2 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64).OpenSubKey(GW2_REGISTRY_KEY, RegistryRights.ReadKey))
            {
                if (regKeyGw2 != null)
                {
                    string strGw2Path = regKeyGw2.GetValue(GW2_REGISTRY_PATH_SV).ToString();

                    if (File.Exists(strGw2Path))
                    {
                        return strGw2Path;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            //Logger.Warn(ex, "Failed to read Guild Wars 2 path from registry value {registryKey} located at {registryPath}.", GW2_REGISTRY_PATH_SV, GW2_REGISTRY_KEY);
        }

        return string.Empty;
    }

    public void TryAttachToGw2()
    {
        // Get process from Mumble if it is defined
        // otherwise just get the first instance running
        this.m_processGw2 = GetDefaultGw2ProcessByName();

        if (this.m_bGw2IsRunning)
        {
            try
            {
                this.m_processGw2.EnableRaisingEvents = true;
                this.m_processGw2.Exited += OnGw2Exit;
            }
            catch (Win32Exception ex) /* [BLISHHUD-W] */
            {
                // Observed as "Access is denied"
                UnityEngine.Debug.Log(ex.Message + "- A Win32Exception was encountered while trying to monitor the Gw2 process. It might be running with different permissions.");
            }
            catch (InvalidOperationException) /* [BLISHHUD-1H] */
            {
                // Can get thrown if the game is closed just as we launched it
                OnGw2Exit(null, EventArgs.Empty);
            }

            // TODO? Make visible?
            /*
            BlishHud.Form.Invoke((MethodInvoker) (() => {
                BlishHud.Form.Visible = true;
            }));
            */
        }
    }

    private float fProcessDumpTimerDebug = 0f;

    private Process GetDefaultGw2ProcessByName()
    {
        Process[] arGw2Processes = new Process[0];

        /*
        if (fProcessDumpTimerDebug < Time.time)
        {
            string strAllProcesses = "";
            Process.GetProcesses().ToList().OrderByDescending(y => y.Id).ToList().ForEach(x => strAllProcesses += $"\n{x.ProcessName} / {x.Id}");
            UnityEngine.Debug.Log("All processes: " + strAllProcesses);

            fProcessDumpTimerDebug += 5f;
        }
        */

        for (int i = 0; i < m_arProcessNames.Length && arGw2Processes.Length < 1; i++)
        {
            arGw2Processes = Process.GetProcessesByName(m_arProcessNames[i]);
            if (arGw2Processes.Length > 0)
                UnityEngine.Debug.Log("found process " + m_arProcessNames[i]);
        }

        return arGw2Processes.Length > 0
                   ? arGw2Processes[0]
                   : null;
    }

    public static IntPtr hwndGetGw2Window()
    {
        UnityEngine.Debug.Log("Getting gw2 window.");

        IDictionary<IntPtr, string> dictWindows = OpenWindowGetter.GetOpenWindows();
        IntPtr hwndGW2 = dictWindows.FirstOrDefault(thing => thing.Value == "Guild Wars 2").Key;
        if (hwndGW2 == default)
        {
            UnityEngine.Debug.Log("Didn't find Guild Wars 2, trying alternative window names.");
            hwndGW2 = dictWindows.FirstOrDefault(thing => thing.Value == "Guild Wars 2 Game Client"
                                            || thing.Value == "Guild Wars 2 Game Client (2)"
                                            || thing.Value == "Guild Wars 2 (2)").Key;
            if (hwndGW2 == default) UnityEngine.Debug.Log("Still couldn't find it. :(");
        }

        // UnityEngine.Debug.Log($"hwndGW2 is {hwndGW2}");

        return hwndGW2;
    }

    public static IntPtr hwndGetWarpWindow()
    {
        IDictionary<IntPtr, string> dictWindows = OpenWindowGetter.GetOpenWindows();
#if UNITY_EDITOR
        return dictWindows.FirstOrDefault(thing => thing.Value.Contains("warp_unity - main_scene")).Key;
#endif
        return dictWindows.FirstOrDefault(thing => thing.Value == "warp").Key;
    }

    public static IntPtr hwndGetWindowHandle(string _strWindowName)
    {
        IDictionary<IntPtr, string> dictWindows = OpenWindowGetter.GetOpenWindows();
        return dictWindows.FirstOrDefault(thing => thing.Value == _strWindowName).Key;
    }

    private void OnGw2Exit(object sender, EventArgs e)
    {
        this.m_processGw2 = null;

        //Logger.Info("Guild Wars 2 application has exited!");

        /*
        if (!Overlay.StayInTray.Value) {
            Overlay.Exit();
        }
        */
    }

    public static void SetAutostartWithGw2(bool _bAutostart)
    {
        string strExePath = Application.dataPath + "/../warp_starter.exe";
        string strShortcutPath = Environment.GetFolderPath(Environment.SpecialFolder.Startup) + "/WARP.lnk";

        if (_bAutostart)
            ShortcutCreator.CreateShortcut(strExePath, strShortcutPath, "WARP", "-silent");
        else
        {
            if (File.Exists(strShortcutPath))
                File.Delete(strShortcutPath);
        }
    }

    public static void CreateDesktopShortcut()
    {
        string strExePath = Application.dataPath + "/../warp_starter.exe";
        string strShortcutPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "/WARP.lnk";
        ShortcutCreator.CreateShortcut(strExePath, strShortcutPath, "WARP");
    }

    // Keeps track of how long it's been since we last checked for the gw2 process
    private float m_fLastGw2Check = 0;

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    public static extern void SwitchToThisWindow(IntPtr hWnd, bool fAltTab);

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    public static extern IntPtr GetActiveWindow();

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hwnd, int nCmdShow);

    public void SetMinimizedWarp(bool _bMinimized)
    {
        if (_bMinimized)
            ShowWindow(s_hwndWarpFormHandle, 2);
        else
            ShowWindow(s_hwndWarpFormHandle, 1);
    }

    public void OnUiScalingChanged()
    {
        UpdateScaleOfCanvases();
    }

    public void UpdateScaleOfCanvases()
    {
        float fDpiScaling = Settings.iRead(Settings.OptionKey.iDpiScaling) / 100f;
        float fGw2UIScaling = 0.9f + (0.1f * (float)MumbleManager.Instance.iUiSize);
        float fScaleFactor = fGw2UIScaling * fDpiScaling;

        float fSmallWindowScale = 1f;
        if (Screen.width < 1024 || Screen.height < 768) // hardcoded in GW2
            fSmallWindowScale = Mathf.Min(Screen.width / 1024f, Screen.height / 768f);

        s_fCanvasScaling = fScaleFactor * fSmallWindowScale;

        foreach (Canvas canvas in liCanvases)
        {
            canvas.GetComponent<CanvasScaler>().scaleFactor = s_fCanvasScaling;
        }
    }

    public void FocusGw2()
    {
        if (this.m_processGw2 != null)
        {
            try
            {
                WindowUtil.SetForegroundWindowEx(this.m_processGw2.MainWindowHandle);
            }
            catch (NullReferenceException e)
            {
                //Logger.Warn(e, "Failed to give focus to GW2 handle.");
            }
        }
    }

#region Chat Interactions
    /// <summary>
    /// Methods related to interaction with the in-game chat.
    /// </summary>
    public interface IGameChat
    {
        void Send(string message);
        void Paste(string text);
        void OpenWhisper(string _strPlayer);

        Task<string> GetInputText();
        void Clear();
    }
    private class GameChat : IGameChat
    {
        /// <summary>
        /// Sends a message to the chat.
        /// </summary>
        public async void Send(string message)
        {
            if (IsBusy() && !IsTextValid(message)) return;
            byte[] prevClipboardContent = await ClipboardUtil.WindowsClipboardService.GetAsUnicodeBytesAsync();
            await ClipboardUtil.WindowsClipboardService.SetTextAsync(message)
                               .ContinueWith(clipboardResult => {
                                    if (clipboardResult.IsFaulted) 
                                    {
                                       UnityEngine.Debug.Log(string.Format("Failed to set clipboard text with message '{0}'!" + message));
                                    }
                                    else Task.Run(() =>
                                    {
                                        Focus();
                                        Keyboard.Press(VirtualKeyShort.LCONTROL, true);
                                        Keyboard.Stroke(VirtualKeyShort.KEY_V, true);
                                        Thread.Sleep(50);
                                        Keyboard.Release(VirtualKeyShort.LCONTROL, true);
                                        Keyboard.Stroke(VirtualKeyShort.RETURN);
                                    }).ContinueWith(result =>
                                    {
                                        if (result.IsFaulted)
                                        {
                                            UnityEngine.Debug.Log("Failed to send message " + message + ", " + result.Exception.Message);
                                                //Logger.Warn(result.Exception, "Failed to send message {message}", message);
                                        }
                                        else if (prevClipboardContent != null)
                                            ClipboardUtil.WindowsClipboardService.SetUnicodeBytesAsync(prevClipboardContent);
                                    });
                               });
        }
        /// <summary>
        /// Adds a string to the input field.
        /// </summary>
        public async void Paste(string text)
        {
            if (IsBusy()) return;
            string currentInput = await GetInputText();
            if (!IsTextValid(currentInput + text)) return;
            byte[] prevClipboardContent = await ClipboardUtil.WindowsClipboardService.GetAsUnicodeBytesAsync();
            await ClipboardUtil.WindowsClipboardService.SetTextAsync(text)
                               .ContinueWith(clipboardResult => {
                                   if (clipboardResult.IsFaulted) { }
                                       //Logger.Warn(clipboardResult.Exception, "Failed to set clipboard text to {text}!", text);
                                       else
                                       Task.Run(() => {
                                           Focus();
                                           Keyboard.Press(VirtualKeyShort.LCONTROL, true);
                                           Keyboard.Stroke(VirtualKeyShort.KEY_V, true);
                                           Thread.Sleep(50);
                                           Keyboard.Release(VirtualKeyShort.LCONTROL, true);
                                       }).ContinueWith(result => {
                                           if (result.IsFaulted)
                                           {
                                                   //Logger.Warn(result.Exception, "Failed to paste {text}", text);
                                               }
                                           else if (prevClipboardContent != null)
                                               ClipboardUtil.WindowsClipboardService.SetUnicodeBytesAsync(prevClipboardContent);
                                       });
                               });
        }
        /// <summary>
        /// Returns the current string in the input field.
        /// </summary>
        public async Task<string> GetInputText()
        {
            if (IsBusy()) return "";
            byte[] prevClipboardContent = await ClipboardUtil.WindowsClipboardService.GetAsUnicodeBytesAsync();
            /*
            await Task.Run(() => {
                Focus();
                Keyboard.Press(VirtualKeyShort.LCONTROL, true);
                Keyboard.Stroke(VirtualKeyShort.KEY_A, true);
                Keyboard.Stroke(VirtualKeyShort.KEY_C, true);
                Thread.Sleep(50);
                Keyboard.Release(VirtualKeyShort.LCONTROL, true);
                Unfocus();
            });
            */
            string inputText = await ClipboardUtil.WindowsClipboardService.GetTextAsync()
                                                  .ContinueWith(result => {
                                                      if (prevClipboardContent != null)
                                                          ClipboardUtil.WindowsClipboardService.SetUnicodeBytesAsync(prevClipboardContent);
                                                      return !result.IsFaulted ? result.Result : "";
                                                  });
            return inputText;
        }
        /// <summary>
        /// Clears the input field.
        /// </summary>
        public void Clear()
        {
            if (IsBusy()) return;
            
            Task.Run(() => {
                Focus();
                Keyboard.Press(VirtualKeyShort.LCONTROL, true);
                Keyboard.Stroke(VirtualKeyShort.KEY_A, true);
                Thread.Sleep(50);
                Keyboard.Release(VirtualKeyShort.LCONTROL, true);
                Keyboard.Stroke(VirtualKeyShort.BACK);
                Unfocus();
            });
        }
        private void Focus()
        {
            Unfocus();
            Keyboard.Stroke(VirtualKeyShort.RETURN);
        }
        private void Unfocus()
        {
            Mouse.Click(MouseButton.LEFT, Screen.width / 2, 0);
        }
        private bool IsTextValid(string text)
        {
            return (text != null && text.Length < 200);
            // More checks? (Symbols: https://wiki.guildwars2.com/wiki/User:MithranArkanere/Charset)
        }
        private bool IsBusy()
        {
            return false;
            return !Instance.m_bGw2IsRunning || !Instance.m_bGw2HasFocus /*|| !Instance.m_bIsInGame*/;
        }

        public void OpenWhisper(string _strPlayer)
        {
            OpenWhisperAsync(_strPlayer);
        }

        public async void OpenWhisperAsync(string _strPlayer)
        {
            if (IsBusy() && !IsTextValid(_strPlayer)) return;

            byte[] prevClipboardContent = await ClipboardUtil.WindowsClipboardService.GetAsUnicodeBytesAsync();
            await ClipboardUtil.WindowsClipboardService.SetTextAsync("/w ")
                               .ContinueWith(clipboardResult => 
                               {
                                   if (clipboardResult.IsFaulted)
                                   {
                                       UnityEngine.Debug.Log("Failed to set clipboard text with message /w!");
                                   }
                                   else Task.Run(() =>
                                   {
                                       Focus();
                                       Keyboard.Press(VirtualKeyShort.LCONTROL, true);
                                       Keyboard.Stroke(VirtualKeyShort.KEY_V, true);
                                       Thread.Sleep(50);
                                       Keyboard.Release(VirtualKeyShort.LCONTROL, true);

                                   }).ContinueWith(result =>
                                   {
                                       if (result.IsFaulted)
                                           UnityEngine.Debug.Log("Failed to send message /w , " + result.Exception.Message);
                                   }).ContinueWith(result =>
                                   {
                                       ClipboardUtil.WindowsClipboardService.SetTextAsync(_strPlayer);
                                   }).ContinueWith(result =>
                                   {
                                       Keyboard.Press(VirtualKeyShort.LCONTROL, true);
                                       Keyboard.Stroke(VirtualKeyShort.KEY_V, true);

                                       Thread.Sleep(50);

                                       Keyboard.Release(VirtualKeyShort.LCONTROL, true);

                                       Thread.Sleep(50);

                                       Keyboard.Stroke(VirtualKeyShort.TAB, true);

                                   }).ContinueWith(result =>
                                   {
                                        if (result.IsFaulted)
                                            UnityEngine.Debug.Log("Failed to send message " + _strPlayer + ", " + result.Exception.Message);
                                        else if (prevClipboardContent != null)
                                            ClipboardUtil.WindowsClipboardService.SetUnicodeBytesAsync(prevClipboardContent);
                                   });

                               });

            /*
            await ClipboardUtil.WindowsClipboardService.SetTextAsync(_strPlayer)
                               .ContinueWith(clipboardResult => {
                                   if (clipboardResult.IsFaulted)
                                   {
                                       UnityEngine.Debug.Log($"Failed to set clipboard text with message '{_strPlayer}'!");
                                   }
                                   else Task.Run(() =>
                                   {
                                       Keyboard.Press(VirtualKeyShort.LCONTROL, true);
                                       Keyboard.Stroke(VirtualKeyShort.KEY_V, true);
                                       Thread.Sleep(50);
                                       Keyboard.Release(VirtualKeyShort.LCONTROL, true);
                                       Thread.Sleep(50);
                                       Keyboard.Stroke(VirtualKeyShort.TAB, true);
                                   }).ContinueWith(result =>
                                   {
                                       if (result.IsFaulted)
                                           UnityEngine.Debug.Log("Failed to send message " + _strPlayer + ", " + result.Exception.Message);
                                       else if (prevClipboardContent != null)
                                           ClipboardUtil.WindowsClipboardService.SetUnicodeBytesAsync(prevClipboardContent);
                                   });
                               });
            */
        }
    }
#endregion
}