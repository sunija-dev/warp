using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class WarpPage : MonoBehaviour
{
    public string strName = "PageTitle";
    public bool bShowSelectorOnlyWhenActive = false;
    public WarpCategory categoryDefault = null;
    [SerializeField] private bool bEnabled = true; 

    [Header("References")]
    public GameObject goSelector;
    public CanvasGroup canvasGroup;

    private WarpCategory categoryActive = null;

    public void OpenCategory(WarpCategory _category)
    {
        categoryActive?.Hide();
        categoryActive = _category;

        categoryActive.Show();
        MainWindow.Instance.categoryActive = categoryActive;
    }

    public void Show()
    {
        gameObject.SetActive(true);

        if (bShowSelectorOnlyWhenActive)
            goSelector.SetActive(true);

        if (categoryActive)
            OpenCategory(categoryActive);
        else if (categoryDefault)
            OpenCategory(categoryDefault);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        if (bShowSelectorOnlyWhenActive)
            goSelector.SetActive(false);
        categoryActive?.Hide();
    }

    public void SetEnabled(bool _bEnabled)
    {
        bEnabled = _bEnabled;
        goSelector.SetActive(_bEnabled);
    }

    public bool bIsEnabled()
    {
        return bEnabled;
    }

    public void LocalizeName(string _strName)
    {
        strName = _strName;
    }
}
