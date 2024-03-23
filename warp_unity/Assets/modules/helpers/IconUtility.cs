using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;

public class IconUtility 
{
    public static List<int> liLoadIconList()
    { 
        TextAsset text = Resources.Load<TextAsset>("icons/icon_list");
        List<string> liLines = text.ToList();
        liLines.RemoveAt(liLines.Count - 1); // last one is just empty line
        return liLines.Select(x => int.Parse(x)).ToList();
    }

    public static Sprite spriteLoadIcon(int _iIcon)
    {
        Sprite sprite = Resources.Load<Sprite>("icons/" + _iIcon);
        sprite = sprite == null ? Resources.Load<Sprite>("icons/" + 0) : sprite; // not found? load default

        return sprite;
    }
}
