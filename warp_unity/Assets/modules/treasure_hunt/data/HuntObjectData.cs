using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class HuntObjectData : ScriptableObject
{
    public GW2Position gw2pos = new GW2Position();
    public Quaternion quaternionRotation;
    public float fSpawnDistance = 30f;
    public string strComment = "";
}
