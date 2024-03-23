using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SuMumbleLinkGW2;
using Mirror;
using UnityEngine.Events;

public class MumbleManager : MonoBehaviour
{
    public static MumbleManager Instance;

    MumbleLink mumbleLink = new MumbleLink();
    public static GW2Info s_gw2Info = new GW2Info();
    public int iMapID = -1;
    public int iUiSize = 1;
    public Vector3 v3CharPos = Vector3.zero;
    public Vector3 v3CameraPos = Vector3.zero;
    public Vector3 v3CameraFront = Vector3.zero;
    public Vector2 v2ContinentPosition = Vector2.zero;
    public Vector2 v2MapCenter = Vector2.zero;
    public bool m_bReadValues = false;

    public bool bMapOpen = false;
    public bool bCompassTopRight = false;
    public bool bCompassHasRotationEnabled = false;
    public bool bGameHasFocus = false;
    public bool bIsInCompetitiveGameMode = false;
    public bool bTextboxHasFocus = false;
    public bool bIsInCombat = false;

    public UnityEvent eventMapOpened = new UnityEvent();
    public UnityEvent eventMapClosed = new UnityEvent();
    public UnityEvent eventMapIdChanged = new UnityEvent();
    public UnityEvent eventUiSizeChanged = new UnityEvent();
    public UnityEvent eventInCompetitiveGameModeChanged = new UnityEvent();

    // Debugging
    private bool m_bUseDebugData = false;

    private const float INCH_TO_METER = 0.0254f;

    private Vector3 v3DebugPos = Vector3.zero;


    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        m_bUseDebugData = WarpManager.Instance.m_bUseMinaData;
    }

    void Update()
    {
        ReadMumbleData();
    }

    void OnApplicationQuit()
    {
        mumbleLink.Dispose();
    }

    private void ReadMumbleData()
    {
        //m_bTestsClient = WarpNetworkManager.Instance.mode == NetworkManagerMode.ClientOnly;
        //m_bTestsHost = WarpNetworkManager.Instance.mode == NetworkManagerMode.Host;

        m_bReadValues = false;

        try
        {
            s_gw2Info = mumbleLink.Read();
            if (s_gw2Info.buildId != 0)
                m_bReadValues = true;
        }
        catch { }

        if (m_bReadValues && s_gw2Info.identity != null)
        {
            v3CameraPos.x = s_gw2Info.fCameraPosition[0];
            v3CameraPos.y = s_gw2Info.fCameraPosition[1];
            v3CameraPos.z = s_gw2Info.fCameraPosition[2];

            v3CameraFront.x = s_gw2Info.fCameraFront[0];
            v3CameraFront.y = s_gw2Info.fCameraFront[1];
            v3CameraFront.z = s_gw2Info.fCameraFront[2];

            v3CharPos.x = s_gw2Info.fAvatarPosition[0];
            v3CharPos.y = s_gw2Info.fAvatarPosition[1];
            v3CharPos.z = s_gw2Info.fAvatarPosition[2];

            v2ContinentPosition.x = s_gw2Info.playerX;
            v2ContinentPosition.y = s_gw2Info.playerY;

            v2MapCenter.x = s_gw2Info.mapCenterX;
            v2MapCenter.y = s_gw2Info.mapCenterY;

            int iMapIDNew = (int)s_gw2Info.identity.uiMapId;
            if (iMapIDNew != iMapID)
            {
                iMapID = iMapIDNew;
                eventMapIdChanged.Invoke();
            }

            int iUiSizeNew = (int)s_gw2Info.identity.uiUiSize;
            if (iUiSize != iUiSizeNew)
            {
                iUiSize = iUiSizeNew;
                eventUiSizeChanged.Invoke();
            }

            uint uiUiBitmask = s_gw2Info.uiState;

            bool bMapOpenNew = uiUiBitmask.IsBitSet(0);
            if (!bMapOpen && bMapOpenNew) eventMapOpened.Invoke();
            if (bMapOpen && !bMapOpenNew) eventMapClosed.Invoke();
            bMapOpen = bMapOpenNew;

            bool bIsInCompetitiveGameModeNew = uiUiBitmask.IsBitSet(4);
            bool bIsInCompetitiveGameModeChanged = bIsInCompetitiveGameModeNew != bIsInCompetitiveGameMode;
            bIsInCompetitiveGameMode = bIsInCompetitiveGameModeNew;
            if (bIsInCompetitiveGameModeChanged)
                eventInCompetitiveGameModeChanged.Invoke();

            bCompassTopRight = uiUiBitmask.IsBitSet(1);
            bCompassHasRotationEnabled = uiUiBitmask.IsBitSet(2);
            bGameHasFocus = uiUiBitmask.IsBitSet(3);
            bTextboxHasFocus = uiUiBitmask.IsBitSet(5);
            bIsInCombat = uiUiBitmask.IsBitSet(6);

            if (m_bUseDebugData)
            {
                if (v3DebugPos == Vector3.zero)
                    v3DebugPos = v3CharPos;
                else
                    v3CharPos = v3DebugPos;

                s_gw2Info.identity.strName = "Mina Demena";
            }
        }
    }
}
