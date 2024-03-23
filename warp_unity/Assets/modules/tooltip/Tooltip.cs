using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Tooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public string strText = "HoverInfo";

    private WindowTooltip windowTooltip;

    public void OnPointerEnter(PointerEventData eventData)
    {
        GameObject goTooltip = Instantiate(ClientManager.Instance.goTooltipPrefab, ClientManager.Instance.canvasHover.transform);
        windowTooltip = goTooltip.GetComponent<WindowTooltip>();
        windowTooltip.Setup(strText);
    }

    /// <summary>
    /// For localization.
    /// </summary>
    public void UpdateText(string _strText)
    {
        strText = _strText;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        DestroySave();
    }

    public void OnDisable()
    {
        DestroySave();
    }

    private void DestroySave()
    {
        if (windowTooltip)
            Destroy(windowTooltip.gameObject);
    }
}
