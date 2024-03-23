using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class CategoryAboutMe : MonoBehaviour
{
    public TMP_Dropdown dropdownExperience;
    public TMP_Dropdown dropdownFightingStyle;
    public TMP_Dropdown dropdownAdult;
    public Slider sliderLore;
    public Slider sliderTopic;
    public Slider sliderStyle;
    public Slider sliderPostLength;
    public TMP_InputField inputExamplePost;
    public GameObject goUnsavedChangesInfo;

    private bool bUnsavedChanges = true;

    public void Awake()
    {
        dropdownExperience.ClearOptions();
        dropdownExperience.AddOptions(SuUtility.GetEnumStrings<AccountSheet.Experience>());
        LocalizeDropdown localizeDropdown = dropdownExperience.GetComponent<LocalizeDropdown>();
        if (localizeDropdown != null)
            localizeDropdown.AddOptions("Main", SuUtility.GetEnumStrings<AccountSheet.Experience>());

        dropdownFightingStyle.ClearOptions();
        dropdownFightingStyle.AddOptions(SuUtility.GetEnumStrings<AccountSheet.FightingStyle>());
        localizeDropdown = dropdownFightingStyle.GetComponent<LocalizeDropdown>();
        if (localizeDropdown != null)
            localizeDropdown.AddOptions("Main", SuUtility.GetEnumStrings<AccountSheet.FightingStyle>());

        dropdownAdult.ClearOptions();
        dropdownAdult.AddOptions(SuUtility.GetEnumStrings<AccountSheet.AdultStatus>());
        localizeDropdown = dropdownAdult.GetComponent<LocalizeDropdown>();
        if (localizeDropdown != null)
            localizeDropdown.AddOptions("Main", SuUtility.GetEnumStrings<AccountSheet.AdultStatus>());


        ClientManager.Instance.eAccountChanged.AddListener(ReloadInfo);

        dropdownExperience.onValueChanged.AddListener((v) => SetUnsavedChanges(true));
        dropdownFightingStyle.onValueChanged.AddListener((v) => SetUnsavedChanges(true));
        dropdownAdult.onValueChanged.AddListener((v) => SetUnsavedChanges(true));
        sliderLore.onValueChanged.AddListener((v) => SetUnsavedChanges(true));
        sliderTopic.onValueChanged.AddListener((v) => SetUnsavedChanges(true));
        sliderStyle.onValueChanged.AddListener((v) => SetUnsavedChanges(true));
        sliderPostLength.onValueChanged.AddListener((v) => SetUnsavedChanges(true));
        inputExamplePost.onValueChanged.AddListener((v) => SetUnsavedChanges(true));

        if (ClientManager.Instance.bAccLoggedIn)
            ReloadInfo();
    }

    private void Start()
    {
        ClientManager.eOnCtrlS.AddListener(() => { if (gameObject.activeInHierarchy) SaveSheet(); });
    }

    public void ReloadInfo()
    {
        AccountSheet accSheet = Player.Instance.accountSheet;
        if (accSheet == null)
            return;

        dropdownExperience.SetValueWithoutNotify((int)accSheet.experience);
        dropdownFightingStyle.SetValueWithoutNotify((int)accSheet.fightingStyle);
        dropdownAdult.SetValueWithoutNotify((int)accSheet.adultStatus);

        sliderLore.SetValueWithoutNotify(accSheet.fLoreStrictness);
        sliderTopic.SetValueWithoutNotify(accSheet.fTopic);
        sliderStyle.SetValueWithoutNotify(accSheet.fStyle);
        sliderPostLength.SetValueWithoutNotify(accSheet.fPostLength);

        inputExamplePost.SetTextWithoutNotify(accSheet.strExamplePost);

        SetUnsavedChanges(false);
    }

    public void SaveSheet()
    {
        Player.Instance.CmdUpdateAccountSheet((AccountSheet.Experience)dropdownExperience.value, (AccountSheet.FightingStyle)dropdownFightingStyle.value,
            (AccountSheet.AdultStatus)dropdownAdult.value, sliderLore.value, sliderTopic.value, sliderStyle.value, sliderPostLength.value, 
            inputExamplePost.text);
        SetUnsavedChanges(false);
    }

    public void DiscardChanges()
    {
        ClientManager.Instance.windowPopup.Init(
                                new WindowPopup.ButtonInfo(true, () => ReloadInfo(), "yes"),
                                new WindowPopup.ButtonInfo(true, null, "no"),
                                "aboutme_discard");
    }

    public void SetUnsavedChanges(bool _bUnsaved)
    {
        if (_bUnsaved != bUnsavedChanges)
            goUnsavedChangesInfo.SetActive(_bUnsaved);

        bUnsavedChanges = _bUnsaved;
    }
}
