using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class WindowHome : MonoBehaviour
{
    public TMP_Text textGreeting;
    
    void Start()
    {
        ClientManager.Instance.eAccountChanged.AddListener(OnAccountChanged);
        if (ClientManager.Instance.bAccLoggedIn)
            OnAccountChanged();
    }

    void OnAccountChanged()
    {
        textGreeting.text = string.Format("Hi {0}!", Player.Instance.account.accountInfo.name); // TODO: Localize!
    }


}
