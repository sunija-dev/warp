using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blish_HUD.Input;
using Blish_HUD;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Warp.Utility
{
    public class WarpInputs
    {

        public void Start()
        {
            InputService input = new InputService();
            input.Load();
            input.EnableHooks();
            input.Keyboard.KeyPressed += (s, args) => { PressedX(args); };
            input.Mouse.LeftMouseButtonPressed += (s, args) => { MouseClick(args); };


            int i = 0;
            while (true)
            {
                i++;
                input.Update();
                if (input.Mouse.SuState.iX > 0)
                    Console.WriteLine(input.Mouse.SuState.iX);

                if (input.Mouse.SuState.bLeftButton)
                    Console.WriteLine(input.Mouse.SuState.bLeftButton);
            }

            input.Unload();
        }

        static void PressedX(KeyboardEventArgs _args)
        {
            // ADD KEYS TO DLL, THEN THE MONOGAME MIGHT NOT BE A PROBLEM
            Console.WriteLine("Pressed " + _args.strKey);
            //bStop = true;
        }

        static void MouseClick(MouseEventArgs _args)
        {
            Console.WriteLine("Clicked Mouse ");
        }
    }
}
