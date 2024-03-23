using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class EditorExtensions
{
    [MenuItem("WARP/Rebuild icon list")]
    public static void RebuildIconList()
    {
        string strIconsPath = Application.dataPath + "/Resources/icons/";
        string strIconListPath = Path.Combine(strIconsPath, "icon_list.txt");
        List<int> liIconIds = new List<int>();
        string strIconList = "";

        DirectoryInfo directoryInfo = new DirectoryInfo(strIconsPath);
        FileInfo[] arFileInfos = directoryInfo.GetFiles();

        foreach (FileInfo file in arFileInfos)
        {
            if (file.Extension != ".txt" && file.Extension != ".meta")
                liIconIds.Add(int.Parse(file.GetFileNameWithoutExtension()));
        }
        liIconIds.Sort();

        liIconIds.ForEach(x => strIconList += x + "\n");


        File.WriteAllText(strIconListPath, strIconList);
        Debug.Log(System.DateTime.Now + " Wrote iconlist.");
    }
}

// from: https://csharp-extension.com/en/method/1002018/fileinfo-getfilenamewithoutextensio
public static partial class Extensions
{
    public static string GetFileNameWithoutExtension(this FileInfo @this)
    {
        return Path.GetFileNameWithoutExtension(@this.FullName);
    }
}
