using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;
using System;

public class ServerManager : MonoBehaviour
{
    public static ServerManager Instance;
    public static Dictionary<NetworkConnection, Player> s_dictPlayers = new Dictionary<NetworkConnection, Player>();

    public WarpInterestManagment m_interestManagment;

    public ServerSettings m_settings;

    public event EventHandler<string> eAccountLoggedIn;
    public event EventHandler<string> eAccountLoggedOut;
    public event EventHandler<string> eCharLoggedIn;
    public event EventHandler<string> eCharLoggedOut;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        Database.Instance.Connect();
        DatabaseDebug.Instance.Connect();

        WarpNetworkManager.Instance.ePlayerDisconnected += (s, _conn) => OnPlayerDisconnected(_conn);

        StartCoroutine(LoadSettingsRepeatedly(10f));
        StartCoroutine(BackupDatabaseRepeatedly(m_settings.fBackupDBInterval));
        StartCoroutine(coCloseServerAtTime());
    }

    public static NetworkConnection connByCharName(string _strCharName)
    {
        return s_dictPlayers.FirstOrDefault(x => x.Value.Character.charInfo.name == _strCharName).Key;
    }

    /// <summary>
    /// Closes the server at a specific time. Used to restart with shell script that restarts.
    /// </summary>
    public IEnumerator coCloseServerAtTime()
    {
        // log every day at utc 3 am
        DateTime dateNow = DateTime.UtcNow;
        DateTime dateTimeDailyLog = new DateTime(dateNow.Year, dateNow.Month, dateNow.Day, 
            (int)m_settings.v3CloseTime.x, (int)m_settings.v3CloseTime.y, (int)m_settings.v3CloseTime.z);
        if (DateTime.UtcNow > dateTimeDailyLog) // if we're already past the time, restart tomorrow
            dateTimeDailyLog = dateTimeDailyLog.AddDays(1);
        yield return new WaitUntil(() => DateTime.UtcNow > dateTimeDailyLog);

        AdminManager.Instance.RpcServerwideMessage("Daily server restart in 2min.\nSave your profile, if you're working on it! <3");
        yield return new WaitForSeconds(120);
        Application.Quit();
    }

    public IEnumerator BackupDatabaseRepeatedly(float _fTime)
    {
        BackupDatabase();
        yield return new WaitForSeconds(_fTime);
        StartCoroutine(BackupDatabaseRepeatedly(_fTime));
    }

    public void BackupDatabase()
    {
        string strDatabasePath = Database.Instance.strGetPath();
        string strBackupName = "Database_" + System.DateTime.UtcNow.ToString("yyyy_MM_dd_HH_mm_ss") + ".sqlite";
        string strBackupPath = System.IO.Path.GetDirectoryName(strDatabasePath) + "/../db_backups";
        if (!System.IO.Directory.Exists(strBackupPath))
            System.IO.Directory.CreateDirectory(strBackupPath);
        strBackupPath = strBackupPath + "/" + strBackupName;
        Debug.Log("Backup path: " + strBackupPath);

        System.IO.File.Copy(strDatabasePath, strBackupPath);
    }

    public void OnPlayerDisconnected(NetworkConnection _conn)
    {
        if (!s_dictPlayers.ContainsKey(_conn))
            return;

        Player player = s_dictPlayers[_conn];
        s_dictPlayers.Remove(_conn);

        if (player.Character.charInfo != null)
            OnCharLogInChanged(player.Character.charInfo.name, false);
        if (player.account.accountInfo != null)
            OnAccountLogInChanged(player.account.accountInfo.id, false);

        // TODO: Destroy player object?
    }

    public void OnAccountLogInChanged(string _strAccountID, bool _bLoggedIn)
    {
        if (_bLoggedIn) eAccountLoggedIn?.Invoke(this, _strAccountID);
        else eAccountLoggedOut?.Invoke(this, _strAccountID);
    }

    public void OnCharLogInChanged(string _strCharName, bool _bLoggedIn)
    {
        if (_bLoggedIn) eCharLoggedIn?.Invoke(this, _strCharName);
        else eCharLoggedOut?.Invoke(this, _strCharName);
    }

    public IEnumerator LoadSettingsRepeatedly(float _fTime)
    {
        LoadSettings();
        yield return new WaitForSeconds(_fTime);
        StartCoroutine(LoadSettingsRepeatedly(_fTime));
    }

    private void LoadSettings()
    {
        m_settings = new ServerSettings();
        string strSettingsPath = Application.dataPath + "/../server_settings.txt";

        if (!System.IO.File.Exists(strSettingsPath))
            System.IO.File.WriteAllText(strSettingsPath, JsonUtility.ToJson(m_settings, true));

        string strSettingsJson = System.IO.File.ReadAllText(strSettingsPath);
        m_settings = JsonUtility.FromJson<ServerSettings>(strSettingsJson);


        // apply values
        m_interestManagment.visRange = (int)m_settings.fVisibilityRange;
        m_interestManagment.rebuildInterval = m_settings.fInterestRebuildInterval;
        WarpNetworkManager.Instance.serverTickRate = m_settings.iTickRate;
    }

    public static string strGetAccountID(NetworkConnection _networkConnection)
    { 
        return s_dictPlayers[_networkConnection].account.accountInfo.id;
    }
}
