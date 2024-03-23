using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CategorySelector : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    [Header("References")]
    public WarpPage page;
    public WarpCategory category;
    public GameObject goHighlight;

    public bool bActive = false;

    public void OnPointerDown(PointerEventData eventData)
    {
        page.OpenCategory(category);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        goHighlight.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!bActive)
            goHighlight.SetActive(false);
    }
}
