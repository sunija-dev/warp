using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blish_HUD.Input;
using Blish_HUD;
using System.Diagnostics;

namespace Warp.Utility
{
    public class WarpUtilityDll
    {
        public bool bGw2HasFocus = false;
        public bool bWarpHasFocus = false;
        public bool bGw2IsRunning = false;
        public string strFeedback = "";

        public static IntPtr hwndWarpFormHandle;

        private GameIntegrationService m_gameService;

        public void Initialize()
        {
            // set window handle of warp
            hwndWarpFormHandle = WindowUtil.hwndGetHwndOfProcess("warp");

            m_gameService = new GameIntegrationService();
            m_gameService.Initialize();
            m_gameService.TryAttachToGw2();
        }

        public void Update()
        {
            m_gameService.Update();

            bGw2IsRunning = m_gameService.Gw2IsRunning;
            bGw2HasFocus = m_gameService.Gw2HasFocus;
            bWarpHasFocus = WindowUtil.GetForegroundWindow() == hwndWarpFormHandle;

            strFeedback = string.Format("GW2 focus: {0} - WARP focus: {1} - GW2 running: {2}", bGw2HasFocus, bWarpHasFocus, bGw2IsRunning);
            Console.WriteLine(strFeedback);
        }

        // set focus to gw2 or warp
        public void FocusGW2()
        {
            m_gameService.FocusGw2();
        }

        public void FocusWarp()
        {
            try
            {
                WindowUtil.SetForegroundWindowEx(hwndWarpFormHandle);
            }
            catch (NullReferenceException e)
            {
                //Logger.Warn(e, "Failed to give focus to GW2 handle.");
            }
        }

    }
}
