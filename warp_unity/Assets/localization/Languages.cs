using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization.Settings;
using TMPro;
using UnityEngine.ResourceManagement.AsyncOperations;

public class Languages : MonoBehaviour
{
    public static Languages Instance;


    private void Awake()
    {
        Instance = this;
    }

    public static void Set(TMP_Text _text, string _strKey)
    {
        Instance.SetAsync(_text, _strKey);
    }

    public void SetAsync(TMP_Text _text, string _strKey)
    {
        StartCoroutine(coSetAsync(_text, _strKey));
    }

    private IEnumerator coSetAsync(TMP_Text _text, string _strKey)
    {
        var operation = LocalizationSettings.StringDatabase.GetLocalizedStringAsync("Main", _strKey);
        yield return new WaitUntil(() => operation.IsDone);
        _text.text = operation.Result;
    }
}
