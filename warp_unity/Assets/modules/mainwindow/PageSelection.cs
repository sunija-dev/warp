using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PageSelection : MonoBehaviour
{
    [Header("References")]
    public GameObject goHighlightMask;
    public GameObject goHighlightBackground;
    public GameObject goHighlightImage;

    private float fXOffsetMask = 0f; // read from tests
    private float fXOffsetImage = -12.72345f; // read from tests

    public void HighlightPage(WarpPage _page)
    {
        StartCoroutine(coHighlightPage(_page));
    }

    public IEnumerator coHighlightPage(WarpPage _page)
    {
        yield return null; // HACKFIX: delay by one frame, so the ui is already built. Else it spawns on bottom left

        GameObject goSelector = _page.goSelector;

        goHighlightImage.SetActive(goSelector != null);
        goHighlightMask.SetActive(goSelector != null);

        // move mask (without moving background)
        if (fXOffsetMask == -1f || fXOffsetImage == -1f)
        {
            fXOffsetMask = goHighlightMask.transform.position.x - goSelector.transform.position.x;
            fXOffsetImage = goHighlightImage.transform.position.x - goSelector.transform.position.x;
        }

        goHighlightBackground.transform.SetParent(goHighlightBackground.transform.parent.parent, false);
        goHighlightMask.transform.position = goSelector.transform.position + Vector3.right * fXOffsetMask;
        goHighlightBackground.transform.SetParent(goHighlightMask.transform, false);

        // move highlight image
        goHighlightImage.transform.position = goSelector.transform.position + Vector3.right * fXOffsetImage;
    }
}
