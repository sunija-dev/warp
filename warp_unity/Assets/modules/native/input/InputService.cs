using Blish_HUD.Controls.Extern;
using Blish_HUD.Input;
using System.Runtime.InteropServices;

namespace Blish_HUD {
    public class InputService {

        private readonly IHookManager hookManager;

        /// <summary>
        /// Provides details about the current mouse state.
        /// </summary>
        public MouseHandler Mouse { get; }

        public InputService()
        {
            Mouse = new MouseHandler();

            hookManager = new WinApiHookManager();
        }

        public void EnableHooks()
        {
            if (hookManager.EnableHook())
            {
                hookManager.RegisterMouseHandler(Mouse.HandleInput);
            }
        }

        public void DisableHooks()
        {
            hookManager.DisableHook();
            hookManager.UnregisterMouseHandler(Mouse.HandleInput);
        }

        public void Load()
        {
            hookManager.Load();
        }

        public void Unload()
        {
            DisableHooks();
            hookManager.Unload();
        }

        public void Update()
        {
            Mouse.Update();
        }

        // ============= OLD SYSTEMS =============

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT lpPoint);

        // retrieves mouse position without hooks enabled
        public static float[] v2GetMousePosWithoutHook()
        {
            float[] v2MousePos = new float[2];
            POINT cursorPos;
            GetCursorPos(out cursorPos);

            v2MousePos[0] = cursorPos.X;
            v2MousePos[1] = cursorPos.Y;

            return v2MousePos;
        }
    }
}
