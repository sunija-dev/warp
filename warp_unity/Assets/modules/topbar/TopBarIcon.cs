using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class TopBarIcon : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    public float fScaleUp = 1.2f;
    public Color colorDefault;
    public Color colorHover;

    public UnityEvent OnClick = new UnityEvent();

    [Header("References")]
    public UnityEngine.UI.Image image;

    private Coroutine coScaling;

    public void OnPointerDown(PointerEventData eventData)
    {
        OnClick.Invoke();
        image.color = colorDefault;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (coScaling != null) StopCoroutine(coScaling);
        coScaling = StartCoroutine(Scale(fScaleUp));
        image.color = colorHover;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (coScaling != null) StopCoroutine(coScaling);
        coScaling = StartCoroutine(Scale(1f));
        image.color = colorDefault;
    }

    private IEnumerator Scale(float _fTarget, float _fLerpTime = 0.05f)
    {
        float fStart = transform.localScale.x;
        float fTimeStarted = Time.time;

        while (fTimeStarted + _fLerpTime > Time.time)
        {
            transform.localScale = Vector3.one * Mathf.Lerp(fStart, _fTarget, (Time.time - fTimeStarted) / _fLerpTime);
            yield return null;
        }

        transform.localScale = Vector3.one * _fTarget;
    }
}
