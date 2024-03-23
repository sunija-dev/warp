using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;

public class StatisticsGlobal : NetworkBehaviour
{
    private static StatisticsGlobal m_instance;
    public static StatisticsGlobal Instance // normal instance sometimes wouldn't be available in build
    { 
        get 
        {
            if (m_instance == null) m_instance = FindObjectOfType<StatisticsGlobal>();
            return m_instance;  
        }
    }

    public float fGroupDistance = 30f;

    public readonly SyncList<PlayGroupInfoPublic> sliPlayGroupInfosPublic = new SyncList<PlayGroupInfoPublic>();

    private List<PlayGroupInfo> liPlayGroups = new List<PlayGroupInfo>();

    private IEnumerable<GlobalEnums.Region> ieRegions = SuUtility.GetEnumValues<GlobalEnums.Region>();
    private IEnumerable<GlobalEnums.Language> ieLanguages = SuUtility.GetEnumValues<GlobalEnums.Language>();

    public class PlayGroupInfo
    {
        public GlobalEnums.Region region;
        public GlobalEnums.Language language;
        public List<Account> liAccountsOnlineToday = new List<Account>();
        public int iBiggestGroup = 0;
        public int iPlayersOnlineAtSameTime = 0;

        public void Update()
        {
            List<Player> liPlayers = ServerManager.s_dictPlayers
                .Where(x => x.Value.account.language == language && x.Value.account.region == region)
                .Select(y => y.Value).ToList();

            iPlayersOnlineAtSameTime = Mathf.Max(iPlayersOnlineAtSameTime, liPlayers.Count);

            foreach (Player player in liPlayers)
            {
                // account online
                if (!liAccountsOnlineToday.Any(x => x.accountInfo.id == player.account.accountInfo.id))
                    liAccountsOnlineToday.Add(player.account);

                // biggest group
                int iGroup = 0;
                foreach (Player playerOther in liPlayers)
                {
                    if (player.GetComponent<WarpNetworkPosition>().fDistanceTo(playerOther.GetComponent<WarpNetworkPosition>()) < Instance.fGroupDistance)
                        iGroup++;
                }
                iBiggestGroup = Mathf.Max(iBiggestGroup, iGroup);
            }
        }
    }

    public struct PlayGroupInfoPublic
    {
        public GlobalEnums.Region region;
        public GlobalEnums.Language language;
        public int iPlayersOnline;
        public int iPlayersOnlineToday;

        public PlayGroupInfoPublic(GlobalEnums.Region _region, GlobalEnums.Language _language)
        {
            region = _region;
            language = _language;
            WorldInfo worldInfo = WorldInfoSpawner.worldInfoGet(_region, _language);
            PlayGroupInfo playGroupInfo = Instance.liPlayGroups.First(x => x.region == _region && x.language == _language);
            iPlayersOnline = worldInfo.sliPlayerInfos.Count;
            iPlayersOnlineToday = playGroupInfo.liAccountsOnlineToday.Count();
        }
    }

    private void Start()
    {
        m_instance = this;

        if (!isServer) return;

        ResetStatistics();
        InvokeRepeating(nameof(UpdatePlayGroups), 2f, 5f);

        StartCoroutine(coLogRepeatedly());
    }

    private void ResetStatistics()
    {
        liPlayGroups.Clear();
        sliPlayGroupInfosPublic.Clear();
        foreach (GlobalEnums.Region region in ieRegions)
        {
            foreach (GlobalEnums.Language language in ieLanguages)
            {
                liPlayGroups.Add(new PlayGroupInfo() { region = region, language = language });
                sliPlayGroupInfosPublic.Add(new PlayGroupInfoPublic(region, language));
            }
        }
    }

    void UpdatePlayGroups()
    {
        System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();

        sliPlayGroupInfosPublic.Clear();
        foreach (PlayGroupInfo playGroupInfo in liPlayGroups)
        {
            playGroupInfo.Update();
            sliPlayGroupInfosPublic.Add(new PlayGroupInfoPublic(playGroupInfo.region, playGroupInfo.language));
        }

        sw.Stop();
        DatabaseDebug.AddAction("StatisticsGlobal: Update PlayGroups", sw.ElapsedMilliseconds);
    }

    IEnumerator coLogRepeatedly()
    {
        WriteToLog();
        ResetStatistics();

        // log every day at utc 3 am
        System.DateTime dateNow = System.DateTime.UtcNow;
        Vector3 v3CloseTime = ServerManager.Instance.m_settings.v3CloseTime;
        System.DateTime dateTimeDailyLog = new System.DateTime(dateNow.Year, dateNow.Month, dateNow.Day, (int)v3CloseTime.x, (int)v3CloseTime.y, (int)v3CloseTime.z);
        dateTimeDailyLog = dateTimeDailyLog.AddDays(1);
        yield return new WaitUntil(() => System.DateTime.UtcNow > dateTimeDailyLog);

        StartCoroutine(coLogRepeatedly());
    }

    private void WriteToLog()
    {
        // en-NA: Online:  12 / Same Time:  6 / Group:  6
        // others...
        // en-NA: accountnames

        string strDailyLog = $"Log from {System.DateTime.Now.ToString("dddd, yyyy MM dd HH:mm:ss")}\n";

        // login numbers
        foreach (PlayGroupInfo playGroupInfo in liPlayGroups)
        {
            strDailyLog += $"{playGroupInfo.language}-{playGroupInfo.region}: Online: {playGroupInfo.liAccountsOnlineToday.Count,5:###} / " +
                $"Same Time: {playGroupInfo.iPlayersOnlineAtSameTime,5:###} / Group: {playGroupInfo.iBiggestGroup,5:###}\n";
        }
        strDailyLog += "\n";

        // accountnames
        foreach (PlayGroupInfo playGroupInfo in liPlayGroups)
        {
            string strPlayerList = "";
            foreach (string strPlayer in playGroupInfo.liAccountsOnlineToday.Select(x => x.accountInfo.name))
                strPlayerList += strPlayer + ", ";
            if (strPlayerList.Length > 2)
                strPlayerList.Remove(strPlayerList.Length - 2);

            strDailyLog += $"{playGroupInfo.language}-{playGroupInfo.region}: {strPlayerList}\n";
        }

        strDailyLog += "\n\n";

        // write to file
        string strDatabasePath = Database.Instance.strGetPath();
        string strDailyLogPath = System.IO.Path.GetDirectoryName(strDatabasePath) + "/../daily_log.txt";

        if (!System.IO.File.Exists(strDailyLogPath))
            System.IO.File.WriteAllText(strDailyLogPath, "");

        using (System.IO.StreamWriter streamWriter = System.IO.File.AppendText(strDailyLogPath))
        {
            streamWriter.WriteLine(strDailyLog);
        }
    }
}
