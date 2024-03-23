using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class ClientSafeData
{
    [Serializable] public class DictionaryOfStringAndString : SerializableDictionary<string, string> { }
    [SerializeField] public DictionaryOfStringAndString dictAccWithAPIKey = new DictionaryOfStringAndString();

    [SerializeField] public static string s_strAccWithAPIKeyName = "accs_with_apikey_json";

    public void Save()
    {
        string strJson = JsonUtility.ToJson(this);
        PlayerPrefs.SetString(s_strAccWithAPIKeyName, strJson);
    }

    public static ClientSafeData Load()
    {
        ClientSafeData safeData = null;
        string strJson = PlayerPrefs.GetString(s_strAccWithAPIKeyName);
        if (!String.IsNullOrEmpty(strJson))
            safeData = JsonUtility.FromJson<ClientSafeData>(strJson);

        DebugOutput(safeData);
        return safeData;
    }

    private static void DebugOutput(ClientSafeData _safeData)
    {
        if (_safeData != null)
        {
            string strDebug = "Loaded safeData: ";
            foreach (KeyValuePair<string, string> kvp in _safeData.dictAccWithAPIKey)
            {
                strDebug += kvp.Key + " " + kvp.Value + "; ";
            }
            Debug.Log(strDebug);
        }
    }
}
