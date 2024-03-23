using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;
using System.Linq;
using UnityEngine.Events;

public class Settings
{
    public static Settings Instance;
    public static UnityEvent eLoaded = new UnityEvent();

    public enum OptionKey 
    { 
        bStartInDebugMode, 
        bFinishedSetup, 
        bAcceptedConditions,
        bHideOnMap,
        bHideInOnlineList,
        iMaxFPS,
        fVolume,
        bStartWarpWithGw2,
        bShowDebugCategory,
        fIconDisplacement,
        fCharInfoPosX,
        fCharInfoPosY,
        iDpiScaling, // in percent
        strVersion,
        iDisplayLanguage,
        fCloseCharListPosX,
        fCloseCharListPosY,
        bShowCharSheetOnHover
    }

    public bool bLoaded = false;
    public List<AccountData> liAccounts = new List<AccountData>();
    public Dictionary<OptionKey, SettingEntryString> dictStringOptions = new Dictionary<OptionKey, SettingEntryString>();
    public Dictionary<OptionKey, SettingEntryBool> dictBoolOptions = new Dictionary<OptionKey, SettingEntryBool>();
    public Dictionary<OptionKey, SettingEntryInt> dictIntOptions = new Dictionary<OptionKey, SettingEntryInt>();
    public Dictionary<OptionKey, SettingEntryFloat> dictFloatOptions = new Dictionary<OptionKey, SettingEntryFloat>();

    private string m_strPath = "";

    public void Init()
    {
        Instance = this;
        m_strPath = Path.Combine(ClientManager.Instance.m_strAddonPath, "settings.json");

        SetDefaultValues();
        SetupCallbacks();
        Load();
        bLoaded = true;
        eLoaded.Invoke();
    }

    private void SetDefaultValues()
    {
        // set default options
        WriteBool(OptionKey.bStartInDebugMode, false, true);
        WriteBool(OptionKey.bFinishedSetup, false, true);
        WriteBool(OptionKey.bAcceptedConditions, false, true);
        WriteBool(OptionKey.bHideOnMap, false, true);
        WriteBool(OptionKey.bHideInOnlineList, false, true);
        WriteInt(OptionKey.iMaxFPS, 60, true);
        WriteFloat(OptionKey.fVolume, 1f, true); // TODO: Add
        WriteBool(OptionKey.bStartWarpWithGw2, true, true);
        WriteBool(OptionKey.bShowDebugCategory, false, true);
        WriteFloat(OptionKey.fIconDisplacement, 0f, true);
        WriteFloat(OptionKey.fCharInfoPosX, 760f, true);
        WriteFloat(OptionKey.fCharInfoPosY, 61f, true);
        WriteFloat(OptionKey.fCloseCharListPosX, 760f, true);
        WriteFloat(OptionKey.fCloseCharListPosY, 61f, true);
        WriteInt(OptionKey.iDpiScaling, 100, true);
        WriteString(OptionKey.strVersion, Application.version, true);
        WriteInt(OptionKey.iDisplayLanguage, (int)GlobalEnums.Language.en, true);
        WriteBool(OptionKey.bShowCharSheetOnHover, true, true);
    }

    private void WriteDefaultConfig()
    {
        File.Delete(m_strPath);
        SetDefaultValues();
        Save();
    }

    private void SetupCallbacks()
    {
        dictBoolOptions[OptionKey.bStartWarpWithGw2].eChangedValue += (s, _bValue) => { GameIntegration.SetAutostartWithGw2(_bValue); };
        dictIntOptions[OptionKey.iMaxFPS].eChangedValue += (s, _iValue) => { ClientManager.Instance.ChangeTargetFramerate(_iValue); };
        dictBoolOptions[OptionKey.bShowDebugCategory].eChangedValue += (s, _bValue) => { ClientManager.Instance.ShowDebugSite(_bValue); };
        dictFloatOptions[OptionKey.fIconDisplacement].eChangedValue += (s, _bValue) => { TopBar.Instance.UpdatePlacement(); };
        dictIntOptions[OptionKey.iDpiScaling].eChangedValue += (s, _iValue) => { GameIntegration.Instance.OnUiScalingChanged(); };
        dictIntOptions[OptionKey.iDisplayLanguage].eChangedValue += (s, _iValue) => { ClientManager.Instance.ChangeDisplayLanguage((GlobalEnums.Language)_iValue); };
        dictBoolOptions[OptionKey.bShowCharSheetOnHover].eChangedValue += (s, _bValue) => { CharHoverManager.Instance.SetShowOnHover(_bValue); };

        // callbacks that have to wait for the player object
        ClientManager.Instance.eCharLoggedIn.AddListener(OnPlayerCreated);
    }

    private void OnPlayerCreated()
    {
        dictBoolOptions[OptionKey.bHideInOnlineList].eChangedValue += (s, _bValue) => { Player.Instance.CmdUpdateHideInPlayerList(_bValue); };
        Player.Instance.CmdUpdateHideInPlayerList(bRead(OptionKey.bHideInOnlineList)); // call callback, bcs setting was set before player was created

        dictBoolOptions[OptionKey.bHideOnMap].eChangedValue += (s, _bValue) => { Player.Instance.CmdUpdateHideOnMap(_bValue); };
        Player.Instance.CmdUpdateHideOnMap(bRead(OptionKey.bHideOnMap));
    }

    public void Save()
    {
        Debug.Log($"Settings: Saving settings to {m_strPath}.");
        File.WriteAllText(m_strPath, JsonConvert.SerializeObject(this, Formatting.Indented));
    }

