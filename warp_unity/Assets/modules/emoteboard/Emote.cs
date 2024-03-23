using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Emote", menuName = "warp/Emote", order = 1)]
public class Emote : ScriptableObject
{
    public string strName = "EmoteName";
    public string strCommand = "command";
    public Sprite sprite;
}
