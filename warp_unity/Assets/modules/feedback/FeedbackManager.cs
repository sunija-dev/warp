using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class FeedbackManager : NetworkBehaviour
{
    public static FeedbackManager InstanceLocal;

    [HideInInspector] public bool? bFeedbackWorked = null;
    [HideInInspector] public string strFeedbackAnswer = "";

    private void Start()
    {
        if (isLocalPlayer)
            InstanceLocal = Player.Instance.GetComponent<FeedbackManager>(); // idk, did it the same way for ipchanger
    }

    [Command]
    public void CmdSendFeedback(string _strFeedback)
    {
        Debug.Log($"Server: Received feedback from {GetComponent<Player>().account.accountInfo.name}");
        Database.feedback feedback = new Database.feedback()
        {
            date = DateTime.UtcNow,
            strAccountSender = ServerManager.s_dictPlayers[connectionToClient].account.accountInfo.name,
            strFeedback = _strFeedback
        };
        Database.Instance.AddFeedback(feedback);
        TargetFeedbackAnswer(true, "");
    }

    [TargetRpc]
    public void TargetFeedbackAnswer(bool _bWorked, string _strText)
    {
        bFeedbackWorked = _bWorked;
        strFeedbackAnswer = _strText;
    }
}
