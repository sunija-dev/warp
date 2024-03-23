using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

// https://wiki.guildwars2.com/api.php?action=parse&page=Kryta&format=json
// https://api.guildwars2.com/v2/continents/1/floors/0
// https://wiki.guildwars2.com/api.php?action=query&prop=extracts&exsentences=10&exlimit=1&titles=Tyria&explaintext=1&format=json


public class WikiMap : MonoBehaviour
{
    public List<Region> liRegions = new List<Region>();
    public List<Map> liMaps = new List<Map>();
    public List<PointOfInterest> liPOIs = new List<PointOfInterest>();

    void Start()
    {
        StartCoroutine(coParseAPI());
    }

    private IEnumerator coParseAPI()
    {
        // debug
        Region regionTest = new Region();
        regionTest.strName = "Catha";
        yield return StartCoroutine(coFindWikiEntry(regionTest));

        // we'll only look at continent 1 (Tyria, not Mists)
        // get all floors from https://api.guildwars2.com/v2/continents/1

        WebRequestAnswer<JObject> answerContinentInfo = new WebRequestAnswer<JObject>();
        yield return DoWebRequest<JObject>($"https://api.guildwars2.com/v2/continents/1", (_output) => answerContinentInfo = _output);
        List<int> liFloorIds = JsonConvert.DeserializeObject<int[]>(answerContinentInfo.responseObject["floors"].ToString()).ToList();

        // go through all floors with https://api.guildwars2.com/v2/continents/1/floors/1
        // - get regions with label_coords
        // - get maps with label_coords
        // -- get points of interest with coords
        foreach (int iFloor in liFloorIds)
        {
            Debug.Log($"Requesting floor {iFloor}");

            // DEBUG
            /*
            if (iFloor > 5)
                break;
            */

            WebRequestAnswer<JObject> answerFloor = new WebRequestAnswer<JObject>();
            yield return DoWebRequest<JObject>($"https://api.guildwars2.com/v2/continents/1/floors/{iFloor}", (_output) => answerFloor = _output);

            Dictionary<int, JObject> dictRegions = JsonConvert.DeserializeObject<Dictionary<int, JObject>>(answerFloor.responseObject["regions"].ToString());
            foreach (KeyValuePair<int, JObject> kvpRegion in dictRegions)
            {
                JObject jobjRegion = kvpRegion.Value;
                Region region = new Region();
                region.iID = int.Parse(jobjRegion["id"].ToString());
                if (liRegions.Any(x => x.iID == region.iID))
                    continue;

                liRegions.Add(region);
                region.strName = jobjRegion["name"].ToString();
                region.v2Coord.x = int.Parse(jobjRegion["label_coord"][0].ToString());
                region.v2Coord.y = int.Parse(jobjRegion["label_coord"][1].ToString());

                //JArray jarMaps = JArray.Parse(jobjRegion["maps"].ToString());
                Dictionary<int, JObject> dictMaps = JsonConvert.DeserializeObject<Dictionary<int, JObject>>(jobjRegion["maps"].ToString());
                foreach (KeyValuePair<int, JObject> kvpMap in dictMaps)
                {
                    JObject jobjMap = kvpMap.Value;
                    Map map = new Map();
                    map.iID = int.Parse(jobjMap["id"].ToString());
                    if (liMaps.Any(x => x.iID == map.iID))
                        continue;

                    if (!region.liMaps.Any(x => x == map.iID))
                        region.liMaps.Add(map.iID);

                    liMaps.Add(map);
                    map.strName = jobjMap["name"].ToString();

                    if (jobjMap["label_coord"] != null)
                    {
                        map.v2Coord.x = int.Parse(jobjMap["label_coord"][0].ToString());
                        map.v2Coord.y = int.Parse(jobjMap["label_coord"][1].ToString());
                    }
                    else if (jobjMap["continent_rect"] != null)
                    { 
                        map.v2Coord.x = (int.Parse(jobjMap["continent_rect"][0][0].ToString()) - int.Parse(jobjMap["continent_rect"][1][0].ToString())) / 2f;
                        map.v2Coord.y = (int.Parse(jobjMap["continent_rect"][0][1].ToString()) - int.Parse(jobjMap["continent_rect"][1][1].ToString())) / 2f;
                    }

                    //JArray jarPOIs = JArray.Parse(jobjMap["points_of_interest"].ToString());
                    Dictionary<int, JObject> dictPOIs = JsonConvert.DeserializeObject<Dictionary<int, JObject>>(jobjMap["points_of_interest"].ToString());
                    foreach (KeyValuePair<int, JObject> kvpPOI in dictPOIs)
                    {
                        JObject jobjPOI = kvpPOI.Value;
                        PointOfInterest poi = new PointOfInterest();
                        poi.iID = int.Parse(jobjPOI["id"].ToString());
                        if (liPOIs.Any(x => x.iID == poi.iID))
                            continue;

                        if (jobjPOI["name"] == null)
                            continue; //vistas, etc

                        liPOIs.Add(poi);
                        poi.strName = jobjPOI["name"].ToString();
                        poi.v2Coord.x = float.Parse(jobjPOI["coord"][0].ToString());
                        poi.v2Coord.y = float.Parse(jobjPOI["coord"][1].ToString());
                    }
                }
            }
        }

        // check which got wiki articles with https://wiki.guildwars2.com/api.php?action=parse&page=Kryta&format=json
        // if it is there, save the address with https://wiki.guildwars2.com/wiki/Tyria
        // also save the plaintext of the first section: https://wiki.guildwars2.com/api.php?action=query&prop=extracts&exsentences=10&exlimit=1&titles=Tyria&explaintext=1&format=json
        // (long version: https://wiki.guildwars2.com/api.php?action=query&prop=extracts&titles=Tyria&explaintext=1&format=json)
        
        foreach (Region region in liRegions)
            yield return StartCoroutine(coFindWikiEntry(region));
        foreach (Map map in liMaps)
            yield return StartCoroutine(coFindWikiEntry(map));
        foreach (PointOfInterest poi in liPOIs)
            yield return StartCoroutine(coFindWikiEntry(poi));
        

        /*
        int iLimit = 400;
        for (int i = 0; i < Mathf.Min(iLimit, liRegions.Count); i++)
            yield return StartCoroutine(coFindWikiEntry(liRegions[i]));
        for (int i = 0; i < Mathf.Min(iLimit, liMaps.Count); i++)
            yield return StartCoroutine(coFindWikiEntry(liMaps[i]));
        for (int i = 0; i < Mathf.Min(iLimit, liPOIs.Count); i++)
            yield return StartCoroutine(coFindWikiEntry(liPOIs[i]));
        */

        // write all that to a text file as json
        WikiMapDB wikiMapDB = new WikiMapDB();
        wikiMapDB.liMaps = liMaps;
        wikiMapDB.liRegions = liRegions;
        wikiMapDB.liPOIs = liPOIs;

        File.WriteAllText($"{Application.dataPath}/{IconDB.c_strApiDownloadFolder}/Resources/wiki_map_db.json", 
            JsonConvert.SerializeObject(wikiMapDB, Formatting.Indented, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            }));
        wikiMapDB.IncreaseReadability();
        File.WriteAllText($"{Application.dataPath}/{IconDB.c_strApiDownloadFolder}/Resources/wiki_map_db_short.json",
            JsonConvert.SerializeObject(wikiMapDB, Formatting.Indented, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            }));

        Debug.Log("FINISHED CREATING DB");

        // (display in map)
    }

    public static WikiMapDB wikidbLoadFromCache()
    {
        return JsonConvert.DeserializeObject<WikiMapDB>(((TextAsset)Resources.Load("wiki_map_db_short")).text);
    }

    /*
    private IEnumerator WriteApiInfoToFile(string _strApiName)
    {
        // paging: https://wiki.guildwars2.com/wiki/API:2#Accessing_resources

        int iPageTotal = 9999;
        List<JObject> liEntries = new List<JObject>();
        for (int i = 0; i < iPageTotal; i++)
        {
            Debug.Log($"Requesting {_strApiName} page {i}/{iPageTotal}.");
            WebRequestAnswer<JObject[]> answerItems = new WebRequestAnswer<JObject[]>();
            yield return DoWebRequest<JObject[]>($"https://api.guildwars2.com/v2/{_strApiName}?page={i}&page_size=200", (_output) => answerItems = _output);
            iPageTotal = int.Parse(answerItems.dictHeaders["x-page-total"]);
            liEntries.AddRange(answerItems.responseObject);
        }

        string strPath = $"{m_strCachePath}/{DateTime.UtcNow.ToString("yyMMdd")}_{_strApiName.Replace('/', '_')}.json";
        File.WriteAllText(strPath, JsonConvert.SerializeObject(liEntries));

        Debug.Log($"Finished downloading API {_strApiName}.");
    }
    */


    private IEnumerator coFindWikiEntry(WikiMapEntry _wikiMapEntry)
    {
        string strName = _wikiMapEntry.strName.Replace(' ', '_').Replace("#", "");
        WebRequestAnswer<JObject> answerWiki = new WebRequestAnswer<JObject>();

        yield return DoWebRequest<JObject>($"https://wiki.guildwars2.com/api.php?action=query&prop=extracts&titles={strName}&explaintext=1&format=json", (_output) => answerWiki = _output);
        if (answerWiki == default || answerWiki.responseObject["query"]["pages"] == null)
            yield break;

        Dictionary<int, JObject> dictPages = JsonConvert.DeserializeObject<Dictionary<int, JObject>>(answerWiki.responseObject["query"]["pages"].ToString());

        if (dictPages.Count > 0 && dictPages.First(x => x.Key != -5).Value["extract"] != null)
        {
            Debug.Log($"Wiki article found for {strName}! <3");
            _wikiMapEntry.strWikiText = dictPages.First(x => x.Key != -5).Value["extract"].ToString();
            _wikiMapEntry.strWikiUrl = $"https://wiki.guildwars2.com/wiki/{strName}";
            Debug.Log($"{_wikiMapEntry.strWikiText.Substring(0, Mathf.Min(100, _wikiMapEntry.strWikiText.Length))}");
        }
        else
        {
            Debug.Log($"No article for {strName}! :(");
        }
    }

    /// <typeparam name="T">What the received string should be parsed to, e.g. JObject[]</typeparam>
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
                try
                {
                    WebRequestAnswer<T> webRequestAnswer = new WebRequestAnswer<T>();
                    var jobjResponse = JsonConvert.DeserializeObject<T>(webRequest.downloadHandler.text);
                    webRequestAnswer.responseObject = jobjResponse;
                    webRequestAnswer.dictHeaders = webRequest.GetResponseHeaders();
                    _actionOutput(webRequestAnswer);
                }
                catch 
                {
                    Debug.Log($"Request went wrong: {_strRequest}");
                }
            }
        }
    }


    private class WebRequestAnswer<T>
    {
        public T responseObject;
        public Dictionary<string, string> dictHeaders;
    }


    [Serializable]
    public abstract class WikiMapEntry
    {
        public int iID = -1;
        public Vector2 v2Coord = Vector2.zero;
        public string strName = "";
        public string strWikiText = "";
        public string strWikiUrl = "";
    }

    [Serializable]
    public class Region : WikiMapEntry
    {
        public List<int> liMaps = new List<int>();
    }

    [Serializable]
    public class Map : WikiMapEntry
    { 
    
    }

    [Serializable]
    public class PointOfInterest : WikiMapEntry
    { 
    
    }

    [Serializable]
    public class WikiMapDB : WikiMapEntry
    {
        public List<Region> liRegions = new List<Region>();
        public List<Map> liMaps = new List<Map>();
        public List<PointOfInterest> liPOIs = new List<PointOfInterest>();

        public void IncreaseReadability()
        {
            foreach (Region region in liRegions)
                IncreaseReadability(region);
            foreach (Map map in liMaps)
                IncreaseReadability(map);
            foreach (PointOfInterest poi in liPOIs)
                IncreaseReadability(poi);
        }

        public void IncreaseReadability(WikiMapEntry _entry)
        {
            Section sectionEverything = new Section();
            sectionEverything.strText = _entry.strWikiText;

            // split into sections
            SplitSection(sectionEverything, "==");
            foreach (Section sectionSub in sectionEverything.liSubSections)
                SplitSection(sectionSub, "===");

            List<string> liUnwantedSections = new List<string> { "Events", "NPCs", "Objects", "Crafting resources", "Map completed", "Locations", "Interactive map", "Gallery" };
            //List<string> liWantedSections = new List<string> { "Notes", "Trivia" };

            sectionEverything.Cull(liUnwantedSections);

            string strText = sectionEverything.strCombined();

            // shorten (characters and lines)
            strText = strText.Substring(0, Mathf.Min(800, strText.Length));
            using (StringReader reader = new StringReader(strText))
            {
                int iLineLimit = 10;

                int iLine = 0;
                string strOutput = "";
                string strLine;
                while ((strLine = reader.ReadLine()) != null)
                {
                    iLine++;
                    strOutput += strLine + "\n";
                    if (iLine > iLineLimit)
                        break;
                }

                strText = strOutput;
            }

            _entry.strWikiText = strText;
        }


        private void SplitSection(Section _sectionParent, string _strSeparator)
        {
            Section sectionCurrent = new Section();
            using (StringReader reader = new StringReader(_sectionParent.strText))
            {
                string strLine;
                while ((strLine = reader.ReadLine()) != null)
                {
                    if (string.IsNullOrEmpty(strLine) || strLine == "\n")
                        continue;

                    if (strLine.Split()[0] != _strSeparator)
                    {
                        sectionCurrent.strText += strLine + "\n";
                    }
                    else
                    {
                        // close up section
                        _sectionParent.liSubSections.Add(sectionCurrent);
                        sectionCurrent = new Section();
                        sectionCurrent.strText += strLine + "\n";
                    }
                }

                _sectionParent.liSubSections.Add(sectionCurrent);
            }
        }

        private class Section
        {
            public List<Section> liSubSections = new List<Section>();
            public string strText = "";

            public void Cull(List<string> _liUnwanted)
            {
                foreach (Section sectionSub in liSubSections)
                    sectionSub.Cull(_liUnwanted);
                liSubSections.RemoveAll(x => x.strText == null);

                if (_liUnwanted.Any(x => strText.StartsWith($"== {x} ==") || strText.StartsWith($"=== {x} ===")))
                {
                    liSubSections = null;
                    strText = null;
                }
            }

            public string strCombined()
            {
                string strOutput = "";
                for (int i = 0; i < liSubSections.Count; i++)
                    strOutput += $"{liSubSections[i].strCombined()}{((i == liSubSections.Count - 1) ? "" : "\n")}"; // no newline on last, because section already has one

                if (liSubSections.Count == 0)
                    strOutput = strText;

                return strOutput;
            }
        }
    }
}
