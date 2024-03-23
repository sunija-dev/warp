using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class WindowReport : MonoBehaviour
{
    public List<string> liReportReasons = new List<string>() { "Inappropriate_Content", "Insults", "Exploiting_GW2", "Exploiting", "Spam" };

    public TMP_InputField inputPlayerName;
    public TMP_Text textReported;
    public TMP_Dropdown dropdownReason;
    public TMP_InputField inputReporterNote;
    public TMP_Text textButtonText;

    public WarpPage pageReport;
    public WarpCategory categoryReport;
    public WindowReport windowReport;

    private bool bInit = false;

    private void Start()
    {
        if (!bInit) Init();
    }

    public void OpenReport(string _strCharName, string _strReportedText)
    {
        MainWindow.Instance.Open();
        MainWindow.Instance.OpenPage(pageReport);
        pageReport.OpenCategory(categoryReport);
        windowReport.SetupWindow(_strCharName, _strReportedText);
    }

    public void Init()
    {
        ResetWindow();
        dropdownReason.ClearOptions();
        dropdownReason.AddOptions(liReportReasons); // TODO: Localize!
        bInit = true;
    }

    public void SendReport()
    {
        StartCoroutine(coSendReport());
    }

    public IEnumerator coSendReport()
    {
        string strReportNote = string.Format("Player name: {0}\nReport Reason: {1}\nReport Note:\n{2}", 
            inputPlayerName.text,
            liReportReasons[dropdownReason.value],
            inputReporterNote.text);

        ReportManager.InstanceLocal.CmdSendReport(inputPlayerName.text, textReported.text, strReportNote);
        float fTimeout = Time.time + 5f;
        yield return new WaitUntil(() => ReportManager.InstanceLocal.bReportWorked != null || Time.time > fTimeout);

        if (Time.time > fTimeout)
        {
            ClientManager.Instance.windowPopup.Init(
                        new WindowPopup.ButtonInfo(true, null, "Ok"),
                        new WindowPopup.ButtonInfo(false, null, ""),
                        "report_timeout");
            yield break;
        }

        if (ReportManager.InstanceLocal.bReportWorked == true)
        {
            ClientManager.Instance.windowPopup.Init(
                    new WindowPopup.ButtonInfo(true, null, "Ok"),
                    new WindowPopup.ButtonInfo(false, null, ""),
                    "report_success");
            ResetWindow();
        }
        else
        {
            ClientManager.Instance.windowPopup.Init(
                        new WindowPopup.ButtonInfo(true, null, "Ok"),
                        new WindowPopup.ButtonInfo(false, null, ""),
                        "report_failed", ReportManager.InstanceLocal.strReportAnswer);
        }

        ReportManager.InstanceLocal.bReportWorked = null;
        ReportManager.InstanceLocal.strReportAnswer = "";
    }

    public void DiscardReport()
    {
        ClientManager.Instance.windowPopup.Init(
                            new WindowPopup.ButtonInfo(true, () => ResetWindow(), "yes"),
                            new WindowPopup.ButtonInfo(true, null, "no"),
                            "report_discard");
    }

    private void ResetWindow()
    {
        inputPlayerName.text = "";
        inputReporterNote.text = "";
        textReported.text = "-";
        dropdownReason.SetValueWithoutNotify(0);
        textButtonText.text = "Send";
    }

    public void SetupWindow(string _strPlayer, string _strReportedText)
    {
        if (!bInit) Init();
        inputPlayerName.SetTextWithoutNotify(_strPlayer);
        textReported.text = _strReportedText;
    }

}
