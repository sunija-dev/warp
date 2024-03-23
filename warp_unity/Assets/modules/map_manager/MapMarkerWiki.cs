using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Linq;

public class MapMarkerWiki : MonoBehaviour, IPointerUpHandler, IPointerDownHandler, IPointerClickHandler
{

    [SerializeField] private Tooltip toolTip;
    private WikiMap.WikiMapEntry entry;

    void Update()
    {
        UpdatePosition();
    }

    public void UpdateData(WikiMap.WikiMapEntry _entry)
    {
        entry = _entry;
        toolTip.strText = $"{entry.strName}\n\n{_entry.strWikiText}\n<alpha=#66>From the official GW2 wiki.\nClick to read full article.";
    }

    private void UpdatePosition()
    {
        float fSmallWindowScale = 1f;
        if (Screen.width < 1024 || Screen.height < 768) // hardcoded in GW2
            fSmallWindowScale = Mathf.Min(Screen.width / 1024f, Screen.height / 768f);

        Vector3 v3Position = (entry.v2Coord - MumbleManager.Instance.v2MapCenter) / MumbleManager.s_gw2Info.mapScale * fSmallWindowScale;

        v3Position.y = (Screen.height / 2f) - v3Position.y;
        v3Position.x += Screen.width / 2f;

        gameObject.transform.position = v3Position;
    }

    private void OpenWikiArticle()
    {
        Application.OpenURL(entry.strWikiUrl);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        OpenWikiArticle();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        //
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        //
    }
}
