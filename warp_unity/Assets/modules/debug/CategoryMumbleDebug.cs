using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CategoryMumbleDebug : MonoBehaviour
{
    public TMP_Text textOutput;

    void Update()
    {
        SuMumbleLinkGW2.GW2Info gw2Info = MumbleManager.s_gw2Info;

        string strOutput = "";
        strOutput += "ServerAddress: " + gw2Info.serverAddress + "\n";
        strOutput += "MapId: " + gw2Info.mapId + "\n";
        strOutput += "MapType: " + gw2Info.mapType + "\n";
        strOutput += "ShardId: " + gw2Info.shardId + "\n";
        strOutput += "Instance: " + gw2Info.instance + "\n";
        strOutput += "BuildID: " + gw2Info.buildId + "\n";
        // Bitmask: Bit 1 = IsMapOpen, Bit 2 = IsCompassTopRight, Bit 3 = DoesCompassHaveRotationEnabled,
        // Bit 4 = Game has focus, Bit 5 = Is in Competitive game mode, Bit 6 = Textbox has focus, Bit 7 = Is in Combat
        uint uiUiBitmask = gw2Info.uiState;
        strOutput += string.Format("UiState: {0}{1}{2}{3}{4}{5}{6}\n",
            uiUiBitmask.IsBitSet(0) ? "MapOpen, " : "",
            uiUiBitmask.IsBitSet(1) ? "CompassTopRight, " : "",
            uiUiBitmask.IsBitSet(2) ? "CompassHasRotationEnabled, " : "",
            uiUiBitmask.IsBitSet(3) ? "GameHasFocus, " : "",
            uiUiBitmask.IsBitSet(4) ? "IsInCompetitiveGameMode, " : "",
            uiUiBitmask.IsBitSet(5) ? "TextboxHasFocus, " : "",
            uiUiBitmask.IsBitSet(6) ? "IsInCombat" : ""
            );

        strOutput += string.Format("CompassSize: ({0}, {1})\n", gw2Info.compassWidth, gw2Info.compassHeight);
        strOutput += "CompassRotation: " + gw2Info.compassRotation + "\n";
        strOutput += string.Format("PlayerXY Continent: ({0}, {1})\n", gw2Info.playerX, gw2Info.playerY);
        strOutput += string.Format("MapCenter Continent: ({0}, {1})\n", gw2Info.mapCenterX, gw2Info.mapCenterY);
        strOutput += "MapScale: " + gw2Info.mapScale + "\n";
        strOutput += "\n";
        strOutput += string.Format("Char: {0}, {1}, {2}, MapID: {3}, WorldID: {4}, Fov: {5}, UISize: {6}\n",
            gw2Info.identity.strName, gw2Info.identity.iProfession, gw2Info.identity.iRace, 
            gw2Info.identity.uiMapId, gw2Info.identity.uiWorldId, gw2Info.identity.fFov, gw2Info.identity.uiUiSize);
        strOutput += string.Format("CharPos: {0}, \nCamPos: {1}, \nCamFront: {2}, \nCam-Char: {3}\n",
            MumbleManager.Instance.v3CharPos.ToString("F3"), MumbleManager.Instance.v3CameraPos.ToString("F3"), MumbleManager.Instance.v3CameraFront.ToString("F3"),
            (MumbleManager.Instance.v3CameraPos - MumbleManager.Instance.v3CharPos).ToString("F3"));



        textOutput.text = strOutput;
    }
}
