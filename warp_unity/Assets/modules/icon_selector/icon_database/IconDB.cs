using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Mostly for creating the icon db.
/// </summary>
public class IconDB : MonoBehaviour
{
    public static Dictionary<int, IconEntry> s_dictIcons = new Dictionary<int, IconEntry>();
    public static List<Tuple<int, string>> s_liTags = new List<Tuple<int, string>>();
    public List<string> liTags = new List<string>();

    public bool m_bUpdateApiCache = false;
    public bool m_bParseApi = false;
    public bool m_bDownloadIcons = false;

    private string m_strCachePath = "[set in start]";

    public const string c_strApiDownloadFolder = "api_downloads";

    public class IconEntry
    {
        public string strLink = "";
        public List<string>[] arNamesByLanguage = { new List<string>(), new List<string>(), 
            new List<string>(), new List<string>(), }; //en, de, fr, es
        public List<int> liTags = new List<int>();
    }

    public class OutputFile
    {
        public Dictionary<int, IconEntry> dictIcons;
        public List<string> liTags;
    }

    IEnumerator Start()
    {
        m_strCachePath = $"{Application.dataPath}/{c_strApiDownloadFolder}/";

        TextAsset textAssetDB = Resources.Load<TextAsset>("icondb_generated");
        OutputFile outputFile = JsonConvert.DeserializeObject<OutputFile>(textAssetDB.text);
        s_dictIcons = outputFile.dictIcons;
        liTags = outputFile.liTags;
        liTags.Add("untagged");

        for (int i = 0; i < liTags.Count; i++)
        {
            s_liTags.Add(new Tuple<int, string>(i, liTags[i]));
        }

        // add "untagged" tag to all icons without tag
        foreach (KeyValuePair<int, IconEntry> kvpIconEntry in s_dictIcons)
        {
            if (kvpIconEntry.Value.liTags.Count == 0)
                kvpIconEntry.Value.liTags.Add(liTags.Count - 1); // last tag is "untagged")
        }

        yield return StartCoroutine(coCreateDB());

        //DeleteFilesWithSameName("C:/Data/warp_files/GW2_RESOURCES/art/icons_eod/icons_all/",
        //    "C:/Data/warp_files/GW2_RESOURCES/art/icons_eod/icons_current/");

        //CheckForImagesWithBlackBorder("C:/Data/warp_files/GW2_RESOURCES/art/icons_eod/icons_all/", 
        //    "C:/Data/warp_files/GW2_RESOURCES/art/icons_eod/cropping/");

        //CropImages("C:/Data/warp_files/GW2_RESOURCES/art/icons_eod/cropping/", "C:/Data/warp_files/GW2_RESOURCES/art/icons_eod/cropped/");
    }

    private IEnumerator coCreateDB()
    {
        if (m_bUpdateApiCache)
            DownloadApiInfos();

        if (m_bParseApi)
        {
            yield return StartCoroutine(ParseApi());
            RegisterMissingIconsFromFolder();

            Debug.Log("Writing output...");
            string strPath = $"{m_strCachePath}/Resources/icondb_generated.json";
            File.WriteAllText(strPath, JsonConvert.SerializeObject(new OutputFile() { dictIcons = s_dictIcons, liTags = liTags }, Formatting.Indented));
            Debug.Log("Finished writing output.");
        }

        if (m_bDownloadIcons)
            StartCoroutine(coDownloadAllIcons());
    }

    // Adds icons that are in the icon folder but were not added during the api crawl
    private void RegisterMissingIconsFromFolder()
    {
        string strIconsPath = Application.dataPath + "/Resources/icons/";
        List<string> liFilePaths = Directory.GetFiles(strIconsPath).ToList();

        foreach (string strPath in liFilePaths)
        {
            string strFileName = Path.GetFileName(strPath);
            if (!strFileName.Contains(".png") || strFileName.Contains(".meta"))
                continue;

            iconGetOrCreate(int.Parse(Path.GetFileNameWithoutExtension(strPath)));
        }
    }

    public IEnumerator coDownloadAllIcons()
    {
        Debug.Log("Started downloading icons.");
        int iMaxDownloadsAtATime = 100;
        string strFolder = $"{Application.dataPath}/../icons_download/";
        if (!Directory.Exists(strFolder))
            Directory.CreateDirectory(strFolder);
        
        int iCurrentDownload = 0;

        Dictionary<int, Coroutine> dictRunningDownloads = new Dictionary<int, Coroutine>();
        foreach (KeyValuePair<int, IconEntry> kvpIcon in s_dictIcons)
        {
            iCurrentDownload++;
            if (iCurrentDownload % 1000 == 0)
                Debug.Log($"Downloading icon {iCurrentDownload}/{s_dictIcons.Count}");

            dictRunningDownloads[kvpIcon.Key] = StartCoroutine(coDownloadFile(kvpIcon.Value.strLink, strFolder));

            while (dictRunningDownloads.Count >= iMaxDownloadsAtATime)
            {
                foreach (KeyValuePair<int, Coroutine> kvp in dictRunningDownloads.Where(x => File.Exists($"{strFolder}/{x.Key}.png")).ToList())
                    dictRunningDownloads.Remove(kvp.Key);
                yield return new WaitForSeconds(0.1f);
            }
        }
        Debug.Log("Finished downloading icons.");
    }

