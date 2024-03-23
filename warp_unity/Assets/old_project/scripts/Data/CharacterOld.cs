using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class CharacterOld 
{
    public GW2APICharInfo charInfo = new GW2APICharInfo();
    public CharSheet charSheet = new CharSheet();

    public GW2Position position = new GW2Position();
    public string strIP = "";
    public bool bHideInCharlist = false;

    //"{\"name\":\"Fiana Dings\",\"profession\":5,\"spec\":0,\"race\":2,\"map_id\":18,\"world_id\":268435458,\"team_color_id\":0,\"commander\":false,\"map"

    public CharacterOld() { }

    public CharacterOld(GW2APICharInfo _charInfo, CharSheet _charSheet)
    {
        charInfo = _charInfo;
        charSheet = _charSheet;
    }

    /// <summary>
    /// Only sets parameters that should be sent to other players.
    /// </summary>
    public CharacterOld(GW2APICharInfo _charInfo, CharSheet _charSheet, GW2Position _position, string _strIP)
    {
        charInfo = _charInfo;
        charSheet = _charSheet;
        position = _position;
        strIP = _strIP;
    }

    public override string ToString()
    {
        string strResult = "";

        strResult += "CharInfo: \n";
        strResult += charInfo.name + ", ";
        strResult += charInfo.race + ", ";
        strResult += charInfo.gender + ", ";
        strResult += charInfo.profession + ", ";
        strResult += charInfo.level + ", ";
        strResult += charInfo.guild + ", ";
        strResult += charInfo.age + ", ";
        strResult += charInfo.created;

        strResult += "\n\nCharSheet: \n";
        strResult += charSheet.strRPName + ", ";
        strResult += charSheet.strRPDesc;

        return strResult;
    }
}

/// <summary>
/// Wrapper for charList, so i can serialize it with Unity json
/// </summary>
[Serializable]
public class CharacterList
{
    public List<CharacterOld> liChars = new List<CharacterOld>();
}
