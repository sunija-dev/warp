using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

/// <summary>
/// Everything about a NetworkIdentity, except its position.
/// </summary>
[System.Serializable]
public class WarpNetworkPosition : NetworkBehaviour
{
    // TODO: Need to hook those and attach to events!

    /// <summary>
    /// Set true, to share it to everyone in the same region/language.
    /// </summary>
    public bool bIsGlobal = false;

    /// <summary>
    /// Set true, to share it to ignore language/region.
    /// </summary>
    public bool bAllRegionsLanguages = false;

    [Command] public void CmdUpdateContinentPosition(Vector2 _v2ContinentPosition) { v2ContinentPosition = _v2ContinentPosition; }
    [SyncVar] public Vector2 v2ContinentPosition = Vector2.zero;

    [Command] public void CmdUpdateRegion(GlobalEnums.Region _value) { region = _value; }
    [SyncVar] public GlobalEnums.Region region = default;

    [Command] public void CmdUpdateLanguage(GlobalEnums.Language _value) { language = _value; }
    [SyncVar] public GlobalEnums.Language language = default;

    [Command] public void CmdUpdateMapId(int _value) { iMapId = _value; }
    [SyncVar] public int iMapId = -1;

    [Command] public void CmdUpdateIP(string _value) { strIP = _value; }
    [SyncVar] public string strIP = "";


    public float fDistanceTo(WarpNetworkPosition _warpPositionOther)
    {
        if (region != _warpPositionOther.region
            || language != _warpPositionOther.language
            || iMapId != _warpPositionOther.iMapId
            || strIP != _warpPositionOther.strIP)
            return float.PositiveInfinity;
        else
            return Vector3.Distance(transform.position, _warpPositionOther.transform.position);
    }

    /*
    private void SyncContextChanged(WarpNetworkPosition _contextOld, WarpNetworkPosition _contextNew)
    {
        if (!isLocalPlayer) return;
        ClientManager.Instance.eCharContextChanged.Invoke();
        Debug.Log("Client: CharContextChanged: " + _contextNew);
    }
    */
}
