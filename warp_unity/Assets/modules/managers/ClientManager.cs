using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;
using UnityEngine.Localization.Settings;
using System.Linq;
using Mirror;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.Networking;

public class ClientManager : MonoBehaviour
{
    public static ClientManager Instance;
    public static Player s_player;
    public static List<Player> s_liClosePlayers = new List<Player>();
    public static UnityEvent s_eClosePlayersUpdated = new UnityEvent();

    public Settings m_settings = new Settings();
    [HideInInspector] public string m_strAddonPath = "";
    [HideInInspector] public string m_strScreenshotPath = "";

    [HideInInspector] public bool bAccLoggedIn = false;
    [HideInInspector] public bool bCharLoggedIn = false;
    [HideInInspector] public List<int> liIconList = new List<int>();


    [Header("References")]
    public WarpNetworkManager networkManager;
    public WarpPage pageSetup;
    public WarpPage pageLoading;
    public WarpPage pageRequestAPIKey;
    public WarpPage pageDebug;

    public MainWindow mainWindow;
    public WindowPopup windowPopup;
    public Canvas canvasHover;
    public Canvas canvasPopupIcons;
    public TMP_InputField inputAdminCommands;
    public GameObject goTooltipPrefab;
    public StatisticsGlobal statisticsGlobal;
    public WindowCharInfo windowCharInfo;
    public WindowCharInfo windowMyCharInfo;

    public GameObject goContextMenuWindowPrefab;

    public WindowReport windowReport;
    public CategoryLoading categoryLoading;

    // EVENTS - only for local player
    [Header("Events")]
    public UnityEvent eAccountChanged = new UnityEvent();
    public UnityEvent eCharacterChanged = new UnityEvent();
    public UnityEvent eCharSheetChanged = new UnityEvent();
    public UnityEvent eCharContextChanged = new UnityEvent();
    public UnityEvent eCharLoggedIn = new UnityEvent();
    public UnityEvent eMapChanged = new UnityEvent();
    public UnityEvent eWindowReady = new UnityEvent();
    public UnityEvent eDisplayLanguageChanged = new UnityEvent();
    public UnityEvent eNotesUpdated = new UnityEvent();

    public static UnityEvent eOnCtrlS = new UnityEvent();



    private void Awake()
    {
        Instance = this;
        InitDirectories();
    }

    void Start()
    {
        mainWindow = MainWindow.Instance;
        eAccountChanged.AddListener(OnAccountLoggedIn);
        eAccountChanged.AddListener(() => CloseCharListWindow.Instance.bShouldBeVisible = true);
        eCharLoggedIn.AddListener(OnCharLoggedIn);
        MumbleManager.Instance.eventMapIdChanged.AddListener(() => eMapChanged.Invoke());

        // load iconlist
        liIconList = IconUtility.liLoadIconList();

        // load settings
        m_settings.Init();

        ShowLoadingPage();

        CategoryLoading categoryLoading = Instance.categoryLoading;
        LocalizationUtility.LocalizeTextAsync(categoryLoading.m_textTitle, "logging_in");
        LocalizationUtility.LocalizeTextAsync(categoryLoading.m_textDescription, "login_request");

        StartCoroutine(coWaitForConnection());
    }

    private void Update()
    {
        if (Keyboard.current.leftCtrlKey.isPressed && Keyboard.current.sKey.isPressed)
            eOnCtrlS.Invoke();
    }

    public IEnumerator coWaitForConnection()
    {
        yield return new WaitUntil(() => Player.Instance != null);

        if (!Settings.bRead(Settings.OptionKey.bFinishedSetup) || !Settings.bRead(Settings.OptionKey.bAcceptedConditions))
            StartSetup();
        else
            mainWindow.Close();
    }

    public void ShowLoadingPage()
    {
        StartCoroutine(coShowLoadingPage());
    }

    private IEnumerator coShowLoadingPage()
    {
        mainWindow.SetVisiblityForAllPageSelectors(false);
        mainWindow.OpenPage(pageLoading);
        mainWindow.Close();

        yield return new WaitUntil(() => Player.Instance != null && bAccLoggedIn && bCharLoggedIn);

        mainWindow.SetVisiblityForAllPageSelectors(true);

        if (mainWindow.pageDefault)
            mainWindow.OpenPage(mainWindow.pageDefault);
    }

    private void StartSetup()
    {
        Debug.Log("Started Setup.");

        mainWindow.Open();

        // make sure that enums are set
        ChangeRegion(GlobalEnums.Region.NA);
        ChangeLanguage(GlobalEnums.Language.en);

        mainWindow.SetVisiblityForAllPageSelectors(false);
        mainWindow.OpenPage(pageSetup);
    }

    public void FinishedSetup()
    {
        Debug.Log("Finished Setup.");

        mainWindow.SetVisiblityForAllPageSelectors(true);
        Settings.WriteBool(Settings.OptionKey.bFinishedSetup, true);
        Settings.WriteBool(Settings.OptionKey.bAcceptedConditions, true);
        Settings.Instance.Save();
        mainWindow.Close();
    }

