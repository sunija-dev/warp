using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class ServerSettings
{
    public string strPassword = "";
    public float fVisibilityRange = 50f;
    public float fInterestRebuildInterval = 1f;
    public float fBackupDBInterval = 12f * 60f * 60f;
    public int iTickRate = 30;
    public Vector3 v3CloseTime = new Vector3(10, 0, 0);

    //public float fDebugGroupsInterval = 240f;
}

