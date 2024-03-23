using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// Information that the player can edit about her character.
/// </summary>
[Serializable]
public class CharSheet 
{
    // info stuff
    public string strRPName = "CharName";
    public string strRPDesc = "";
    public string strKnownFor = "";
    public List<Aspect> liAspects = new List<Aspect>() { new Aspect(), new Aspect(), new Aspect(), new Aspect() };

    [Serializable]
    public struct Aspect
    {
        public string strName;
        public string strDesc;
        public int iIconId;
    }

    public CharSheet() { }

    public CharSheet(string _strName, string _strDesc, string _strKnownFor, List<Aspect> _liAspects)
    {
        strRPName = _strName;
        strRPDesc = _strDesc;
        strKnownFor = _strKnownFor;
        liAspects = _liAspects;
    }

    public override string ToString()
    {
        string strOutput = string.Format("CharSheet: {0}\nDescription:\n{1}\nKnownFor:\n{2}\n", strRPName, strRPDesc, strKnownFor);
        foreach (Aspect aspect in liAspects)
        {
            strOutput += string.Format("Aspect: {0}\n{1}\n", aspect.strName, aspect.strDesc);
        }
        return strOutput;
    }

    public CharSheet charSheetCopy()
    {
        return (CharSheet)this.MemberwiseClone();
    }
}


