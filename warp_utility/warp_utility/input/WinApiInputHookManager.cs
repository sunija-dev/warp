﻿using System;
using System.Collections.Generic;
using Blish_HUD.Input.WinApi;

namespace Blish_HUD.Input {

    internal abstract class WinApiInputHookManager<THandlerDelegate> 
    {

        private readonly HookExtern.HookCallbackDelegate hookProc; // Store the callback delegate, otherwise it might get garbage collected
        private          IntPtr                          hook;


        public WinApiInputHookManager() { hookProc = HookCallback; }


        protected abstract HookType HookType { get; }

        protected IList<THandlerDelegate> Handlers { get; } = new SynchronizedCollection<THandlerDelegate>();


        public virtual bool EnableHook() {
            if (hook != IntPtr.Zero) return true;

            Console.WriteLine("Enabling");

            hook = HookExtern.SetWindowsHookEx(this.HookType, hookProc, IntPtr.Zero, 0);
            return hook != IntPtr.Zero;
        }

        public virtual void DisableHook() {
            if (hook == IntPtr.Zero) return;

            Console.WriteLine("Disabling");

            HookExtern.UnhookWindowsHookEx(hook);
            hook = IntPtr.Zero;
        }

        public virtual void RegisterHandler(THandlerDelegate handleInputCallback) { this.Handlers.Add(handleInputCallback); }

        public virtual void UnregisterHandler(THandlerDelegate handleInputCallback) { this.Handlers.Remove(handleInputCallback); }

        protected abstract int HookCallback(int nCode, IntPtr wParam, IntPtr lParam);

    }

}
