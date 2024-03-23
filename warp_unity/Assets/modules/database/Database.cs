using SQLite; // from https://github.com/praeclarum/sqlite-net
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;

public partial class Database : MonoBehaviour
{
    public static Database Instance;
    public string strDatabaseFileName = "Database.sqlite";
    SQLiteConnection connection;

    class accounts
    {
        [PrimaryKey]
        public string strID { get; set; }
        public string strName { get; set; }
        public string strRegion { get; set; }
        public string strAccountType { get; set; }
        public DateTime dateCreatedInWarp { get; set; }
        public DateTime dateLastLogin { get; set; }
        public int iMaxItems { get; set; }
        public bool bBanned { get; set; }
        public DateTime dateBannedUntil { get; set; }
        public int iAdminLevel { get; set; }
    }
    public class characters
    {
        public string strName { get; set; }
        [Indexed]
        public string strOwnedByAccount { get; set; }
        public string strRace { get; set; }
        public string strGender { get; set; }
        public string strProfession { get; set; }
        public int iLevel { get; set; }
        public string strDateCreatedInGW2 { get; set; }
        public DateTime dateCreatedInWarp { get; set; }
        public DateTime dateLastLogin { get; set; }
        public bool bOnline { get; set; }
        public DateTime dateLastSaved { get; set; }
        public bool bDeleted { get; set; }
    }
    class char_sheets
    {
        public string strOwnedByChar { get; set; }
        public string strOwnedByAccount { get; set; }
        public DateTime dateLastEdit { get; set; }
        public string strName { get; set; }
        public string strDescription { get; set; }
        public string strKnownFor { get; set; }
        public string strAspectsJson { get; set; }

        // PRIMARY KEY (character, account) is created manually.
    }

    class account_sheets
    {
        [PrimaryKey]
        public string strOwnedByAccount { get; set; }
        public DateTime dateLastEdit { get; set; }
        public string strExperience { get; set; }
        public string strFightingStyle { get; set; }
        public string strAdultStatus { get; set; }
        public float fLoreStrictness { get; set; }
        public float fTopic { get; set; }
        public float fStyle { get; set; }
        public float fPostLength { get; set; }
        public string strExamplePost { get; set; }

        // PRIMARY KEY (character, account) is created manually.
    }

    class char_notes
    {
        public string strOwnedByChar { get; set; }
        public string strOwnedByAccount { get; set; }
        public DateTime dateLastEdit { get; set; }
        public string strNotes { get; set; }

        // PRIMARY KEY (character, account) is created manually.
    }

    public class reports
    {
        [PrimaryKey, AutoIncrement]
        //[Column("iId")]
        public int? iId { get; set; }
        public DateTime dateReported { get; set; }
        public string strAccountThatReported { get; set; }
        public string strReportedAccount { get; set; }
        public string strReportedText { get; set; }
        public string strReporterNote { get; set; }
    }

    public class feedback
    {
        [PrimaryKey, AutoIncrement]
        public int? iId { get; set; }
        public DateTime date { get; set; }
        public string strAccountSender { get; set; }
        public string strFeedback { get; set; }
    }

    public class spots
    {
        [PrimaryKey, AutoIncrement]
        public int? iId { get; set; }
        public DateTime dateAdded { get; set; }
        public int iMapID { get; set; }
        public float fX { get; set; }
        public float fY { get; set; }
        public float fZ { get; set; }
        public float fXMap { get; set; }
        public float fYMap { get; set; }
        public bool bApproved { get; set; }
        public string strAuthorAccount { get; set; }
        public string strAuthorCharacter { get; set; }
        public int iLanguage { get; set; }
        public bool bInstanced { get; set; }
        public bool bInterior { get; set; }
        public float fQuality { get; set; }
        public string strTitleEn { get; set; }
        public string strTitleDe { get; set; }
        public string strTitleFr { get; set; }
        public string strTitleEs { get; set; }
        public string strDescEn { get; set; }
        public string strDescDe { get; set; }
        public string strDescFr { get; set; }
        public string strDescEs { get; set; }
        public int iRequiredLevel { get; set; }
        public int iRequiredAddon { get; set; }
        public int iRequiredMount { get; set; }
        public string strRequiredEn { get; set; }
        public string strRequiredDe { get; set; }
        public string strRequiredFr { get; set; }
        public string strRequiredEs { get; set; }

    }

    private void Awake()
    {
        Instance = this;
    }

