﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
//using Humanizer;
using Microsoft.Xna.Framework.Input;

namespace Blish_HUD.Input {
    public static class KeysUtil {

        [DllImport("user32.dll")]
        private static extern uint MapVirtualKey(uint uCode, uint uMapType);

        private static readonly Dictionary<Keys, string> _friendlyKeyNames;

        static KeysUtil() {
            _friendlyKeyNames = new Dictionary<Keys, string>() {
                {Keys.Add, "Add (NUM)"},
                {Keys.Back, "Backspace"},
                {Keys.Decimal, "Decimal (NUM)"},
                {Keys.Divide, "Divide (NUM)"},
                {Keys.LeftControl, "Left Ctrl"},
                {Keys.LeftAlt, "Left Alt"},
                {Keys.LeftShift, "Left Shift"},
                {Keys.Multiply, "Multiply (NUM)"},
                {Keys.None, ""},
                {Keys.NumPad0, "0 (NUM)"},
                {Keys.NumPad1, "1 (NUM)"},
                {Keys.NumPad2, "2 (NUM)"},
                {Keys.NumPad3, "3 (NUM)"},
                {Keys.NumPad4, "4 (NUM)"},
                {Keys.NumPad5, "5 (NUM)"},
                {Keys.NumPad6, "6 (NUM)"},
                {Keys.NumPad7, "7 (NUM)"},
                {Keys.NumPad8, "8 (NUM)"},
                {Keys.NumPad9, "9 (NUM)"},
                {Keys.PageDown, "Page Down"},
                {Keys.PageUp, "Page Up"},
                {Keys.RightControl, "Right Ctrl"},
                {Keys.RightAlt, "Right Alt"},
                {Keys.RightShift, "Right Shift"},
                {Keys.Space, "Space"},
                {Keys.Subtract, "Subtract (NUM)"}
            };

            string CreateFriendlyName(Keys key) {
                string friendlyName = key.ToString();

                return friendlyName.StartsWith("F")
                           ? friendlyName // F1-F24
                           : friendlyName; //.Humanize(LetterCasing.Title);
            }

            foreach (Keys key in Enum.GetValues(typeof(Keys))) {
                if (_friendlyKeyNames.ContainsKey(key)) continue;

                // 2 = MAPVK_VK_TO_CHAR and then we mask out the dead key indicator
                uint mappedCharCode = MapVirtualKey((uint) key, 2) & 0x0FFFFFFF;

                char mappedChar;

                if (mappedCharCode == 0 || char.IsControl(mappedChar = Convert.ToChar(mappedCharCode))) {
                    _friendlyKeyNames.Add(key, CreateFriendlyName(key));
                } else {
                    _friendlyKeyNames.Add(key, mappedChar.ToString());
                }
            }
        }

        /// <summary>
        /// Returns the <see cref="ModifierKeys"/> that represents the provided <see cref="Keys"/> value.
        /// </summary>
        public static ModifierKeys ModifierKeyFromKey(Keys key) {
            switch (key) {
                case Keys.LeftControl:
                case Keys.RightControl:
                    return ModifierKeys.Ctrl;
                case Keys.LeftAlt:
                case Keys.RightAlt:
                    return ModifierKeys.Alt;
                case Keys.LeftShift:
                case Keys.RightShift:
                    return ModifierKeys.Shift;
            }

            return ModifierKeys.None;
        }

        /// <summary>
        /// Returns the <see cref="ModifierKeys"/> found in an <see cref="Enumerable"/> of <see cref="Keys"/>.
        /// </summary>
        public static ModifierKeys ModifiersFromKeys(IEnumerable<Keys> keys) {
            return keys.Aggregate(ModifierKeys.None, (current, key) => current | ModifierKeyFromKey(key));
        }

        /// <summary>
        /// Returns the friendly display name of the provided <see cref="Keys"/> value.
        /// </summary>
        public static string GetFriendlyName(Keys key) {
            return _friendlyKeyNames.TryGetValue(key, out string friendlyName)
                       ? friendlyName
                       : key.ToString();
        }

        /// <summary>
        /// Gets a display string representing a <see cref="ModifierKeys"/> and
        /// <see cref="Keys"/> pair suitable for display in the UI.
        /// </summary>
        public static string GetFriendlyName(ModifierKeys modifierKeys, Keys primaryKey) {
            string displayText = "";

            if (primaryKey != Keys.None) {
                if (modifierKeys != ModifierKeys.None) {
                    displayText = $"{modifierKeys.ToString().Replace(", ", " + ")} + ";
                }

                displayText += GetFriendlyName(primaryKey);
            }

            return displayText;
        }

        /// <summary>
        /// Returns a flag indicating the set <see cref="ModifierKeys"/> and the remaining <see cref="Keys"/> value
        /// suitable for a <see cref="KeyBinding"/> from an <see cref="Enumerable"/> of <see cref="Keys"/>.
        /// </summary>
        public static (ModifierKeys, Keys) SplitToBindingPair(IEnumerable<Keys> keys) {
            var modifiers = ModifierKeys.None;
            var key       = Keys.None;

            var firstModifier = Keys.None;

            foreach (var providedKey in keys) {
                var modifier = ModifierKeyFromKey(providedKey);

                if (modifier == ModifierKeys.None) { 
                    if (key == Keys.None) {
                        key = providedKey;
                    }
                } else {
                    firstModifier = firstModifier == Keys.None
                                        ? providedKey
                                        : firstModifier;

                    modifiers |= modifier;
                }
            }

            return key == Keys.None ? (ModifierKeys.None, firstModifier) : (modifiers, key);
        }

    }
}
