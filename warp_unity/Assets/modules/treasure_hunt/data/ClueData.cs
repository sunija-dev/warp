using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable, CreateAssetMenu(fileName = "clue", menuName = "ScriptableObjects/TreasureHunt/Clue", order = 1)]
public class ClueData : HuntObjectData
{
    [TextArea]
    public string strText = "clue";
}
