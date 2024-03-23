using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class SpotManager : NetworkBehaviour
{
    public static SpotManager Instance;

    [HideInInspector] public bool? bSendingWorked = null;
    [HideInInspector] public string strSendingAnswer = "";

    private void Awake()
    {
        Instance = this;
    }

    [Command]
    public void CmdSendSpot(int _iLanguage, bool _bInstanced, bool _bInterior, float _fQuality, int _iLevel, int _iAddon, int _iMount, 
        string _strTitle, string _strDesc, string strReq)
    {
        Player player = ServerManager.s_dictPlayers[connectionToClient];
        Account account = player.account;

        Database.spots spot = new Database.spots();
        spot.iLanguage = _iLanguage;
        spot.bInstanced = _bInstanced;
        spot.bInterior = _bInterior;
        spot.fQuality = _fQuality;
        spot.iRequiredLevel = Mathf.Clamp(_iLevel, 1, 80);
        spot.iRequiredAddon = _iAddon;
        spot.iRequiredMount = _iMount;

        spot.fX = player.transform.position.x;
        spot.fY = player.transform.position.y;
        spot.fZ = player.transform.position.z;
        spot.fXMap = player.warpNetworkPosition.v2ContinentPosition.x;
        spot.fYMap = player.warpNetworkPosition.v2ContinentPosition.y;
        spot.iMapID = player.warpNetworkPosition.iMapId;
        spot.bApproved = account.iAdminLevel > 0;
        spot.strAuthorAccount = account.accountInfo.id;
        spot.strAuthorCharacter = player.Character.charInfo.name;
        spot.dateAdded = System.DateTime.UtcNow;

        switch ((GlobalEnums.Language)spot.iLanguage)
        {
            default:
            case GlobalEnums.Language.en:
                spot.strTitleEn = _strTitle;
                spot.strDescEn = _strDesc;
                spot.strRequiredEn = strReq;
                break;
            case GlobalEnums.Language.de:
                spot.strTitleDe = _strTitle;
                spot.strDescDe = _strDesc;
                spot.strRequiredDe = strReq;
                break;
            case GlobalEnums.Language.fr:
                spot.strTitleFr = _strTitle;
                spot.strDescFr = _strDesc;
                spot.strRequiredFr = strReq;
                break;
            case GlobalEnums.Language.es:
                spot.strTitleEs = _strTitle;
                spot.strDescEs = _strDesc;
                spot.strRequiredEs = strReq;
                break;
        }

        Database.Instance.AddSpot(spot);
        TargetAnswer(true, "");
    }

    [TargetRpc]
    public void TargetAnswer(bool _bWorked, string _strText)
    {
        bSendingWorked = _bWorked;
        strSendingAnswer = _strText;
    }
}