    public void Connect()
    {
#if UNITY_EDITOR
        string path = Path.Combine(Directory.GetParent(Application.dataPath).FullName, strDatabaseFileName);
#else
        string path = Path.Combine(Application.dataPath, databaseFile);
#endif

        // Creates database file if necessary
        connection = new SQLiteConnection(path);

        // Create tables
        connection.CreateTable<accounts>();
        connection.CreateTable<characters>();
        connection.CreateIndex(nameof(characters), new[] { "strName", "strOwnedByAccount" }, true); // set primary keys
        connection.CreateTable<char_sheets>();
        connection.CreateIndex(nameof(char_sheets), new[] { "strOwnedByChar", "strOwnedByAccount" }, true); // set primary keys
        connection.CreateTable<account_sheets>();
        connection.CreateTable<char_notes>();
        connection.CreateIndex(nameof(char_notes), new[] { "strOwnedByChar", "strOwnedByAccount" }, true); // set primary keys
        connection.CreateTable<reports>();
        connection.CreateTable<feedback>();
        connection.CreateTable<spots>();
    }

    void OnApplicationQuit()
    {
        connection?.Close();
    }

    public bool LoginAndLoadAccount(Player _player)
    {
        if (_player == null )
        {
            Debug.Log($"Server: Player was null during login.");
            return false;
        }
        if (_player.account == null)
        {
            Debug.Log($"Server: Account was null during login.");
            return false;
        }

        Account account = _player.account;

        // try to get account
        accounts accountData = connection.FindWithQuery<accounts>("SELECT * FROM accounts WHERE strID=?", account.accountInfo.id);
        bool bAccountExists = accountData != null;

        if (!bAccountExists)
        {
            GW2APIAccountInfo accInfo = account.accountInfo;
            connection.Insert(new accounts
            {
                strID = accInfo.id,
                strName = accInfo.name,
                strRegion = account.region.ToString(),
                strAccountType = accInfo.access.ToString(),
                dateCreatedInWarp = DateTime.UtcNow,
                dateLastLogin = DateTime.UtcNow,
                iMaxItems = account.iMaxItems,
                bBanned = false,
                dateBannedUntil = DateTime.UtcNow,
                iAdminLevel = 0
            }) ; 
        }
        else
        {
            connection.Execute("UPDATE accounts SET dateLastLogin=?, strAccountType=? WHERE strID=?", 
                DateTime.UtcNow, account.accountInfo.access.ToString(), account.accountInfo.id);

            account.iAdminLevel = accountData.iAdminLevel;
            account.iBanned = accountData.bBanned ? 1 : -1;
        }

        // create empty sheet if necessary
        bool bSheetExists = connection.FindWithQuery<account_sheets>("SELECT * FROM account_sheets WHERE strOwnedByAccount=?",
                                                                    account.accountInfo.id) != null;
        if (!bSheetExists)
            SaveAccountSheet(account.accountInfo.id, new AccountSheet());

        // update characters
        for (int i = 0; i < account.arGW2Characters.Length; i++)
        {
            SaveCharacter(_player, account.arGW2Characters[i], false);
        }

        return true;
    }

    public bool SaveCharacter(Player _player, GW2APICharInfo _charInfo, bool _bOnline, bool useTransaction = true)
    {
        if (useTransaction) connection.BeginTransaction();

        GW2APIAccountInfo _accountInfo = _player.account.accountInfo;

        characters charInfoDB = connection.FindWithQuery<characters>("SELECT * FROM characters WHERE strName=? AND strOwnedByAccount=?",
                        _charInfo.name, _accountInfo.id);
        bool bCharExists = charInfoDB != null;

        if (bCharExists)
        {
            // don't overwrite permanent stuff
            connection.InsertOrReplace(new characters
            {
                strName = _charInfo.name,
                strOwnedByAccount = _accountInfo.id,
                strRace = _charInfo.race.ToString(),
                strGender = _charInfo.gender.ToString(),
                strProfession = _charInfo.profession.ToString(),
                iLevel = _charInfo.level,
                strDateCreatedInGW2 = _charInfo.created,
                //dateCreatedInWarp = DateTime.UtcNow,
                dateLastLogin = DateTime.UtcNow,
                bOnline = _bOnline,
                dateLastSaved = DateTime.UtcNow//,
                //bDeleted = false
            });
        }
        else
        {
            // create blank char
            connection.InsertOrReplace(new characters
            {
                strName = _charInfo.name,
                strOwnedByAccount = _accountInfo.id,
                strRace = _charInfo.race.ToString(),
                strGender = _charInfo.gender.ToString(),
                strProfession = _charInfo.profession.ToString(),
                iLevel = _charInfo.level,
                strDateCreatedInGW2 = _charInfo.created,
                dateCreatedInWarp = DateTime.UtcNow,
                dateLastLogin = DateTime.UtcNow,
                bOnline = _bOnline,
                dateLastSaved = DateTime.UtcNow,
                bDeleted = false
            });
        }

        // create empty sheet if necessary
        bool bSheetExists = connection.FindWithQuery<char_sheets>("SELECT * FROM char_sheets WHERE strOwnedByChar=? AND strOwnedByAccount=?",
                _charInfo.name, _accountInfo.id) != null;
        if (!bSheetExists)
        {
            // set some default values
            _player.Character.charSheet.strRPName = _player.Character.charInfo.name;
            SaveCharSheet(_player.account.accountInfo.id, _charInfo.name, _player.Character.charSheet);
        }

        if (useTransaction) connection.Commit();

        return true;
    }

