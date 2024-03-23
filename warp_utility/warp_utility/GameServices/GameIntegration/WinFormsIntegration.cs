using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
//using Blish_HUD.GameServices;

namespace Blish_HUD.GameIntegration {
    public class WinFormsIntegration /*: ServiceModule<GameIntegrationService>*/ {

        private Form _formWrapper;

        private NotifyIcon _trayIcon;

        private ToolStripItem _launchGw2Tsi;
        private ToolStripItem _launchGw2AutoTsi;
        private ToolStripItem _exitTsi;

        public GameIntegrationService _service;

        /// <summary>
        /// The menu displayed when the tray icon is right-clicked.
        /// </summary>
        public ContextMenuStrip TrayIconMenu { get; private set; }

        /*
        public WinFormsIntegration(GameIntegrationService service) : base(service) {
            WrapMainForm();
            BuildTrayIcon();
        }

        private void WrapMainForm() {
            _formWrapper = new Form();
            BlishHud.Form.Hide();
            BlishHud.Form.Show(_formWrapper);
            BlishHud.Form.Visible = false;
        }
        */

        public WinFormsIntegration(GameIntegrationService service)
        {
            _service = service;
        }

        public void SetShowInTaskbar(bool showInTaskbar) {
            WindowUtil.SetShowInTaskbar(Warp.Utility.WarpUtilityDll.hwndWarpFormHandle, showInTaskbar);
        }

        private void BuildTrayIcon() {
            string trayIconText = "WARP";

            /*
            if (ApplicationSettings.Instance.MumbleMapName != null) {
                trayIconText += $" ({ApplicationSettings.Instance.MumbleMapName})";
            }
            */

            this.TrayIconMenu = new ContextMenuStrip();

            // Found this here: https://stackoverflow.com/a/25409865/595437
            // Extract the tray icon from our assembly
            _trayIcon = new NotifyIcon() {
                Icon             = Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location),
                Text             = trayIconText,
                Visible          = true,
                ContextMenuStrip = this.TrayIconMenu
            };

            // Populate TrayIconMenu items
            _launchGw2AutoTsi = this.TrayIconMenu.Items.Add($"{"Launch GW2"} - {"Autologin"}");
            _launchGw2Tsi = this.TrayIconMenu.Items.Add("Launch GW2");

            _launchGw2AutoTsi.Click += delegate { LaunchGw2(true); };
            _launchGw2Tsi.Click     += delegate { LaunchGw2(false); };

            _trayIcon.DoubleClick += delegate {
                if (!_service.Gw2IsRunning) {
                    LaunchGw2(true);
                }
            };

            this.TrayIconMenu.Items.Add(new ToolStripSeparator());
            _exitTsi = this.TrayIconMenu.Items.Add("Exit WARP");

            // TODO! Exit warp
            //_exitTsi.Click += delegate { GameService.Overlay.Exit(); };

            this.TrayIconMenu.Opening += TrayIconMenuOnOpening;
        }

        private void TrayIconMenuOnOpening(object sender, CancelEventArgs e) {
            _launchGw2Tsi.Enabled = _launchGw2AutoTsi.Enabled = !_service.Gw2IsRunning
                                                             && File.Exists(_service.Gw2ExecutablePath);
        }

        private void LaunchGw2(bool autologin = false) {
            var args = new List<string>();

            // Auto login
            if (autologin) {
                args.Add("-autologin");
            }

            /*
            // Mumble target name
            if (ApplicationSettings.Instance.MumbleMapName != null) {
                args.Add($"-mumble \"{ApplicationSettings.Instance.MumbleMapName}\"");
            }
            */

            if (File.Exists(_service.Gw2ExecutablePath)) {
                var gw2Proc = new Process {
                    StartInfo = {
                        FileName  = _service.Gw2ExecutablePath,
                        Arguments = string.Join(" ", args)
                    }
                };

                gw2Proc.Start();
            }
        }

        internal void Unload() {
            if (_trayIcon != null) {
                _trayIcon.Visible = false;
                _trayIcon.Dispose();
            }
        }

    }
}
