using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;
using UnityEngine.Events;
using System;

public class DiceManager : NetworkBehaviour
{
    public int iMaxRollsSaved = 50;

    public static DiceManager Instance;
    public static List<DiceRoll> s_liDiceRolls = new List<DiceRoll>();

    public MessageEvent eRollReceived;

    /// <summary>
    /// Position, playerName, result, sides.
    /// </summary>
    public class MessageEvent : UnityEvent<Vector3, string, int, int> { } 

    private void Awake()
    {
        Instance = this;
    }

    [Command]
    public void CmdRollDie(int _iSides)
    {
        Player playerSending = ServerManager.s_dictPlayers[connectionToClient];
        int iResult = UnityEngine.Random.Range(1, _iSides + 1);
        RpcReceivedRoll(playerSending.transform.position, playerSending.Character.charInfo.name, iResult, _iSides);

        Debug.Log(string.Format("{0} rolled a {1} ({2}).", playerSending.Character.charInfo.name, iResult, _iSides));
    }

    [ClientRpc]
    // is affected by network visibility (aka interest management)
    public void RpcReceivedRoll(Vector3 _v3Position, string _strName, int _iResult, int _iSides)
    {
        s_liDiceRolls.Add(new DiceRoll() { v3Position = _v3Position, strPlayerName = _strName, iResult = _iResult, iSides = _iSides });
        if (s_liDiceRolls.Count > iMaxRollsSaved)
            s_liDiceRolls.RemoveAt(0);

        eRollReceived.Invoke(_v3Position, _strName, _iResult, _iSides);
    }


    public class DiceRoll
    {
        public Vector3 v3Position = Vector3.zero;
        public string strPlayerName = "Player";
        public int iResult = 0;
        public int iSides = 0;
    }
}
