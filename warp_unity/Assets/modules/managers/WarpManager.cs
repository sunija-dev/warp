using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class WarpManager : MonoBehaviour
{
    public static WarpManager Instance;

    [Header("Test Options")]
    public WarpMode m_warpMode;
    public enum WarpMode { HOST, CLIENT, SERVER }
    [Tooltip("Connects to your local pc instead of the warp server.")]
    [ShowIf(nameof(m_warpMode), WarpMode.CLIENT)] public bool m_bConnectToLocal = false;
    [Tooltip("Simulates minimal mumble data (character name and position) so you can test without having GW2 open.")]
    public bool m_bUseMinaData = false;
    public bool m_bUsePTRPort = false;

    [Header("Other Options")]
    public bool m_bAutoStart = true;
    public int[] arVersionDeleteSettingsIfOlder;

    [Header("References")]
    public WarpNetworkManager networkManager;
    public GameObject m_goTestMarker;

    private void Awake()
    {
        Instance = this;

        if (m_warpMode == WarpMode.HOST)
            m_bConnectToLocal = true;

        if (m_bConnectToLocal)
            networkManager.networkAddress = "localhost";

        if (m_bUsePTRPort)
        {
            networkManager.GetComponent<Mirror.TelepathyTransport>().port = 7778;
            if (m_goTestMarker)
                m_goTestMarker.SetActive(true);
        }
    
        // start first, because setup needs the player object (to save language and region)
        if (m_bAutoStart)
        {
            if (Application.platform == RuntimePlatform.LinuxPlayer || Application.platform == RuntimePlatform.LinuxServer)
            {
                Debug.Log("Starting server");
                networkManager.StartServer();
            }
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
