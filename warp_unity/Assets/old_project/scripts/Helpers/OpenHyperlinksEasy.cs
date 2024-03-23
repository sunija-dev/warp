using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenHyperlinksEasy : MonoBehaviour
{
    public string m_strLink = "";

    public void OpenHyperlink()
    {
        Application.OpenURL(m_strLink);
    }
    
}
