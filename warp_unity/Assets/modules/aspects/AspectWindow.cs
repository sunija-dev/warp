using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class AspectWindow : MonoBehaviour
{
    public Vector2 v2Padding = Vector2.zero;
    public float fExtraYSpace = 100f;

    [Header("References")]
    public Image imgIcon;
    public TMP_Text textName;
    public TMP_Text textDesc;

    public GameObject goBackgroundMask;

    private RectTransform rectMask;
    private RectTransform rectParent;
    private RectTransform rect;
    private Vector2 v2MinSize = Vector2.zero;

    void Awake()
    {
        rectMask = goBackgroundMask.GetComponent<RectTransform>();
        rectParent = GetComponent<RectTransform>();
        rect = GetComponent<RectTransform>();
        v2MinSize = rect.sizeDelta;
    }

    public void Setup(CharSheet.Aspect _aspect)
    {
        this.gameObject.SetActive(true);
        imgIcon.sprite = IconUtility.spriteLoadIcon(_aspect.iIconId);
        textName.SetText(_aspect.strName);
        textDesc.SetText(_aspect.strDesc);
        textDesc.ForceMeshUpdate();
        Vector2 v2TextSize = textDesc.GetRenderedValues(false);

        if (v2TextSize.y + v2Padding.y + fExtraYSpace > v2MinSize.y)
        {
            Vector2 v2NewSize = v2TextSize + v2Padding;
            v2NewSize.x = v2MinSize.x;
            //rectMask.sizeDelta = v2NewSize;
            rect.sizeDelta = v2NewSize + Vector2.up * fExtraYSpace;
        }

        UpdatePosition();
    }

    private void UpdatePosition()
    {
        int iYOffset = 0;
        Vector3 v3Position = Mouse.current.position.ReadValue() + rect.sizeDelta / 2f;
        v3Position.y -= rect.sizeDelta.y - iYOffset;

        if (v3Position.x - rectMask.localPosition.x + rectMask.sizeDelta.x / 2f > Screen.width) v3Position.x -= rectMask.sizeDelta.x;
        if (v3Position.y - rectMask.localPosition.y - rectParent.sizeDelta.y / 2f < 0) v3Position.y += rectParent.sizeDelta.y + iYOffset * 2f;

        transform.position = v3Position;
    }

    private void Update()
    {
        UpdatePosition();
    }
}
