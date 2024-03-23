using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Atm only scales y direction
/// </summary>
public class ScaleWithOther : MonoBehaviour
{
    public RectTransform rectThis;
    public RectTransform rectOther;

    public Vector2 v2MaxSize = Vector2.zero;
    public Vector2 v2Offset = Vector2.zero;
    

    private Vector2 v2LastSizeOther = Vector2.zero;
    private Vector2 v2RectThis = Vector2.zero;

    void Update()
    {
        if (rectOther.hasChanged
            && (v2LastSizeOther.x != rectOther.sizeDelta.x || v2LastSizeOther.y != rectOther.sizeDelta.y)) // kinda stays true
        {
            float fTargetSizeY = rectOther.rect.height;
            if (v2MaxSize.y != 0)
                fTargetSizeY = Mathf.Min(fTargetSizeY, v2MaxSize.y);

            v2RectThis.x = rectThis.sizeDelta.x + v2Offset.x;
            v2RectThis.y = fTargetSizeY + v2Offset.y;
            rectThis.sizeDelta = v2RectThis;
            v2LastSizeOther = rectOther.sizeDelta;
            //Debug.Log($"{gameObject.name}: Switched size to {rectThis.sizeDelta.y}");
        }
    }
}
