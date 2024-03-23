using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class LocalizationUtility : MonoBehaviour
{
    public static LocalizationUtility Instance;

    private void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// Changes the text in the _text async to the localized version
    /// </summary>
    public static void LocalizeTextAsync(TMP_Text _text, string _strKey, string[] _arArguments = null)
    {
        // TODO: could run this on the specific text?
        Instance.StartCoroutine(Instance.LocalizeText(_text, _strKey, _arArguments));
    }

    private IEnumerator LocalizeText(TMP_Text _text, string _strKey, string[] _arArguments)
    {
        LocalizedString lstrOption = new LocalizedString() { TableReference = "Main", TableEntryReference = _strKey };

        var handleGetLocalizedString = lstrOption.GetLocalizedStringAsync();
        yield return handleGetLocalizedString;

        if (_text == null)
            yield break;

        if (_arArguments != null && _arArguments.Length > 0)
            _text.text = string.Format(handleGetLocalizedString.Result, _arArguments);
        else
            _text.text = handleGetLocalizedString.Result;
    }

    // textInfoText.GetComponent<LocalizeStringEvent>().StringReference = new LocalizedString() { TableReference = "Main", TableEntryReference = "en" }; ;
}
