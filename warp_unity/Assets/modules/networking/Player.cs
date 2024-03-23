using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SuMumbleLinkGW2;
using Mirror;
using System;
using UnityEngine.Events;
using System.Linq;
using Newtonsoft.Json;

/// <summary>
/// Contains all user information that is shared via the network.
/// </summary>
public class Player : NetworkBehaviour
{
    public static Player Instance; // only valid for client
    public Account account = new Account(); // only valid for owner & server

    public float m_fUpdateContinentPosIfBiggerThan = 10f;

    public Character Character => character;
    [SerializeField] private Character character; // never change reference, because it is not synced (it's just a container)
    [SyncVar] public AccountSheet accountSheet = new AccountSheet();
    public string strNotes = ""; // notes for that character. Updated via TargetRPC

    [Header("Options")]
    [SyncVar] public bool bHideInPlayerList = false;
    [SyncVar] public bool bHideOnMap = false;
    [SyncVar] public bool bInCompetitiveGameMode = false;

    // references
    public GameObject goCollisionCapsule;
    public Material matDebugCapsule;
    public WarpNetworkPosition warpNetworkPosition;
    private MumbleManager mumbleManager;
    private GW2Api m_gw2Api;
    private ClientManager m_clientManager;

    /// <summary>
    /// Not called during login, but when requesting a single character.
    /// </summary>
    public UnityEvent<Character> m_eCharacterReceived;
    public UnityEvent<List<string>> m_eCharSheetNamesReceived;

    // internals
    private Coroutine m_coClientLoginProcess = null;
    private Coroutine m_coCharLoginRoutine = null;
    private List<Account> m_liRequestedAccountInfos = null;
    private bool m_bNewApiKeyReceived = false;

    public enum ErrorType { NULL, OLD_VERSION, RESTART_LOGIN_PROCESS, API_KEY_INVALID, API_NO_CHAR_SCOPE, API_DOWN }

    private void Start()
    {
        ClientManager.s_liClosePlayers.Add(this);
        ClientManager.s_eClosePlayersUpdated.Invoke();

        m_gw2Api = GetComponent<GW2Api>();

        if (!isLocalPlayer) return;

        Instance = this;
        ClientManager.s_player = this;
        mumbleManager = MumbleManager.Instance;
        m_clientManager = ClientManager.Instance;

        m_clientManager.eAccountChanged.AddListener(CmdUpdateObjectName);
        m_clientManager.eCharacterChanged.AddListener(CmdUpdateObjectName);

        m_coClientLoginProcess = StartCoroutine(coClientLoginProcess());

        mumbleManager.eventInCompetitiveGameModeChanged.AddListener(() => CmdUpdateCompetitiveGameMode(mumbleManager.bIsInCompetitiveGameMode));

        //SpawnDebugCapsule();
    }

    public IEnumerator coClientLoginProcess()
    {
        yield return new WaitUntil(() => Settings.bRead(Settings.OptionKey.bFinishedSetup) == true);
        Debug.Log("Start login process");
        // logged in? get character name
        // not logged in? wait
        yield return new WaitUntil(() => !string.IsNullOrEmpty(MumbleManager.s_gw2Info.identity.strName));
        string strCharLoggedIn = MumbleManager.s_gw2Info.identity.strName;

        Settings.AccountData accountDataCurrent = new Settings.AccountData();

        // find out which account to use for login
        while (string.IsNullOrEmpty(accountDataCurrent.strAccountName))
        {
            // search account that contains char name
            if (Settings.Instance.liAccounts.Where(x => x.liCharNames.Contains(strCharLoggedIn)).Count() > 0) // found? Take it
                accountDataCurrent = Settings.Instance.liAccounts.First(x => x.liCharNames.Contains(strCharLoggedIn));
            else // not found? request charnames for all accounts & check again
            {
                Debug.Log(string.Format("Char {0} not found in settings. Requesting all chars...", strCharLoggedIn));

                List<Settings.AccountData> liAccounts = Settings.Instance.liAccounts;
                m_liRequestedAccountInfos = null;
                CmdRequestAccountInfos(liAccounts.Select(x => x.strApiKey).ToList());
                yield return new WaitUntil(() => m_liRequestedAccountInfos != null);
                Debug.Log("Account infos received.");

                foreach (Account accountCurr in m_liRequestedAccountInfos)
                {
                    // TODO: pushing in the language/region here is hacky.
                    // maybe sending the full accounts is the better idea (instead only the api keys), so server fills only necessary
                    Settings.AccountData accountData = liAccounts.First(x => x.strApiKey == accountCurr.strApiKey);
                    accountCurr.language = accountData.language;
                    accountCurr.region = accountData.region;
                    Settings.Instance.AddAccount(accountCurr);
                }
                    

                if (Settings.Instance.liAccounts.Where(x => x.liCharNames.Contains(strCharLoggedIn)).Count() > 0) // found? Take it
                    accountDataCurrent = Settings.Instance.liAccounts.First(x => x.liCharNames.Contains(strCharLoggedIn));
                else // api key not there yet? Request it
                {
                    ClientManager.Instance.RequestMissingAPIKey(() => { m_bNewApiKeyReceived = true; });
                    yield return new WaitUntil(() => m_bNewApiKeyReceived);
                    m_bNewApiKeyReceived = false;

                    string strAccountNames = "";
                    Settings.Instance.liAccounts.ForEach(x => strAccountNames += x.strAccountName + ", ");
                }
            }
        }

        // log in using the account
        account.language = accountDataCurrent.language;
        account.region = accountDataCurrent.region;
        account.strApiKey = accountDataCurrent.strApiKey;
        CmdLoginAccount(account, Application.version);
        m_coClientLoginProcess = null;
    }

