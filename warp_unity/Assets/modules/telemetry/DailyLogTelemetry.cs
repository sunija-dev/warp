using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

/*
public class DailyLogTelemetry : MonoBehaviour
{
    //public DailyTelemetry m_dailyTelemetry = new DailyTelemetry();
    private System.DateTime dateTimeDailyLog = System.DateTime.Now;

    public void Start()
    {
        StartCoroutine(DailyLogRepeatedly());
    }

    public IEnumerator DailyLogRepeatedly()
    {
        // is it set to 3am?
        if (dateTimeDailyLog.Hour != 3)
        {
            System.DateTime dateNow = System.DateTime.Now;
            dateTimeDailyLog = new System.DateTime(dateNow.Year, dateNow.Month, dateNow.Day, 3, 0, 0);
            //dateTimeDailyLog.AddDays(1);
        }

        // is it time to log?
        if (System.DateTime.Now >= dateTimeDailyLog)
        {
            // LOG
            DailyLog();
            dateTimeDailyLog = dateTimeDailyLog.AddDays(1);
        }

        yield return new WaitForSeconds(60f);
        StartCoroutine(DailyLogRepeatedly());
    }

    public void DailyLog()
    {
        if (m_dailyTelemetry == null)
            m_dailyTelemetry = new DailyTelemetry();

        string strPlayerList = "";
        foreach (string strPlayer in m_dailyTelemetry.liAccountNamesOnline)
        {
            strPlayerList += strPlayer + ", ";
        }
        if (strPlayerList.Length > 2)
            strPlayerList.Remove(strPlayerList.Length - 2);

        string strDailyLog = "Log from " + System.DateTime.Now.ToString("dddd, yyyy MM dd HH:mm:ss");
        strDailyLog += string.Format("\nNew Accounts: {0}", m_dailyTelemetry.iNewAccounts);
        strDailyLog += string.Format("\nPlayers online: {0} ({1} at same time)", m_dailyTelemetry.liAccountNamesOnline.Count, m_dailyTelemetry.iLoggedInAtSameTime);
        strDailyLog += string.Format("\nBiggest Group: {0}", m_dailyTelemetry.iBiggestGroup);
        strDailyLog += string.Format("\nPlayer List: {0}", strPlayerList);
        strDailyLog += "\n\n";

        // write to file
        string strDatabasePath = Database.Instance.strGetPath();
        string strDailyLogPath = Path.GetDirectoryName(strDatabasePath) + "/../daily_log.txt";

        if (!File.Exists(strDailyLogPath))
            File.WriteAllText(strDailyLogPath, "");

        using (StreamWriter sw = File.AppendText(strDailyLogPath))
        {
            sw.WriteLine(strDailyLog);
        }

        StatisticsGlobal.Instance.ResetStatistics(); // TODO: the global statistics should maybe reset themselves
        //m_dailyTelemetry = new DailyTelemetry();
    }


    [System.Serializable]
    public class DailyTelemetry
    {
        public List<string> liAccountNamesOnline = new List<string>();
        public int iLoggedInAtSameTime = 0;
        public int iBiggestGroup = 0;
        public int iNewAccounts = 0;
    }
}
*/
