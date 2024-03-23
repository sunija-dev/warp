using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;

public class PageRequestApiKey : MonoBehaviour
{
    public TMP_Text textAPISubmitButton;
    public TMP_InputField inputAPIKey;
    public GameObject goKeyFailedFeedback;

    public bool? m_bAPIKeyWorked = false;
    public Account m_accountChecked;
    private System.Action m_actionFinished;

    public void SetFinishedAction(System.Action _action)
    {
        m_actionFinished = _action;
    }

    public void SubmitAPIKey()
    {
        StartCoroutine(coSubmitAPIKey());
    }

    public IEnumerator coSubmitAPIKey()
    {
        // start key checking
        ClientManager.s_player.CmdCheckAPIKey(inputAPIKey.text);
        textAPISubmitButton.text = "Checking...";
        goKeyFailedFeedback.SetActive(false);
        m_bAPIKeyWorked = null;

        // wait for answer
        float fTimeOut = Time.time + 10f;
        yield return new WaitUntil(() => m_bAPIKeyWorked != null || Time.time > fTimeOut);

        if (Time.time > fTimeOut)
            Debug.Log("Timeout when requesting API info");

        if (m_bAPIKeyWorked == true)
        {
            // worked
            Debug.Log("API key worked: " + m_accountChecked.strApiKey + ", account: " + m_accountChecked.accountInfo.name);

            // take langauge/region from another account
            if (Settings.Instance.liAccounts.Count > 0)
            {
                Settings.AccountData accountDataReference = Settings.Instance.liAccounts[0];
                m_accountChecked.language = accountDataReference.language;
                m_accountChecked.region = accountDataReference.region;
            }

            Settings.Instance.AddAccount(m_accountChecked);

            m_actionFinished.Invoke();
            //ClientManager.Instance.mainWindow.SetVisiblityForAllPageSelectors(false);
            GetComponent<WarpPage>().Hide();
            ClientManager.Instance.ShowLoadingPage();

            CategoryLoading categoryLoading = ClientManager.Instance.categoryLoading;
            LocalizationUtility.LocalizeTextAsync(categoryLoading.m_textTitle, "logging_in");
            LocalizationUtility.LocalizeTextAsync(categoryLoading.m_textDescription, "login_request");
        }
        else
        {
            // failed
            Debug.Log("API key didn't work.");
            goKeyFailedFeedback.SetActive(true);
        }

        textAPISubmitButton.text = "Submit";
    }

    public void PasteAPIKey()
    {
        inputAPIKey.text = SuUtility.strGetClipboardText();
    }
}
