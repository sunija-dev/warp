using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using Newtonsoft.Json;

public class MapManager : MonoBehaviour
{
    public static MapManager Instance;

    public MumbleManager mumbleManager;
    public GameObject goMapMarkerCharPrefab;
    public Canvas canvasMap;

    public bool bShowDebug = false;

    private List<MapMarkerChar> m_liMapMarkersChars = new List<MapMarkerChar>();

    // wikimap
    public bool bShowWikiMap = false;
    public bool bShowWikiButton = false;
    public GameObject goWikiButton;
    private List<MapMarkerWiki> m_liMapMarkerWikis = new List<MapMarkerWiki>();
    private WikiMap.WikiMapDB wikiDB;
    public GameObject goMapMarkerWikiPrefab;
    private bool bWikiLoaded = false;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        mumbleManager.eventMapOpened.AddListener(OnMapOpened);
        mumbleManager.eventMapClosed.AddListener(OnMapClosed);

        RebuildWikiMarkers();
    }

    public void RebuildWikiMarkers()
    {
        foreach (MapMarkerWiki goMapMarker in m_liMapMarkerWikis)
            Destroy(goMapMarker.gameObject);
        m_liMapMarkerWikis.Clear();

        if (!bShowWikiMap)
            return;

        if (!bWikiLoaded)
        {
            wikiDB = WikiMap.wikidbLoadFromCache();
            wikiDB.IncreaseReadability();
            bWikiLoaded = true;
        }

        foreach (WikiMap.Region region in wikiDB.liRegions)
            SpawnWikiMarker(region);
        foreach (WikiMap.Map map in wikiDB.liMaps)
            SpawnWikiMarker(map);
        foreach (WikiMap.PointOfInterest POI in wikiDB.liPOIs)
            SpawnWikiMarker(POI);
    }

    private void SpawnWikiMarker(WikiMap.WikiMapEntry _entry)
    {
        if (string.IsNullOrEmpty(_entry.strWikiText.Replace("\n", "")))
            return;

        GameObject goMapMarkerNew = Instantiate(goMapMarkerWikiPrefab, canvasMap.transform);
        MapMarkerWiki mapMarkerWiki = goMapMarkerNew.GetComponent<MapMarkerWiki>();
        m_liMapMarkerWikis.Add(mapMarkerWiki);
        mapMarkerWiki.UpdateData(_entry);
    }

    void OnMapOpened()
    {
        if (WorldInfo.InstanceClient == null) // not generated at very start
            return;

        Rebuild();
        canvasMap.gameObject.SetActive(true);

        WorldInfo.InstanceClient.sliPlayerInfos.Callback += OnPlayerInfosUpdated;

        goWikiButton.SetActive(bShowWikiButton);
    }

    void OnMapClosed()
    {
        if (WorldInfo.InstanceClient == null) // not generated at very start
            return;

        canvasMap.gameObject.SetActive(false);

        if (canvasMap.gameObject.activeSelf) // HACK: event might not be added, if map was open before worldinfo was there
            WorldInfo.InstanceClient.sliPlayerInfos.Callback -= OnPlayerInfosUpdated;
    }

    public void ToggleWikiMap()
    {
        bShowWikiMap = !bShowWikiMap;
        RebuildWikiMarkers();
    }

    private void Rebuild()
    {
        //Debug.Log("Rebuilding map.");
        foreach (MapMarkerChar goMapMarker in m_liMapMarkersChars)
            Destroy(goMapMarker.gameObject);
        m_liMapMarkersChars.Clear();

        foreach (WorldInfo.PlayerInfo playerInfo in WorldInfo.InstanceClient.sliPlayerInfos)
        {
            if (!playerInfo.bHideOnMap)
                SpawnMapMarker(playerInfo);
        }
    }

    private void OnPlayerInfosUpdated(SyncList<WorldInfo.PlayerInfo>.Operation _operation, int _iIndex,
        WorldInfo.PlayerInfo _playerInfoOld, WorldInfo.PlayerInfo _playerInfoNew)
    {
        switch (_operation)
        {
            case SyncList<WorldInfo.PlayerInfo>.Operation.OP_ADD:
                if (!_playerInfoNew.bHideOnMap)
                    SpawnMapMarker(_playerInfoNew);
                break;
            case SyncList<WorldInfo.PlayerInfo>.Operation.OP_CLEAR:
                Rebuild();
                break;
            case SyncList<WorldInfo.PlayerInfo>.Operation.OP_INSERT:
                SpawnMapMarker(_playerInfoNew);
                break;
            case SyncList<WorldInfo.PlayerInfo>.Operation.OP_REMOVEAT:
                // index is where it got removed in the list
                // oldItem is the item that was removed
                RemoveMapMarker(_playerInfoOld);
                break;
            case SyncList<WorldInfo.PlayerInfo>.Operation.OP_SET:
                int iIndexSet = m_liMapMarkersChars.FindIndex(x => x.playerInfo.strName == _playerInfoOld.strName);
                if (iIndexSet >= 0)
                    m_liMapMarkersChars[iIndexSet].UpdateData(_playerInfoNew);

                // if hide option was changed, remove/spawn if necessary
                if (!_playerInfoOld.bHideOnMap && _playerInfoNew.bHideOnMap)
                    RemoveMapMarker(_playerInfoNew);
                else if (_playerInfoOld.bHideOnMap && !_playerInfoNew.bHideOnMap)
                    SpawnMapMarker(_playerInfoNew);
                break;
        }
    }

    public void RemoveMapMarker(WorldInfo.PlayerInfo _playerInfo)
    {
        int iIndex = m_liMapMarkersChars.FindIndex(x => x.playerInfo.strName == _playerInfo.strName);
        if (iIndex < 0)
            return;

        Destroy(m_liMapMarkersChars[iIndex].gameObject);
        m_liMapMarkersChars.RemoveAt(iIndex);
    }

    public void SpawnMapMarker(WorldInfo.PlayerInfo _playerInfo)
    {
        GameObject goMapMarkerNew = Instantiate(goMapMarkerCharPrefab, canvasMap.transform);
        MapMarkerChar mapMarkerChar = goMapMarkerNew.GetComponent<MapMarkerChar>();
        m_liMapMarkersChars.Add(mapMarkerChar);
        mapMarkerChar.UpdateData(_playerInfo);
    }
}
