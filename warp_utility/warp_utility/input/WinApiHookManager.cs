using System;
using System.Threading;
using Blish_HUD.Input.WinApi;

namespace Blish_HUD.Input {

    internal class WinApiHookManager : IHookManager {

        //private static readonly Logger Logger = Logger.GetLogger<WinApiHookManager>();

        private readonly IMouseHookManager    mouseHookManager;
        private readonly IKeyboardHookManager keyboardHookManager;
        private          bool                 stopRequested = false;
        private          Thread               thread;
        private          uint                 threadId;
        private          AutoResetEvent       inputHookEvent  = new AutoResetEvent(false);
        private          bool                 inputSuccessful = false;

        public WinApiHookManager() {
            mouseHookManager    = new WinApiMouseHookManager();
            keyboardHookManager = new WinApiKeyboardHookManager();
        }

        public void Load() { }

        public void Unload() { DisableHook(); }

        public bool EnableHook() {
            if (thread != null) return false;

            //Logger.Debug("Enabling WinAPI input hooks");

            thread = new Thread(Loop);
            thread.Start();

            // Wait for the hook to be completed and return the status
            inputHookEvent.WaitOne();
            return inputSuccessful;
        }

        public void DisableHook() {
            if ((thread == null) || stopRequested) return;

            //Logger.Debug("Disabling WinAPI input hooks");

            mouseHookManager.DisableHook();
            keyboardHookManager.DisableHook();

            stopRequested = true;
            HookExtern.PostThreadMessage(threadId, HookExtern.WM_QUIT, IntPtr.Zero, IntPtr.Zero);
            thread.Join();

            stopRequested = false;
            thread        = null;
        }

        public void RegisterMouseHandler(HandleMouseInputDelegate handleMouseInputCallback) { mouseHookManager.RegisterHandler(handleMouseInputCallback); }

        public void UnregisterMouseHandler(HandleMouseInputDelegate handleMouseInputCallback) { mouseHookManager.UnregisterHandler(handleMouseInputCallback); }

        public void RegisterKeyboardHandler(HandleKeyboardInputDelegate handleKeyboardInputCallback) { keyboardHookManager.RegisterHandler(handleKeyboardInputCallback); }

        public void UnregisterKeyboardHandler(HandleKeyboardInputDelegate handleKeyboardInputCallback) { keyboardHookManager.UnregisterHandler(handleKeyboardInputCallback); }

        private void Loop() {
            threadId = HookExtern.GetCurrentThreadId();

            // Forces the thread's message queue to exist, so DisableHook's WM_QUIT can't get lost
            HookExtern.PeekMessage(out HookExtern.MSG msg, IntPtr.Zero, 0, 0, HookExtern.PM_NOREMOVE);

            if (mouseHookManager.EnableHook() && keyboardHookManager.EnableHook()) inputSuccessful = true;
            inputHookEvent.Set();

            //Logger.Debug("Starting message loop");

            // Low-level hooks are called through this thread's message loop. GetMessage returns 0 on WM_QUIT and -1 on error.
            while (HookExtern.GetMessage(out msg, IntPtr.Zero, 0, 0) > 0) {
                HookExtern.TranslateMessage(ref msg);
                HookExtern.DispatchMessage(ref msg);
            }

            //Logger.Debug("Message loop stopped");
        }

        #region IDisposable Support

        private bool isDisposed = false;

        protected virtual void Dispose(bool isDisposing) {
            if (!isDisposed) {
                if (isDisposing) {
                    inputHookEvent.Dispose();
                    Unload();
                }

                isDisposed = true;
            }
        }

        public void Dispose() { Dispose(true); }

        #endregion

    }

}
