using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapMarkerChar : MonoBehaviour
{
    public float fLerpSpeed = 10f;
    public WorldInfo.PlayerInfo playerInfo { get; private set; }
    [SerializeField] private Tooltip toolTip;

    private Vector2 v2ContinentPositionLerp = Vector2.zero;

    public void UpdateData(WorldInfo.PlayerInfo _playerInfo)
    {
        playerInfo = _playerInfo;
        toolTip.strText = playerInfo.strName;
        UpdatePosition();
    }

    private void Update()
    {
        UpdatePosition();
        //UpdateScale();
    }

    private void UpdatePosition()
    {
        float fSmallWindowScale = 1f;
        if (Screen.width < 1024 || Screen.height < 768) // hardcoded in GW2
            fSmallWindowScale = Mathf.Min(Screen.width / 1024f, Screen.height / 768f);

        v2ContinentPositionLerp = Vector3.Lerp(v2ContinentPositionLerp, playerInfo.v2ContinentPosition, Time.deltaTime * fLerpSpeed);
        Vector3 v3Position = (v2ContinentPositionLerp - MumbleManager.Instance.v2MapCenter) / MumbleManager.s_gw2Info.mapScale * fSmallWindowScale;

        v3Position.y = (Screen.height / 2f) - v3Position.y;
        v3Position.x += Screen.width / 2f;

        gameObject.transform.position = v3Position;
    }

    private void UpdateScale()
    {
        float fScale = Settings.iRead(Settings.OptionKey.iDpiScaling) / 100f;
        if (fScale != transform.localScale.x)
            transform.localScale = Vector3.one * fScale;
    }
}
