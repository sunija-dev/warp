﻿using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using Microsoft.Xna.Framework;

// ReSharper disable FieldCanBeMadeReadOnly.Local
// ReSharper disable InconsistentNaming

namespace Blish_HUD
{

    public static class WindowUtil
    {

        //private static readonly Logger Logger = Logger.GetLogger(typeof(WindowUtil));

        private const uint WS_EX_TOPMOST = 0x00000008;
        private const uint WS_EX_TRANSPARENT = 0x00000020;
        private const uint WS_EX_TOOLWINDOW = 0x00000080;
        private const uint WS_EX_CONTROLPARENT = 0x00010000;
        private const uint WS_EX_APPWINDOW = 0x00040000;
        private const uint WS_EX_LAYERED = 0x00080000;

        private const int GWL_STYLE = -16;
        private const int GWL_EXSTYLE = -20;

        private const int SW_HIDE = 0;
        private const int SW_SHOW = 5;

        private const uint CS_VREDRAW = 0x0001;
        private const uint CS_HREDRAW = 0x0002;

        private static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);
        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);

        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOMOVE = 0x0002;

        private const int MINIMIZED_POS = -32000;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool ClientToScreen(IntPtr hWnd, ref System.Drawing.Point lpPoint);

        [DllImport("Dwmapi.dll")]
        private static extern int DwmExtendFrameIntoClientArea(IntPtr hWnd, ref Margins pMarInset);

        [DllImport("user32.dll")]
        private static extern IntPtr GetActiveWindow();

        [DllImport("User32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern long GetClassName(IntPtr hwnd, StringBuilder lpClassName, long nMaxCount);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool GetClientRect(IntPtr hWnd, ref RECT lpRect);

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
        private static extern IntPtr GetWindowLongPtr32(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr")]
        private static extern IntPtr GetWindowLongPtr64(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);

        [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
        private static extern int SetWindowLong32(IntPtr hWnd, int nIndex, uint dwNewLong);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
        private static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, UIntPtr dwNewLong);

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        private struct Margins
        {
            public int cxLeftWidth;
            public int cxRightWidth;
            public int cyTopHeight;
            public int cyBottomHeight;
        }

        public enum GW : uint
        {
            HWNDFIRST = 0,
            HWNDLAST = 1,
            HWNDNEXT = 2,
            HWNDPREV = 3,
            OWNER = 4,
            CHILD = 5,
            ENABLEDPOPUP = 6
        }

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr GetWindow(IntPtr hWnd, GW uCmd);

        public static void SetForegroundWindowEx(IntPtr handle) => SetForegroundWindow(handle);

        private static IntPtr GetWindowLong(IntPtr hWnd, int nIndex) => IntPtr.Size == 8 ? GetWindowLongPtr64(hWnd, nIndex) : GetWindowLongPtr32(hWnd, nIndex);

        public static int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong) => IntPtr.Size != 8 ? SetWindowLong32(hWnd, nIndex, dwNewLong) : (int)SetWindowLongPtr64(hWnd, nIndex, new UIntPtr(dwNewLong));

        public static void SetWindowParam(IntPtr winHandle, bool showInTaskbar = false)
        {
            uint windowParam = WS_EX_TOPMOST | WS_EX_TRANSPARENT | WS_EX_CONTROLPARENT | WS_EX_LAYERED;

            if (showInTaskbar)
            {
                ShowWindow(winHandle, SW_HIDE);
                windowParam |= WS_EX_APPWINDOW;
            }
            else
            {
                windowParam |= WS_EX_TOOLWINDOW;
            }

            SetWindowLong(winHandle, GWL_EXSTYLE, windowParam);

            if (showInTaskbar)
            {
                ShowWindow(winHandle, SW_SHOW);
            }
        }

        public static void SetShowInTaskbar(IntPtr winHandle, bool showInTaskbar)
        {
            SetWindowParam(winHandle, showInTaskbar);
        }

        public static void SetupOverlay(IntPtr winHandle)
        {
            SetWindowLong(winHandle, GWL_STYLE, CS_HREDRAW | CS_VREDRAW);

            // Let Gw2InstanceIntegration sync behavior with game runtime
            SetShowInTaskbar(winHandle, false);

            SetLayeredWindowAttributes(winHandle, 0, 0, 1);
            SetLayeredWindowAttributes(winHandle, 0, 255, 2);
        }

        public enum OverlayUpdateResponse
        {
            WithFocus,
            WithoutFocus,
            Minimized,
            Errored
        }

        private static Rectangle rectanglePos;


        /*
        // This method is not used. I basically re-implemented it in unity.
        public static (OverlayUpdateResponse Response, bool Minimized, int ErrorCode) UpdateOverlay(IntPtr winHandle, IntPtr gw2WindowHandle, bool wasOnTop)
        {
            var clientRect = new RECT();
            bool errGetClientRectResult = GetClientRect(gw2WindowHandle, ref clientRect);

            // Probably errors caused by gw2 closing at the time of the call or the call is super early and has the wrong handle somehow
            if (!errGetClientRectResult)
            {
                int errorCode = Marshal.GetLastWin32Error();

                //Logger.Warn($"{nameof(GetClientRect)} failed with error code {errorCode}.");
                return (OverlayUpdateResponse.Errored, false, errorCode);
            }

            var screenPoint = System.Drawing.Point.Empty;
            bool errClientToScreen = ClientToScreen(gw2WindowHandle, ref screenPoint);

            // Probably errors caused by gw2 closing at the time of the call
            if (!errClientToScreen)
            {
                int errorCode = Marshal.GetLastWin32Error();

                //Logger.Warn($"{nameof(ClientToScreen)} failed with error code {errorCode}.");
                return (OverlayUpdateResponse.Errored, screenPoint.X == MINIMIZED_POS, errorCode);
            }

            //GameService.Debug.StartTimeFunc("GetForegroundWindow");
            var activeWindowHandle = GetForegroundWindow();
            //GameService.Debug.StopTimeFunc("GetForegroundWindow");

            // If Guild Wars 2 is not the focused application, set Blish HUD to
            // be above Guild Wars 2, but below the next application's window in z-order
            if (activeWindowHandle != gw2WindowHandle && Form.ActiveForm == null)
            {
                if (wasOnTop)
                {
                    //Logger.Debug("GW2 is no longer the active window.");

                    var nextHandle = GetWindow(gw2WindowHandle, GW.HWNDPREV);
                    if (nextHandle != IntPtr.Zero && nextHandle != winHandle)
                    {
                        SetWindowPos(winHandle, nextHandle, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE);
                    }
                }
                return (OverlayUpdateResponse.WithoutFocus, screenPoint.X == MINIMIZED_POS, 0);
            }

            if (!wasOnTop)
            {
                //Logger.Debug("GW2 is now the active window - reactivating the overlay.");
            }

            // SCALE WINDOW TO WINDOWED MODE

            
            if (clientRect.Left + screenPoint.X != rectanglePos.X 
                || clientRect.Top + screenPoint.Y != rectanglePos.Y 
                || clientRect.Right - clientRect.Left != rectanglePos.Width 
                || clientRect.Bottom - clientRect.Top != rectanglePos.Height 
                || wasOnTop == false)
            {
                rectanglePos = new Rectangle(
                    clientRect.Left + screenPoint.X,
                    clientRect.Top + screenPoint.Y,
                    clientRect.Right - clientRect.Left,
                    clientRect.Bottom - clientRect.Top
                );

                
                //GameService.Graphics.Resolution = new Point(
                //    pos.Width,
                //    pos.Height
                //);
                

                SetWindowPos(winHandle, HWND_TOPMOST, clientRect.Left + screenPoint.X, clientRect.Top + screenPoint.Y, clientRect.Right - clientRect.Left, clientRect.Bottom - clientRect.Top, 0);

                var marg = new Margins
                {
                    cxLeftWidth = 0,
                    cyTopHeight = 0,
                    cxRightWidth = clientRect.Right,
                    cyBottomHeight = clientRect.Bottom
                };

                DwmExtendFrameIntoClientArea(winHandle, ref marg);
            }
            
            // set default to topmost, as long as scaling doesn't work
            //SetWindowPos(winHandle, HWND_TOPMOST, 0, 0, 0, 0, 0);


            return (OverlayUpdateResponse.WithFocus, screenPoint.X == MINIMIZED_POS, 0);
        }
        */

        public static void ScaleToGW2(IntPtr winHandle, IntPtr gw2WindowHandle, bool wasOnTop)
        {
            RECT rectClient = new RECT();
            bool errGetClientRectResult = GetClientRect(gw2WindowHandle, ref rectClient);
            // Probably errors caused by gw2 closing at the time of the call or the call is super early and has the wrong handle somehow
            if (!errGetClientRectResult)
            {
                int errorCode = Marshal.GetLastWin32Error();
                return;
            }

            System.Drawing.Point screenPoint = System.Drawing.Point.Empty;
            bool errClientToScreen = ClientToScreen(gw2WindowHandle, ref screenPoint);
            // Probably errors caused by gw2 closing at the time of the call
            if (!errClientToScreen)
            {
                int errorCode = Marshal.GetLastWin32Error();
                return;
            }

            if (rectClient.Left + screenPoint.X != rectanglePos.X 
                || rectClient.Top + screenPoint.Y != rectanglePos.Y 
                || rectClient.Right - rectClient.Left != rectanglePos.Width 
                || rectClient.Bottom - rectClient.Top != rectanglePos.Height 
                || wasOnTop == false)
            {
                rectanglePos = new Rectangle(
                    rectClient.Left + screenPoint.X,
                    rectClient.Top + screenPoint.Y,
                    rectClient.Right - rectClient.Left,
                    rectClient.Bottom - rectClient.Top
                );

                SetWindowPos(winHandle, HWND_TOPMOST, rectClient.Left + screenPoint.X, rectClient.Top + screenPoint.Y, rectClient.Right - rectClient.Left, rectClient.Bottom - rectClient.Top, 0);

                Margins margins = new Margins
                {
                    cxLeftWidth = 0,
                    cyTopHeight = 0,
                    cxRightWidth = rectClient.Right,
                    cyBottomHeight = rectClient.Bottom
                };

                DwmExtendFrameIntoClientArea(winHandle, ref margins);
            }
        }

        public static (int, int, int, int) rectGetGW2Window(IntPtr _gw2WindowHandle)
        {
            RECT rectClient = new RECT();
            bool errGetClientRectResult = GetClientRect(_gw2WindowHandle, ref rectClient);
            // Probably errors caused by gw2 closing at the time of the call or the call is super early and has the wrong handle somehow
            if (!errGetClientRectResult)
            {
                int errorCode = Marshal.GetLastWin32Error();
                return (-1, -1, -1, -1);
            }

            System.Drawing.Point screenPoint = System.Drawing.Point.Empty;
            bool errClientToScreen = ClientToScreen(_gw2WindowHandle, ref screenPoint);
            // Probably errors caused by gw2 closing at the time of the call
            if (!errClientToScreen)
            {
                int errorCode = Marshal.GetLastWin32Error();
                return (-1, -1, -1, -1);
            }

            rectanglePos = new Rectangle(
                rectClient.Left + screenPoint.X,
                rectClient.Top + screenPoint.Y,
                rectClient.Right - rectClient.Left,
                rectClient.Bottom - rectClient.Top
            );

            return (rectanglePos.X, rectanglePos.Y, rectanglePos.Width, rectanglePos.Height);
        }

        /*
        public static (int, int) ScaleToGW2Part2(IntPtr winHandle, IntPtr gw2WindowHandle, bool wasOnTop)
        {
            var clientRect = new RECT();
            bool errGetClientRectResult = GetClientRect(gw2WindowHandle, ref clientRect);

            // Probably errors caused by gw2 closing at the time of the call or the call is super early and has the wrong handle somehow
            if (!errGetClientRectResult)
            {
                int errorCode = Marshal.GetLastWin32Error();
                return (-1, -1);
            }

            var screenPoint = System.Drawing.Point.Empty;
            bool errClientToScreen = ClientToScreen(gw2WindowHandle, ref screenPoint);

            // Probably errors caused by gw2 closing at the time of the call
            if (!errClientToScreen)
            {
                int errorCode = Marshal.GetLastWin32Error();
                return (-1, -1);
            }

            rectanglePos = new Rectangle(
                clientRect.Left + screenPoint.X,
                clientRect.Top + screenPoint.Y,
                clientRect.Right - clientRect.Left,
                clientRect.Bottom - clientRect.Top
            );

            SetWindowPos(winHandle, HWND_TOPMOST, clientRect.Left + screenPoint.X, clientRect.Top + screenPoint.Y, clientRect.Right - clientRect.Left, clientRect.Bottom - clientRect.Top, 0);

            var marg = new Margins
            {
                cxLeftWidth = 0,
                cyTopHeight = 0,
                cxRightWidth = clientRect.Right,
                cyBottomHeight = clientRect.Bottom
            };

            DwmExtendFrameIntoClientArea(winHandle, ref marg);

            //SetWindowPos(winHandle, HWND_TOPMOST, 0, 0, 0, 0, 0);

            return (rectanglePos.Width, rectanglePos.Height);
        }
        */

        public static string GetClassNameOfWindow(IntPtr hwnd)
        {
            string className = "";
            StringBuilder classText = null;
            try
            {
                int cls_max_length = 1000;
                classText = new StringBuilder("", cls_max_length + 5);
                GetClassName(hwnd, classText, cls_max_length + 2);

                if (!string.IsNullOrEmpty(classText.ToString()))
                    className = classText.ToString();
            }
            catch (Exception ex)
            {
                className = ex.Message;
            }
            return className;
        }

        public static IntPtr hwndGetHwndOfProcess(string _strProcess)
        {
            System.Diagnostics.Process[] arProcesses = System.Diagnostics.Process.GetProcessesByName(_strProcess);
            if (arProcesses.Length > 0)
                return arProcesses[0].MainWindowHandle;
            else
                return IntPtr.Zero;
        }
    }
}
