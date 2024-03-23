using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;

/// <summary>
/// Should be placed on the player, so the player has authority.
/// </summary>
public class IpChangeManagerPlayer : NetworkBehaviour
{
    public Player player;
    public static IpChangeManagerPlayer InstanceClient;

    private void Start()
    {
        if (isLocalPlayer)
            InstanceClient = Player.Instance.GetComponent<IpChangeManagerPlayer>(); // = this?
    }

    public void RequestIpChange(string _strPlayerRequested)
    {
        CmdRequestIPChange(_strPlayerRequested);
        ClientManager.Instance.windowPopup.Init(new WindowPopup.ButtonInfo(true, null, "ok"),
                                new WindowPopup.ButtonInfo(false, null, ""), 
                                "taxi_ask", _strPlayerRequested);
    }

    [Command]
    public void CmdRequestIPChange(string _strPlayerRequested)
    {
        Debug.Log($"IP change requested for {_strPlayerRequested}");
        NetworkConnection connPlayerRequested = ServerManager.connByCharName(_strPlayerRequested);
        if (connPlayerRequested == null)
        {
            player.TargetError(false, Player.ErrorType.NULL, "taxi_offline", _strPlayerRequested);
            return;
        }
        
        Debug.Log($"IP change is requested by {ServerManager.s_dictPlayers[connectionToClient].Character.name} for {ServerManager.s_dictPlayers[connPlayerRequested].Character.name}");

        TargetShowIPRequest(connPlayerRequested, player.strGetCharName());

        // backup method
        ServerManager.s_dictPlayers[connPlayerRequested].GetComponent<IpChangeManagerPlayer>().TargetShowIPRequest(connPlayerRequested, player.strGetCharName());
    }

    // Requested
    [TargetRpc]
    public void TargetShowIPRequest(NetworkConnection _connTarget, string _strPlayerRequesting)
    {
        // TODO: Localization!
        Debug.Log("Target Show IP Request");
        ClientManager.Instance.windowPopup.Init(
                                new WindowPopup.ButtonInfo(true, () => InstanceClient.Agree(_strPlayerRequesting), "ok"),
                                new WindowPopup.ButtonInfo(true, () => InstanceClient.CmdDecline(_strPlayerRequesting, Player.Instance.Character.CharInfo.name), "decline"),
                                "taxi_request", _strPlayerRequesting);
    }

    // Requested
    public void Agree(string _strPlayerRequesting)
    {
        Debug.Log("Agreed");
        StartCoroutine(coAgree(_strPlayerRequesting));
    }

    private IEnumerator coAgree(string _strPlayerRequesting)
    {
        GameIntegration.Instance.FocusGw2();
        yield return new WaitUntil(() => GameIntegration.Instance.m_bGw2HasFocus);
        GameIntegration.Instance.Chat.Send(string.Format("/invite {0}", _strPlayerRequesting));
        CmdAgree(_strPlayerRequesting, Player.Instance.Character.CharInfo.name);

    }

    [Command]
    public void CmdAgree(string _strPlayerRequesting, string _strPlayerRequested)
    {
        NetworkConnection conn = ServerManager.connByCharName(_strPlayerRequesting);
        if (conn != default)
            TargetAnswerToRequest(conn, _strPlayerRequested, true);
    }

    // Requested
    [Command]
    public void CmdDecline(string _strPlayerName, string _strPlayerRequested)
    {
        NetworkConnection conn = ServerManager.connByCharName(_strPlayerName);
        if (conn != default)
            TargetAnswerToRequest(conn, _strPlayerRequested, false);
    }

    // Requester
    [TargetRpc]
    public void TargetAnswerToRequest(NetworkConnection _connTarget, string _strPlayerName, bool _bAccepted)
    {
        if (_bAccepted)
        {
            ClientManager.Instance.windowPopup.Init(
                                    new WindowPopup.ButtonInfo(true, null, "ok"),
                                    new WindowPopup.ButtonInfo(false, null, ""),
                                    "taxi_accepted", _strPlayerName);
        }
        else
        {
            ClientManager.Instance.windowPopup.Init(
                                        new WindowPopup.ButtonInfo(true, null, "ok"),
                                        new WindowPopup.ButtonInfo(false, null, ""),
                                        "taxi_declined", _strPlayerName);
        }
        
    }
}
