using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;

public class CloseCharListWindow : MonoBehaviour
{
    public static CloseCharListWindow Instance;

    public float fRotationAnimationSpeed = 10f;

    public TMP_Text textCharNumber;
    public GameObject goCloseCharEntryPrefab;
    public Transform transEntryParent;
    public CanvasGroup canvasGroup;
    public GameObject goFoldContent;
    public Transform transFoldButton;
    public GameObject goCloseButton;

    private bool bFoldedOut = true;
    private bool bVisible = true;
    public bool bShouldBeVisible = true;
    private bool bBlockFold = false;

    private List<GameObject> liEntryGOs = new List<GameObject>();
    private Coroutine coFoldAnimation;
    private float fTargetRotation;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        ClientManager.s_eClosePlayersUpdated.AddListener(RebuildList);
        RebuildList();
        HideCloseButton();
        Hide();
    }

    public void RebuildList()
    {
        textCharNumber.text = $"Chars: {ClientManager.s_liClosePlayers.Count}";

        // create all new entries
        if (bFoldedOut)
        {
            ClearList();

            foreach (Player player in ClientManager.s_liClosePlayers)
            {
                GameObject goEntry = Instantiate(goCloseCharEntryPrefab, transEntryParent);
                CloseCharEntry closeCharEntry = goEntry.GetComponent<CloseCharEntry>();
                closeCharEntry.Init(player.Character.CharSheet);
                player.Character.eCharSheetChanged.AddListener(() => { if (closeCharEntry != null) closeCharEntry.Init(player.Character.CharSheet); }); // maybe performance horrible, because it creates anonymous methods that never get deleted
                liEntryGOs.Add(goEntry);
            }
        }

        // hide list, if it is open, but there is only one person (you) in the list
        if (ClientManager.s_liClosePlayers.Count <= 1)
            Hide(bShouldBeVisible);
        else if (bShouldBeVisible)
            Show(bShouldBeVisible);
    }

    public void Show()
    {
        Show(_bShouldBeVisible: true);
    }

    public void Hide()
    {
        Hide(_bShouldBeVisible: false);
    }

    public void Show(bool _bShouldBeVisible)
    {
        gameObject.SetActive(true);
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        bShouldBeVisible = _bShouldBeVisible;
        bVisible = true;
    }

    public void Hide(bool _bShouldBeVisible)
    {
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        bShouldBeVisible = _bShouldBeVisible;
        bVisible = false;
        gameObject.SetActive(false);
    }

    public void OnDrag()
    {
        bBlockFold = true;
    }

    public void ToggleFold()
    {
        if (bBlockFold)
        {
            bBlockFold = false;
            return;
        }

        if (bFoldedOut)
            FoldIn();
        else
            FoldOut();
    }

    public void FoldOut()
    {
        bFoldedOut = true;
        goFoldContent.SetActive(true);
        RebuildList();

        fTargetRotation = 90;
        if (coFoldAnimation == null)
            coFoldAnimation = StartCoroutine(coFoldButtonRotation());
    }

    public void FoldIn()
    {
        bFoldedOut = false;
        goFoldContent.SetActive(false);
        ClearList();

        fTargetRotation = -90;
        if (coFoldAnimation == null)
            coFoldAnimation = StartCoroutine(coFoldButtonRotation());
    }

    private void ClearList()
    {
        liEntryGOs.ForEach(x => Destroy(x));
        liEntryGOs.Clear();
    }

    private IEnumerator coFoldButtonRotation()
    {
        while (Mathf.Abs(fTargetRotation - transFoldButton.rotation.eulerAngles.z) > 1f)
        {
            transFoldButton.rotation = Quaternion.Euler(0, 0, Mathf.LerpAngle(transFoldButton.rotation.eulerAngles.z, fTargetRotation, fRotationAnimationSpeed * Time.deltaTime));
            yield return null;
        }
        coFoldAnimation = null;
    }

    public void HideCloseButton()
    {
        goCloseButton.SetActive(false);
    }

    public void ShowCloseButton()
    {
        goCloseButton.SetActive(true);
    }
}
