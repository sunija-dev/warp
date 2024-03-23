using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DiceWindow : MonoBehaviour
{
    public TMP_Text chatHistory;
    public Scrollbar scrollbar;
    public TMP_InputField chatMessage;

    public void Send()
    {
        if (chatMessage.text.Trim() == "")
            return;

        // TODO: take the right die, send CmdRollDie then
        // TODO: OnEnable or so, rebuild the chat window
        //DiceManager.Instance.CmdRollDie(chatMessage.text.Trim());
        chatMessage.text = "";
    }

    internal void AppendMessage(string message)
    {
        StartCoroutine(AppendAndScroll(message));
    }

    IEnumerator AppendAndScroll(string message)
    {
        chatHistory.text += message + "\n";

        // it takes 2 frames for the UI to update ?!?!
        yield return null;
        yield return null;

        // slam the scrollbar down
        scrollbar.value = 0;
    }
}
