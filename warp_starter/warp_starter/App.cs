using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Collections;
using System.Net;
using System.IO;
using System.IO.Compression;
using System.Globalization;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

// Code from BlishHUD, heavily edited

namespace warp_starter
{
    public partial class App : System.Windows.Application
    {
        private Timer timer;
        private static string strLanguage = "en";

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // close duplicate warp starter
            var arProcesses = new Process[0];
            arProcesses = Process.GetProcessesByName("warp_starter");
            if (arProcesses.Length > 1)
                Quit();

            BuildTrayIcon();

            bool bSilentStart = e.Args.Length > 0 && e.Args[0] == "-silent";
            if (!bSilentStart)
                StartWarp();

            timer = new Timer();
            timer.Tick += new EventHandler(CheckWarp);
            timer.Interval = 2000; // in miliseconds
            timer.Start();
        }
       
        private void CheckWarp(object sender, EventArgs e)
        {
            if (GetDefaultGw2ProcessByName() != null && !bIsWarpRunning())
                StartWarp();
        }

        public static void StartWarp()
        {
            // from https://stackoverflow.com/questions/5710127/get-operating-system-language-in-c-sharp/27642206
            CultureInfo ci = CultureInfo.CurrentCulture;
            //strLanguage = ci.Name.Substring(0, 2);

            /*
            MainWindow wnd = new MainWindow();
            wnd.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            wnd.Title = "WARP Starter";
            wnd.textStartWarp.Text = strLocalize("Starting WARP...",
                                            "Starte WARP...",
                                            "Démarrage de WARP...",
                                            "Iniciando WARP...");
            */
            //wnd.Show();

            string strInstallPath = Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName) + "/";
            //string strClientPath = strInstallPath + "WARP";
            string strExePath = strInstallPath + "/warp.exe";
            Stopwatch stopwatch = Stopwatch.StartNew();
            if (File.Exists(strExePath))
            {
                Process p = new Process();
                p.StartInfo = new ProcessStartInfo(strExePath)//, "-popupwindow")
                {
                    WorkingDirectory = strInstallPath
                };
                p.Start();
                HideWarpWindowOnStartup(p, 4000);
            }

            System.Threading.Thread.Sleep(Math.Max(0, 4000 - (int)stopwatch.ElapsedMilliseconds));
            //wnd.Hide();
        }

        // Unity shows its window as a plain white window until it has loaded. Make it transparent the moment it exists.
        // WARP restores the window style itself once it has turned into the overlay (TransparentWindow.SetupOverlay).
        private static void HideWarpWindowOnStartup(Process _process, int _iTimeoutMs)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            timeBeginPeriod(1); // so Sleep(1) really waits ~1ms instead of ~15ms
            try
            {
                while (stopwatch.ElapsedMilliseconds < _iTimeoutMs && !_process.HasExited)
                {
                    IntPtr hwnd = IntPtr.Zero;
                    while ((hwnd = FindWindowEx(IntPtr.Zero, hwnd, "UnityWndClass", null)) != IntPtr.Zero)
                    {
                        GetWindowThreadProcessId(hwnd, out uint uProcessId);
                        if (uProcessId != (uint)_process.Id)
                            continue;

                        SetWindowLong(hwnd, GWL_EXSTYLE, GetWindowLong(hwnd, GWL_EXSTYLE) | WS_EX_LAYERED);
                        SetLayeredWindowAttributes(hwnd, 0, 0, LWA_ALPHA);
                        return;
                    }
                    System.Threading.Thread.Sleep(1);
                }
            }
            finally
            {
                timeEndPeriod(1);
            }
        }

        private const int GWL_EXSTYLE = -20;
        private const uint WS_EX_LAYERED = 0x00080000;
        private const uint LWA_ALPHA = 0x00000002;

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll")]
        private static extern uint GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);

        [DllImport("user32.dll")]
        private static extern bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);

        [DllImport("winmm.dll")]
        private static extern uint timeBeginPeriod(uint uPeriod);

        [DllImport("winmm.dll")]
        private static extern uint timeEndPeriod(uint uPeriod);


        //private Form _formWrapper;
        private NotifyIcon _trayIcon;
        //private ToolStripItem _launchGw2Tsi;
        //private ToolStripItem _launchGw2AutoTsi;
        private ToolStripItem _exitTsi;

        /// <summary>
        /// The menu displayed when the tray icon is right-clicked.
        /// </summary>
        public ContextMenuStrip TrayIconMenu { get; private set; }

        private void BuildTrayIcon()
        {
            string trayIconText = "WARP Starter";

            this.TrayIconMenu = new ContextMenuStrip();

            // Found this here: https://stackoverflow.com/a/25409865/595437
            // Extract the tray icon from our assembly
            _trayIcon = new NotifyIcon()
            {
                Icon = Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location),
                Text = trayIconText,
                Visible = true,
                ContextMenuStrip = this.TrayIconMenu
            };

            // Populate TrayIconMenu items
            //_launchGw2AutoTsi = this.TrayIconMenu.Items.Add($"{Strings.GameServices.GameIntegrationService.TrayIcon_LaunchGuildWars2} - {Strings.GameServices.GameIntegrationService.TrayIcon_Autologin}");
            //_launchGw2Tsi = this.TrayIconMenu.Items.Add(Strings.GameServices.GameIntegrationService.TrayIcon_LaunchGuildWars2);

            //_launchGw2AutoTsi.Click += delegate { LaunchGw2(true); };
            //_launchGw2Tsi.Click += delegate { LaunchGw2(false); };

            /*
            _trayIcon.DoubleClick += delegate {
                if (!_service.Gw2IsRunning)
                {
                    LaunchGw2(true);
                }
            };
            */

            //this.TrayIconMenu.Items.Add(new ToolStripSeparator());
            _exitTsi = this.TrayIconMenu.Items.Add("Exit WARP");

            _exitTsi.Click += delegate { Quit(true); };

            //this.TrayIconMenu.Opening += TrayIconMenuOnOpening;
        }

        private void Quit(bool _bCloseWARP = false)
        {
            if (_trayIcon != null)
            {
                _trayIcon.Visible = false;
                _trayIcon.Dispose();
            }

            if (_bCloseWARP)
                CloseWARP();
            Current.Shutdown();
        }


        private readonly string[] _processNames = { "Gw2-64", "Gw2", "KZW" };

        private Process GetDefaultGw2ProcessByName()
        {
            var gw2Processes = new Process[0];

            for (int i = 0; i < _processNames.Length && gw2Processes.Length < 1; i++)
            {
                gw2Processes = Process.GetProcessesByName(_processNames[i]);
            }

            return gw2Processes.Length > 0
                       ? gw2Processes[0]
                       : null;
        }

        private bool bIsWarpRunning()
        {
            var arProcesses = new Process[0];
            arProcesses = Process.GetProcessesByName("warp");

            return arProcesses.Length > 0;
        }

        private void CloseWARP()
        {
            if (!bIsWarpRunning())
                return;

            var arProcesses = new Process[0];
            arProcesses = Process.GetProcessesByName("warp");
            arProcesses[0].CloseMainWindow();
        }


        private static string strLocalize(string _strEn, string _strDe, string _strFr, string _strEs)
        {
            if (strLanguage == "en")
                return _strEn;
            if (strLanguage == "de" && !string.IsNullOrEmpty(_strDe))
                return _strDe;
            if (strLanguage == "fr" && !string.IsNullOrEmpty(_strFr))
                return _strFr;
            if (strLanguage == "es" && !string.IsNullOrEmpty(_strEs))
                return _strEs;

            return _strEn;
        }
    }
}