    private void Update() 
    {
        if (!isLocalPlayer || !ClientManager.Instance.bAccLoggedIn) return;

        transform.position = mumbleManager.v3CharPos;

        if (mumbleManager.m_bReadValues && MumbleManager.s_gw2Info.identity != null)
        {
            bool bWrongCharLoggedIn = m_clientManager.bCharLoggedIn && Character.CharInfo.name != MumbleManager.s_gw2Info.identity.strName;

            // log in other character if needed
            if (m_clientManager.bAccLoggedIn
                && (!m_clientManager.bCharLoggedIn || bWrongCharLoggedIn)
                && m_coCharLoginRoutine == null)
            {
                m_coCharLoginRoutine = StartCoroutine(coLoginChar());
            }

            if (!NetworkClient.ready) // stop if client not ready
                return;

            // send server changes (only context. Charsheet is done by functions, api info comes from server itself)
            if (Vector3.Distance(warpNetworkPosition.v2ContinentPosition, mumbleManager.v2ContinentPosition) > m_fUpdateContinentPosIfBiggerThan)
                warpNetworkPosition.CmdUpdateContinentPosition(mumbleManager.v2ContinentPosition);
            if (warpNetworkPosition.region != account.region) warpNetworkPosition.CmdUpdateRegion(account.region);
            if (warpNetworkPosition.language != account.language) warpNetworkPosition.CmdUpdateLanguage(account.language);
            if (warpNetworkPosition.iMapId != (int)MumbleManager.s_gw2Info.mapId) warpNetworkPosition.CmdUpdateMapId((int)MumbleManager.s_gw2Info.mapId);
            if (warpNetworkPosition.strIP != strGetIPAddress()) warpNetworkPosition.CmdUpdateIP(strGetIPAddress());
        }
    }

    private void OnDestroy()
    {
        ClientManager.s_liClosePlayers.Remove(this);
        ClientManager.s_eClosePlayersUpdated.Invoke();
    }