    public Character charLoad(Character _character, string _strAccID, string _strCharName)
    {
        characters charInfoDB = connection.FindWithQuery<characters>("SELECT * FROM characters WHERE strName=? AND strOwnedByAccount=?",
                        _strCharName, _strAccID);

        GW2APICharInfo charInfo = new GW2APICharInfo(
            _strCharName,
            SuUtility.StrToEnum<GW2APICharInfo.Race>(charInfoDB.strRace),
            SuUtility.StrToEnum<GW2APICharInfo.Gender>(charInfoDB.strGender),
            SuUtility.StrToEnum<GW2APICharInfo.Profession>(charInfoDB.strProfession),
            charInfoDB.iLevel,
            "", // Guild TODO
            0, // age TODO
            charInfoDB.strDateCreatedInGW2); // created Date TODO 

        CharSheet charSheet = charSheetLoad(_strAccID, _strCharName);

        _character.charInfo = charInfo;
        _character.charSheet = charSheet;

        Debug.Log("Loaded character " + _strCharName + " from DB.");

        return _character;
    }

    public bool SaveCharSheet(string _strAccountID, string _strOwnedByChar, CharSheet _sheet)
    {
        System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();

        connection.InsertOrReplace(new char_sheets
        {
            strOwnedByChar = _strOwnedByChar,
            strOwnedByAccount = _strAccountID,
            dateLastEdit = DateTime.UtcNow,
            strName = !string.IsNullOrEmpty(_sheet.strRPName) ? _sheet.strRPName : _strOwnedByChar,
            strDescription = _sheet.strRPDesc,
            strKnownFor = _sheet.strKnownFor,
            strAspectsJson = JsonUtility.ToJson(new AspectList(_sheet.liAspects))
        });

        sw.Stop();
        DatabaseDebug.AddAction("DB: Saved CharSheet", sw.ElapsedMilliseconds, $"{_strOwnedByChar}");

        return true;
    }

    public bool SaveNotes(string _strAccountID, string _strOwnedByChar, string _strNotes)
    {
        connection.InsertOrReplace(new char_notes
        {
            strOwnedByChar = _strOwnedByChar,
            strOwnedByAccount = _strAccountID,
            dateLastEdit = DateTime.UtcNow,
            strNotes = _strNotes
        });

        return true;
    }

    public string strLoadNotes(string _strAccountID, string _strOwnedByChar)
    {
        char_notes charNotes = connection.FindWithQuery<char_notes>("SELECT * FROM char_notes WHERE strOwnedByChar=? AND strOwnedByAccount=?",
                            _strOwnedByChar, _strAccountID);

        if (charNotes == null) // missing entry? create empty one
        {
            SaveNotes(_strAccountID, _strOwnedByChar, ""); 
            return "";
        }

        return charNotes.strNotes;
    }

    public List<string> liLoadAllCharSheetNames(string _strAccID)
    {
        return connection.Table<char_sheets>().Where(entry => entry.strOwnedByAccount == _strAccID)
                .Select(x => x.strOwnedByChar).ToList();
    }
    
    public CharSheet charSheetLoad(string _strAccID, string _strCharName)
    {
        bool bSheetExists = connection.FindWithQuery<char_sheets>("SELECT * FROM char_sheets WHERE strOwnedByChar=? AND strOwnedByAccount=?",
                                                                    _strCharName, _strAccID) != null;

        if (!bSheetExists)
            return null;

        char_sheets sheetInfoDB = connection.FindWithQuery<char_sheets>("SELECT * FROM char_sheets WHERE strOwnedByChar=? AND strOwnedByAccount=?",
                    _strCharName, _strAccID);

        return new CharSheet(sheetInfoDB.strName, sheetInfoDB.strDescription, sheetInfoDB.strKnownFor,
            JsonUtility.FromJson<AspectList>(sheetInfoDB.strAspectsJson).liAspects);
    }

