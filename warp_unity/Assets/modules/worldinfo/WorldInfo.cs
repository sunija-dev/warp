using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;

public class WorldInfo : NetworkBehaviour
{
    public static WorldInfo InstanceClient { get { return WorldInfoSpawner.worldInfoGet(); } }

    [SyncVar] public float fMinDistanceForUpdateSquared = 25f;
    public float fUpdateEvery = 3f;

    [SyncVar] public GlobalEnums.Region region;
    [SyncVar] public GlobalEnums.Language language;

    public readonly SyncList<PlayerInfo> sliPlayerInfos = new SyncList<PlayerInfo>(); // could also be a dictionary?


    public struct PlayerInfo
    {
        public string strName;
        public int iMapID;
        public string strIP;
        public Vector3 v3Position;
        public Vector2 v2ContinentPosition;
        public bool bHideInPlayerList;
        public bool bHideOnMap;
        public bool bInCompetitiveGameMode;

        public PlayerInfo(Player _player)
        {
            strName = _player.Character.charInfo.name;
            iMapID = _player.warpNetworkPosition.iMapId;
            strIP = _player.warpNetworkPosition.strIP;
            v3Position = _player.transform.position;
            v2ContinentPosition = _player.warpNetworkPosition.v2ContinentPosition;
            bHideInPlayerList = _player.bHideInPlayerList;
            bHideOnMap = _player.bHideOnMap; 
            bInCompetitiveGameMode = _player.bInCompetitiveGameMode;

            //don't show competitive players
            if (bInCompetitiveGameMode)
            {
                v3Position = Vector3.zero;
                bHideOnMap = true;
            }
        }
    }

    public override void OnStartServer()
    {
        region = GetComponent<WarpNetworkPosition>().region;
        language = GetComponent<WarpNetworkPosition>().language;

        StartCoroutine(coUpdatePlayerInfos());
    }

    [Server]
    public IEnumerator coUpdatePlayerInfos()
    {
        int iDebugCounter = 0;
        int iDebugEveryTimes = 30;
        float fUpdateTimeAccumulated = 0f;

        while (true)
        {
            System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();

            // get all players
            List<Player> liPlayers = ServerManager.s_dictPlayers
                .Where(x => x.Value.account.language == language && x.Value.account.region == region)
                .Select(y => y.Value).ToList();

            foreach (Player player in liPlayers)
            {
                int iPlayerInfo = sliPlayerInfos.FindIndex(x => x.strName == player.Character.charInfo.name);

                if (iPlayerInfo < 0) // not there? add it
                {
                    sliPlayerInfos.Add(new PlayerInfo(player));
                }
                else // changed too much? rebuild it
                {
                    PlayerInfo playerInfoOld = sliPlayerInfos[iPlayerInfo];
                    PlayerInfo playerInfoNew = new PlayerInfo(player);

                    // TODO: this could be a utility function of playerinfo
                    if (playerInfoOld.strIP != playerInfoNew.strIP || playerInfoOld.iMapID != playerInfoNew.iMapID ||
                        SuUtility.SqrMagnitude(playerInfoOld.v3Position - playerInfoNew.v3Position) > fMinDistanceForUpdateSquared
                        || playerInfoOld.bHideInPlayerList != playerInfoNew.bHideInPlayerList 
                        || playerInfoOld.bHideOnMap != playerInfoNew.bHideOnMap)
                    {
                        // update value
                        sliPlayerInfos[iPlayerInfo] = playerInfoNew;
                        
                        //sliPlayerInfos.Remove(playerInfoOld);
                        //sliPlayerInfos.Add(playerInfoNew);
                    }
                }
            }

            // remove players that went offline
            List<PlayerInfo> liPlayersGone = sliPlayerInfos.Where(playerInfo => !liPlayers.Any(player => player.Character.charInfo.name == playerInfo.strName)).ToList();
            foreach (PlayerInfo playerInfo in liPlayersGone)
            {
                sliPlayerInfos.Remove(playerInfo);
            }

            sw.Stop();
            fUpdateTimeAccumulated += sw.ElapsedMilliseconds;
            iDebugCounter++;

            if (iDebugCounter > iDebugEveryTimes)
            {
                DatabaseDebug.AddAction("WorldInfo: Update Player Infos", fUpdateTimeAccumulated / (float)iDebugEveryTimes);
                iDebugCounter = 0;
                fUpdateTimeAccumulated = 0f;
            }

            

            yield return new WaitForSeconds(fUpdateEvery);
        }

        
    }


}
