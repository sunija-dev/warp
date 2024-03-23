using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class WindowSelectIcon : MonoBehaviour
{
    public static WindowSelectIcon Instance;
    public static string s_strLastSearch = "";

    public int iDisplayedIcons = 36;
    public event System.EventHandler<int> eOnClickOk;
    
    [Header("References")]
    public GameObject goGrid;
    public GameObject goGridIconPrefab;
    public GameObject goSelectionHighlight;
    public TMP_InputField inputSearch;
    public ScrollRect scrollRect;

    public bool bDontScrollOnNextSearch = false;

    private List<int> liIconIds; // cached
    private List<int> liIconIdsFound = new List<int>(); // result of the search 
    private List<GameObject> liGridIcons = new List<GameObject>();

    private int iSelectedIcon = 0;
    private int iLastStartIndex = 0;

    private bool bUpdateDisplayedIcons = false;
    private bool bSearchAtEndOfFrame = false;

    private void Start()
    {
        if (Instance != null) // close other selection window, if still open
            Instance.Close();

        Instance = this;

        inputSearch.onValueChanged.AddListener((s) => SearchAtEndOfFrame());
        scrollRect.verticalScrollbar.onValueChanged.AddListener(OnScroll);
    }

    public void LateUpdate()
    {
        if (bSearchAtEndOfFrame)
        {
            Search();
            bSearchAtEndOfFrame = false;
        }

        if (bUpdateDisplayedIcons)
        {
            bUpdateDisplayedIcons = false;
            UpdateDisplayedIcons();
        }
    }

    public void Setup(int _iIconId)
    {
        liIconIds = ClientManager.Instance.liIconList;
        StartCoroutine(coMoveHighlightDelayed());
        inputSearch.text = s_strLastSearch;
        iSelectedIcon = _iIconId;
        Search();
    }

    private void OnScroll(float _fValue)
    {
        bUpdateDisplayedIcons = true;
    }


    public void SearchAtEndOfFrame()
    {
        bSearchAtEndOfFrame = true;
    }

    public void Search(bool _bResetScrolling = true)
    {
        // by tag, id, "untagged", excluding tag, word

        if (bDontScrollOnNextSearch)
            _bResetScrolling = false;
        bDontScrollOnNextSearch = false;

        float fLastScroll = scrollRect.verticalScrollbar.value;

        string strInput = inputSearch.text.ToLowerInvariant();
        s_strLastSearch = strInput;
        var watch = System.Diagnostics.Stopwatch.StartNew();

        List<string> liInputs = strInput.Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries).ToList();

        // get tags (including and excluding)
        List<System.Tuple<int, string>> liTagsAll = liInputs.Select(x => tuGetTagId(x)).Where(y => y.Item1 >= 0).ToList();
        List<string> liInputSearchWords = liInputs.Except(liTagsAll.Select(x => x.Item2)).Where(x => !x.StartsWith("-")).ToList();
        List<System.Tuple<int, string>> liTagNames = liTagsAll.Where(x => !x.Item2.StartsWith("-")).ToList();
        List<System.Tuple<int, string>> liExclusiveTagNames = liTagsAll.Except(liTagNames).ToList();
        List<int> liTags = liTagNames.Select(x => x.Item1).ToList();
        List<int> liExclusiveTags = liExclusiveTagNames.Select(x => x.Item1).ToList();

        liIconIdsFound = new List<int>();

        // add currently selected icon for comparison
        liIconIdsFound.Add(iSelectedIcon);

        // get icons by id
        List<int> liInputIds = liInputs.Where(x => int.TryParse(x, out int _i) && IconDB.s_dictIcons.ContainsKey(_i)).Select(x => int.Parse(x)).ToList();
        liInputSearchWords = liInputSearchWords.Except(liInputIds.Select(x => x.ToString())).ToList();
        liIconIdsFound.AddRange(liInputIds);

        // get icons by id range
        foreach (string strRange in liInputs.Where(x => x.Contains("-")))
        {
            if (int.TryParse(strRange.Split('-')[0], out int _i) 
                && int.TryParse(strRange.Split('-')[1], out int _j)
                && _j > _i)
            {
                for (int i = _i; i < _j + 1; i++)
                { 
                    if (IconDB.s_dictIcons.ContainsKey(i))
                        liIconIdsFound.Add(i);
                }
            }
        }

        // get icons by tag/searchword
        liIconIdsFound.AddRange(IconDB.s_dictIcons
            .Where(iconEntry => 
                liTags.All(iTag => iconEntry.Value.liTags.Contains(iTag)) // contains all tags
                && !liExclusiveTags.Any(iTagExclusive => iconEntry.Value.liTags.Contains(iTagExclusive)) // doesn't contain exlusives
                && (liInputSearchWords.Count == 0 || iconEntry.Value.arNamesByLanguage[0].Any(strName => liInputSearchWords.Any(strIn => strName.Contains(strIn)))) // contains search words
            ).Select(x => x.Key).ToList());

        // no tags/search words? add all
        if (liInputs.Count == 0)
            liIconIdsFound.AddRange(IconDB.s_dictIcons.Select(x => x.Key).ToList());

        watch.Stop();
        long lElapsedTime = watch.ElapsedMilliseconds;

        // debug output
        
        string strTags = "";
        string strSearchWords = "";
        liTags.ForEach(x => strTags += $"{IconDB.s_liTags[x].Item2}, ");
        liExclusiveTags.ForEach(x => strTags += $"no:{IconDB.s_liTags[x].Item2}, ");
        liInputSearchWords.ForEach(x => strSearchWords += $"{x}, ");
        Debug.Log($"Search took {lElapsedTime / 1000f}ms with {liIconIdsFound.Count} results for '{strInput}'. Tags: {strTags} Words: {strSearchWords}");
        

        // update scrollrect size
        Vector2 v2ScrollRectNewSize = scrollRect.content.sizeDelta;
        v2ScrollRectNewSize.y = (float)liIconIdsFound.Count / 10f * (64f + goGrid.GetComponent<GridLayoutGroup>().spacing.y);
        scrollRect.content.sizeDelta = v2ScrollRectNewSize;
        scrollRect.verticalScrollbar.value = _bResetScrolling ? 1f : fLastScroll; // scroll

        UpdateDisplayedIcons(_bForceUpdate:true);
    }

    /// <summary>
    /// Adds the tag id, but does not alter the string.
    /// </summary>
    private System.Tuple<int, string> tuGetTagId(string _str)
    {
        string strCleanedUp = _str;
        // remove leading minus if necessary
        if (strCleanedUp.StartsWith("-", System.StringComparison.OrdinalIgnoreCase))
        {
            if (strCleanedUp.Length > 1)
                strCleanedUp = strCleanedUp.Remove(0, 1);
            else
                return new System.Tuple<int, string>(-1, strCleanedUp);
        }

        System.Tuple<int, string> tuFoundTag = IconDB.s_liTags.FirstOrDefault(x => x.Item2 == strCleanedUp || SuUtility.StringDistanceLevenshtein(x.Item2, strCleanedUp) <= 1);
        if (tuFoundTag != default)
        {
            int iTag = IconDB.s_liTags.FindIndex(x => x.Item2 == tuFoundTag.Item2);
            return new System.Tuple<int, string>(iTag, _str);
        }
        else
        {
            return new System.Tuple<int, string>(-1, _str);
        }
    }

    public void SelectIcon(int _iIcon)
    {
        iSelectedIcon = _iIcon;
        eOnClickOk?.Invoke(this, iSelectedIcon);
        Close();
    }

    public void Close()
    {
        Destroy(gameObject);
    }

    public void ToggleTag(string _strTag)
    {
        string strInput = inputSearch.text;
        List<string> liInputs = strInput.Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries).ToList();

        if (liInputs.Contains(_strTag))
            liInputs.RemoveAll(x => x == _strTag);
        else
            liInputs.Insert(0, _strTag);

        strInput = "";
        liInputs.ForEach(x => strInput += " " + x);
        strInput = strInput.Trim();
        inputSearch.text = strInput;
    }

    public void UpdateDisplayedIcons(bool _bForceUpdate = false)
    {
        int iStartIndex = (int)((1f - (float)scrollRect.verticalScrollbar.value) * liIconIdsFound.Count);
        iStartIndex = (int)Mathf.Round(iStartIndex / 10) * 10;
        iStartIndex = Mathf.Clamp(iStartIndex, 0, liIconIdsFound.Count);

        int iEndIndex = iStartIndex + iDisplayedIcons;
        iEndIndex = Mathf.Clamp(iEndIndex, 0, liIconIdsFound.Count);

        if (!_bForceUpdate && iStartIndex == iLastStartIndex) // don't update if index didn't change
            return;

        liGridIcons.ForEach(x => Destroy(x));
        liGridIcons.Clear();

        for (int i = iStartIndex; i < iEndIndex; i++)
        {
            int iIconId = liIconIdsFound[i];
            GameObject goGridIcon = Instantiate(goGridIconPrefab, goGrid.transform);
            goGridIcon.GetComponentInChildren<Image>().sprite = IconUtility.spriteLoadIcon(iIconId);
            GridIcon gridIcon = goGridIcon.GetComponent<GridIcon>();
            gridIcon.iIcon = iIconId;
            gridIcon.tooltip.strText = iIconId + "\n" + string.Join(", ", IconDB.s_dictIcons[iIconId].liTags.Select(x => IconDB.s_liTags[x].Item2));

            // add callback to tell us that is was selected
            gridIcon.eOnClick += (e, _iIcon) => SelectIcon(_iIcon);

            liGridIcons.Add(goGridIcon);
        }

        iLastStartIndex = iStartIndex;

        StartCoroutine(coMoveHighlightDelayed());
    }


    private void MoveHighlight()
    {
        GameObject goGridIconSelected = liGridIcons.FirstOrDefault(x => x.GetComponent<GridIcon>().iIcon == iSelectedIcon);

        if (goGridIconSelected != null)
        {
            goSelectionHighlight.SetActive(true);
            goSelectionHighlight.transform.position = goGridIconSelected.transform.position;
        }
        else
        {
            goSelectionHighlight.SetActive(false);
        }
    }

    private IEnumerator coMoveHighlightDelayed()
    {
        yield return null;
        yield return null;
        MoveHighlight();
    }



        /*
        public int iFindPageWithIcon(int _iIcon)
    {
        int iStartIndex = 0;
        int iLastIconOnPage = liIconIds[iStartIndex + iDisplayedIcons - 1];
        while (_iIcon > iLastIconOnPage)
        {
            iStartIndex += iDisplayedIcons;

            if (iStartIndex > liIconIds.Count - 1) // icon number too high
            {
                Debug.Log("ERROR: Icon has too high id: " + _iIcon);
                iStartIndex = liIconIds.Count - 1;
                break;
            }
            else
            {
                iLastIconOnPage = liIconIds[Mathf.Min(iStartIndex + iDisplayedIcons - 1, liIconIds.Count - 1)];
            }
        }

        return iStartIndex;
    }

    public void DisplayPage(int _iStartIndex)
    {
        liGridIcons.ForEach(x => Destroy(x));
        liGridIcons.Clear();

        iStartIndex = Mathf.Clamp(_iStartIndex, 0, liIconIds.Count - liIconIds.Count % iDisplayedIcons);
        iEndIndex = Mathf.Min(iStartIndex + iDisplayedIcons, liIconIds.Count);

        for (int iIndex = iStartIndex; iIndex < iEndIndex; iIndex++)
        {
            GameObject goGridIcon = Instantiate(goGridIconPrefab, goGrid.transform);
            goGridIcon.GetComponentInChildren<UnityEngine.UI.Image>().sprite = IconUtility.spriteLoadIcon(liIconIds[iIndex]);
            GridIcon gridIcon = goGridIcon.GetComponent<GridIcon>();
            gridIcon.iIconIndex = iIndex;
            gridIcon.iIcon = liIconIds[iIndex];

            // add callback to tell us that is was selected
            gridIcon.eOnClick += (e, _iIndex) => SelectIcon(_iIndex);


            liGridIcons.Add(goGridIcon);
        }

        // move highlight
        MoveHighlight();
    }
        */
}
