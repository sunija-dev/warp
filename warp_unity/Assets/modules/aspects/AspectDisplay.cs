using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class AspectDisplay : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public static AspectDisplay s_aspectHovering = null;

    public CharSheet.Aspect aspect;

    public GameObject goAspectWindowPrefab;
    public Image imgAspectDisplay;
    public GameObject goHoverPreviewPrefab;

    public UnityEvent eventOnSwitch; // TODO: this might rather be another script, so the aspectdisplay is really just a display
    public bool bAllowDragging = false;

    private AspectWindow aspectWindow;
    private GameObject goHoverPreview = null;

    public void SetAspect(CharSheet.Aspect _aspect)
    {
        aspect = _aspect;
        if (imgAspectDisplay)
            imgAspectDisplay.sprite = IconUtility.spriteLoadIcon(_aspect.iIconId);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        GameObject goAspectWindow = Instantiate(goAspectWindowPrefab, ClientManager.Instance.canvasHover.transform);
        aspectWindow = goAspectWindow.GetComponent<AspectWindow>();
        aspectWindow.Setup(aspect);
        s_aspectHovering = this;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        s_aspectHovering = null;
        DestroySave();
    }

    private void OnDisable()
    {
        DestroySave();
    }

    private void DestroySave()
    {
        if (aspectWindow)
            Destroy(aspectWindow.gameObject);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!bAllowDragging)
            return;

        if (goHoverPreview != null)
            Destroy(goHoverPreview);

        goHoverPreview = Instantiate(goHoverPreviewPrefab, ClientManager.Instance.canvasHover.transform);
        goHoverPreview.GetComponent<Image>().sprite = imgAspectDisplay.sprite;
        goHoverPreview.transform.position = transform.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!bAllowDragging)
            return;

        if (goHoverPreview)
            goHoverPreview.transform.position = eventData.position;
    }


    public void OnEndDrag(PointerEventData eventData)
    {
        if (!bAllowDragging)
            return;

        Destroy(goHoverPreview);
        goHoverPreview = null;

        if (s_aspectHovering)
            SwitchAspects(this, s_aspectHovering);
    }

    public void SwitchAspects(AspectDisplay aspectDisplay1, AspectDisplay aspectDisplay2)
    {
        aspectDisplay1.DestroySave();
        aspectDisplay2.DestroySave();
        CharSheet.Aspect aspect2 = aspectDisplay2.aspect;
        aspectDisplay2.SetAspect(aspectDisplay1.aspect);
        aspectDisplay1.SetAspect(aspect2);
        eventOnSwitch.Invoke();
    }
}
