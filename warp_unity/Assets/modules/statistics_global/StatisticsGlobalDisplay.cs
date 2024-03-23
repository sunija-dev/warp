using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;

public class StatisticsGlobalDisplay : MonoBehaviour
{
    public GameObject goEntryPrefab;
    public Transform transContainer;

    private List<GameObject> liEntries = new List<GameObject>();
    private float m_fUpdateTimer = 0f;
    private float m_fUpdateEvery = 3f;

    private void Update()
    {
        m_fUpdateTimer += Time.deltaTime;
        if (m_fUpdateEvery < m_fUpdateTimer)
        {
            m_fUpdateTimer = 0f;
            Rebuild();
        }
    }

    void Rebuild()
    {
        foreach (GameObject goEntry in liEntries)
            Destroy(goEntry);
        liEntries.Clear();

        // yours should be first
        if (StatisticsGlobal.Instance.sliPlayGroupInfosPublic.Count > 0)
        {
            StatisticsGlobal.PlayGroupInfoPublic playGroupInfoForMe = StatisticsGlobal.Instance.sliPlayGroupInfosPublic.FirstOrDefault(x => x.language == Player.Instance.account.language && x.region == Player.Instance.account.region);
            Spawn(playGroupInfoForMe);
        }

        foreach (StatisticsGlobal.PlayGroupInfoPublic playGroupInfo in StatisticsGlobal.Instance.sliPlayGroupInfosPublic)
        {
            if ((playGroupInfo.region == Player.Instance.account.region && playGroupInfo.language == Player.Instance.account.language)
                || (playGroupInfo.iPlayersOnline == 0 && playGroupInfo.iPlayersOnlineToday == 0))
                continue;
            else
                Spawn(playGroupInfo);
        }
    }

    private void Spawn(StatisticsGlobal.PlayGroupInfoPublic _playGroupInfo)
    {
        GameObject goNewEntry = Instantiate(goEntryPrefab, transContainer);
        liEntries.Add(goNewEntry);
        goNewEntry.GetComponent<TMP_Text>().text = $"{_playGroupInfo.language}-{_playGroupInfo.region}: " +
            $"{_playGroupInfo.iPlayersOnline} ({_playGroupInfo.iPlayersOnlineToday})";
    }
}
