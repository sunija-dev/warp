
using SQLite; // from https://github.com/praeclarum/sqlite-net
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;
using Mirror;

public class DatabaseDebug : MonoBehaviour
{
    public static DatabaseDebug Instance;

    // file name
    public string databaseFile = "Database_debug.sqlite";

    public float fFrameTimesEvery = 60f;
    public float fOnlinePlayersEvery = 60f;

    // connection
    SQLiteConnection connection;

    private List<float> liFrameTimes = new List<float>();
    private float fFrameTimeTimer = 0f;
    private float fOnlinePlayersTimer = 0f;

    public class frame_times
    {
        [PrimaryKey, AutoIncrement]
        //[Column("iId")]
        public int? iId { get; set; }
        public DateTime dateTimestamp { get; set; }
        public int iMin { get; set; }
        public int iMax { get; set; }
        public int iAvg { get; set; }
        public int iMedian { get; set; }
        public int iUpper10Percent { get; set; }
    }

    public class online_players
    {
        [PrimaryKey, AutoIncrement]
        //[Column("iId")]
        public int? iId { get; set; }
        public DateTime dateTimestamp { get; set; }
        public int iTotal { get; set; }
        public string strByRegion { get; set; }
    }

    public class actions
    {
        [PrimaryKey, AutoIncrement]
        //[Column("iId")]
        public int? iId { get; set; }
        public DateTime dateTimestamp { get; set; }
        public string strName { get; set; }
        public float fTime { get; set; }
        public string strInfo { get; set; }
    }

    public void Connect()
    {
        // database path: Application.dataPath is always relative to the project,
        // but we don't want it inside the Assets folder in the Editor (git etc.),
        // instead we put it above that.
        // we also use Path.Combine for platform independent paths
        // and we need persistentDataPath on android
#if UNITY_EDITOR
        string path = Path.Combine(Directory.GetParent(Application.dataPath).FullName, databaseFile);
#elif UNITY_ANDROID
        string path = Path.Combine(Application.persistentDataPath, databaseFile);
#elif UNITY_IOS
        string path = Path.Combine(Application.persistentDataPath, databaseFile);
#else
        string path = Path.Combine(Application.dataPath, databaseFile);
#endif

        // open connection
        // note: automatically creates database file if not created yet
        connection = new SQLiteConnection(path);

        // create tables if they don't exist yet or were deleted
        connection.CreateTable<frame_times>();
        connection.CreateTable<online_players>();
        connection.CreateTable<actions>();

        StartCoroutine(coTrackFrameTimesRepeatedly());
        StartCoroutine(coTrackOnlinePlayersRepeatedly());
    }

    private void Awake()
    {
        Instance = this;
    }

    // close connection when Unity closes to prevent locking
    void OnApplicationQuit()
    {
        connection?.Close();
    }

    private System.Collections.IEnumerator coTrackOnlinePlayersRepeatedly()
    {
        while (true)
        {
            fOnlinePlayersTimer += Time.deltaTime;

            if (fOnlinePlayersTimer > fOnlinePlayersEvery)
            {
                online_players onlinePlayersLine = new online_players();
                onlinePlayersLine.dateTimestamp = DateTime.UtcNow;
                onlinePlayersLine.iTotal = ServerManager.s_dictPlayers.Count();

                string strRegionInfo = "";
                foreach (StatisticsGlobal.PlayGroupInfoPublic playGroupInfo in StatisticsGlobal.Instance.sliPlayGroupInfosPublic)
                {
                    if (playGroupInfo.region == GlobalEnums.Region.NA &&
                        (playGroupInfo.language == GlobalEnums.Language.de ||
                         playGroupInfo.language == GlobalEnums.Language.fr ||
                         playGroupInfo.language == GlobalEnums.Language.es))
                        continue;
                    else
                        strRegionInfo += $"{playGroupInfo.language}-{playGroupInfo.region}: {playGroupInfo.iPlayersOnline}, ";
                }

                onlinePlayersLine.strByRegion = strRegionInfo;

                connection.BeginTransaction();
                connection.Insert(onlinePlayersLine);
                connection.Commit();

                fOnlinePlayersTimer = 0f;
            }

            yield return null;
        }
    }

    private System.Collections.IEnumerator coTrackFrameTimesRepeatedly()
    {
        while (true)
        {
            liFrameTimes.Add(Time.deltaTime);
            fFrameTimeTimer += Time.deltaTime;

            if (fFrameTimeTimer > fFrameTimesEvery)
            {
                liFrameTimes = liFrameTimes.OrderBy(f => f).ToList(); // sort
                frame_times frameTimesLine = new frame_times();
                frameTimesLine.dateTimestamp = DateTime.UtcNow;
                frameTimesLine.iMin = Mathf.RoundToInt(liFrameTimes.Min() * 1000f);
                frameTimesLine.iMax = Mathf.RoundToInt(liFrameTimes.Max() * 1000f);
                frameTimesLine.iAvg = Mathf.RoundToInt(liFrameTimes.Average() * 1000f);
                frameTimesLine.iMedian = Mathf.RoundToInt(liFrameTimes[liFrameTimes.Count / 2] * 1000f);
                frameTimesLine.iUpper10Percent = Mathf.RoundToInt(liFrameTimes.Skip(Math.Max(0, liFrameTimes.Count - (int)(liFrameTimes.Count * 0.1f))).Average() * 1000f);
                connection.BeginTransaction();
                connection.Insert(frameTimesLine);
                connection.Commit();

                fFrameTimeTimer = 0f;
                liFrameTimes.Clear();
            }

            yield return null;
        }
    }

    private float fRoundTo3PlacesBehindComma(float _fValue)
    {
        return (Mathf.RoundToInt(_fValue * 1000) / 1000f);
    }

    public static void AddAction(string _strName, float _fTime, string _strInfo = "")
    {
        Instance.connection.Insert(new actions() { dateTimestamp = DateTime.UtcNow, strName = _strName, fTime = _fTime, strInfo = _strInfo });
    }

    // System.GC.GetTotalMemory()
}
