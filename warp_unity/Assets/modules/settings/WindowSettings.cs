using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Collections.Generic;

public class WindowSettings : MonoBehaviour
{
    public List<int> liFPSSelection;
    public List<int> liUIScalingSelection;

    [Header("References")]
    public Transform transSettingParent;

    public GameObject goSettingButtonPrefab;
    public GameObject goSettingDropdownPrefab;
    public GameObject goSettingHeaderPrefab;
    public GameObject goSettingTextPrefab;
    public GameObject goSettingSliderPrefab;
    public GameObject goSettingTogglePrefab;

    private List<GameObject> liSettings = new List<GameObject>();

    public void Start()
    {
        ClientManager.Instance.eDisplayLanguageChanged.AddListener(Rebuild);
    }

    public void Rebuild()
    {
        foreach (GameObject go in liSettings)
            Destroy(go);
        liSettings.Clear();

        // TODO: Localization!
        AddSettingHeader("settings_header_rp");
        AddSettingToggle(Settings.OptionKey.bHideOnMap);
        AddSettingToggle(Settings.OptionKey.bHideInOnlineList);
        AddSettingToggle(Settings.OptionKey.bShowCharSheetOnHover);
        AddSettingButton("settings_open_minicharlist", CloseCharListWindow.Instance.Show);
        AddSettingButton("reset_windows", ClientManager.Instance.ResetWindows);

        AddSettingHeader("settings_tech");
        AddSettingToggle(Settings.OptionKey.bStartWarpWithGw2);
        AddSettingButton("settings_create_desktop", ClientManager.Instance.CreateDesktopShortcut);
        AddSettingSlider(Settings.OptionKey.fIconDisplacement);
        AddSettingDropdown(Settings.OptionKey.iMaxFPS.ToString(),
            (int _iValue) => { Settings.WriteInt(Settings.OptionKey.iMaxFPS, liFPSSelection[_iValue]); },
            liFPSSelection.FindIndex(x => x == Settings.iRead(Settings.OptionKey.iMaxFPS)),
            liFPSSelection.Select(x => x.ToString()).ToArray());
        //AddSettingSlider(Settings.OptionKey.fVolume);
        AddSettingDropdown(Settings.OptionKey.iDpiScaling.ToString(),
            (int _iValue) => { Settings.WriteInt(Settings.OptionKey.iDpiScaling, liUIScalingSelection[_iValue]); },
            liUIScalingSelection.FindIndex(x => x == Settings.iRead(Settings.OptionKey.iDpiScaling)),
            liUIScalingSelection.Select(x => x.ToString() + "%").ToArray());

        AddSettingHeader("settings_language");
        
        List<GlobalEnums.Language> liLanguages = new List<GlobalEnums.Language>((GlobalEnums.Language[])System.Enum.GetValues(typeof(GlobalEnums.Language)));
        AddSettingDropdown("settings_ui_language",
            (int _iValue) => { Settings.WriteInt(Settings.OptionKey.iDisplayLanguage, _iValue); },
            (int)Settings.iRead(Settings.OptionKey.iDisplayLanguage),
            liLanguages.Select(x => x.ToString()).ToArray());
        AddSettingText("settings_translator_credit");

        AddSettingDropdown("setting_rp_language",
            (int _iValue) => { ClientManager.Instance.ChangeLanguage((GlobalEnums.Language)_iValue); },
            (int)Player.Instance.account.language,
            liLanguages.Select(x => x.ToString()).ToArray());

        List<GlobalEnums.Region> liRegions = new List<GlobalEnums.Region>((GlobalEnums.Region[])System.Enum.GetValues(typeof(GlobalEnums.Region)));
        AddSettingDropdown("settings_region",
            (int _iValue) => { ClientManager.Instance.ChangeRegion((GlobalEnums.Region)_iValue); },
            (int)Player.Instance.account.region,
            liRegions.Select(x => x.ToString()).ToArray());

        AddSettingHeader("settings_other");
        AddSettingToggle(Settings.OptionKey.bShowDebugCategory);
    }

    public void AddSettingSlider(Settings.OptionKey _optionKey)
    {
        AddSettingSlider(_optionKey.ToString(), 
            (float _fValue) => { Settings.WriteFloat(_optionKey, _fValue); },
            Settings.fRead(_optionKey));
    }

