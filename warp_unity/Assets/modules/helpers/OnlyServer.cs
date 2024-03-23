using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnlyServer : MonoBehaviour
{
    public void Awake()
    {
        if (!(WarpManager.Instance.m_warpMode == WarpManager.WarpMode.HOST 
            || WarpManager.Instance.m_warpMode == WarpManager.WarpMode.SERVER
            || Application.platform == RuntimePlatform.LinuxPlayer))
        {
            gameObject.SetActive(false);
            Destroy(this.gameObject);
        }
    }
}
