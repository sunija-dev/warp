using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PageSelector : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    public WarpPage page;
    public UnityEngine.UI.Image imgIcon;

    private Color colorIcon;
    private float fHiddenAlpha = 0.66f;

    public void Start()
    {
        colorIcon = imgIcon.color;
        colorIcon.a = fHiddenAlpha;
        imgIcon.color = colorIcon;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        MainWindow.Instance.OpenPage(page);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        colorIcon.a = 1f;
        imgIcon.color = colorIcon;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        colorIcon.a = fHiddenAlpha;
        imgIcon.color = colorIcon;
    }
}
