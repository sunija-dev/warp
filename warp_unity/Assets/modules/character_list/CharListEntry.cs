using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CharListEntry : MonoBehaviour
{
    public TMP_Text textName;
    public TMP_Text textMap;
    public TMP_Text textIP;
    public Button buttonRequestIPTaxi;
    public Image imageDarkBackground;
    public ContextMenu contextMenu;

    public void Start()
    {
        contextMenu.ClearOptions();
        contextMenu.AddOptions(
            new ContextMenu.Option("whisper", () => { GameIntegration.Instance.Chat.OpenWhisper(textName.text); }),
            new ContextMenu.Option("invite_to_group", () => { ClientManager.Instance.InviteToGroup(textName.text); })
            );
    }
}