    public bool SaveAccountSheet(string _strAccountID, AccountSheet _sheet)
    {
        connection.InsertOrReplace(new account_sheets
        {
            strOwnedByAccount = _strAccountID,
            dateLastEdit = DateTime.UtcNow,
            strExperience = _sheet.experience.ToString(),
            strFightingStyle = _sheet.fightingStyle.ToString(),
            strAdultStatus = _sheet.adultStatus.ToString(),
            fLoreStrictness = _sheet.fLoreStrictness,
            fTopic = _sheet.fTopic,
            fStyle = _sheet.fStyle,
            fPostLength = _sheet.fPostLength,
            strExamplePost = _sheet.strExamplePost
        });

        return true;
    }

    public AccountSheet accountSheetLoad(string _strAccountID)
    {
        account_sheets sheetInfoDB = connection.FindWithQuery<account_sheets>("SELECT * FROM account_sheets WHERE strOwnedByAccount=?",
                    _strAccountID);

        return new AccountSheet(SuUtility.StrToEnum<AccountSheet.Experience>(sheetInfoDB.strExperience),
            SuUtility.StrToEnum<AccountSheet.FightingStyle>(sheetInfoDB.strFightingStyle),
            SuUtility.StrToEnum<AccountSheet.AdultStatus>(sheetInfoDB.strAdultStatus),
            sheetInfoDB.fLoreStrictness, sheetInfoDB.fTopic, sheetInfoDB.fStyle, sheetInfoDB.fPostLength, sheetInfoDB.strExamplePost);
    }

    public void AddReport(reports _report, bool useTransaction = true)
    {
        if (useTransaction) connection.BeginTransaction();
        connection.Insert(_report);
        if (useTransaction) connection.Commit();
    }

    public void AddFeedback(feedback _feedback, bool useTransaction = true)
    {
        if (useTransaction) connection.BeginTransaction();
        connection.Insert(_feedback);
        if (useTransaction) connection.Commit();
    }

    public characters charactersGetEntry(string _strCharName)
    {
        return connection.FindWithQuery<characters>("SELECT * FROM characters WHERE strName=?", _strCharName);
    }

    public void AddSpot(spots _spot, bool useTransaction = true)
    {
        if (useTransaction) connection.BeginTransaction();
        connection.Insert(_spot);
        if (useTransaction) connection.Commit();
    }

    public spots spotLoad(int _iID)
    {
        spots spot = connection.FindWithQuery<spots>("SELECT * FROM char_sheets WHERE iID=?", _iID);
        if (spot == null) Debug.Log($"Could not find spot with id {_iID}");
        return spot;
    }

    // UTILITY

    public void SetBan(string _strAccountName, bool _bBan)
    {
        connection.Execute("UPDATE accounts SET bBanned=? WHERE strName=?", _bBan, _strAccountName);
    }

    public void SetItemLimit(string _strAccountId, int _iItemLimit)
    {
        connection.Execute("UPDATE accounts SET iMaxItems=? WHERE strID=?", _iItemLimit, _strAccountId);
    }

    public bool bIsCharOwnedByAccount(string _strAccID, string _strCharName)
    {
        return connection.FindWithQuery<characters>("SELECT * FROM characters WHERE strName=? AND strOwnedByAccount=?",
                _strCharName, _strAccID) != null;
    }

    public string strGetPath()
    {
        // database path: Application.dataPath is always relative to the project,
        // but we don't want it inside the Assets folder in the Editor (git etc.),
        // instead we put it above that.
        // we also use Path.Combine for platform independent paths
        // and we need persistentDataPath on android
#if UNITY_EDITOR
        string path = Path.Combine(Directory.GetParent(Application.dataPath).FullName, strDatabaseFileName);
#elif UNITY_ANDROID
        string path = Path.Combine(Application.persistentDataPath, databaseFile);
#elif UNITY_IOS
        string path = Path.Combine(Application.persistentDataPath, databaseFile);
#else
        string path = Path.Combine(Application.dataPath, databaseFile);
#endif

        return path;
    }



    /// <summary>
    /// Helper class, because Unity json cannot directly serialize strings.
    /// </summary>
    [Serializable]
    public class AspectList
    {
        [SerializeField]
        public List<CharSheet.Aspect> liAspects = new List<CharSheet.Aspect>();

        public AspectList(List<CharSheet.Aspect> _liAspects)
        {
            liAspects = _liAspects;
        }
    }
}
