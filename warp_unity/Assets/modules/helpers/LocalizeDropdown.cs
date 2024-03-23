
// https://forum.unity.com/threads/localizing-ui-dropdown-options.896951/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;


[RequireComponent(typeof(TMP_Dropdown))]
[AddComponentMenu("Localization/Localize Dropdown")]
public class LocalizeDropdown : MonoBehaviour
{
    // Fields
    // =======
    public List<LocalizedDropdownOption> options;

    public int selectedOptionIndex = 0;

    private Locale currentLocale = null;

    // Properties
    // ===========
    private TMP_Dropdown Dropdown => GetComponent<TMP_Dropdown>();

    // Methods
    // ========

    public void AddOptions(string _strTable, List<string> _liLocaleKeys)
    {
        foreach (string strLocaleKey in _liLocaleKeys)
        { 
            LocalizedString lstrOption = new LocalizedString() { TableReference = _strTable, TableEntryReference = strLocaleKey };
            options.Add(new LocalizedDropdownOption() { text = lstrOption });
        }

        StartCoroutine(PopulateAndWait());
        //UpdateDropdownOptions(LocalizationSettings.SelectedLocale);
    }

    private IEnumerator PopulateAndWait()
    {
        yield return PopulateDropdown();
    }

    /*
    private IEnumerator Start()
    {
        yield return PopulateDropdown();
    }
    */

    private void OnEnable()
    {
        var locale = LocalizationSettings.SelectedLocale;
        if (currentLocale != null && locale != currentLocale)
        {
            UpdateDropdownOptions(locale);
            currentLocale = locale;
        }
        LocalizationSettings.SelectedLocaleChanged += UpdateDropdownOptions;
    }

    private void OnDisable() => LocalizationSettings.SelectedLocaleChanged -= UpdateDropdownOptions;

    private void OnDestroy() => LocalizationSettings.SelectedLocaleChanged -= UpdateDropdownOptions;

    private IEnumerator PopulateDropdown()
    {
        List<TMP_Dropdown.OptionData> liDropdownOptions = new List<TMP_Dropdown.OptionData>();

        for (var i = 0; i < options.Count; ++i)
        {
            var option = options[i];
            var localizedText = string.Empty;
            Sprite localizedSprite = null;

            // If the option has text, fetch the localized version
            if (!option.text.IsEmpty)
            {
                var localizedTextHandle = option.text.GetLocalizedStringAsync();
                yield return localizedTextHandle;

                localizedText = localizedTextHandle.Result;

                // If this is the selected item, also update the caption text
                if (i == selectedOptionIndex)
                {
                    UpdateSelectedText(localizedText);
                }
            }

            // If the option has a sprite, fetch the localized version
            if (option.sprite != null && !option.sprite.IsEmpty)
            {
                var localizedSpriteHandle = option.sprite.LoadAssetAsync();
                yield return localizedSpriteHandle;

                localizedSprite = localizedSpriteHandle.Result;

                // If this is the selected item, also update the caption text
                if (i == selectedOptionIndex)
                {
                    UpdateSelectedSprite(localizedSprite);
                }
            }

            // Finally add the option with the localized content
            liDropdownOptions.Add(new TMP_Dropdown.OptionData(localizedText, localizedSprite));
        }

        // Clear any options that might be present
        selectedOptionIndex = Dropdown.value;
        Dropdown.ClearOptions();
        Dropdown.onValueChanged.RemoveListener(UpdateSelectedOptionIndex);
        Dropdown.AddOptions(liDropdownOptions);

        // Update selected option, to make sure the correct option can be displayed in the caption
        Dropdown.SetValueWithoutNotify(selectedOptionIndex);
        //Dropdown.SetValueWithoutNotify(selectedOptionIndex);
        Dropdown.onValueChanged.AddListener(UpdateSelectedOptionIndex);
        currentLocale = LocalizationSettings.SelectedLocale;

    }

    private void UpdateDropdownOptions(Locale locale)
    {
        // Updating all options in the dropdown
        // Assumes that this list is the same as the options passed on in the inspector window
        for (var i = 0; i < Dropdown.options.Count; ++i)
        {
            var optionI = i;
            var option = options[i];

            // Update the text
            if (!option.text.IsEmpty)
            {
                var localizedTextHandle = option.text.GetLocalizedStringAsync(locale);
                localizedTextHandle.Completed += (handle) =>
                {
                    if (this == null) return;

                    Dropdown.options[optionI].text = handle.Result;

                    // If this is the selected item, also update the caption text
                    if (optionI == selectedOptionIndex)
                        UpdateSelectedText(handle.Result);
                };
            }

            // Update the sprite
            if (option.sprite != null && !option.sprite.IsEmpty)
            {
                var localizedSpriteHandle = option.sprite.LoadAssetAsync();
                localizedSpriteHandle.Completed += (handle) =>
                {
                    if (this == null) return;

                    Dropdown.options[optionI].image = localizedSpriteHandle.Result;

                    // If this is the selected item, also update the caption sprite
                    if (optionI == selectedOptionIndex)
                        UpdateSelectedSprite(localizedSpriteHandle.Result);
                };
            }
        }
    }

    private void UpdateSelectedOptionIndex(int index) => selectedOptionIndex = index;

    private void UpdateSelectedText(string text)
    {
        if (this != null && Dropdown.captionText != null)
            Dropdown.captionText.text = text;
    }

    private void UpdateSelectedSprite(Sprite sprite)
    {
        if (this != null && Dropdown.captionImage != null)
            Dropdown.captionImage.sprite = sprite;
    }
}

[Serializable]
public class LocalizedDropdownOption
{
    public LocalizedString text;

    public LocalizedSprite sprite;
}

