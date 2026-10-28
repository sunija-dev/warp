using System;
using Blish_HUD.Input.WinApi;

namespace Blish_HUD.Input {

    public class MouseHandler {

        public int PositionRawX { get; private set; }

        public int PositionRawY { get; private set; }

        /// <summary>
        /// ONLY POSITION WORKING YET
        /// </summary>
        public SuMouseState SuState { get; private set; }

        private bool _cameraDragging;

        private bool _hudFocused = false;

        private MouseEventArgs _mouseEvent;

        internal MouseHandler() { }

        public bool HandleInput(MouseEventArgs mouseEventArgs)
        {
            if (mouseEventArgs.EventType == MouseEventType.MouseMoved)
            {
                this.PositionRawX = mouseEventArgs.PointX;
                this.PositionRawY = mouseEventArgs.PointY;
                return false;
            }

            if (_cameraDragging && mouseEventArgs.EventType == MouseEventType.RightMouseButtonReleased)
            {
                _cameraDragging = false;
            }
            else if (_hudFocused)
            {
                _mouseEvent = mouseEventArgs;
                return mouseEventArgs.EventType != MouseEventType.LeftMouseButtonReleased;
            }
            else if (mouseEventArgs.EventType == MouseEventType.RightMouseButtonPressed)
            {
                _cameraDragging = true;
            }

            return false;
        }

        private bool HandleHookedMouseEvent(MouseEventArgs e)
        {
            switch (e.EventType) {
                case MouseEventType.LeftMouseButtonPressed:
                    this.LeftMouseButtonPressed?.Invoke(this, e);
                    break;
                case MouseEventType.LeftMouseButtonReleased:
                    this.LeftMouseButtonReleased?.Invoke(this, e);
                    break;
                case MouseEventType.RightMouseButtonPressed:
                    this.RightMouseButtonPressed?.Invoke(this, e);
                    break;
                case MouseEventType.RightMouseButtonReleased:
                    this.RightMouseButtonReleased?.Invoke(this, e);
                    break;
                case MouseEventType.MouseWheelScrolled:
                    this.MouseWheelScrolled?.Invoke(this, e);
                    break;
                default:
                    return false;
            }

            return true;
        }

        public void Update()
        {
            if (_cameraDragging)
            {
                return;
            }

            // GetAsyncKeyState reports physical buttons, so swap them back
            bool bSwapped = HookExtern.GetSystemMetrics(HookExtern.SM_SWAPBUTTON) != 0;
            bool bLeft    = IsButtonDown(bSwapped ? HookExtern.VK_RBUTTON : HookExtern.VK_LBUTTON);
            bool bRight   = IsButtonDown(bSwapped ? HookExtern.VK_LBUTTON : HookExtern.VK_RBUTTON);

            this.SuState = new SuMouseState(PositionRawX,
                                        PositionRawY,
                                        _mouseEvent?.WheelDelta ?? 0,
                                        bLeft,
                                        IsButtonDown(HookExtern.VK_MBUTTON),
                                        bRight,
                                        IsButtonDown(HookExtern.VK_XBUTTON1),
                                        IsButtonDown(HookExtern.VK_XBUTTON2));

            // Handle mouse events blocked by the mouse hook
            if (_mouseEvent != null)
            {
                HandleHookedMouseEvent(_mouseEvent);
                _mouseEvent = null;
            }
        }

        private static bool IsButtonDown(int _iVirtualKey) { return (HookExtern.GetAsyncKeyState(_iVirtualKey) & 0x8000) != 0; }

        public void SetHudFocused(bool _bFocused)
        {
            _hudFocused = _bFocused;
        }

        #region Events

        public event EventHandler<MouseEventArgs> LeftMouseButtonPressed;
        public event EventHandler<MouseEventArgs> LeftMouseButtonReleased;
        public event EventHandler<MouseEventArgs> RightMouseButtonPressed;
        public event EventHandler<MouseEventArgs> RightMouseButtonReleased;
        public event EventHandler<MouseEventArgs> MouseWheelScrolled;

        #endregion

    }

    public class SuMouseState
    {
        public int iX = 0;
        public int iY = 0;
        public int iWheelDelta = 0;
        public bool bLeftButton = false;
        public bool bMiddleButton = false;
        public bool bRightButton = false;
        public bool bXButton1 = false;
        public bool bXButton2 = false;

        public SuMouseState(int iX, int iY, int iWheelDelta, bool bLeftButton, bool bMiddleButton, bool bRightButton, bool bXButton1, bool bXButton2)
        {
            this.iX = iX;
            this.iY = iY;
            this.iWheelDelta = iWheelDelta;
            this.bLeftButton = bLeftButton;
            this.bMiddleButton = bMiddleButton;
            this.bRightButton = bRightButton;
            this.bXButton1 = bXButton1;
            this.bXButton2 = bXButton2;
        }
    }

}
