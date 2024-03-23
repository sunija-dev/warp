using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class WindowFeedback : MonoBehaviour
{
    public TMP_InputField inputFeedback;


    public void SendFeedback()
    {
        StartCoroutine(coSendFeedback());
    }

    public IEnumerator coSendFeedback()
    {
        FeedbackManager.InstanceLocal.CmdSendFeedback(inputFeedback.text);

        float fTimeout = Time.time + 5f;
        yield return new WaitUntil(() => FeedbackManager.InstanceLocal.bFeedbackWorked != null || Time.time > fTimeout);

        if (Time.time > fTimeout)
        {
            ClientManager.Instance.windowPopup.Init(
                        new WindowPopup.ButtonInfo(true, null, "ok"),
                        new WindowPopup.ButtonInfo(false, null, ""),
                        "feedback_timeout");
            yield break;
        }

        if (FeedbackManager.InstanceLocal.bFeedbackWorked == true)
        {
            ClientManager.Instance.windowPopup.Init(
                    new WindowPopup.ButtonInfo(true, null, "ok"),
                    new WindowPopup.ButtonInfo(false, null, ""),
                    "feedback_success");
            ResetWindow();
        }
        else
        {
            ClientManager.Instance.windowPopup.Init(
                        new WindowPopup.ButtonInfo(true, null, "Ok"),
                        new WindowPopup.ButtonInfo(false, null, ""),
                        "feedback_failed", ReportManager.InstanceLocal.strReportAnswer);
        }

        ReportManager.InstanceLocal.bReportWorked = null;
        ReportManager.InstanceLocal.strReportAnswer = "";
    }

    private void ResetWindow()
    {
        inputFeedback.text = "";
    }
}
