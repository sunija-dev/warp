using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Camera cameraMain;
    private MumbleManager mumbleManager;

    void Start()
    {
        mumbleManager = MumbleManager.Instance;
    }

    void Update()
    {
        if (mumbleManager.m_bReadValues && MumbleManager.s_gw2Info.identity != null)
        {
            transform.position = mumbleManager.v3CameraPos;
            transform.rotation = Quaternion.LookRotation(mumbleManager.v3CameraFront);

            float fFovMultiplier = MumbleManager.s_gw2Info.identity.fFov;
            if (fFovMultiplier < 0.1f)
                fFovMultiplier = 1f;
            cameraMain.fieldOfView = 60f * fFovMultiplier;
        }
    }
}
