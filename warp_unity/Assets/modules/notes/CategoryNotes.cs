using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CategoryNotes : MonoBehaviour
{
    public TMP_InputField inputNote;

    public GameObject goUnsavedChangesInfo;

    private bool bNotesChanged = false;
    private bool bUnsavedChanges = true;

    public void Awake()
    {
        ClientManager.Instance.eCharacterChanged.AddListener(ReloadInfo);

        inputNote.onValueChanged.AddListener((v) => SetUnsavedChanges(true));

        if (ClientManager.Instance.bCharLoggedIn)
            ReloadInfo();
    }

    public void Start()
    {
        ClientManager.Instance.eNotesUpdated.AddListener(() => bNotesChanged = true);
        ClientManager.Instance.eNotesUpdated.AddListener(ReloadInfo);
        ClientManager.eOnCtrlS.AddListener(() => { if (gameObject.activeInHierarchy) SaveNotes(); });
    }

    public void ReloadInfo()
    {
        inputNote.SetTextWithoutNotify(Player.Instance.strNotes);
        SetUnsavedChanges(false);
    }

    public void SaveNotes()
    {
        StartCoroutine(coSaveNotes());
    }

    private IEnumerator coSaveNotes()
    {
        Player.Instance.CmdUpdateNotes(inputNote.text);
        SetUnsavedChanges(false);

        bNotesChanged = false;
        yield return new WaitUntil(() => bNotesChanged);
        bNotesChanged = false;

        inputNote.SetTextWithoutNotify(Player.Instance.strNotes);
    }

    public void OnShowCategory()
    {
    }

    public void OnHideCategory()
    {
    }

    public void DiscardChanges()
    {
        ClientManager.Instance.windowPopup.Init(
                            new WindowPopup.ButtonInfo(true, () => ReloadInfo(), "yes"),
                            new WindowPopup.ButtonInfo(true, null, "no"),
                            "notes_discard");
    }

    public void SetUnsavedChanges(bool _bUnsaved)
    {
        if (_bUnsaved != bUnsavedChanges)
            goUnsavedChangesInfo.SetActive(_bUnsaved);

        bUnsavedChanges = _bUnsaved;
    }
}