    public void Load()
    {
        Debug.Log("Settings: Loading settings.");

        if (!File.Exists(m_strPath))
        {
            Debug.Log($"Settings: Didn't find settings.json at '{m_strPath}', creating new one.");
            Save();
        }

        // load settings and override current (default) values. This ensures that patched-in default values persist.
        string strSettingsJson = File.ReadAllText(m_strPath);

        // TEMPORARY: delete old settings file, if "deleting old" isn't available in good way
        if (!strSettingsJson.Contains("\"strVersion\": {"))
        {
            WriteDefaultConfig();
            Load();
            return;
        }

        Settings settingsLoaded = JsonConvert.DeserializeObject<Settings>(strSettingsJson);
        liAccounts = settingsLoaded.liAccounts;

        foreach (KeyValuePair<OptionKey, SettingEntryString> kvp in settingsLoaded.dictStringOptions)
            WriteString(kvp.Key, kvp.Value.value);
        foreach (KeyValuePair<OptionKey, SettingEntryBool> kvp in settingsLoaded.dictBoolOptions)
            WriteBool(kvp.Key, kvp.Value.value);
        foreach (KeyValuePair<OptionKey, SettingEntryInt> kvp in settingsLoaded.dictIntOptions)
            WriteInt(kvp.Key, kvp.Value.value);
        foreach (KeyValuePair<OptionKey, SettingEntryFloat> kvp in settingsLoaded.dictFloatOptions)
            WriteFloat(kvp.Key, kvp.Value.value);

        // can only check for version once it was loaded
        int[] arVersion = ClientManager.arParseVersion(strRead(OptionKey.strVersion));
        if (arVersion == null || ClientManager.bVersionIsOlder(arVersion, WarpManager.Instance.arVersionDeleteSettingsIfOlder))
        {
            WriteDefaultConfig();
            Load();
            return;
        }
    }


    public class AccountData
    {
        public string strApiKey = "";
        public string strAccountName = "";
        public GlobalEnums.Language language = default;
        public GlobalEnums.Region region = default;
        public List<string> liCharNames = new List<string>();

        public AccountData() { }
        public AccountData(Account _account)
        {
            strApiKey = _account.strApiKey;
            strAccountName = _account.accountInfo != null ? _account.accountInfo.name : "";
            language = _account.language;
            region = _account.region;
            if (_account.arGW2Characters != null)
                liCharNames = new List<GW2APICharInfo>(_account.arGW2Characters).Select(x => x.name).ToList();
        }
    }

    public void AddAccount(Account _account)
    {
        if (_account == null || string.IsNullOrEmpty(_account.strApiKey))
            return;

        // check for duplicates
        AccountData accountDataNew = new AccountData(_account);
        if (liAccounts.Any(x => x.strAccountName == accountDataNew.strAccountName))
            liAccounts.RemoveAll(x => x.strAccountName == accountDataNew.strAccountName);

        liAccounts.Add(accountDataNew);
    }

    public static void WriteBool(OptionKey _optionKey, bool _bValue, bool _bNoEvent = false)
    {
        if (!Instance.dictBoolOptions.ContainsKey(_optionKey))
            Instance.dictBoolOptions[_optionKey] = new SettingEntryBool(){ optionKey = _optionKey };
        Instance.dictBoolOptions[_optionKey].Set(_bValue, _bNoEvent); 
    }
    public static bool bRead(OptionKey _optionKey)
    { return Instance.dictBoolOptions[_optionKey].value; }

    public static void WriteString(OptionKey _optionKey, string _strValue, bool _bNoEvent = false)
    {
        if (!Instance.dictStringOptions.ContainsKey(_optionKey))
            Instance.dictStringOptions[_optionKey] = new SettingEntryString() { optionKey = _optionKey };
        Instance.dictStringOptions[_optionKey].Set(_strValue, _bNoEvent); 
    }
    public static string strRead(OptionKey _optionKey)
    { return Instance.dictStringOptions[_optionKey].value; }

    public static void WriteInt(OptionKey _optionKey, int _iValue, bool _bNoEvent = false)
    {
        if (!Instance.dictIntOptions.ContainsKey(_optionKey))
            Instance.dictIntOptions[_optionKey] = new SettingEntryInt() { optionKey = _optionKey };
        Instance.dictIntOptions[_optionKey].Set(_iValue, _bNoEvent); 
    }
    public static int iRead(OptionKey _optionKey)
    { return Instance.dictIntOptions[_optionKey].value; }

    public static void WriteFloat(OptionKey _optionKey, float _fValue, bool _bNoEvent = false)
    {
        if (!Instance.dictFloatOptions.ContainsKey(_optionKey))
            Instance.dictFloatOptions[_optionKey] = new SettingEntryFloat() { optionKey = _optionKey };
        Instance.dictFloatOptions[_optionKey].Set(_fValue, _bNoEvent); 
    }
    public static float fRead(OptionKey _optionKey)
    { return Instance.dictFloatOptions[_optionKey].value; }


    [System.Serializable] public class SettingEntryBool : SettingEntry<bool> { }
    [System.Serializable] public class SettingEntryInt : SettingEntry<int> { }
    [System.Serializable] public class SettingEntryFloat : SettingEntry<float> { }
    [System.Serializable] public class SettingEntryString : SettingEntry<string> { }

    [System.Serializable]
    public class SettingEntry<T> : SettingEntryBase
    {
        public T value;
        public event System.EventHandler<T> eChangedValue;

        public void Set(T _value, bool _bNoEvent = false)
        {
            value = _value;

            if (!_bNoEvent)
                eChangedValue?.Invoke(this, value);
        }
    }

    [System.Serializable]
    public abstract class SettingEntryBase
    {
        public OptionKey optionKey;
    }
}
