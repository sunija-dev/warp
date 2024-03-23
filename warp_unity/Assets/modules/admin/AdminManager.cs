using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Linq;

public class AdminManager : NetworkBehaviour
{
    public static AdminManager Instance;

    [Header("References")]
    public ServerManager serverManager;

    private void Awake()
    {
        Instance = this;
    }

    [Server]
    public void AdminCommand(NetworkConnection _conn, string _strCommand)
    {
        /*
        [pw] setban [accid] [0/1]
        [pw] itemlimit [accname] [maxitem]
        [pw] deleteitem [itemid]
        [pw] message “[the message]”

        */

        string[] arCommand = _strCommand.Split(' ');
        Account accountRequesting = ServerManager.s_dictPlayers[_conn].account;

        Debug.Log(string.Format("{0} admin command: {1}", accountRequesting.accountInfo.name, _strCommand));

        // check password
        if (arCommand[0] != serverManager.m_settings.strPassword)
        {
            Debug.Log(string.Format("WARNING! AdminCommand tried with wrong password.\nPW: {0}\nCommand: {1}", arCommand[0], _strCommand));
            return;
        }

        if (accountRequesting.iAdminLevel <= 1)
        {
            Debug.Log(string.Format("WARNING! AdminCommand without admin account.\nPW: {0}\nCommand: {1}", arCommand[0], _strCommand));
            return;
        }

        switch (arCommand[1])
        {
            case "setban":
                SetBan(arCommand[1], arCommand[2] == "1" ? true : false);
                break;
            case "message":
                string strMessage = _strCommand.Split('"')[1];
                RpcServerwideMessage(strMessage);
                break;
            default:
                TargetMessage(_conn, "Did not recognize command " + arCommand[0] + ".");
                break;
        }
    }

    [Server]
    private void SetBan(string _strAccountName, bool _bBan)
    {
        Database.Instance.SetBan(_strAccountName, _bBan);
        KickPlayer(_strAccountName);
    }

    [Server]
    private void KickPlayer(string _strAccountName)
    {
        NetworkConnection connBanned = ServerManager.s_dictPlayers.FirstOrDefault(x => x.Value.account.accountInfo.name == _strAccountName).Key;

        if (connBanned == default)
            return;

        connBanned?.Disconnect();
    }

    [ClientRpc]
    public void RpcServerwideMessage(string _strMessage)
    {
        Debug.Log("Serverwide Message: " + _strMessage);
        ClientManager.Instance.windowPopup.Init(
            new WindowPopup.ButtonInfo(true, null, "Ok"),
            new WindowPopup.ButtonInfo(false, null, ""),
            _strMessage);
    }

    [TargetRpc]
    public void TargetMessage(NetworkConnection _conn, string _strMessage)
    {
        Debug.Log(_strMessage);
        ClientManager.Instance.windowPopup.Init(
            new WindowPopup.ButtonInfo(true, null, "Ok"),
            new WindowPopup.ButtonInfo(false, null, ""),
            _strMessage);
    }

}
