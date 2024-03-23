using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MumbleIdentity 
{
    public string strName;
    //public GW2APICharInfo.Profession profession;
    //public int iSpec;
    //public GW2APICharInfo.Race race;
    //public int iMapId;
    //public int iWorldID;
    //public int iTeamColorID;
    //public bool bCommander;

    public MumbleIdentity() { }

    public MumbleIdentity(char[] _arInput)
    {
        SetValues(_arInput);
    }

    public void SetValues(char[] _arInput)
    {
        string strIdentity = new string(_arInput);
        strIdentity = strIdentity.Replace("\0", "") + "}";

        // HACK: cut it after "name", which is the first item
        strIdentity = strIdentity.Remove(strIdentity.IndexOf(",")) + "}";

        Dictionary<string, object> dictJson = (Dictionary<string, object>)MiniJSON.Json.Deserialize(strIdentity);

        // HACK: Only reads name, because _arInput somehow doesn't contain the whole json. Maybe because every second char is a \0.
        strName = dictJson["name"].ToString();
        //profession = (GW2APICharInfo.Profession)System.Enum.Parse(typeof(GW2APICharInfo.Profession), dictJson["profession"].ToString());
        //iSpec = GW2Helpers.iFromDictObj(dictJson["spec"]);
        //race = (GW2APICharInfo.Race)System.Enum.Parse(typeof(GW2APICharInfo.Race), dictJson["race"].ToString());
        //iMapId = GW2Helpers.iFromDictObj(dictJson["map_id"]);
        //iWorldID = GW2Helpers.iFromDictObj(dictJson["world_id"]);
        //iTeamColorID = GW2Helpers.iFromDictObj(dictJson["team_color_id"]);
        //bCommander = (bool)dictJson["commander"];
    }

}
