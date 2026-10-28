using System;
using System.Collections;
using System.Collections.Generic;
using Blish_HUD.Input.WinApi;

namespace Blish_HUD.Input {

    internal abstract class WinApiInputHookManager<THandlerDelegate>
    {

        private readonly HookExtern.HookCallbackDelegate hookProc; // Store the callback delegate, otherwise it might get garbage collected
        private          IntPtr                          hook;


        public WinApiInputHookManager() { hookProc = HookCallback; }


        protected abstract HookType HookType { get; }

        // Callers lock on ((IList) Handlers).SyncRoot, since the hook thread iterates it
        protected List<THandlerDelegate> Handlers { get; } = new();


        public virtual bool EnableHook() {
            if (hook != IntPtr.Zero) return true;

            hook = HookExtern.SetWindowsHookEx(this.HookType, hookProc, IntPtr.Zero, 0);
            return hook != IntPtr.Zero;
        }

        public virtual void DisableHook() {
            if (hook == IntPtr.Zero) return;

            HookExtern.UnhookWindowsHookEx(hook);
            hook = IntPtr.Zero;
        }

        public virtual void RegisterHandler(THandlerDelegate handleInputCallback) {
            lock (((IList) this.Handlers).SyncRoot) this.Handlers.Add(handleInputCallback);
        }

        public virtual void UnregisterHandler(THandlerDelegate handleInputCallback) {
            lock (((IList) this.Handlers).SyncRoot) this.Handlers.Remove(handleInputCallback);
        }

        protected abstract int HookCallback(int nCode, IntPtr wParam, IntPtr lParam);

    }

}
