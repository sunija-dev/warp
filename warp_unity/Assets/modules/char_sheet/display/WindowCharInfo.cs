using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class WindowCharInfo : MonoBehaviour
{
    public float fFadeOutTime = 0.5f;
    public bool bIsVisible = false;

    [Header("References")]
    public GameObject goCharWindow;
    public TMP_Text textName;
    public TMP_Text textDescription;
    public AspectDisplay[] arAspectDisplays;
    public Tooltip hoverInfoKnownFor;
    public CanvasGroup canvasGroup;
    public WindowAccountInfo windowAccountInfo;
    public ContextMenu contextMenuOptions;

    private Coroutine m_coSetVisible = null;

    public CharSheet charSheetSelected = null;

    public void SetCharacter(CharSheet _charSheet, AccountSheet _accountSheet)
    {
        windowAccountInfo.SetAccount(_accountSheet);

        charSheetSelected = _charSheet;

        textName.text = _charSheet.strRPName;
        textDescription.text = _charSheet.strRPDesc;

        for (int i = 0; i < arAspectDisplays.Length; i++)
        {
            if (i >= _charSheet.liAspects.Count) // if aspects aren't set at the start
            {
                arAspectDisplays[i].SetAspect(new CharSheet.Aspect());
                continue;
            }

            CharSheet.Aspect aspect = _charSheet.liAspects[i];
            bool bVisible = !string.IsNullOrEmpty(aspect.strName) || !string.IsNullOrEmpty(aspect.strDesc);
            arAspectDisplays[i].gameObject.SetActive(bVisible);

            arAspectDisplays[i].SetAspect(aspect);
        }

        // TODO: Needs localization!
        hoverInfoKnownFor.strText = _charSheet.strKnownFor;

        contextMenuOptions.ClearOptions();
        contextMenuOptions.AddOptions(
            new ContextMenu.Option("report_player", () => { ClientManager.OpenReport(_charSheet.strRPName, _charSheet.ToString()); })
            //new ContextMenu.Option("Show Backstory", () => { Debug.Log("Show Backstory"); })
            );
    }

    public void Show()
    {
        bIsVisible = true;
        goCharWindow.SetActive(true);
        if (m_coSetVisible != null)
            StopCoroutine(m_coSetVisible);
        m_coSetVisible = StartCoroutine(coSetVisible(true));
    }

    public void Hide()
    {
        bIsVisible = false;

        if (!isActiveAndEnabled)
            return;

        if (m_coSetVisible != null)
            StopCoroutine(m_coSetVisible);
        m_coSetVisible = StartCoroutine(coSetVisible(false));

        charSheetSelected = null;
    }

    private IEnumerator coSetVisible(bool _bVisible)
    {
        if (_bVisible)
            goCharWindow.SetActive(true);

        float fStartTime = Time.time;
        float fEndTime = fStartTime + fFadeOutTime;
        float fTarget = _bVisible ? 0.9f : 0f;

        while (Time.time < fEndTime)
        {
            canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, fTarget, (Time.time - fStartTime) / fFadeOutTime);
            yield return null;
        }
        canvasGroup.alpha = fTarget;

        if (!_bVisible)
        {
            goCharWindow.SetActive(false);
            windowAccountInfo.Hide();
        }    
    }
}
