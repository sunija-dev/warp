using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class GW2Position 
{
    public Vector3 v3Pos = Vector3.zero;
    public int iMapID = -1;

    public GW2Position() { }

    public GW2Position(Vector3 _v3Pos, int _iMapID)
    {
        v3Pos = _v3Pos;
        iMapID = _iMapID;
    }

    public GW2Position(GW2Position _position)
    {
        v3Pos = _position.v3Pos;
        iMapID = _position.iMapID;
    }

    public float fDistanceTo(GW2Position positionOther)
    {
        if (iMapID != positionOther.iMapID)
            return Mathf.Infinity;

        return (v3Pos - positionOther.v3Pos).magnitude;
    }

    public override string ToString()
    {
        return "pos: " + v3Pos.ToString() + ", map: " + iMapID;
    }
}
