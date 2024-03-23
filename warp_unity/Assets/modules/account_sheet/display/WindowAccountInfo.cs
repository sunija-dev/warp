using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WindowAccountInfo : MonoBehaviour
{
    public float fFadeOutTime = 0.5f;
    public bool bIsVisible = false;

    [Header("References")]
    public GameObject goAccountWindow;
    public CanvasGroup canvasGroup;

    public TMP_Text textExperienced;
    public TMP_Text textFightingStyle;
    public TMP_Text textAdultStatus;

    public Slider sliderLoreStrictness;
    public Slider sliderTopic;
    public Slider sliderStyle;
    public Slider sliderPostLength;

    public TMP_Text textExamplePost;

    private AccountSheet accountSheet;
    private Coroutine m_coSetVisible = null;

    public void SetAccount(AccountSheet _accountSheet)
    {
        accountSheet = _accountSheet;

        // TODO: Needs localization!
        textExperienced.text = accountSheet.experience.ToString();
        textFightingStyle.text = accountSheet.fightingStyle.ToString();
        textAdultStatus.text = accountSheet.adultStatus.ToString();

        sliderLoreStrictness.value = accountSheet.fLoreStrictness;
        sliderTopic.value = accountSheet.fTopic;
        sliderStyle.value = accountSheet.fStyle;
        sliderPostLength.value = accountSheet.fPostLength;

        textExamplePost.text = accountSheet.strExamplePost;
    }

    // Visualization

    public void Show()
    {
        bIsVisible = true;
        goAccountWindow.SetActive(true);
        if (m_coSetVisible != null)
            StopCoroutine(m_coSetVisible);
        m_coSetVisible = StartCoroutine(coSetVisible(true));
    }

    public void Hide()
    {
        bIsVisible = false;

        if (!isActiveAndEnabled)
            return;

        if (m_coSetVisible != null)
            StopCoroutine(m_coSetVisible);
        m_coSetVisible = StartCoroutine(coSetVisible(false));
    }

    private IEnumerator coSetVisible(bool _bVisible)
    {
        if (_bVisible)
            goAccountWindow.SetActive(true);

        float fStartTime = Time.time;
        float fEndTime = fStartTime + fFadeOutTime;
        float fTarget = _bVisible ? 0.8f : 0f;

        while (Time.time < fEndTime)
        {
            canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, fTarget, (Time.time - fStartTime) / fFadeOutTime);
            yield return null;
        }
        canvasGroup.alpha = fTarget;

        if (!_bVisible)
            goAccountWindow.SetActive(false);
    }
}