    [Command]
    public void CmdRequestAccountInfos(List<string> _liApiKeys)
    {
        StartCoroutine(coRequestAccountInfos(_liApiKeys));
    }
    public IEnumerator coRequestAccountInfos(List<string> _liApiKeys)
    {
        List<Account> liAccountsResult = new List<Account>();
        foreach (string strApiKey in _liApiKeys)
        {
            System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();

            if (string.IsNullOrEmpty(strApiKey))
            {
                Debug.Log("Api Key was empty");
                continue;
            }    

            Account account = new Account();
            account.accountInfo = new GW2APIAccountInfo();
            account.strApiKey = strApiKey;
            
            GW2APICharInfos charInfos = new GW2APICharInfos();
            GW2Api gw2Api = m_gw2Api;

            // Ask Gw2 api for account info
            CoroutineFeedback feedbackAccInfo = new CoroutineFeedback();
            CoroutineFeedback feedbackCharInfo = new CoroutineFeedback();
            StartCoroutine(gw2Api.GetAccountInfo(account.strApiKey, account.accountInfo, feedbackAccInfo)); // return values by filling the references
            StartCoroutine(gw2Api.GetCharacterInfo(account.strApiKey, charInfos, feedbackCharInfo));

            // wait for webrequests to finish
            while (!feedbackAccInfo.bFinished || !feedbackCharInfo.bFinished)
                yield return new WaitForSeconds(0.1f);

            sw.Stop();

            if (!feedbackAccInfo.bSuccessfull || !feedbackCharInfo.bSuccessfull)
            {
                ErrorType errorType = ErrorType.RESTART_LOGIN_PROCESS;

                if (feedbackAccInfo.exception != null && feedbackAccInfo.exception.Message == "key_not_valid")
                    errorType = ErrorType.API_KEY_INVALID;

                if (feedbackCharInfo.exception != null && feedbackCharInfo.exception.Message == "char_scope_missing")
                    errorType = ErrorType.API_NO_CHAR_SCOPE;

                string strExceptionMessage = feedbackAccInfo.exception != null ? feedbackAccInfo.exception.Message : "";
                strExceptionMessage += feedbackCharInfo.exception != null ? feedbackCharInfo.exception.Message : "";

                TargetError(true, errorType, strExceptionMessage);
                yield break;
            }

            account.arGW2Characters = charInfos.arCharacterInfos;
            liAccountsResult.Add(account);

            DatabaseDebug.AddAction("Player: Request API info", sw.ElapsedMilliseconds, $"Account: {account.accountInfo.name}");
        }

        TargetSetAccountInfos(liAccountsResult);
    }
    [TargetRpc]
    private void TargetSetAccountInfos(List<Account> _liAccountsInfos)
    {
        m_liRequestedAccountInfos = _liAccountsInfos;
    }

    [TargetRpc]
    public void TargetError(bool _bCausesDisconnect, ErrorType _errorType, string _strTextKey, params string[] _arArguments)
    {
        Debug.Log("SERVER ERROR: " + _strTextKey);
        m_clientManager.windowPopup.Init( 
            new WindowPopup.ButtonInfo(true, null, "Ok"), 
            new WindowPopup.ButtonInfo(false, null, ""),
            "SERVER ERROR: " + _strTextKey, _arArguments);

        switch (_errorType)
        {
            case ErrorType.OLD_VERSION:
                m_clientManager.StartPatcher();
                break;
            case ErrorType.RESTART_LOGIN_PROCESS:
                if (m_coClientLoginProcess != null)
                    StopCoroutine(m_coClientLoginProcess);
                m_coClientLoginProcess = StartCoroutine(coClientLoginProcess());
                break;
            case ErrorType.API_NO_CHAR_SCOPE:
            case ErrorType.API_KEY_INVALID:
                if (m_coClientLoginProcess != null)
                    StopCoroutine(m_coClientLoginProcess);
                Settings.Instance.liAccounts.Clear();
                m_coClientLoginProcess = StartCoroutine(coClientLoginProcess());
                break;
            default:
                break;
        }

        if (_bCausesDisconnect)
            WarpNetworkManager.Instance.StopClient();  
    }

    [Command]
    public void CmdLoginAccount(Account _account, string _strApplicationVersion)
    {
        Debug.Log("Login request for " + _account.strApiKey);
        // correct version?
        if (_strApplicationVersion != Application.version)
        {
            Debug.Log("version mismatch: " + _account.strApiKey + " expected:" + Application.version + " received: " + _strApplicationVersion);
            TargetError(true, ErrorType.OLD_VERSION, "New version available. Downloading it right now, please wait.\n " +
                "If the download doesn't start, download it again from <link=https://gw2warp.com>gw2warp.com</link>.");
            return;
        }

        // Create account, chars and sheets in DB. Log in account.
        StartCoroutine(LoginAccProcess(_account));
    }

