using System;
using System.Runtime.InteropServices;

namespace Blish_HUD.Input.WinApi {

    internal static class HookExtern {

        public delegate int HookCallbackDelegate(int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern IntPtr SetWindowsHookEx(HookType idHook, HookCallbackDelegate lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll")]
        public static extern int CallNextHookEx(HookType idHook, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool UnhookWindowsHookEx(IntPtr hook);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr GetModuleHandleW(IntPtr fakezero);

        // Raw message loop for the hook thread. Replaces WinForms' Application.Run, whose static
        // constructor throws on keyboard layouts Mono doesn't know (e.g. es-419, LCID 0x580A).
        public const uint WM_QUIT     = 0x0012;
        public const uint PM_NOREMOVE = 0x0000;

        [StructLayout(LayoutKind.Sequential)]
        public struct MSG {
            public IntPtr hwnd;
            public uint   message;
            public IntPtr wParam;
            public IntPtr lParam;
            public uint   time;
            public int    ptX;
            public int    ptY;
        }

        [DllImport("user32.dll")]
        public static extern int GetMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool PeekMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax, uint wRemoveMsg);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool TranslateMessage(ref MSG lpMsg);

        [DllImport("user32.dll")]
        public static extern IntPtr DispatchMessage(ref MSG lpMsg);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool PostThreadMessage(uint idThread, uint msg, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll")]
        public static extern uint GetCurrentThreadId();

        // Mouse button state without MonoGame's Mouse.GetState, which goes through WinForms
        public const int VK_LBUTTON    = 0x01;
        public const int VK_RBUTTON    = 0x02;
        public const int VK_MBUTTON    = 0x04;
        public const int VK_XBUTTON1   = 0x05;
        public const int VK_XBUTTON2   = 0x06;
        public const int SM_SWAPBUTTON = 23;

        [DllImport("user32.dll")]
        public static extern short GetAsyncKeyState(int vKey);

        [DllImport("user32.dll")]
        public static extern int GetSystemMetrics(int nIndex);

    }
}
