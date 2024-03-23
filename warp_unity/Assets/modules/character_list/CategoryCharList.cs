using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CategoryCharList : MonoBehaviour
{
    public GameObject goEntryPrefab;
    public Transform transListContent;

    private List<GameObject> liEntries = new List<GameObject>();
    private WorldInfo m_worldInfo;

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

    public void Rebuild()
    {
        foreach (GameObject goEntry in liEntries)
            Destroy(goEntry);
        liEntries.Clear();

        m_worldInfo = WorldInfoSpawner.worldInfoGet(Player.Instance.account.region, Player.Instance.account.language);
        Player player = Player.Instance;

        // sort list
        List<WorldInfo.PlayerInfo> liPlayerInfos = new List<WorldInfo.PlayerInfo>();
        foreach (WorldInfo.PlayerInfo playerInfo in m_worldInfo.sliPlayerInfos)
            liPlayerInfos.Add(playerInfo);
        liPlayerInfos = liPlayerInfos.Where(x => !x.bHideInPlayerList).ToList();
        liPlayerInfos = liPlayerInfos.OrderByDescending(x => x.strIP)
                                    .GroupBy(x => x.iMapID)
                                    .OrderByDescending(g => g.Count())
                                    .SelectMany(g => g).ToList();

        // spawn + fill entries (button only if same map)
        int iEntry = 0;
        foreach (WorldInfo.PlayerInfo playerInfo in liPlayerInfos)
        {
            GameObject goEntryNew = Instantiate(goEntryPrefab, transListContent);
            CharListEntry charListEntry = goEntryNew.GetComponent<CharListEntry>();
            charListEntry.textName.text = playerInfo.strName;
            charListEntry.textMap.text = MapNames.strGetName(playerInfo.iMapID);
            charListEntry.textIP.text = playerInfo.strIP;
            if (iEntry % 2 != 0)
                goEntryNew.GetComponent<UnityEngine.UI.Image>().enabled = false;

            if (MumbleManager.Instance.iMapID == playerInfo.iMapID
                && player.strGetIPAddress() != playerInfo.strIP // DEBUG!
                && player.Character.charInfo.name != playerInfo.strName // DEBUG!
                )
            {
                string strPlayerName = playerInfo.strName;
                charListEntry.buttonRequestIPTaxi.gameObject.SetActive(true);
                charListEntry.buttonRequestIPTaxi.onClick.AddListener(() => { IpChangeManagerPlayer.InstanceClient.RequestIpChange(strPlayerName); });
            }
            else
            {
                charListEntry.buttonRequestIPTaxi.gameObject.SetActive(false);
            }

            liEntries.Add(goEntryNew);
            iEntry++;
        }
    }
}
