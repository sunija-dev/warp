using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnlyClient : MonoBehaviour
{
    public void Awake()
    {
        if (Application.platform == RuntimePlatform.LinuxPlayer)
            gameObject.SetActive(false);
    }    
}
