using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using System.Linq;

public class CloseCharEntry : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
{
    public TMP_Text textCharName;
    public AspectDisplay[] arAspectDisplays;
    public Tooltip tooltipKnownFor;

    private CharSheet charSheet;

    public void Init(CharSheet _charSheet)
    {
        if (!gameObject) /*|| !gameObject.activeInHierarchy)*/
            return;

        charSheet = _charSheet;

        textCharName.text = _charSheet.strRPName;
        tooltipKnownFor.strText = _charSheet.strKnownFor;

        for (int i = 0; i < arAspectDisplays.Length; i++)
        {
            CharSheet.Aspect aspect = _charSheet.liAspects[i];
            bool bVisible = !string.IsNullOrEmpty(aspect.strName) || !string.IsNullOrEmpty(aspect.strDesc);
            arAspectDisplays[i].gameObject.SetActive(bVisible);

            arAspectDisplays[i].SetAspect(aspect);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Player player = ClientManager.s_liClosePlayers.FirstOrDefault(x => x.Character.CharSheet.strRPName == charSheet.strRPName);
        if (player == default)
            return;

        CharHoverManager charHoverManager = CharHoverManager.Instance;
        if (charHoverManager.windowCharInfo.charSheetSelected != null
                && charHoverManager.windowCharInfo.charSheetSelected.strRPName == player.Character.CharSheet.strRPName)
        {
            // char already selected? close it
            charHoverManager.windowCharInfo.Hide();
        }
        else if (player != default)
        {
            CharHoverManager.Instance.SelectChar(player.transform, true);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        
    }

    void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
    {

    }
}