    public IEnumerator ParseApi()
    {
        Debug.Log("Parsing Api.");
        
        yield return StartCoroutine(ParseItems());
        yield return StartCoroutine(ParseSimple("finishers", "finisher"));
        yield return StartCoroutine(ParseMasteries());
        yield return StartCoroutine(ParseSimple("mounts_skins", "mount"));
        yield return StartCoroutine(ParseSimple("outfits", "outfit"));
        yield return StartCoroutine(ParseSimple("pets", "pet"));
        yield return StartCoroutine(ParseSkills());

        // remove duplicate names
        foreach (KeyValuePair<int, IconEntry> kvpEntry in s_dictIcons)
        {
            List<string> liWordList = new List<string>();
            kvpEntry.Value.arNamesByLanguage[0].ForEach(x => liWordList.AddRange(x.ToLower().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)));

            List<string> liWordListSorted = liWordList.GroupBy(n => n)
                .OrderByDescending(g => g.Count())
                .ThenBy(g => g.Key)
                .SelectMany(g => g)
                .Distinct() // remove duplicates
                .Where(x => x != "of" && x != "the")
                .ToList();

            // only take the 4 most common words
            int iKeepWords = 5;
            if (liWordListSorted.Count > iKeepWords)
                liWordListSorted.RemoveRange(iKeepWords, liWordListSorted.Count - iKeepWords);

            kvpEntry.Value.arNamesByLanguage[0] = liWordListSorted;
        }
    }

    private IEnumerator ParseItems()
    {
        Debug.Log("Parsing items...");
        string strCachePath = Directory.GetFiles(m_strCachePath).First(x => x.Contains("items"));
        List<JObject> liItems = JsonConvert.DeserializeObject<JObject[]>(File.ReadAllText(strCachePath)).ToList();

        int iInterruptEvery = 1000;
        int iStep = 0;
        foreach (JObject jobj in liItems)
        {
            if (jobj["icon"] == null || string.IsNullOrEmpty(jobj["icon"].ToString()))
                continue;

            string strType = jobj["type"].ToString().ToLower();
            string strPath = jobj["icon"].ToString();
            int iId = int.Parse(Path.GetFileNameWithoutExtension(strPath));
            IconEntry iconEntry = iconGetOrCreate(iId, strPath);

            iconEntry.arNamesByLanguage[(int)GlobalEnums.Language.en].Add(jobj["name"].ToString());

            AddTag(iconEntry, "item");
            AddTag(iconEntry, strType);

            if (strType == "armor")
            {
                AddTag(iconEntry, jobj["details"]["type"].ToString());
                AddTag(iconEntry, jobj["details"]["weight_class"].ToString());
            }

            if (strType == "consumable")
            {
                AddTag(iconEntry, jobj["details"]["type"].ToString(), "food", "utility", "booze", "currency");
                if (jobj["details"]["unlock_type"] != null)
                    AddTag(iconEntry, jobj["details"]["unlock_type"].ToString(), "dye", "minipet", "gliderskin", "outfit");
            }

            if (strType == "gathering")
            {
                AddTag(iconEntry, jobj["details"]["type"].ToString());
            }

            if (strType == "trinket")
            {
                AddTag(iconEntry, jobj["details"]["type"].ToString());
            }

            if (strType == "upgradecomponent")
            {
                AddTag(iconEntry, jobj["details"]["type"].ToString());
            }

            if (strType == "weapon")
            {
                AddTag(iconEntry, jobj["details"]["type"].ToString());
                AddTag(iconEntry, jobj["details"]["damage_type"].ToString());
            }

            iStep++;
            if (iStep > iInterruptEvery)
            {
                iStep = 0;
                yield return null;
            }
        }
    }

    private IEnumerator ParseMasteries()
    {
        Debug.Log("Parsing masteries...");
        string strCachePath = Directory.GetFiles(m_strCachePath).First(x => x.Contains("finishers"));
        List<JObject> liEntries = JsonConvert.DeserializeObject<JObject[]>(File.ReadAllText(strCachePath)).ToList();

        int iInterruptEvery = 1000;
        int iStep = 0;
        foreach (JObject jobj in liEntries)
        {
            if (jobj["levels"] == null 
                || jobj["levels"]["icon"] == null 
                || string.IsNullOrEmpty(jobj["levels"]["icon"].ToString()))
                continue;

            string strPath = jobj["levels"]["icon"].ToString();
            int iId = int.Parse(Path.GetFileNameWithoutExtension(strPath));
            IconEntry iconEntry = iconGetOrCreate(iId, strPath);

            iconEntry.arNamesByLanguage[(int)GlobalEnums.Language.en].Add(jobj["name"].ToString());

            AddTag(iconEntry, "mastery");

            iStep++;
            if (iStep > iInterruptEvery)
            {
                iStep = 0;
                yield return null;
            }
        }
    }

    private IEnumerator ParseSkills()
    {
        Debug.Log("Parsing skills...");
        string strCachePath = Directory.GetFiles(m_strCachePath).First(x => x.Contains("skills"));
        List<JObject> liEntries = JsonConvert.DeserializeObject<JObject[]>(File.ReadAllText(strCachePath)).ToList();

        int iInterruptEvery = 1000;
        int iStep = 0;
        foreach (JObject jobj in liEntries)
        {
            if (jobj["icon"] == null || string.IsNullOrEmpty(jobj["icon"].ToString()))
                continue;

            string strPath = jobj["icon"].ToString();
            int iId = int.Parse(Path.GetFileNameWithoutExtension(strPath));
            IconEntry iconEntry = iconGetOrCreate(iId, strPath);

            iconEntry.arNamesByLanguage[(int)GlobalEnums.Language.en].Add(jobj["name"].ToString());

            AddTag(iconEntry, "skill");
            if (jobj["type"] != null)
                AddTag(iconEntry, jobj["type"].ToString(), "damage", "heal", "buff");

            iStep++;
            if (iStep > iInterruptEvery)
            {
                iStep = 0;
                yield return null;
            }
        }
    }

    // adds one tag to the icon and all keys given in the list
    private IEnumerator ParseSimple(string _strFileName, string _strTagToAdd, List<string> _liKeysForTagging = null)
    {
        Debug.Log($"Parsing {_strFileName}...");
        string strCachePath = Directory.GetFiles(m_strCachePath).First(x => x.Contains(_strFileName));
        List<JObject> liEntries = JsonConvert.DeserializeObject<JObject[]>(File.ReadAllText(strCachePath)).ToList();

        int iInterruptEvery = 1000;
        int iStep = 0;
        foreach (JObject jobj in liEntries)
        {
            if (jobj["icon"] == null || string.IsNullOrEmpty(jobj["icon"].ToString()))
                continue;

            string strPath = jobj["icon"].ToString();
            int iId = int.Parse(Path.GetFileNameWithoutExtension(strPath));
            IconEntry iconEntry = iconGetOrCreate(iId, strPath);

            iconEntry.arNamesByLanguage[(int)GlobalEnums.Language.en].Add(jobj["name"].ToString());

            AddTag(iconEntry, _strTagToAdd);

            if (_liKeysForTagging != null)
            {
                foreach (string _strKey in _liKeysForTagging)
                {
                    AddTag(iconEntry, jobj[_strKey].ToString());
                }
            }
  
            iStep++;
            if (iStep > iInterruptEvery)
            {
                iStep = 0;
                yield return null;
            }
        }
    }

    private void AddTag(IconEntry _icon, string _strTag, params string[] _arOnly)
    {
        _strTag = _strTag.ToLower();
        if (string.IsNullOrEmpty(_strTag)
            || (_arOnly.Length > 0 && !_arOnly.Contains(_strTag)))
            return;

        int iTagId = -1;
        if (liTags.Contains(_strTag))
        {
            iTagId = liTags.IndexOf(_strTag);
        }
        else
        {
            liTags.Add(_strTag);
            iTagId = liTags.Count - 1;
        }

        if (!_icon.liTags.Contains(iTagId))
            _icon.liTags.Add(iTagId);
    }

    private IconEntry iconGetOrCreate(int _iId, string _strUrl = "")
    {
        if (!s_dictIcons.TryGetValue(_iId, out IconEntry o_iconEntry))
        {
            o_iconEntry = new IconEntry();
            if (!string.IsNullOrEmpty(_strUrl))
                o_iconEntry.strLink = _strUrl;

            s_dictIcons.Add(_iId, o_iconEntry);
        }

        return o_iconEntry;
    }

    private void DeleteFilesWithSameName(string _strSearchInDir, string _strDeleteInDir)
    {
        List<string> liFileNames = Directory.GetFiles(_strSearchInDir).Select(x => Path.GetFileName(x)).ToList();
        List<string> liFilesToDelete = liFileNames.Where(x => File.Exists($"{_strDeleteInDir}{x}")).Select(x => $"{_strDeleteInDir}{x}").ToList();

        liFilesToDelete.ForEach(x => File.Delete(x));

        Debug.Log($"Files found: {liFilesToDelete.Count}");
    }

    private void CheckForImagesWithBlackBorder(string _strPathSearch, string _strCopyTo)
    {
        List<string> liFilePaths = Directory.GetFiles(_strPathSearch).ToList();

        foreach (string strPath in liFilePaths)
        {
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(File.ReadAllBytes(strPath));

            // only crop big pictures
            if (texture.height != 128)
                continue;

            float fTotal = 0;
            int iCheckWidth = 12;

            // top
            for (int iX = 0; iX < texture.width; iX++)
            {
                for (int iY = 0; iY < iCheckWidth; iY++)
                {
                    Color colorPixel = texture.GetPixel(iX, iY);
                    fTotal += colorPixel.r + colorPixel.g + colorPixel.b;
                }
            }

            // bottom
            for (int iX = 0; iX < texture.width; iX++)
            {
                for (int iY = texture.height - iCheckWidth; iY < texture.height; iY++)
                {
                    Color colorPixel = texture.GetPixel(iX, iY);
                    fTotal += colorPixel.r + colorPixel.g + colorPixel.b;
                }
            }

            fTotal /= 2f;

            if (fTotal < 100f)
            {
                File.Move(strPath, $"{_strCopyTo}{Path.GetFileName(strPath)}");
            }

            
        }
    }

    private void CropImages(string _strPathSource, string _strSaveTo)
    {
        List<string> liFilePaths = Directory.GetFiles(_strPathSource).ToList();

        foreach (string strPath in liFilePaths)
        {
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(File.ReadAllBytes(strPath));

            // only crop big pictures
            if (texture.height == 128)
            {
                Color[] arColor = texture.GetPixels(15, 15, 96, 96);
                Texture2D texCropped = new Texture2D(96, 96);
                texCropped.SetPixels(arColor);
                texCropped.Apply();

                File.WriteAllBytes($"{_strSaveTo}{Path.GetFileName(strPath)}", texCropped.EncodeToPNG());
            }
            else if (texture.height == 256)
            {
                Color[] arColor = texture.GetPixels(37, 37, 180, 180);
                Texture2D texCropped = new Texture2D(180, 180);
                texCropped.SetPixels(arColor);
                texCropped.Apply();

                File.WriteAllBytes($"{_strSaveTo}{Path.GetFileName(strPath)}", texCropped.EncodeToPNG());
            }
        }
    }

    // ============================== DOWNLOAD APIS ==============================

    private IEnumerator coDownloadFile(string _strUrl, string _strFolder)
    {
        UnityWebRequest webRequest = new UnityWebRequest(_strUrl);
        webRequest.method = UnityWebRequest.kHttpVerbGET;
        string strFilePath = Path.Combine(_strFolder, Path.GetFileName(_strUrl));
        DownloadHandlerFile downloadHandler = new DownloadHandlerFile(strFilePath);
        downloadHandler.removeFileOnAbort = true;
        webRequest.downloadHandler = downloadHandler;

        yield return webRequest.SendWebRequest();
        if (webRequest.isNetworkError || webRequest.isHttpError)
        {
            Debug.Log(webRequest.error);
        }
    }

    private void DownloadApiInfos()
    {
        if (!Directory.Exists(m_strCachePath))
            Directory.CreateDirectory(m_strCachePath);

        StartCoroutine(WriteApiInfoToFile("items"));
        StartCoroutine(WriteApiInfoToFile("finishers"));
        StartCoroutine(WriteApiInfoToFile("mounts/skins"));
        StartCoroutine(WriteApiInfoToFile("outfits"));
        StartCoroutine(WriteApiInfoToFile("pets"));
        StartCoroutine(WriteApiInfoToFile("skills"));
        StartCoroutine(WriteApiInfoToFile("professions"));
    }

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
                WebRequestAnswer<T> webRequestAnswer = new WebRequestAnswer<T>();
                var jobjResponse = JsonConvert.DeserializeObject<T>(webRequest.downloadHandler.text);
                webRequestAnswer.responseObject = jobjResponse;
                webRequestAnswer.dictHeaders = webRequest.GetResponseHeaders();
                _actionOutput(webRequestAnswer);
            }
        }
    }


    private class WebRequestAnswer<T>
    {
        public T responseObject;
        public Dictionary<string, string> dictHeaders;
    }
}
