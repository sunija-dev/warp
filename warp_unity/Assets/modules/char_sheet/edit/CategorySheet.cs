using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CategorySheet : MonoBehaviour
{
    public TMP_Text textCharName;
    public TMP_InputField inputCharDesc;
    public TMP_InputField inputKnownFor;
    public Image[] arAspectImages;

    public GameObject goFirstGlanceEdit;
    public CopySheetPopup copySheetPopup;
    public TMP_InputField inputGlanceName;
    public TMP_InputField inputGlanceDesc;
    public IconSelector iconSelector;
    public Button buttonFirstGlanceClose;
    public AspectDisplay[] arAspectDisplays = new AspectDisplay[4];
    public GameObject goUnsavedChangesInfo;
    public WindowCharInfo windowCharInfo;
    public ContextMenu contextMenuOptions;

    private bool bUnsavedChanges = true;
    private bool bCharSheetChanged = false;

    public void Awake()
    {
        ClientManager.Instance.eCharSheetChanged.AddListener(ReloadInfo);

        contextMenuOptions.ClearOptions();
        contextMenuOptions.AddOptions(
            new ContextMenu.Option("copy_sheet", () => copySheetPopup.Open())
            //new ContextMenu.Option("Show Backstory", () => { Debug.Log("Show Backstory"); })
            );

        inputCharDesc.onValueChanged.AddListener((v) => SetUnsavedChanges(true));
        inputKnownFor.onValueChanged.AddListener((v) => SetUnsavedChanges(true));

        if (ClientManager.Instance.bCharLoggedIn)
            ReloadInfo();
    }

    public void Start()
    {
        ClientManager.Instance.eCharSheetChanged.AddListener(() => bCharSheetChanged = true);
        ClientManager.Instance.eCharSheetChanged.AddListener(ReloadInfo);
        ClientManager.eOnCtrlS.AddListener(() => { if (gameObject.activeInHierarchy) SaveSheet(); });
    }

    public void ReloadInfo()
    {
        Character character = Player.Instance.Character;
        AccountSheet accountSheet = Player.Instance.accountSheet;
        windowCharInfo.SetCharacter(character.charSheet, accountSheet);
        if (gameObject.activeInHierarchy)
            windowCharInfo.Show();

        Character charActive = character;
        if (charActive != null)
        {
            if (charActive.charInfo != null && !string.IsNullOrEmpty(charActive.charInfo.name))
                textCharName.text = charActive.charInfo.name;

            if (charActive.charSheet != null)
                SetCharSheet(charActive.charSheet);
        }

        SetUnsavedChanges(false);
    }

    public void SetCharSheet(CharSheet _charSheet)
    {
        inputCharDesc.text = _charSheet.strRPDesc;
        inputKnownFor.text = _charSheet.strKnownFor;

        // reset aspects first
        CharSheet.Aspect aspectDefault = new CharSheet.Aspect();
        for (int i = 0; i < arAspectDisplays.Length; i++)
        {
            arAspectImages[i].sprite = IconUtility.spriteLoadIcon(aspectDefault.iIconId);
            arAspectDisplays[i].aspect = aspectDefault;
        }

        for (int i = 0; i < _charSheet.liAspects.Count; i++)
        {
            arAspectImages[i].sprite = IconUtility.spriteLoadIcon(_charSheet.liAspects[i].iIconId);
            arAspectDisplays[i].aspect = _charSheet.liAspects[i];
        }
    }


    public void OpenEditAspect(int _iAspect)
    {
        goFirstGlanceEdit.SetActive(true);

        inputGlanceName.text = arAspectDisplays[_iAspect].aspect.strName;
        inputGlanceDesc.text = arAspectDisplays[_iAspect].aspect.strDesc;
        iconSelector.SetIcon(arAspectDisplays[_iAspect].aspect.iIconId);

        buttonFirstGlanceClose.onClick.RemoveAllListeners();
        buttonFirstGlanceClose.onClick.AddListener(() => CloseEditAspect(_iAspect));
    }

    public void CloseEditAspect(int _iAspect)
    {
        goFirstGlanceEdit.SetActive(false);
        arAspectDisplays[_iAspect].aspect = new CharSheet.Aspect()
        {
            strName = inputGlanceName.text,
            strDesc = inputGlanceDesc.text,
            iIconId = iconSelector.iSelectedIcon
        };

        arAspectImages[_iAspect].sprite = IconUtility.spriteLoadIcon(iconSelector.iSelectedIcon);
        SetUnsavedChanges(true);
    }

    public void SaveSheet()
    {
        StartCoroutine(coSaveSheet());
    }

    private IEnumerator coSaveSheet()
    {
        List<CharSheet.Aspect> liAspects = new List<CharSheet.Aspect>();
        for (int i = 0; i < arAspectDisplays.Length; i++)
            liAspects.Add(arAspectDisplays[i].aspect);

        Player.Instance.CmdUpdateSheet(inputCharDesc.text, inputKnownFor.text, liAspects.ToArray());
        SetUnsavedChanges(false);

        bCharSheetChanged = false;
        yield return new WaitUntil(() => bCharSheetChanged);
        bCharSheetChanged = false;

        // show your updated character info 
        Character character = Player.Instance.GetComponent<Character>();
        AccountSheet accountSheet = Player.Instance.GetComponent<Player>().accountSheet;
        windowCharInfo.SetCharacter(character.charSheet, accountSheet);
        windowCharInfo.Show();
    }

    public void OnShowCategory()
    {
        // don't update, so previous changes are not deleted
        windowCharInfo.Show();
    }

    public void OnHideCategory()
    {
        windowCharInfo.Hide();
    }

    public void DiscardChanges()
    {
        ClientManager.Instance.windowPopup.Init(
                            new WindowPopup.ButtonInfo(true, () => ReloadInfo(), "yes"),
                            new WindowPopup.ButtonInfo(true, null, "no"),
                            "sheet_discard");
    }

    public void SetUnsavedChanges(bool _bUnsaved)
    {
        if (_bUnsaved != bUnsavedChanges)
            goUnsavedChangesInfo.SetActive(_bUnsaved);

        bUnsavedChanges = _bUnsaved;
    }
}
