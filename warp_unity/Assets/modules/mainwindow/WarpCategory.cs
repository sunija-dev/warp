using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class WarpCategory : MonoBehaviour
{
    public string strName = "CategoryTitle";
    public UnityEvent eventOnShow;
    public UnityEvent eventOnHide;

    [Header("References")]
    public CategorySelector selector;
    public CanvasGroup canvasGroup;

    private Coroutine coFading;


    public void Show()
    {
        gameObject.SetActive(true);
        if (coFading != null) StopCoroutine(coFading);
        canvasGroup.alpha = 0f;
        coFading = StartCoroutine(Fade(1f));

        if (selector)
        {
            selector.bActive = true;
            selector.goHighlight.SetActive(true);
        }

        eventOnShow.Invoke();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        selector.bActive = false;
        selector.goHighlight.SetActive(false);
        eventOnHide.Invoke();
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
