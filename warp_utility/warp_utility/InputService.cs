using Blish_HUD.Input;
using Microsoft.Xna.Framework;
using System.Runtime.InteropServices;

namespace Blish_HUD {
    public class InputService {

        private readonly IHookManager hookManager;

        /// <summary>
        /// Provides details about the current mouse state.
        /// </summary>
        public MouseHandler Mouse { get; }

        /// <summary>
        /// Provides details about the current keyboard state.
        /// </summary>
        public KeyboardHandler Keyboard { get; }

        public InputService() 
        {
            Mouse = new MouseHandler();
            Keyboard = new KeyboardHandler();

            hookManager = new WinApiHookManager();
        }

        public void EnableHooks() 
        {
            if (hookManager.EnableHook()) 
            {
                hookManager.RegisterMouseHandler(Mouse.HandleInput);
                //hookManager.RegisterKeyboardHandler(Keyboard.HandleInput);
            }
        }

        public void DisableHooks() 
        {
            hookManager.DisableHook();
            hookManager.UnregisterMouseHandler(Mouse.HandleInput);
            //hookManager.UnregisterKeyboardHandler(Keyboard.HandleInput);
        }

        protected void Initialize() { /* NOOP */ }

        public void Load() 
        {
            hookManager.Load();
            //GameIntegration.Gw2AcquiredFocus += (s, e) => EnableHooks();
            //GameIntegration.Gw2LostFocus += (s, e) => DisableHooks();
            //GameIntegration.Gw2Closed += (s, e) => DisableHooks();
        }

        public void Unload() 
        {
            DisableHooks();
            hookManager.Unload();
        }

        public void Update() 
        {
            Mouse.Update();
            //Keyboard.Update();
        }

        // ============= OLD SYSTEMS =============

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out Point lpPoint);

        // retrieves mouse position without hooks enabled
        public static float[] v2GetMousePosWithoutHook()
        {
            float[] v2MousePos = new float[2];
            Point cursorPos;
            GetCursorPos(out cursorPos);

            v2MousePos[0] = cursorPos.X;
            v2MousePos[1] = cursorPos.Y;

            return v2MousePos;
        }
    }
}
