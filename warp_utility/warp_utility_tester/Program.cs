using System;
using Blish_HUD.Input;
using Blish_HUD;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Warp.Utility;

namespace warp_utility_tester
{
    class Program
    {
        static bool bStop = false;

        static void Main(string[] args)
        {
            GameIntegrationTest();
            //InputTest();
        }

        static void GameIntegrationTest()
        {
            WarpUtilityDll warpUtility = new WarpUtilityDll();
            warpUtility.Initialize();

            int i = 0;
            while (true)
            {
                warpUtility.Update();
                //Console.WriteLine(string.Format("GW2 focus: {0} - WARP focus: {1} - GW2 running: {2}", warpUtility.bGw2HasFocus, warpUtility.bWarpHasFocus, warpUtility.bGw2IsRunning));

                i++;
                if (bStop) break;
            }
        }

        static void InputTest()
        {
            InputService input = new InputService();
            input.Load();
            input.EnableHooks();
            input.Mouse.SetHudFocused(true);

            // DEBUG
            ActivateTests(input);

            int i = 0;
            while (true)
            {
                input.Update();

                i++;
                if (bStop)
                    break;
            }

            input.Unload();
        }

        static void ActivateTests(InputService _input)
        {
            _input.Keyboard.KeyPressed += (s, args) => { PressedKeyTest(args); }; ;
        }

        static void PressedKeyTest(KeyboardEventArgs _args)
        {
            // ADD KEYS TO DLL, THEN THE MONOGAME MIGHT NOT BE A PROBLEM
            Console.WriteLine("Pressed " + _args.strKey);
        }

        static void MouseClick(MouseEventArgs _args)
        {
            Console.WriteLine("Clicked Mouse " + _args.EventType.ToString());
        }


    }
}
