using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HighlightOnHover : MonoBehaviour
{
    public Color colorHighlight = Color.white;
    public Color colorNormal = Color.white;
    public float fLerpSpeed = 2f;
    private Image image;
    private bool bHighlight = false;

    private void Start()
    {
        image = GetComponent<Image>();
    }

    public void SetHighlight(bool _bHighlight)
    {
        bHighlight = _bHighlight;
    }

    void Update()
    {
        Color colorTarget = bHighlight ? colorHighlight : colorNormal;
        image.color = Color.Lerp(image.color, colorTarget, fLerpSpeed * Time.deltaTime);
    }
}