    private void AddSettingSlider(string _strTextKey, System.Action<float> _actionValueChanged, float _fInitValue)
    {
        GameObject goSettingSlider = Instantiate(goSettingSliderPrefab, transSettingParent);
        TMP_Text textTitle = goSettingSlider.GetComponent<SettingElement>().textName;
        textTitle.text = _strTextKey;
        LocalizationUtility.LocalizeTextAsync(textTitle, _strTextKey);

        Slider slider = goSettingSlider.GetComponentInChildren<Slider>();
        slider.SetValueWithoutNotify(_fInitValue);
        slider.onValueChanged.AddListener((float _fValue) => { _actionValueChanged.Invoke(_fValue); } );

        liSettings.Add(goSettingSlider);
    }

    private void AddSettingToggle(Settings.OptionKey _optionKey)
    {
        AddSettingToggle(_optionKey.ToString(), 
            (bool _bValue) => { Settings.WriteBool(_optionKey, _bValue); },
            Settings.bRead(_optionKey));
    }

    private void AddSettingToggle(string _strTextKey, System.Action<bool> _actionValueChanged, bool _bInitValue)
    {
        GameObject goSettingToggle = Instantiate(goSettingTogglePrefab, transSettingParent);
        TMP_Text textTitle = goSettingToggle.GetComponent<SettingElement>().textName;
        textTitle.text = _strTextKey;
        LocalizationUtility.LocalizeTextAsync(textTitle, _strTextKey);

        Toggle toggle = goSettingToggle.GetComponentInChildren<Toggle>();
        toggle.SetIsOnWithoutNotify(_bInitValue);
        toggle.onValueChanged.AddListener((bool _bValue) => { _actionValueChanged.Invoke(_bValue); });

        liSettings.Add(goSettingToggle);
    }

    private void AddSettingDropdown(string _strTextKey, System.Action<int> _actionValueChanged, int _iInitValue, params string[] _arOptions)
    {
        GameObject goSettingDropdown = Instantiate(goSettingDropdownPrefab, transSettingParent);

        TMP_Text textTitle = goSettingDropdown.GetComponent<SettingElement>().textName;
        textTitle.text = _strTextKey;
        LocalizationUtility.LocalizeTextAsync(textTitle, _strTextKey);

        TMP_Dropdown dropdown = goSettingDropdown.GetComponentInChildren<TMP_Dropdown>();
        dropdown.ClearOptions();
        dropdown.AddOptions(_arOptions.ToList());
        dropdown.SetValueWithoutNotify(_iInitValue);
        dropdown.onValueChanged.AddListener((int _iValue) => { _actionValueChanged.Invoke(_iValue); });

        // localize!
        LocalizeDropdown localizeDropdown = dropdown.GetComponent<LocalizeDropdown>();
        if (localizeDropdown != null)
            localizeDropdown.AddOptions("Main", _arOptions.ToList());
        

        liSettings.Add(goSettingDropdown);
    }

    private void AddSettingButton(string _strTextKey, System.Action _actionOnClick)
    {
        GameObject goSettingButton = Instantiate(goSettingButtonPrefab, transSettingParent);

        TMP_Text textTitle = goSettingButton.GetComponent<SettingElement>().textName;
        textTitle.text = _strTextKey;
        LocalizationUtility.LocalizeTextAsync(textTitle, _strTextKey);

        goSettingButton.GetComponentInChildren<Button>().onClick.AddListener(() => { _actionOnClick.Invoke(); });

        liSettings.Add(goSettingButton);
    }

    private void AddSettingHeader(string _strTextKey)
    {
        GameObject goSettingHeader = Instantiate(goSettingHeaderPrefab, transSettingParent);

        TMP_Text textTitle = goSettingHeader.GetComponent<SettingElement>().textName;
        textTitle.text = _strTextKey;
        LocalizationUtility.LocalizeTextAsync(textTitle, _strTextKey);

        liSettings.Add(goSettingHeader);
    }

    private void AddSettingText(string _strTextKey)
    {
        GameObject goSettingsText = Instantiate(goSettingTextPrefab, transSettingParent);

        TMP_Text textTitle = goSettingsText.GetComponent<SettingElement>().textName;
        textTitle.text = _strTextKey;
        LocalizationUtility.LocalizeTextAsync(textTitle, _strTextKey);

        liSettings.Add(goSettingsText);
    }
}
