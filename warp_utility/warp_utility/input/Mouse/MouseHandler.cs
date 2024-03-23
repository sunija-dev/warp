using System;
//using Blish_HUD.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Warp.Utility;

namespace Blish_HUD.Input {

    public class MouseHandler : IInputHandler {

        //private static readonly Logger Logger = Logger.GetLogger<MouseHandler>();

        /// <summary>
        ///     The current position of the mouse relative to the application.
        /// </summary>
        public Point Position => this.State.Position;

        public Point PositionRaw { get; private set; }

        /// <summary>
        ///     The current state of the mouse.
        /// </summary>
        public MouseState State { get; private set; }

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
                this.PositionRaw = new Point(mouseEventArgs.PointX, mouseEventArgs.PointY);
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
                    //Logger.Debug("Got unsupported input {mouseDataMessage}.", e.EventType);
                    return false;
            }

            return true;
        }

        public void Update() 
        {
            /*
            if (!GameService.GameIntegration.Gw2IsRunning || !GameService.GameIntegration.Gw2HasFocus) 
            {
                _hudFocused = false;
                return;
            }
            */

            if (_cameraDragging) 
            {
                return;
            }

            var prevMouseState = this.State;

            var rawMouseState = Microsoft.Xna.Framework.Input.Mouse.GetState();

            /*
            this.State = new MouseState((int) (rawMouseState.X / GameService.Graphics.UIScaleMultiplier),
                                        (int) (rawMouseState.Y / GameService.Graphics.UIScaleMultiplier),
                                        _mouseEvent?.WheelDelta ?? 0, 
                                        rawMouseState.LeftButton,
                                        rawMouseState.MiddleButton,
                                        rawMouseState.RightButton,
                                        rawMouseState.XButton1,
                                        rawMouseState.XButton2);
            */

            this.SuState = new SuMouseState((int)(PositionRaw.X),
                                        (int)(PositionRaw.Y),
                                        _mouseEvent?.WheelDelta ?? 0,
                                        rawMouseState.LeftButton == ButtonState.Pressed,
                                        rawMouseState.MiddleButton == ButtonState.Pressed,
                                        rawMouseState.RightButton == ButtonState.Pressed,
                                        rawMouseState.XButton1 == ButtonState.Pressed,
                                        rawMouseState.XButton2 == ButtonState.Pressed);

            // Handle mouse moved
            if (prevMouseState.Position != this.State.Position) 
            {
                if (true) 
                {
                    //this.ActiveControl = this.ActiveControl.MouseOver ? this.ActiveControl : null;
                    // TODO
                }
                //this.ActiveControl = GameService.Graphics.SpriteScreen.TriggerMouseInput(MouseEventType.MouseMoved, this.State);
                this.MouseMoved?.Invoke(this, new MouseEventArgs(MouseEventType.MouseMoved));
            }

            //// Handle mouse events blocked by the mouse hook
            if (_mouseEvent != null) 
            {
                if (HandleHookedMouseEvent(_mouseEvent)) 
                {
                    //GameService.Graphics.SpriteScreen.TriggerMouseInput(_mouseEvent.EventType, this.State);
                }

                _mouseEvent = null;
            }

            // Handle mouse left pressed/released
            if (prevMouseState.LeftButton != this.State.LeftButton) 
            {
                switch (this.State.LeftButton) 
                {
                    case ButtonState.Pressed:
                        this.LeftMouseButtonPressed?.Invoke(this, new MouseEventArgs(MouseEventType.LeftMouseButtonPressed));
                        break;
                    case ButtonState.Released:
                        this.LeftMouseButtonReleased?.Invoke(this, new MouseEventArgs(MouseEventType.LeftMouseButtonReleased));
                        break;
                }
            }

            // Handle mouse right pressed/released
            if (prevMouseState.RightButton != this.State.RightButton) {
                switch (this.State.RightButton) {
                    case ButtonState.Pressed:
                        this.RightMouseButtonPressed?.Invoke(this, new MouseEventArgs(MouseEventType.RightMouseButtonPressed));
                        break;
                    case ButtonState.Released:
                        this.RightMouseButtonReleased?.Invoke(this, new MouseEventArgs(MouseEventType.RightMouseButtonReleased));
                        break;
                }
            }

            // Handle mouse scroll
            if (this.State.ScrollWheelValue != 0) {
                this.MouseWheelScrolled?.Invoke(this, new MouseEventArgs(MouseEventType.MouseWheelScrolled));
            }
        }

        public void OnEnable() {
            /* NOOP */
        }

        public void OnDisable() {
            /* NOOP */
        }

        public void SetHudFocused(bool _bFocused)
        {
            _hudFocused = _bFocused;
        }

        #region Events

        public event EventHandler<MouseEventArgs> MouseMoved;
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