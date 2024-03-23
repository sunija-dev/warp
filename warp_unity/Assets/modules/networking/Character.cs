using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.Events;

public class Character : NetworkBehaviour
{
    // client writes it (provided by api or the db)
    public GW2APICharInfo CharInfo => charInfo;
    [SyncVar(hook = nameof(SyncCharacterChanged))]
    public GW2APICharInfo charInfo = new GW2APICharInfo();
    public UnityEvent eCharSheetChanged = new UnityEvent();

    // everything except position (map id, etc)
    public WarpNetworkPosition netpos;

    // player writes it (= provided by player or the db)
    public CharSheet CharSheet => charSheet;
    [SyncVar(hook = nameof(SyncCharSheetChanged))]
    public CharSheet charSheet = new CharSheet();
    [Command] public void CmdUpdateCharSheet(CharSheet _charSheet) { charSheet = _charSheet; }



    private void SyncCharacterChanged(GW2APICharInfo _charInfoOld, GW2APICharInfo _charInfoNew)
    {
        if (!isLocalPlayer) return;
        ClientManager.Instance.eCharacterChanged.Invoke();
        Debug.Log("Client: CharacterChanged: " + _charInfoNew.name);

        if (_charInfoOld.name != _charInfoNew.name)
            ClientManager.Instance.eCharLoggedIn.Invoke();
    }

    private void SyncCharSheetChanged(CharSheet _charSheetOld, CharSheet _charSheetNew)
    {
        eCharSheetChanged.Invoke();

        if (!isLocalPlayer) return;
        ClientManager.Instance.eCharSheetChanged.Invoke();
        Debug.Log("Client: CharSheetChanged: " + _charSheetNew);
    }
}