    public void InitDirectories()
    {
        m_strAddonPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments, Environment.SpecialFolderOption.DoNotVerify),
                                        @"Guild Wars 2\addons\warp");
        Directory.CreateDirectory(m_strAddonPath);

        m_strScreenshotPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments, Environment.SpecialFolderOption.DoNotVerify),
                                        @"Guild Wars 2\Screens");
        Directory.CreateDirectory(m_strScreenshotPath);
    }

    public void ChangeLanguage(string _strLanguage)
    {
        if (_strLanguage == "en") ChangeLanguage(GlobalEnums.Language.en);
        if (_strLanguage == "de") ChangeLanguage(GlobalEnums.Language.de);
        if (_strLanguage == "fr") ChangeLanguage(GlobalEnums.Language.fr);
        if (_strLanguage == "es") ChangeLanguage(GlobalEnums.Language.es);
    }

    public void ChangeLanguage(GlobalEnums.Language _language)
    {
        Player.Instance.account.language = _language;
        Player.Instance.CmdChangeLanguage(_language);
        Settings.Instance.AddAccount(Player.Instance.account); // save account
    }

    public void ChangeDisplayLanguage(string _strLanguage)
    {
        if (_strLanguage == "en") ChangeDisplayLanguage(GlobalEnums.Language.en);
        if (_strLanguage == "de") ChangeDisplayLanguage(GlobalEnums.Language.de);
        if (_strLanguage == "fr") ChangeDisplayLanguage(GlobalEnums.Language.fr);
        if (_strLanguage == "es") ChangeDisplayLanguage(GlobalEnums.Language.es);
    }

    public void ChangeDisplayLanguage(GlobalEnums.Language _language)
    {
        StartCoroutine(coChangeDisplayLanguage(_language));
    }

    public void ChangeRegion(string _strRegion)
    {
        if (_strRegion == "na") ChangeRegion(GlobalEnums.Region.NA);
        if (_strRegion == "eu") ChangeRegion(GlobalEnums.Region.EU);
    }

    public void ChangeRegion(GlobalEnums.Region _region)
    {
        Player.Instance.account.region = _region;
        Player.Instance.CmdChangeRegion(_region);
        Settings.Instance.AddAccount(Player.Instance.account); // save account
    }

    private IEnumerator coChangeDisplayLanguage(GlobalEnums.Language _language)
    {
        yield return LocalizationSettings.InitializationOperation;

        Locale locale = LocalizationSettings.AvailableLocales.Locales.First(x => SuUtility.StrToEnum<GlobalEnums.Language>(x.Identifier.Code) == _language);
        LocalizationSettings.SelectedLocale = locale;

        eDisplayLanguageChanged.Invoke();
    }

    public void OnCharLoggedIn()
    {
        Debug.Log("OnCharLoggedIn");
        bCharLoggedIn = true;

        // set initial values
        Player.Instance.CmdUpdateCompetitiveGameMode(MumbleManager.Instance.bIsInCompetitiveGameMode); // could be in MumbleManager
    }

    public void OnAccountLoggedIn()
    {
        Debug.Log("OnAccountLoggedIn");
        bAccLoggedIn = true;
        ChangeLanguage(Player.Instance.account.language);
    }

    private void OnApplicationQuit()
    {
        m_settings.Save();
    }

    public void RequestMissingAPIKey(Action _action)
    {
        mainWindow.SetVisiblityForAllPageSelectors(false);
        pageRequestAPIKey.GetComponent<PageRequestApiKey>().SetFinishedAction(_action);
        mainWindow.OpenPage(pageRequestAPIKey);
    }

    public void StartPatcher()
    {
        StartCoroutine(coStartPatcher());
    }

    public IEnumerator coStartPatcher()
    {
        string strPatcherPath = Application.dataPath + "/../../warp_patcher.exe";

        // maybe renamed by download?
        if (!File.Exists(strPatcherPath))
        {
            string[] arFiles = Directory.GetFiles(Application.dataPath + "/../../");
            foreach (string strFileCurr in arFiles)
            {
                if (strFileCurr.Contains("warp_patcher"))
                    strPatcherPath = strFileCurr;
            }
        }

        // not there? download again
        if (!File.Exists(strPatcherPath))
            yield return StartCoroutine(coDownloadPatcher(strPatcherPath));

        if (!File.Exists(strPatcherPath)) // cannot download it? error msg already tells to download it manually
            yield break;

        try
        {
            Debug.Log(strPatcherPath);
            System.Diagnostics.Process p = new System.Diagnostics.Process();
            p.StartInfo = new System.Diagnostics.ProcessStartInfo(strPatcherPath);
            p.Start();
            CloseWARP();
        }
        catch
        {
            windowPopup.Init(
                new WindowPopup.ButtonInfo(true, null, "ok"),
                new WindowPopup.ButtonInfo(false, null, ""),
                "error_patcher_not_found", strPatcherPath);
            Debug.Log("Could find patcher at " + strPatcherPath + " .");
        }
    }

    // from: https://stackoverflow.com/questions/50689859/download-large-file (heavily adapted)
    private IEnumerator coDownloadPatcher(string _strPath)
    {
        string strUrl = "https://gw2warp.com/data/warp_patcher.exe";

        if (!Directory.Exists(Path.GetDirectoryName(_strPath)))
            Directory.CreateDirectory(Path.GetDirectoryName(_strPath));

        UnityWebRequest webRequest = new UnityWebRequest(strUrl);
        webRequest.method = UnityWebRequest.kHttpVerbGET;
        DownloadHandlerFile downloadHandler = new DownloadHandlerFile(_strPath);
        downloadHandler.removeFileOnAbort = true;
        webRequest.downloadHandler = downloadHandler;

        string strPopupBase = "Could not find patcher at " + _strPath + ", downloading it. Won't take long. :)\n";
        windowPopup.Init(
            new WindowPopup.ButtonInfo(false, null, "Ok"),
            new WindowPopup.ButtonInfo(false, null, ""),
            strPopupBase);

        webRequest.SendWebRequest();

        while (!webRequest.isDone)
        {
            windowPopup.textInfoText.text = $"{strPopupBase}Downloading... {webRequest.downloadProgress,5:###}%";
            yield return null;
        }

        if (webRequest.isNetworkError || webRequest.isHttpError)
            Debug.Log(webRequest.error);
        else
            Debug.Log($"Downloaded patcher to: {_strPath.Replace("/", "\\")} \r\n{webRequest.error}");
    }

    // Utility

    public static void OpenReport(string _strCharName, string _strReportedText)
    {
        Instance.windowReport.OpenReport(_strCharName, _strReportedText);
    }

    public void CloseWARP()
    {
        // close warp_starter
        foreach (System.Diagnostics.Process processStarter in System.Diagnostics.Process.GetProcessesByName("warp_starter"))
            processStarter.CloseMainWindow();

        Application.Quit();
    }

    public void RestartWarp()
    {
        // start warp starter
        //StartWarpStarter(); // TODO!

        // warp_starter will restart WARP
        Application.Quit();
    }

    public void StartWarpStarter()
    {
        IDictionary<IntPtr, string> dictWindows = OpenWindowGetter.GetOpenWindows();
        if (dictWindows.FirstOrDefault(thing => thing.Value == "warp_starter").Key == default)
        {
            Debug.Log("Did not find warp starter");
            string strStarterPath = Application.dataPath + "/../warp_starter.exe";
            if (File.Exists(strStarterPath))
            {
                System.Diagnostics.Process p = new System.Diagnostics.Process();
                p.StartInfo = new System.Diagnostics.ProcessStartInfo(strStarterPath);
                p.Start();
            }
        }
    }

    public void CreateDesktopShortcut()
    {
        GameIntegration.CreateDesktopShortcut();

        windowPopup.Init(
                        new WindowPopup.ButtonInfo(true, null, "ok"),
                        new WindowPopup.ButtonInfo(false, null, ""),
                        "created_shortcut");
    }

    public void ChangeTargetFramerate(int _iTargetFramerate)
    {
        Application.targetFrameRate = _iTargetFramerate;
    }

    public void ShowDebugSite(bool _bShow)
    {
        pageDebug.SetEnabled(_bShow);
    }

    public void RunAdminCommand()
    {
        Player.Instance.CmdRunAdminCommand(inputAdminCommands.text);
    }

    public void ResetWindows()
    {
        windowCharInfo.GetComponentInChildren<DragHandler>()?.Move(new Vector3(700, 60, 0));
        windowMyCharInfo.GetComponentInChildren<DragHandler>()?.Move(new Vector3(700, 60, 0));
        CloseCharListWindow.Instance.GetComponentInChildren<DragHandler>()?.Move(new Vector3(825, -150, 0));
    }

    public static int[] arParseVersion(string _strVersion)
    {
        int[] arResult = new int[3];
        string[] arParts = _strVersion.Split('.');
        if (arParts.Length < 3)
        {
            Debug.Log("Error: Couldn't parse version: Wrong length.");
            return null;
        }

        for (int i = 0; i < arParts.Length; i++)
        {
            bool bParseWorked = int.TryParse(arParts[i], out arResult[i]);
            if (!bParseWorked)
            {
                Debug.Log($"Error: Couldn't parse version: Part {i} couldn't be parsed to int.");
                return null;
            }
        }

        return arResult;
    }

    public static bool bVersionIsOlder(int[] _arVersion, int[] _arVersionToCompareTo)
    {
        for (int i = 0; i < 3; i++)
        {
            if (_arVersion[i] < _arVersionToCompareTo[i])
                return true;

            if (_arVersionToCompareTo[i] < _arVersion[i])
                return false;
        }

        return false;
    }

    public void InviteToGroup(string _strPlayer)
    {
        StartCoroutine(coInviteToGroup(_strPlayer));
    }

    private IEnumerator coInviteToGroup(string _strPlayer)
    {
        yield return new WaitUntil(() => GameIntegration.Instance.m_bGw2HasFocus);
        GameIntegration.Instance.Chat.Send($"/invite {_strPlayer}");
    }
}
