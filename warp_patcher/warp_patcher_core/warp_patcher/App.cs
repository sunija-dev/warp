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
using System.Diagnostics;

namespace warp_patcher_ui
{
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // Create the startup window
            MainWindow wnd = new MainWindow();
            // Do stuff here, e.g. to the window
            wnd.Title = "WARP Patcher";
            // Show the window
            wnd.Show();

            Patcher.Start(wnd);
        }
    }

    public static class Patcher
    {
        public static event EventHandler eDownloadProcessChanged;
        public static int iDownloadProcess = 0;
        private static string strInstallPath = "";
        private static string strClientDownload = "";
        private static string strLanguage = "en";

        public static async void Start(MainWindow _mainWindow)
        {
            // close WARP / WARP starter if necessary
            var arProcesses = new Process[0];
            arProcesses = Process.GetProcessesByName("warp_starter");
            foreach (Process process in arProcesses)
                process.Kill();
            arProcesses = Process.GetProcessesByName("warp");
            foreach (Process process in arProcesses)
                process.Kill();

            Init();
            // from https://stackoverflow.com/questions/5710127/get-operating-system-language-in-c-sharp/27642206
            CultureInfo ci = CultureInfo.CurrentCulture;
            strLanguage = ci.Name.Substring(0, 2);

            _mainWindow.textTitle.Content = strLocalize("Guild Wars 2 - Unofficial Roleplay Addon",
                                                        "Guild Wars 2 - Unoffizielles Roleplay Addon",
                                                        "Guild Wars 2 - Addon Roleplay Officieux",
                                                        "Guild Wars 2 - Complemento de Roleplay No Oficial");
            _mainWindow.textInfo.Text = strLocalize("Downloading WARP updates. Please wait...",
                                                    "Downloade Updates für WARP. Bitte warten...",
                                                    "Download WARP updates. Attendez...",
                                                    "Descargando actualizaciones de WARP. Por favor espera...");
            _mainWindow.textDisclaimer.Content = strLocalize("WARP is a fan-made overlay for Guild Wars 2. It is not associated in any way with ArenaNet, LLC.",
                                                             "WARP ist ein fan-made Overlay für Guild Wars 2. Es bestehen keine Verbindungen zu ArenaNet, LLC.", 
                                                             "",
                                                            "WARP es un Overlay no oficial para Guild Wars 2. No está asociado de ninguna forma con ArenaNet, LLC.");
            

            _mainWindow.textFeedback.Text = strLocalize("Starting download...", "Starte download...", "Commencer download...", "Iniciando descarga...");
            StartDownload();

            int iMaxWidth = 504;
            while (iDownloadProcess < 100)
            {
                _mainWindow.textFeedback.Text = string.Format(strLocalize("Download at {0}%", "Download bei {0}%", "Download à {0}%", ""), iDownloadProcess);
                _mainWindow.imgFill.Width = (float)Patcher.iDownloadProcess / (float)100 * (float)iMaxWidth;
                await Task.Delay(100);
            }
            _mainWindow.textFeedback.Text = string.Format(strLocalize("Download at {0}%", "Download bei {0}%", "Download à {0}%", "Descarga en {0}%"), 100);
            _mainWindow.imgFill.Width = iMaxWidth;

            _mainWindow.textFeedback.Text = strLocalize("Download finished. Removing old client...", "Download fertig. Entferne altes WARP...", "Download terminé. Suppression de la version précédente...", "Descarga finalizada. Quitando el cliente antiguo.");
            RemoveOldClient();
            _mainWindow.textFeedback.Text = strLocalize("Download finished. Extracting new client...", "Download fertig. Entpacke neues WARP...", "Download terminé. Extraction de la nouvelle version...", "");
            UnzipWarp();
            _mainWindow.textFeedback.Text = strLocalize("Everything set. <3 Starting your new WARP.", "Alles fertig. <3 Starte dein neues WARP.", "Tout est réglé. <3 Commencer votre nouvelle version.", "");
            StartWarp();

            System.Threading.Thread.Sleep(2000);

            _mainWindow.Close();
        }

        public static void Init()
        {
            strInstallPath = Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName) + "/";
            strClientDownload = strInstallPath + "WARP.zip";
        }

        public static void StartDownload()
        {
            using (WebClient wc = new WebClient())
            {
                //Console.WriteLine("Saved to " + strInstallPath);
                wc.DownloadProgressChanged += wc_DownloadProgressChanged;
                wc.DownloadFileAsync(
                    // Param1 = Link of file
                    new System.Uri("https://weltenrast.de/data/recore/WARP.zip"),
                    // Param2 = Path to save    
                    strClientDownload
                );
            }
        }

        public static void RemoveOldClient()
        {
            // rename or delete old folder, so there is space for new one
            string strClientPath = strInstallPath + "WARP";
            try
            {
                Console.WriteLine("Deleting old client.");
                if (Directory.Exists(strClientPath))
                    Directory.Delete(strClientPath, true);
            }
            catch
            {
                // INFO: it won't delete the folder, but doesn't matter. unzipping works.
            }
        }

        public static void UnzipWarp()
        {
            ZipFile.ExtractToDirectory(strClientDownload, strInstallPath);

            // delete zip
            if (!string.IsNullOrEmpty(strClientDownload) && File.Exists(strClientDownload))
                File.Delete(strClientDownload);
        }

        public static void StartWarp()
        {
            string strClientPath = strInstallPath + "WARP";
            string strExePath = strClientPath + "/warp_starter.exe";
            System.Diagnostics.Process p = new System.Diagnostics.Process();
            p.StartInfo = new System.Diagnostics.ProcessStartInfo(strExePath);
            p.Start();
        }

        // Event to track the progress
        private static void wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            iDownloadProcess = e.ProgressPercentage;
            eDownloadProcessChanged?.Invoke(sender, null);
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
