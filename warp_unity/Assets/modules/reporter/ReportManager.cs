using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class ReportManager : NetworkBehaviour
{
    public static ReportManager InstanceLocal;

    [HideInInspector] public bool? bReportWorked = null;
    [HideInInspector] public string strReportAnswer = "";

    private void Start()
    {
        if (isLocalPlayer)
            InstanceLocal = Player.Instance.GetComponent<ReportManager>(); // idk, did it the same way for ipchanger
    }

    [Command]
    public void CmdSendReport(string _strCharName, string _strReportedText, string _strReporterNote)
    {
        Debug.Log($"Server: Received report from {GetComponent<Player>().account.accountInfo.name}");
        Database.characters characterEntry = Database.Instance.charactersGetEntry(_strCharName);
        if (characterEntry != null)
        {
            Database.reports reportEntry = new Database.reports()
            {
                dateReported = DateTime.UtcNow,
                strAccountThatReported = ServerManager.s_dictPlayers[connectionToClient].account.accountInfo.name,
                strReportedAccount = characterEntry.strOwnedByAccount,
                strReportedText = _strReportedText,
                strReporterNote = _strReporterNote
            };
            Database.Instance.AddReport(reportEntry);
            TargetReportAnswer(true, "");
        }
        else
        {
            TargetReportAnswer(false, "Couldn't find player " + _strCharName);
        }
    }

    [TargetRpc]
    public void TargetReportAnswer(bool _bWorked, string _strText)
    {
        bReportWorked = _bWorked;
        strReportAnswer = _strText;
    }
}
