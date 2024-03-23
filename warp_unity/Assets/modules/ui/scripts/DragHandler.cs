using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// From: https://forum.unity.com/threads/drag-window-script-with-window-clamped-within-canvas.453340/
public class DragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    
    public bool bRememberPosition = false;
    public Settings.OptionKey optionPosX;
    public Settings.OptionKey optionPosY;
    public float m_fAllowPercentageOffscreen = 0.7f;
    public bool m_bStayInWindow = true;
    public RectTransform rtransPanel;

    private Vector2 m_v2PointerOffset;
    private RectTransform rtransCanvas;

    public void Start()
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null)
            rtransCanvas = canvas.transform as RectTransform;

        if (!rtransPanel)
            rtransPanel = transform as RectTransform;

        if (bRememberPosition)
        {
            if (Settings.Instance.bLoaded)
                LoadPositionFromSettings();
            Settings.eLoaded.AddListener(LoadPositionFromSettings);
        }

        if (m_bStayInWindow)
            rtransPanel.localPosition = SuUtility.v2ClampToWindow(rtransPanel, m_fAllowPercentageOffscreen);
    }

    private void LoadPositionFromSettings()
    {
        rtransPanel.localPosition = new Vector2(Settings.fRead(optionPosX), Settings.fRead(optionPosY));
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        //panelRectTransform.SetAsLastSibling();
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rtransPanel, eventData.position, eventData.pressEventCamera, out m_v2PointerOffset);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (rtransPanel == null)
            return;

        Vector2 v2LocalPointerPosition;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rtransCanvas, eventData.position, eventData.pressEventCamera, out v2LocalPointerPosition))
        {
            rtransPanel.localPosition = v2LocalPointerPosition - m_v2PointerOffset;
            if (m_bStayInWindow)
                rtransPanel.localPosition = SuUtility.v2ClampToWindow(rtransPanel, m_fAllowPercentageOffscreen);
        }
    }


    public void OnEndDrag(PointerEventData eventData)
    {
        if (bRememberPosition)
            SavePosition();
    }

    public void SavePosition()
    {
        Settings.WriteFloat(optionPosX, rtransPanel.localPosition.x);
        Settings.WriteFloat(optionPosY, rtransPanel.localPosition.y);
    }

    public void Move(Vector3 _v3Position)
    {
        if (!rtransPanel)
            rtransPanel = transform as RectTransform;
        rtransPanel.localPosition = _v3Position;
        SavePosition();
    }


}