    // SERVER
    private IEnumerator LoginAccProcess(Account _account)
    {
        account = _account;
        GW2Api gw2Api = m_gw2Api;
        account.accountInfo = new GW2APIAccountInfo();
        GW2APICharInfos charInfos = new GW2APICharInfos();
        account.iMaxItems = 200;

        // Ask Gw2 api for account info
        CoroutineFeedback feedbackAccInfo = new CoroutineFeedback();
        CoroutineFeedback feedbackCharInfo = new CoroutineFeedback();
        StartCoroutine(gw2Api.GetAccountInfo(account.strApiKey, account.accountInfo, feedbackAccInfo)); // return values by filling the references
        StartCoroutine(gw2Api.GetCharacterInfo(account.strApiKey, charInfos, feedbackCharInfo));

        // wait for webrequests to finish
        while (!feedbackAccInfo.bFinished || !feedbackCharInfo.bFinished)
            yield return new WaitForSeconds(0.1f); // TODO: Could be "waituntil"?

        if (!feedbackAccInfo.bSuccessfull || !feedbackCharInfo.bSuccessfull)
        {
            if (feedbackAccInfo.exception.Message == "key_not_valid")
                TargetError(true, ErrorType.API_KEY_INVALID, feedbackAccInfo.exception.Message);
            else if (feedbackAccInfo.exception.Message == "char_scope_missing")
                TargetError(true, ErrorType.API_NO_CHAR_SCOPE, feedbackAccInfo.exception.Message);
            else
                TargetError(true, ErrorType.NULL, feedbackAccInfo.exception.Message);

            yield break;
        }

        // log out old account if there is some
        if (ServerManager.s_dictPlayers.ContainsKey(connectionToClient)
            && ServerManager.s_dictPlayers[connectionToClient] != null
            && ServerManager.s_dictPlayers[connectionToClient].account.accountInfo != null)
            ServerManager.Instance.OnAccountLogInChanged(ServerManager.s_dictPlayers[connectionToClient].account.accountInfo.id, false);

        // update DB & load account
        account.arGW2Characters = charInfos.arCharacterInfos;
        Database.Instance.LoginAndLoadAccount(this); // also loads some info

        // banned?
        if (account.iBanned > 0)
        {
            Debug.Log("Banned account tried to log in: " + account.accountInfo.name);
            TargetError(true, Player.ErrorType.NULL, "Your account is banned.");
            yield break;
        }

        // add to list on server // TODO: necessary? I think it also happens when creating the player
        if (!ServerManager.s_dictPlayers.ContainsKey(connectionToClient))
            ServerManager.s_dictPlayers[connectionToClient] = this;       

        Debug.Log("Server: login successful: " + account.accountInfo.name);
        ServerManager.Instance.OnAccountLogInChanged(account.accountInfo.id, true);
        TargetAccountLogin(account);
    }

    [TargetRpc]
    private void TargetAccountLogin(Account _account)
    {
        account = _account;
        Settings.Instance.AddAccount(account);
        ClientManager.Instance.eAccountChanged.Invoke();
    }

    [Client]
    private IEnumerator coLoginChar() 
    {
        string strCharName = MumbleManager.s_gw2Info.identity.strName;

        // char is from different account? Change account
        if (!account.arGW2Characters.Any(x => x.name == strCharName)) // m_coClientLoginProcess == null && // TODO: Try like this, so it definitely restarts process
        {
            Debug.Log("Client: Tried login for char that doesn't belong to account. Restarting login process.");
            StopCoroutine(m_coClientLoginProcess);
            m_coClientLoginProcess = StartCoroutine(coClientLoginProcess());
            yield break;
        }

        Debug.Log("Client: Try login for char " + strCharName);

        CmdLoginChar(MumbleManager.s_gw2Info.identity.strName);
        yield return new WaitForSeconds(10f);

        if (m_clientManager.bCharLoggedIn) m_coCharLoginRoutine = null;
        else
        {
            Debug.Log("Client: Login timeout (10s) for char " + strCharName);
            m_coCharLoginRoutine = StartCoroutine(coLoginChar());
        } 
    }

    [Command]
    void CmdLoginChar(string _strCharName)
    {
        Debug.Log(string.Format("Server: Login tried with {0} ({1}).", _strCharName, ServerManager.s_dictPlayers[connectionToClient].account.accountInfo.name));

        // check if char actually belongs to that account
        string strAccountID = ServerManager.s_dictPlayers[connectionToClient].account.accountInfo.id;
        if (!Database.Instance.bIsCharOwnedByAccount(strAccountID, _strCharName))
        {
            Debug.Log("ERROR: User tried to log in with char that didn't belong to account");
            return;
        }

        // log out old char if there is some
        if (ServerManager.s_dictPlayers[connectionToClient].character != null
            && ServerManager.s_dictPlayers[connectionToClient].character.charInfo != null)
            ServerManager.Instance.OnCharLogInChanged(ServerManager.s_dictPlayers[connectionToClient].character.charInfo.name, false);

        //->load in cache
        Database.Instance.charLoad(character, strAccountID, _strCharName); // loads data directly into the syncvars. Maybe unify?
        accountSheet = Database.Instance.accountSheetLoad(strAccountID);
        strNotes = Database.Instance.strLoadNotes(strAccountID, _strCharName);
        TargetUpdatedNotes(strNotes);

        //->set active
        ServerManager.s_dictPlayers[connectionToClient].character = character;
        ServerManager.Instance.OnCharLogInChanged(character.charInfo.name, true);
    }

