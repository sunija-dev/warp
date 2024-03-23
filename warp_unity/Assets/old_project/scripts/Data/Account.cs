using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Account
{
    public string strApiKey;
    public System.DateTime dateCreated;
    public GW2APIAccountInfo accountInfo;
    public GW2APICharInfo[] arGW2Characters;

    public GlobalEnums.Language language;
    public GlobalEnums.Region region;

    public int iMaxItems = -1;
    public int iBanned = -1;
    public int iAdminLevel = -1;
}
