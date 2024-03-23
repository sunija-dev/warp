using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;
using UnityEngine.UI;

public class PageSetup : MonoBehaviour
{
    public static PageSetup Instance;

    public List<WarpCategory> liCategorySteps = new List<WarpCategory>();
    private int iCurrentCategory = 0;

    public GameObject goLanguageFlagHighlight;
    public GameObject goRegionHighlight;
    public TMP_InputField inputAPIKey;
    public TMP_Text textAPISubmitButton;
    public GameObject goKeyFailedFeedback;
    public Toggle toggleAutostart;

    // api key feedback
    public bool? m_bAPIKeyWorked = null;
    public Account m_accountChecked = null;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        liCategorySteps[0].Show();
    }

    public void SetAutostartWithGW2()
    { 
        GameIntegration.SetAutostartWithGw2(toggleAutostart.isOn);
    }

    public void OpenCategory(string _strCategoryName)
    {
        liCategorySteps[iCurrentCategory].Hide();
        iCurrentCategory = liCategorySteps.FindIndex(x => x.strName == _strCategoryName);
        liCategorySteps[iCurrentCategory].Show();
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

        if (m_bAPIKeyWorked == true)
        {
            // worked
            Debug.Log("API key worked: " + m_accountChecked.strApiKey + ", account: " + m_accountChecked.accountInfo.name);
            m_accountChecked.language = Player.Instance.account.language;
            m_accountChecked.region = Player.Instance.account.region;
            Settings.Instance.AddAccount(m_accountChecked);

            liCategorySteps[iCurrentCategory].Hide();
            iCurrentCategory = liCategorySteps.FindIndex(x => x.strName == "Change Options");
            liCategorySteps[iCurrentCategory].Show();
        }
        else
        {
            // failed
            Debug.Log("API key didn't work.");
            goKeyFailedFeedback.SetActive(true);
        }

        textAPISubmitButton.text = "Submit";
    }

    public void MoveLanguageHighlighter(Transform _trans)
    {
        goLanguageFlagHighlight.transform.position = _trans.position;
    }

    public void MoveRegionHighlighter(Transform _trans)
    {
        goRegionHighlight.transform.position = _trans.position;
    }

    public void PasteAPIKey()
    {
        inputAPIKey.text = SuUtility.strGetClipboardText();
    }

    public void Quit()
    {
        Application.Quit();
    }
}
