using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class IconSelector : MonoBehaviour, IPointerClickHandler
{
    public int iSelectedIcon = 0;
    public Image imageIcon;
    public GameObject goIconSelectionPrefab;

    public void OnPointerClick(PointerEventData eventData)
    {
        GameObject goWindowSelectIcon = Instantiate(goIconSelectionPrefab, transform.position, Quaternion.identity, ClientManager.Instance.canvasPopupIcons.transform);
        goWindowSelectIcon.GetComponent<WindowSelectIcon>().Setup(iSelectedIcon);
        WindowSelectIcon selectIcon = goWindowSelectIcon.GetComponent<WindowSelectIcon>();
        selectIcon.eOnClickOk += (e, iIcon) => SetIcon(iIcon);
    }

    public void SetIcon(int _iIcon)
    {
        iSelectedIcon = _iIcon;
        imageIcon.sprite = IconUtility.spriteLoadIcon(_iIcon);
    }
}
