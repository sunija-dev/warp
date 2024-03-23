using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Components;

public class WindowPopup : MonoBehaviour
{
    public TMP_Text textInfoText;
    public GameObject goButtonYes;
    public GameObject goButtonNo;
    public TMP_Text textYes;
    public TMP_Text textNo;

    private ButtonInfo buttonInfoYes;
    private ButtonInfo buttonInfoNo;

    public class ButtonInfo
    {
        public bool bActive = true;
        public Action action;
        public string strText = "";

        public ButtonInfo(bool bActive, Action action, string strText)
        {
            this.bActive = bActive;
            this.action = action;
            this.strText = strText;
        }
    }

    public void Init(ButtonInfo _buttonInfoYes, ButtonInfo _buttonInfoNo, string _strTextKey, params string[] _arArguments)
    {
        this.gameObject.SetActive(true);

        buttonInfoYes = _buttonInfoYes;
        buttonInfoNo = _buttonInfoNo;

        goButtonYes.SetActive(buttonInfoYes.bActive);
        goButtonNo.SetActive(buttonInfoNo.bActive);

        textYes.text = buttonInfoYes.strText;
        textNo.text = buttonInfoNo.strText;
        textInfoText.text = _strTextKey;

        LocalizationUtility.LocalizeTextAsync(textYes, buttonInfoYes.strText);
        LocalizationUtility.LocalizeTextAsync(textNo, buttonInfoNo.strText);
        LocalizationUtility.LocalizeTextAsync(textInfoText, _strTextKey, _arArguments);
    }

    private void CleanUp()
    {
        this.gameObject.SetActive(false);
        buttonInfoYes = null;
        buttonInfoNo = null;
    }

    public void Yes()
    {
        buttonInfoYes.action?.Invoke();
        CleanUp();
    }

    public void No()
    {
        buttonInfoNo.action?.Invoke();
        CleanUp();
    }
}
