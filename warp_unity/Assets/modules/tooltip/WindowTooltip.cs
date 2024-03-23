using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class WindowTooltip : MonoBehaviour
{
    public Vector2 v2Padding = Vector2.zero;

    [Header("References")]
    public GameObject goBackgroundMask;
    public GameObject goOutline;
    public TMP_Text textInfo;

    private Vector2 v2OriginalScaleBackground = Vector2.zero;
    private RectTransform rectMask;
    private RectTransform rect;

    void Awake()
    {
        rectMask = goBackgroundMask.GetComponent<RectTransform>();
        rect = GetComponent<RectTransform>();
    }

    public void Setup(string _strText)
    {
        // adapt background to text size
        this.gameObject.SetActive(true);
        textInfo.SetText(_strText);
        textInfo.ForceMeshUpdate();
        Vector2 v2TextSize = textInfo.GetRenderedValues(false);
        RectTransform rectOutline = goOutline.GetComponent<RectTransform>();
        v2OriginalScaleBackground = rectMask.sizeDelta;
        rectMask.sizeDelta = v2TextSize + v2Padding;
        rectOutline.sizeDelta = v2TextSize + v2Padding;
        Vector3 v3NewPos = Vector3.zero;
        float fDpiScaling = GameIntegration.s_fCanvasScaling;
        v3NewPos.y = rectMask.position.y + (v2OriginalScaleBackground - v2TextSize - v2Padding).y / 2f * fDpiScaling;
        v3NewPos.x = rectMask.position.x + (-v2OriginalScaleBackground + v2TextSize + v2Padding).x / 2f * fDpiScaling;

        rectMask.position = v3NewPos;
        rectOutline.position = rectMask.position;
    }

    private void Update()
    {
        float fDpiScaling = GameIntegration.s_fCanvasScaling;
        int iYOffset = 50;
        Vector2 v2MousePos = Mouse.current.position.ReadValue();
        Vector2 v2Position = v2MousePos;
        v2Position.y -= iYOffset;

        if (v2Position.x + rectMask.sizeDelta.x > Screen.width) v2Position.x = v2MousePos.x - rectMask.sizeDelta.x;
        if (v2Position.y - rectMask.sizeDelta.y < 0) v2Position.y = v2MousePos.y + rectMask.sizeDelta.y * fDpiScaling;

        transform.position = v2Position;
    }
}
