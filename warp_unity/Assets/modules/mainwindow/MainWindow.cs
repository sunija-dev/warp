using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MainWindow : MonoBehaviour
{
    public static MainWindow Instance;

    public WarpPage pageDefault;

    [Header("References")]
    public CanvasGroup canvasGroup;
    public PageSelection pageSelection;
    public TMP_Text textPageTitle;
    public List<WarpPage> liPages;

    private Coroutine coFading = null;
    private WarpPage pageActive = null;
    public WarpCategory categoryActive = null;
    private bool bIsOpen = true;

    private void Awake()
    {
        Instance = this;
    }

    IEnumerator Start()
    {
        yield return null;

        if (bIsOpen) Open();
        else Close();
    }

    public void SetVisiblityForAllPageSelectors(bool _bVisible)
    {
        foreach (WarpPage page in liPages)
        {
            bool bShow = page.bIsEnabled() && _bVisible;
            page.goSelector.SetActive(bShow);
        }
    }

    public void OpenPage(WarpPage _page)
    {
        pageActive?.Hide();
        pageActive = _page;

        pageActive.Show();
        pageSelection.HighlightPage(_page);
        textPageTitle.text = _page.strName;  
    }

    public void Toggle()
    {
        if (bIsOpen) Close();
        else Open();
    }

    public void Open()
    {
        bIsOpen = true;
        if (coFading != null) StopCoroutine(coFading);
        canvasGroup.blocksRaycasts = true;
        coFading = StartCoroutine(Fade(1f));

        // outside of screen? move to center. Used at the start
        RectTransform rect = GetComponent<RectTransform>();
        if (rect.position.x < -rect.sizeDelta.x
            || rect.position.x > (Screen.width + rect.sizeDelta.x) / 2f
            || rect.position.y < -rect.sizeDelta.y
            || rect.position.y > (Screen.height + rect.sizeDelta.y) / 2f)
            rect.position = new Vector2(Screen.width / 2f, Screen.height / 2f);

        pageActive?.Show();
        categoryActive?.Show();
    }

    public void Close()
    {
        bIsOpen = false;
        if (coFading != null) StopCoroutine(coFading);
        canvasGroup.blocksRaycasts = false;
        coFading = StartCoroutine(Fade(0f));

        pageActive?.Hide();
        categoryActive?.Hide();
    }

    private IEnumerator Fade(float _fTarget, float _fLerpTime = 0.2f)
    {
        float fStart = canvasGroup.alpha;
        float fTimeStarted = Time.time;

        while (fTimeStarted + _fLerpTime > Time.time)
        {
            canvasGroup.alpha = Mathf.Lerp(fStart, _fTarget, (Time.time - fTimeStarted) / _fLerpTime);
            yield return null;
        }

        canvasGroup.alpha = _fTarget;
    }
}
