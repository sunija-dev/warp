using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable, CreateAssetMenu(fileName = "clue", menuName = "ScriptableObjects/TreasureHunt/QuestGiver", order = 1)]
public class QuestGiverData : HuntObjectData
{
    public string strName = "name";
    [TextArea] public string strText = "clue";
    public string strKeyword = "keyword";

    [TextArea] public string strTextSuccess = "success";
    [TextArea] public string strTextFail = "fail";
}
