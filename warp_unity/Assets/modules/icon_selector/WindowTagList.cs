using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using System.Linq;

public class WindowTagList : MonoBehaviour
{
    public GameObject m_goTagEntry;
    public Transform m_transTagGrid;
    public TMP_InputField m_inputSearch;
    public WindowSelectIcon m_windowSelectIcon;

    private List<string> liTagsSorted = new List<string>();

    void Start()
    {
        liTagsSorted = liLoadTagList();
        foreach (string strTag in liTagsSorted)
        {
            GameObject goTagEntry = Instantiate(m_goTagEntry, m_transTagGrid);
            goTagEntry.GetComponentInChildren<TMP_Text>().text = strTag;

            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerDown;
            entry.callback.AddListener((eventData) => { m_windowSelectIcon.ToggleTag(strTag); });
            goTagEntry.GetComponentInChildren<EventTrigger>().triggers.Add(entry);
        }
    }

    public void Open()
    {
        gameObject.SetActive(true);
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    public void Toggle()
    {
        gameObject.SetActive(!gameObject.activeInHierarchy);
    }

    public List<string> liLoadTagList()
    {
        TextAsset text = Resources.Load<TextAsset>("taglist_sorted");
        List<string> liLines = text.ToList().Select(x => x.Replace("\r", "")).ToList();
        liLines.RemoveAll(x => string.IsNullOrEmpty(x));
        return liLines;
    }
}
