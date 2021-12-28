using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace Utility.HotKey
{
    public class RawHotKey
    {
        private readonly List<ModifierKeys> hotKeyModifiers;
        private readonly Key key;

        private RawHotKey(List<ModifierKeys> modifiers, Key k)
        {
            this.hotKeyModifiers = modifiers;
            this.key = k;
        }

        public Key Key
        {
            get
            {
                return key;
            }
        }

        public ModifierKeys HotKeyModifiers
        {
            get
            {
                ModifierKeys hotKeyModifier = ModifierKeys.None;
                foreach (ModifierKeys modifier in hotKeyModifiers)
                {
                    hotKeyModifier |= modifier;
                }
                return hotKeyModifier;
            }
        }

        private static bool ParseHotKeyModifiers(string keyStr, out ModifierKeys hotKeyModifiers)
        {
            switch (keyStr.ToLower())
            {
                case "ctrl":
                    hotKeyModifiers = ModifierKeys.Control; return true;
                case "alt":
                    hotKeyModifiers = ModifierKeys.Alt; return true;
                case "shift":
                    hotKeyModifiers = ModifierKeys.Shift; return true;
                case "windows":
                    hotKeyModifiers = ModifierKeys.Windows; return true;

                default:
                    hotKeyModifiers = ModifierKeys.None; break;
            }

            return false;
        }

        public static RawHotKey Parse(string hotKeyStr)
        {
            List<ModifierKeys> hotKeyModifiers = new List<ModifierKeys>();
            Key key = Key.None;
            hotKeyStr = hotKeyStr.Trim();
            if (hotKeyStr.Length == 0)
            {
                return new RawHotKey(hotKeyModifiers, key);
            }

            var keys = hotKeyStr.Split("+");
            if (keys.Length == 0)
            {
                return new RawHotKey(hotKeyModifiers, key);
            }

            foreach (string k in keys)
            {
                if (ParseHotKeyModifiers(k, out ModifierKeys hkm))
                {
                    hotKeyModifiers.Add(hkm);
                }
                else if (key != Key.None)
                {
                    throw new InvalidHotKeyStringException("Invalid hotkey string: " + hotKeyStr + ". Mutilple key found.");
                }
                else
                {
                    if (!Enum.TryParse(k, out key))
                    {
                        throw new InvalidHotKeyStringException("Invalid hotkey string: " + hotKeyStr + ". Invalid key.");
                    }
                }

            }

            var rawHotKey = new RawHotKey(hotKeyModifiers, key);
            return rawHotKey;
        }
    }
}
