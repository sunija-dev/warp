using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Blish_HUD;

public class GridIcon : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public int iIcon = 0;
    public Image imgHighlight;
    public float fHighlightSpeed = 0.2f;
    public ContextMenu contextMenu;
    public Tooltip tooltip;

    public event EventHandler<int> eOnClick;

    private Color colorHighlight = Color.white;
    private Coroutine coHighlightChange = null;


    void Start()
    {
        contextMenu.ClearOptions();
        contextMenu.AddOptions(
            new ContextMenu.Option("copy_id", () => { ClipboardUtil.WindowsClipboardService.SetTextAsync(iIcon.ToString()); }),
            new ContextMenu.Option("compare", () => { WindowSelectIcon.Instance.bDontScrollOnNextSearch = true; WindowSelectIcon.Instance.ToggleTag(iIcon.ToString()); })
            );
    }

    void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
            eOnClick?.Invoke(this, iIcon);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // only for click handler
    }

    void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
    {
        // only for click handler
    }

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    {
        ChangeHighlight(false);
    }

    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
        ChangeHighlight(true);
    }

    private void ChangeHighlight(bool _bOn)
    {
        if (coHighlightChange != null)
            StopCoroutine(coHighlightChange);
        coHighlightChange = StartCoroutine(coChangeHighlight(_bOn));
    }

    IEnumerator coChangeHighlight(bool _bOn)
    {
        if (_bOn && !imgHighlight.gameObject.activeInHierarchy)
            imgHighlight.gameObject.SetActive(true);

        while (true)
        {
            colorHighlight.a = Mathf.Lerp(imgHighlight.color.a, _bOn ? 1f : 0f, fHighlightSpeed * Time.deltaTime);
            imgHighlight.color = colorHighlight;

            if (_bOn && imgHighlight.color.a > 0.99f)
            {
                colorHighlight.a = 1f;
                imgHighlight.color = colorHighlight;
                break;
            }
            else if (!_bOn && imgHighlight.color.a < 0.01f)
            {
                colorHighlight.a = 0f;
                imgHighlight.color = colorHighlight;
                imgHighlight.gameObject.SetActive(false);
                break;
            }

            yield return null;
        }
    }
}