    // CHECK API KEY
    [Command]
    public void CmdCheckAPIKey(string _strAPIKey)
    {
        StartCoroutine(coCheckAPIKey(_strAPIKey));
    }
    private IEnumerator coCheckAPIKey(string _strAPIKey)
    {
        Account account = new Account();
        account.strApiKey = _strAPIKey;
        account.accountInfo = new GW2APIAccountInfo();
        GW2APICharInfos charInfos = new GW2APICharInfos();
        CoroutineFeedback feedbackAccInfo = new CoroutineFeedback();
        CoroutineFeedback feedbackCharInfo = new CoroutineFeedback();
        yield return StartCoroutine(m_gw2Api.GetAccountInfo(_strAPIKey, account.accountInfo, feedbackAccInfo)); // return values by filling the references
        yield return StartCoroutine(m_gw2Api.GetCharacterInfo(_strAPIKey, charInfos, feedbackCharInfo));
        TargetOnAPIKeyChecked(account, feedbackAccInfo.bSuccessfull && feedbackCharInfo.bSuccessfull);
    }

    [TargetRpc]
    private void TargetOnAPIKeyChecked(Account _account, bool _bWorked)
    {
        // TODO: this is hacky. rather do via a feedback action
        if (m_clientManager.pageRequestAPIKey)
        {
            PageRequestApiKey pageRequestApiKey = m_clientManager.pageRequestAPIKey.GetComponent<PageRequestApiKey>();
            pageRequestApiKey.m_bAPIKeyWorked = _bWorked;
            if (_bWorked)
                pageRequestApiKey.m_accountChecked = _account;
        }
            

        if (PageSetup.Instance)
        {
            PageSetup.Instance.m_bAPIKeyWorked = _bWorked;
            if (_bWorked)
                PageSetup.Instance.m_accountChecked = _account;
        }
    }

    [Command]
    public void CmdUpdateSheet(string _strDescription, string _strKnownFor, CharSheet.Aspect[] _arAspects)
    {
        CharSheet charSheetNew = character.charSheet.charSheetCopy(); // needs new object, so syncvar updates it

        charSheetNew.strRPName = character.charInfo.name;
        charSheetNew.strRPDesc = _strDescription;
        charSheetNew.strKnownFor = _strKnownFor;
        charSheetNew.liAspects = _arAspects.ToList();

        character.charSheet = charSheetNew;

        Database.Instance.SaveCharSheet(account.accountInfo.id, character.charInfo.name, character.charSheet);
        Debug.Log("Player " + character.charInfo.name + " updated sheet:\n" + character.charSheet.strRPDesc);
    }

    [Command]
    public void CmdUpdateNotes(string _strNotes)
    {
        strNotes = _strNotes;
        Database.Instance.SaveNotes(account.accountInfo.id, character.charInfo.name, _strNotes);
        TargetUpdatedNotes(_strNotes); // not really necessary, but rather want eveything go through server

        Debug.Log($"Player {character.charInfo.name} updated notes:\n {strNotes}");
    }

    [TargetRpc]
    private void TargetUpdatedNotes(string _strNotes)
    {
        strNotes = _strNotes;
        ClientManager.Instance.eNotesUpdated.Invoke();
    }

    [Command]
    public void CmdUpdateAccountSheet(AccountSheet.Experience _experience, AccountSheet.FightingStyle _fighting, AccountSheet.AdultStatus _adult,
        float _fLoreStrictness, float _fTopic, float _fStyle, float _fPostLength, string _strExamplePost)
    {
        AccountSheet accountSheetNew = accountSheet.accountSheetCopy();

        accountSheetNew.experience = _experience;
        accountSheetNew.fightingStyle = _fighting;
        accountSheetNew.adultStatus = _adult;
        accountSheetNew.fLoreStrictness = _fLoreStrictness;
        accountSheetNew.fTopic = _fTopic;
        accountSheetNew.fStyle = _fStyle;
        accountSheetNew.fPostLength = _fPostLength;
        accountSheetNew.strExamplePost = _strExamplePost;

        accountSheet = accountSheetNew;

        Database.Instance.SaveAccountSheet(account.accountInfo.id, accountSheet);
        Debug.Log("Account " + account.accountInfo.name + " updated sheet.\n");
    }

