using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;

/// <summary>
/// Spawns worldinfos for every region/language combination.
/// </summary>
public class WorldInfoSpawner : NetworkBehaviour
{
    public GameObject goWorldInfoPrefab;

    private static List<WorldInfo> liWorldInfos = new List<WorldInfo>();

    public override void OnStartServer()
    {
        // spawns worldinfos for every region/language
        // so players get the info synced that they need
        IEnumerable<GlobalEnums.Region> ieRegions = SuUtility.GetEnumValues<GlobalEnums.Region>();
        IEnumerable<GlobalEnums.Language> ieLanguages = SuUtility.GetEnumValues<GlobalEnums.Language>();

        foreach (GlobalEnums.Region region in ieRegions)
        {
            foreach (GlobalEnums.Language language in ieLanguages)
            {
                GameObject goWorldInfo = Instantiate(goWorldInfoPrefab, this.transform);
                liWorldInfos.Add(goWorldInfo.GetComponent<WorldInfo>());
                WarpNetworkPosition netPos = goWorldInfo.GetComponent<WarpNetworkPosition>();
                netPos.language = language;
                netPos.region = region;
                NetworkServer.Spawn(goWorldInfo);
            }
        }
    }

    public static WorldInfo worldInfoGet()
    {
        if (Player.Instance == null)
            return new WorldInfo();

        return worldInfoGet(Player.Instance.account.region, Player.Instance.account.language);
    }

    public static WorldInfo worldInfoGet(GlobalEnums.Region _region, GlobalEnums.Language _language)
    {
        if (liWorldInfos.Count == 0)
            liWorldInfos = FindObjectsOfType<WorldInfo>().ToList();

        WorldInfo worldInfo = liWorldInfos.FirstOrDefault(x => x.region == _region
                                    && x.language == _language);

        //if (worldInfo == default)
        //    worldInfo = liWorldInfos[0];

        return worldInfo;
    }


}
