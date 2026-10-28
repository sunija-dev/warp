using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;


namespace SuMumbleLinkGW2
{
    /// <summary>
    /// Just the MumbleLinkedMemory in a more convenient format.
    /// </summary>
    public struct GW2Info
    {
        public uint uiVersion;
        public uint uiTick;
        public float[] fAvatarPosition;
        public float[] fAvatarFront;
        public float[] fAvatarTop;
        public string name;
        public float[] fCameraPosition;
        public float[] fCameraFront;
        public float[] fCameraTop;
        public Identity identity;
        public uint context_len;

        // start of context
        public string serverAddress;
        public uint mapId;
        public uint mapType;
        public uint shardId;
        public uint instance;
        public uint buildId;
        public uint uiState;
        public int compassWidth;                    // pixels 
        public int compassHeight;                   // pixels 
        public float compassRotation;               // radians 
        public float playerX;                       // continentCoords 
        public float playerY;                       // continentCoords
        public float mapCenterX;                    // continentCoords 
        public float mapCenterY;                    // continentCoords
        public float mapScale;
        //end of context


        public void initGW2Info(MumbleLinkedMemory _linkedMemory)
        {
            unsafe
            { 
                uiVersion = _linkedMemory.uiVersion;
                uiTick = _linkedMemory.uiTick;
                fAvatarPosition = new float[3] { _linkedMemory.fAvatarPosition[0], _linkedMemory.fAvatarPosition[1], _linkedMemory.fAvatarPosition[2] };
                fAvatarFront = new float[3] { _linkedMemory.fAvatarFront[0], _linkedMemory.fAvatarFront[1], _linkedMemory.fAvatarFront[2] };
                fAvatarTop = new float[3] { _linkedMemory.fAvatarTop[0], _linkedMemory.fAvatarTop[1], _linkedMemory.fAvatarTop[2] };
                name = _linkedMemory.strGetName();
                fCameraPosition = new float[3] { _linkedMemory.fCameraPosition[0], _linkedMemory.fCameraPosition[1], _linkedMemory.fCameraPosition[2] };
                fCameraFront = new float[3] { _linkedMemory.fCameraFront[0], _linkedMemory.fCameraFront[1], _linkedMemory.fCameraFront[2] };
                fCameraTop = new float[3] { _linkedMemory.fCameraTop[0], _linkedMemory.fCameraTop[1], _linkedMemory.fCameraTop[2] };
                identity = _linkedMemory.identityGet();
                context_len = _linkedMemory.context_len;
                serverAddress = _linkedMemory.strGetServerAddress();
                mapId = _linkedMemory.mapId;
                mapType = _linkedMemory.mapType;
                shardId = _linkedMemory.shardId;
                instance = _linkedMemory.instance;
                buildId = _linkedMemory.buildId;
                uiState = _linkedMemory.uiState;
                compassWidth = _linkedMemory.compassWidth;
                compassHeight = _linkedMemory.compassHeight;
                compassRotation = _linkedMemory.compassRotation;
                playerX = _linkedMemory.playerX;
                playerY = _linkedMemory.playerY;
                mapCenterX = _linkedMemory.mapCenterX;
                mapCenterY = _linkedMemory.mapCenterY;
                mapScale = _linkedMemory.mapScale;
            }
        }
    }
}
