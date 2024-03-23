using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AccountSheet
{
    public enum Experience { Just_Started, Beginner, Medium, Experienced }
    public Experience experience;

    public enum FightingStyle { Emote, Dice, Any }
    public FightingStyle fightingStyle;

    public enum AdultStatus { Teen, Adult }
    public AdultStatus adultStatus;

    public float fLoreStrictness = 0.5f;
    public float fTopic = 0.5f;
    public float fStyle = 0.5f;
    public float fPostLength = 0.5f;

    public string strExamplePost = "";

    public AccountSheet() { }

    public AccountSheet(Experience experience, FightingStyle fightingStyle, AdultStatus adultStatus, 
        float fLoreStrictness, float fTopic, float fStyle, float fPostLength, string strExamplePost)
    {
        this.experience = experience;
        this.fightingStyle = fightingStyle;
        this.adultStatus = adultStatus;
        this.fLoreStrictness = fLoreStrictness;
        this.fTopic = fTopic;
        this.fStyle = fStyle;
        this.fPostLength = fPostLength;
        this.strExamplePost = strExamplePost;
    }

    public AccountSheet accountSheetCopy()
    {
        return (AccountSheet)this.MemberwiseClone();
    }
}
