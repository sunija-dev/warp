using System;
using System.Runtime.InteropServices;
using Blish_HUD.Controls.Extern;

namespace Blish_HUD.Input.WinApi {

    [StructLayout(LayoutKind.Sequential)]
    public struct MouseLLHookStruct {

        public POINT  Point     { get; }
        public int    MouseData { get; }
        public int    Flags     { get; }
        public int    Time      { get; }
        public IntPtr Extra     { get; }

    }

}
