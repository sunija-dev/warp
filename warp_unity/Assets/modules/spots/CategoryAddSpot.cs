using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Linq;

public class CategoryAddSpot : MonoBehaviour
{
    [Header("References")]
    public TMP_Dropdown dropdownLanguage;
    public TMP_InputField inputTitle;
    public TMP_Text textLocation;
    public Toggle toggleInstanced;
    public Toggle toggleInterior;
    public TMP_Dropdown dropdownQuality;
    public TMP_InputField inputDescription;
    public TMP_InputField inputLevel;
    public TMP_Dropdown dropdownAddon;
    public TMP_Dropdown dropdownMount;
    public TMP_InputField inputRequirements;

    void Start()
    {
        dropdownLanguage.ClearOptions();
        dropdownLanguage.AddOptions(SuUtility.GetEnumStrings<GlobalEnums.Language>());
        dropdownQuality.ClearOptions();
        dropdownQuality.AddOptions(new List<string>() { "1", "2", "3", "4", "5" });
        dropdownAddon.ClearOptions();
        dropdownAddon.AddOptions(SuUtility.GetEnumStrings<GlobalEnums.Addon>());
        dropdownMount.ClearOptions();
        dropdownMount.AddOptions(SuUtility.GetEnumStrings<GlobalEnums.Mount>());

        ResetWindow();
    }

    private void Update()
    {
        SuMumbleLinkGW2.GW2Info gw2Info = MumbleManager.s_gw2Info;
        string strMapName = MapNames.strGetName((int)gw2Info.mapId);
        textLocation.text = $"Location: {strMapName}, {gw2Info.fAvatarPosition[0]:0} / {gw2Info.fAvatarPosition[1]:0} / {gw2Info.fAvatarPosition[2]:0}";
    }

    public void SaveSpot()
    {
        StartCoroutine(coSaveSpot());
    }

    public IEnumerator coSaveSpot()
    {
        int.TryParse(inputLevel.text, out int iLevel);
        SpotManager.Instance.CmdSendSpot(dropdownLanguage.value, toggleInstanced.isOn, toggleInterior.isOn, (float)dropdownQuality.value, Mathf.Clamp(iLevel, 1, 80),
            dropdownAddon.value, dropdownMount.value, inputTitle.text, inputDescription.text, inputRequirements.text);

        float fTimeout = Time.time + 5f;
        yield return new WaitUntil(() => SpotManager.Instance.bSendingWorked != null || Time.time > fTimeout);

        if (Time.time > fTimeout)
        {
            ClientManager.Instance.windowPopup.Init(
                        new WindowPopup.ButtonInfo(true, null, "ok"),
                        new WindowPopup.ButtonInfo(false, null, ""),
                        "spot_timeout");
            yield break;
        }

        if (SpotManager.Instance.bSendingWorked == true)
        {
            ClientManager.Instance.windowPopup.Init(
                    new WindowPopup.ButtonInfo(true, null, "ok"),
                    new WindowPopup.ButtonInfo(false, null, ""),
                    "spot_success");
            ResetWindow(_bKeepSimilar: true);
        }
        else
        {
            ClientManager.Instance.windowPopup.Init(
                        new WindowPopup.ButtonInfo(true, null, "ok"),
                        new WindowPopup.ButtonInfo(false, null, ""),
                        "Saving spot failed.\n{0}", SpotManager.Instance.strSendingAnswer);
        }

        SpotManager.Instance.bSendingWorked = null;
        SpotManager.Instance.strSendingAnswer = "";
    }

    /// <param name="_bKeepSimilar">Keep some parameters that might not change when adding a lot of spots.</param>
    private void ResetWindow(bool _bKeepSimilar = false)
    {
        dropdownLanguage.value = Settings.iRead(Settings.OptionKey.iDisplayLanguage);
        inputTitle.text = "";
        toggleInstanced.isOn = false;
        toggleInterior.isOn = false;
        dropdownQuality.value = 2;
        inputDescription.text = "";
        dropdownMount.value = 0;
        inputRequirements.text = "";

        if (!_bKeepSimilar)
        {
            inputLevel.text = "1";
            dropdownAddon.value = 0;
        }
    }
}
