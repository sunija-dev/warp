using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarpManager : MonoBehaviour
{
    public static WarpManager Instance;

    public WarpMode m_warpMode;

    public enum WarpMode { HOST, CLIENT, SERVER }

    [Header("TestOptions")]
    public bool m_bLocalHost = false;
    public bool m_bLocalClientMina = false;
    public bool m_bOnlineTest = false;

    [Header("Detailed Test Options")]
    public bool m_bUseMinaData = false;
    public bool m_bLocalTest = false;

    [Header("Other Options")]
    public bool m_bAutoStart = true;
    public int[] arVersionDeleteSettingsIfOlder;

    [Header("References")]
    public WarpNetworkManager networkManager;
    public GameObject m_goTestMarker;

    private void Awake()
    {
        Instance = this;

        if (m_bLocalTest)
        {
            networkManager.networkAddress = "localhost";
        }

        if (m_bLocalClientMina)
        {
            m_bUseMinaData = true;
            m_warpMode = WarpMode.CLIENT;
            networkManager.networkAddress = "localhost";
        }

        if (m_bLocalHost)
        {
            m_bUseMinaData = false;
            m_warpMode = WarpMode.HOST;
            networkManager.networkAddress = "localhost";
        }

        if (m_bOnlineTest)
        {
            networkManager.GetComponent<Mirror.TelepathyTransport>().port = 7778;
            if (m_goTestMarker)
                m_goTestMarker.SetActive(true);
        }
            

        // start first, because setup needs the player object (to save language and region)
        if (m_bAutoStart)
        {
            if (Application.platform == RuntimePlatform.LinuxPlayer)
                networkManager.StartServer();
            else
            {
                if (m_warpMode == WarpMode.HOST)
                    networkManager.StartHost();
                else if (m_warpMode == WarpMode.CLIENT)
                    networkManager.StartClient();
                else if (m_warpMode == WarpMode.SERVER)
                    networkManager.StartServer();
            }
        }
    }
}