    // should go to game integration?
    public string strGetIPAddress()
    {
        string[] arSplit = MumbleManager.s_gw2Info.serverAddress.Replace(" ", "").Split(',');

        string strLastIP = "";
        for (int i = 0; i < arSplit.Length; i++)
        {
            if (!string.IsNullOrEmpty(arSplit[i]) && arSplit[i] != "0")
                strLastIP = arSplit[i];
        }
        return strLastIP;
    }

    [Command]
    public void CmdUpdateObjectName()
    {
        gameObject.name = string.Format("player_{0}_{1}"
            , account != null ? account.accountInfo.name : ""
            , character != null ? character.charInfo.name : "");
    }

    [Command]
    public void CmdUpdateHideInPlayerList(bool _bHide)
    {
        bHideInPlayerList = _bHide;
    }

    [Command]
    public void CmdUpdateHideOnMap(bool _bHide)
    {
        bHideOnMap = _bHide;
    }

    [Command]
    public void CmdUpdateCompetitiveGameMode(bool _binCompetitiveGameMode)
    {
        bInCompetitiveGameMode = _binCompetitiveGameMode;
    }

    [Command]
    public void CmdRunAdminCommand(string _strCommand)
    {
        AdminManager.Instance.AdminCommand(connectionToClient, _strCommand);
    }

    [Command]
    public void CmdChangeLanguage(GlobalEnums.Language _language)
    {
        account.language = _language;
        warpNetworkPosition.language = _language;
    }

    [Command]
    public void CmdChangeRegion(GlobalEnums.Region _region)
    {
        account.region = _region;
        warpNetworkPosition.region = _region;
    }

    public string strGetCharName()
    {
        if (character != null
            && character.charInfo != null)
            return character.charInfo.name;
        else
            return "";
    }

    public string strGetAccountName()
    {
        if (account != null
            && account.accountInfo != null)
            return account.accountInfo.name;
        else
            return "";
    }

    public string strGetAccountId()
    {
        if (account != null
                && account.accountInfo != null)
            return account.accountInfo.id;
        else
            return "";
    }

    public void SpawnDebugCapsule()
    {
        MeshRenderer meshRenderer = goCollisionCapsule.AddComponent<MeshRenderer>();
        meshRenderer.material = matDebugCapsule;
    }

    [Command]
    public void CmdRequestCharacter(string _strCharName)
    {
        // check if char actually belongs to that account
        string strAccountID = ServerManager.s_dictPlayers[connectionToClient].account.accountInfo.id;
        if (!Database.Instance.bIsCharOwnedByAccount(strAccountID, _strCharName))
        {
            Debug.Log("ERROR: User tried to request char that didn't belong to account");
            return;
        }

        Character character = new Character();
        Database.Instance.charLoad(character, strAccountID, _strCharName); // loads data directly into the syncvars. Maybe unify?
        TargetReceiveCharacter(character);
    }

    [TargetRpc]
    public void TargetReceiveCharacter(Character _character)
    {
        if (_character == null)
            return;
        
        Debug.Log($"Received character {_character.name}.");
        m_eCharacterReceived.Invoke(_character);
    }

    public IEnumerator coRequestAllAvailableSheets(List<string> _liNamesOutput)
    {
        _liNamesOutput = null;
        Action<List<string>> action = (liNames) => _liNamesOutput = liNames;
        m_eCharSheetNamesReceived.AddListener(action.Invoke);
        CmdRequestAllAvailableSheets();
        yield return new WaitUntil(() => _liNamesOutput != null);
        m_eCharSheetNamesReceived.RemoveListener(action.Invoke);
    }

    [Command]
    private void CmdRequestAllAvailableSheets()
    {
        string strAccountID = ServerManager.s_dictPlayers[connectionToClient].account.accountInfo.id;
        TargetReceiveCharSheetList(Database.Instance.liLoadAllCharSheetNames(strAccountID));
    }

    [TargetRpc]
    private void TargetReceiveCharSheetList(List<string> _liCharSheetNames)
    {
        if (_liCharSheetNames == null)
            return;

        Debug.Log($"Received charSheetList.");
        m_eCharSheetNamesReceived.Invoke(_liCharSheetNames);
    }



}
