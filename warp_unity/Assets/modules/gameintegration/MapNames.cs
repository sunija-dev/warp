using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System;

public class MapNames : MonoBehaviour
{
    public static Dictionary<int, string> s_dictMapNames = new Dictionary<int, string>();

    public bool bGenerateLists = false;
    public List<TextAsset> liMapNamesMapFiles;

    void Start()
    {
        // load from file
        ClientManager.Instance.eDisplayLanguageChanged.AddListener(OnDisplayLanguageChanged);
        ClientManager.Instance.eAccountChanged.AddListener(OnDisplayLanguageChanged);
        OnDisplayLanguageChanged(); // init names?
    }

    public static string strGetName(int _iId)
    {
        if (!s_dictMapNames.ContainsKey(_iId))
            return _iId.ToString();

        return s_dictMapNames[_iId];
    }

    private void OnDisplayLanguageChanged()
    {
        if (Player.Instance == null)
            return;

        GlobalEnums.Language language = Player.Instance.account.language;
        string strLanguageJson = liMapNamesMapFiles[(int)language].text;

        s_dictMapNames = MapDictWrapper.dictLoadFromJson(strLanguageJson);
    }

    
    private void Update()
    {
        if (bGenerateLists)
        {
            GenerateMapList();
            bGenerateLists = false;
        }
    }
    

    private void GenerateMapList()
    {
        GlobalEnums.Language[] arLanguages = (GlobalEnums.Language[])System.Enum.GetValues(typeof(GlobalEnums.Language));
        foreach (GlobalEnums.Language language in arLanguages)
        {
            StartCoroutine(coGenerateMappingFileBatched("maps", language));
        }
    }

    /*
    public IEnumerator coGenerateMappingFile(GlobalEnums.Language _language, int _iStart = 1, int _iEnd = 1700)
    {
        Dictionary<int, string> dictMapNames = new Dictionary<int, string>();

        for (int i = _iStart; i < _iEnd; i++)
        {
            System.Uri uriRequest = new System.Uri("https://api.guildwars2.com/v2/maps/" + i + "?lang=" + _language.ToString()); //
            using (UnityWebRequest webRequest = UnityWebRequest.Get(uriRequest))
            {
                yield return webRequest.SendWebRequest();

                if (webRequest.isNetworkError)
                    Debug.Log(": Error: " + webRequest.error);
                else
                {
                    if (webRequest.downloadHandler.isDone)
                    {
                        JObject jobjData = (JObject) JsonConvert.DeserializeObject(webRequest.downloadHandler.text);
                        int iId = jobjData["id"].Value<int>();
                        string strName = jobjData["name"].Value<string>();
                        dictMapNames[iId] = strName;

                        Debug.Log(iId + ": " + strName);
                    }
                }
            }
            yield return new WaitForSeconds(1f);
        } 

        MapDictWrapper wrapperDict = new MapDictWrapper();
        string strPath = string.Format("{0}/Resources/map_names/maps_{1}.txt", Application.dataPath, _language.ToString());
        System.IO.File.WriteAllText(strPath, wrapperDict.strToJson(dictMapNames));
        Debug.Log("Updated map list");
    }
    */

    private IEnumerator coGenerateMappingFileBatched(string _strApiName, GlobalEnums.Language _language)
    {
        // paging: https://wiki.guildwars2.com/wiki/API:2#Accessing_resources

        int iPageTotal = 9999;
        List<JObject> liEntries = new List<JObject>();
        for (int i = 0; i < iPageTotal; i++)
        {
            Debug.Log($"Requesting {_strApiName} page {i}/{iPageTotal}.");
            WebRequestAnswer<JObject[]> answerItems = new WebRequestAnswer<JObject[]>();
            yield return DoWebRequest<JObject[]>($"https://api.guildwars2.com/v2/{_strApiName}?page={i}&page_size=200&lang={_language}", (_output) => answerItems = _output);
            iPageTotal = int.Parse(answerItems.dictHeaders["x-page-total"]);
            liEntries.AddRange(answerItems.responseObject);
        }

        //string strPath = $"{m_strCachePath}/{DateTime.UtcNow.ToString("yyMMdd")}_{_strApiName.Replace('/', '_')}.json";
        //File.WriteAllText(strPath, JsonConvert.SerializeObject(liEntries));

        Dictionary<int, string> dictMapNames = new Dictionary<int, string>();
        foreach (JObject jobjEntry in liEntries)
        {
            int iId = jobjEntry["id"].Value<int>();
            dictMapNames[iId] = jobjEntry["name"].Value<string>();
        }

        MapDictWrapper wrapperDict = new MapDictWrapper();
        string strPath = string.Format($"{Application.dataPath}/Resources/map_names/maps_{_language}.txt");
        System.IO.File.WriteAllText(strPath, wrapperDict.strToJson(dictMapNames));

        Debug.Log($"Finished downloading API {_strApiName}.");
    }

    /// COPIED OVER FROM IconDB.cs TODO: refactor, e.g. to a general "api cacher"
    private IEnumerator DoWebRequest<T>(string _strRequest, Action<WebRequestAnswer<T>> _actionOutput)
    {
        Uri uriRequest = new Uri(_strRequest);
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uriRequest))
        {
            yield return webRequest.SendWebRequest();
            if (webRequest.isNetworkError)
            {
                Debug.Log(": Error: " + webRequest.error);
            }
            else if (webRequest.downloadHandler.isDone)
            {
                WebRequestAnswer<T> webRequestAnswer = new WebRequestAnswer<T>();
                var jobjResponse = JsonConvert.DeserializeObject<T>(webRequest.downloadHandler.text);
                webRequestAnswer.responseObject = jobjResponse;
                webRequestAnswer.dictHeaders = webRequest.GetResponseHeaders();
                _actionOutput(webRequestAnswer);
            }
        }
    }

    /// COPIED OVER FROM IconDB.cs TODO: refactor
    private class WebRequestAnswer<T>
    {
        public T responseObject;
        public Dictionary<string, string> dictHeaders;
    }

    /// <summary>
    /// Wrapper to make Dictionary serializable on Linux (with unity built-in json thingy).
    /// </summary>
    [System.Serializable]
    public class MapDictWrapper
    {
        [SerializeField] public List<int> liKeys = new List<int>();
        [SerializeField] public List<string> liValues = new List<string>(); 

        public string strToJson(Dictionary<int, string> _dictMapNames)
        {
            liKeys.Clear();
            liValues.Clear();

            foreach (KeyValuePair<int, string> kvp in _dictMapNames)
            {
                liKeys.Add(kvp.Key);
                liValues.Add(kvp.Value);
            }

            return JsonUtility.ToJson(this, true);
        }

        public static Dictionary<int, string> dictLoadFromJson(string _strJson)
        {
            MapDictWrapper wrapper = JsonUtility.FromJson<MapDictWrapper>(_strJson);
            Dictionary<int, string> dictMapNames = new Dictionary<int, string>();

            for (int i = 0; i < wrapper.liKeys.Count; i++)
            {
                dictMapNames[wrapper.liKeys[i]] = wrapper.liValues[i];
            }

            return dictMapNames;
        }
    }
}
