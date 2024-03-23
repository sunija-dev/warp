using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Coffee.UIExtensions;
using UnityEngine.UI;


public class ButtonEffect : MonoBehaviour
{
    public SoftMask softMask;
    public Image imageColor;

    public Color colorClick;
    public Color colorNormal;

    public float fMaskFadeTime = 1f;
    public AnimationCurve animMaskFade;
    public float fColorFadeTime = 0.2f;
    public AnimationCurve animColorFade;

    private float fMaskFadeCurrent = 0f;
    private Coroutine m_coChangeMask;
    private Coroutine m_coChangeColor;


    private IEnumerator coChangeMask(bool _bFadeIn)
    {
        float fTarget = _bFadeIn ? 1f : 0f;
        while (Mathf.Abs(fTarget - fMaskFadeCurrent) > 0.02f)
        {
            if (_bFadeIn)
                fMaskFadeCurrent += Time.deltaTime / fMaskFadeTime;
            else
                fMaskFadeCurrent -= Time.deltaTime / fMaskFadeTime;

            fMaskFadeCurrent = Mathf.Clamp01(fMaskFadeCurrent);
            softMask.alpha = animMaskFade.Evaluate(fMaskFadeCurrent);
            softMask.softness = 1f - animMaskFade.Evaluate(fMaskFadeCurrent);
            yield return null;
        }
        fMaskFadeCurrent = fTarget;
        softMask.alpha = animMaskFade.Evaluate(fMaskFadeCurrent);
        softMask.softness = 1f - animMaskFade.Evaluate(fMaskFadeCurrent);
    }

    public void OnPointerEnter()
    {
        if (m_coChangeMask != null)
            StopCoroutine(m_coChangeMask);
        m_coChangeMask = StartCoroutine(coChangeMask(true));
    }

    public void OnPointerExit()
    {
        if (m_coChangeMask != null)
            StopCoroutine(m_coChangeMask);
        m_coChangeMask = StartCoroutine(coChangeMask(false));
    }

    public void OnPointerDown()
    {
        imageColor.color = colorClick;
    }

    public void OnPointerUp()
    {
        imageColor.color = colorNormal;
    }
}
