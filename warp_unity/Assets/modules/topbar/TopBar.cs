using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class TopBar : MonoBehaviour
{
    public static TopBar Instance;

    public float fHideAlpha = 0.66f;
    public float fIconDisplacementMax = 200f;

    [Header("References")]
    public CanvasGroup canvasGroup;
    public RectTransform rect;

    private Coroutine coFading;

    private bool bHoveringLast = false;
    private int iUiSize = 1;
    private int iUiSizeLast = 1;

    private void Awake()
    {
        Instance = this;
    }

    public void Start()
    {
        coFading = StartCoroutine(Fade(fHideAlpha));
    }

    public void Update()
    {
        bool bHovering = RectTransformUtility.RectangleContainsScreenPoint(rect, Mouse.current.position.ReadValue());

        if (bHovering != bHoveringLast)
        {
            if (bHovering) OnPointerEnter();
            else OnPointerExit();
        }

        bHoveringLast = bHovering;

    }

    public void UpdatePlacement()
    {
        rect.sizeDelta = new Vector2(322 + Settings.fRead(Settings.OptionKey.fIconDisplacement) * fIconDisplacementMax, rect.sizeDelta.y);
        iUiSizeLast = iUiSize;
    }

    public void OnPointerEnter()
    {
        if (coFading != null) StopCoroutine(coFading);
        coFading = StartCoroutine(Fade(1f));
    }

    public void OnPointerExit()
    {
        if (coFading != null) StopCoroutine(coFading);
        coFading = StartCoroutine(Fade(fHideAlpha));
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
