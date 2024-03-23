using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;
using Newtonsoft.Json;

namespace SuMumbleLinkGW2
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public unsafe struct MumbleLinkedMemory
    {
        public uint uiVersion;                      // 4
        public uint uiTick;                         // 4
        public fixed float fAvatarPosition[3];      // 12
        public fixed float fAvatarFront[3];         // 12
        public fixed float fAvatarTop[3];           // 12
        public fixed byte name[512];                // 512
        public fixed float fCameraPosition[3];      // 12
        public fixed float fCameraFront[3];         // 12
        public fixed float fCameraTop[3];           // 12
        public fixed byte identity[512];            // 512
        public uint context_len;                    // 4
        //public fixed byte context[512];           // 512 // not needed, because is following
        // start of context
        public fixed byte serverAddress[28];        // context[0], contains sockaddr_in or sockaddr_in6
        public uint mapId;                          // context[28]
        public uint mapType;                        // context[32]
        public uint shardId;                        // context[36]
        public uint instance;                       // context[40]
        public uint buildId;                        // context[44]
        public uint uiState;                        // context[48]
        public UInt16 compassWidth;                 // pixels // context[50]
        public UInt16 compassHeight;                // pixels // context[52]
        public float compassRotation;               // radians // context[56]
        public float playerX;                       // continentCoords // context[60]
        public float playerY;                       // continentCoords // context[64]
        public float mapCenterX;                    // continentCoords // context[68]
        public float mapCenterY;                    // continentCoords // context[72]
        public float mapScale;                      // context[76]
        //end of context
        public fixed byte contextUnused[432];
        public fixed byte description[4096];        // 4096
  

        public override string ToString()
        {
            var str = new StringBuilder();

            // TODO: renmae vars to be small starting
            str.AppendLine("uiVersion : " + uiVersion);
            str.AppendLine("uiTick : " + uiTick);
            str.AppendFormat("fAvatarPosition : [{0}, {1}, {2}]\n", fAvatarPosition[0], fAvatarPosition[1], fAvatarPosition[2]);
            str.AppendFormat("fAvatarFront : [{0}, {1}, {2}]\n", fAvatarFront[0], fAvatarFront[1], fAvatarFront[2]);
            str.AppendFormat("fAvatarTop : [{0}, {1}, {2}]\n", fAvatarTop[0], fAvatarTop[1], fAvatarTop[2]);
            str.AppendLine("strName: " + strGetName());
            str.AppendFormat("fCameraPosition : [{0}, {1}, {2}]\n", fCameraPosition[0], fCameraPosition[1], fCameraPosition[2]);
            str.AppendFormat("fCameraFront : [{0}, {1}, {2}]\n", fCameraFront[0], fCameraFront[1], fCameraFront[2]);
            str.AppendFormat("fCameraTop : [{0}, {1}, {2}]\n", fCameraTop[0], fCameraTop[1], fCameraTop[2]);
            str.AppendLine("identity: \n" + identityGet().ToString());
            str.AppendLine("serverAdress: " + strGetServerAddress());
            str.AppendLine("mapId : " + mapId);
            str.AppendLine("mapType : " + mapType);
            str.AppendLine("shardId : " + shardId);
            str.AppendLine("instance : " + instance);
            str.AppendLine("buildId : " + buildId);
            str.AppendLine("uiState : " + uiState);
            str.AppendLine("compassWidth : " + compassWidth);
            str.AppendLine("compassHeight : " + compassHeight);
            str.AppendLine("compassRotation : " + compassRotation);
            str.AppendLine("playerX : " + playerX);
            str.AppendLine("playerY : " + playerY);
            str.AppendLine("mapCenterX : " + mapCenterX);
            str.AppendLine("mapCenterY : " + mapCenterY);
            str.AppendLine("mapScale : " + mapScale);
            str.AppendLine("description : " + strGetDescription());

            return str.ToString();
        }

        public string strGetName()
        {
            fixed (MumbleLinkedMemory* _data = &this)
            {
                byte[] strBytes = new byte[512];
                IntPtr strBytesIntPtr = new IntPtr((void*)_data->name);
                Marshal.Copy(strBytesIntPtr, strBytes, 0, 512);
                return Encoding.Unicode.GetString(strBytes, 0, 512).Trim('\0');
            }
        }

        public Identity identityGet()
        {
            unsafe
            {
                fixed (MumbleLinkedMemory* _data = &this)
                {
                    string strIdentity;
                    byte[] strBytes = new byte[512];
                    IntPtr strBytesIntPtr = new IntPtr((void*)_data->identity);
                    Marshal.Copy(strBytesIntPtr, strBytes, 0, 512);
                    strIdentity = Encoding.Unicode.GetString(strBytes);
                    strIdentity = strIdentity.Substring(0, strIdentity.IndexOf('}') + 1);
                    Identity _id = new Identity();
                    if (strIdentity.Length > 0)
                    {
                        _id = JsonConvert.DeserializeObject<Identity>(strIdentity);
                    }
                    return _id;
                }
            }
        }

        public string strGetServerAddress()
        {
            fixed (MumbleLinkedMemory* _data = &this)
            {
                string strResult = "";
                byte[] strBytes = new byte[28];
                IntPtr strBytesIntPtr = new IntPtr((void*)_data->serverAddress);
                Marshal.Copy(strBytesIntPtr, strBytes, 0, 28);
                for (int i = 0; i < strBytes.Length; i++)
                {
                    strResult += strBytes[i].ToString();
                    if (i != strBytes.Length - 1)
                    {
                        strResult += ", ";
                    }
                }
                return strResult;
            }
        }

        public string strGetDescription()
        {
            fixed (MumbleLinkedMemory* _data = &this)
            {
                byte[] strBytes = new byte[4096];
                IntPtr strBytesIntPtr = new IntPtr((void*)_data->description);
                Marshal.Copy(strBytesIntPtr, strBytes, 0, 4096);
                return Encoding.Unicode.GetString(strBytes, 0, 4096).Trim('\0');
            }
        }
    }

    public class Identity
    {
        [JsonProperty("name")]
        public string strName;

        [JsonProperty("profession")]
        public uint iProfession;

        [JsonProperty("spec")]
        public uint iSpec;

        [JsonProperty("race")]
        public uint iRace;

        [JsonProperty("map_id")]
        public uint uiMapId;

        [JsonProperty("world_id")]
        public uint uiWorldId;

        [JsonProperty("team_color_id")]
        public uint uiTeamColorId;

        [JsonProperty("commander")]
        public bool bIsCommander;

        [JsonProperty("fov")]
        public float fFov;

        [JsonProperty("uisz")]
        public uint uiUiSize;

        public override string ToString()
        {
            var str = new StringBuilder();

            str.AppendLine("name: " + strName);
            str.AppendLine("Profession: " + iProfession);
            str.AppendLine("iSpec: " + iSpec);
            str.AppendLine("iRace: " + iRace);
            str.AppendLine("MapId: " + uiMapId);
            str.AppendLine("WorldId: " + uiWorldId);
            str.AppendLine("TeamColorId: " + uiTeamColorId);
            str.AppendLine("IsCommander: " + bIsCommander);
            str.AppendLine("fFov: " + fFov);
            str.AppendLine("iUiSize: " + uiUiSize);

            return str.ToString();
        }
    }
}
