using UnityEngine;
using UnityEngine.InputSystem;
using System;
using System.Collections.Generic;

namespace RTGLite
{
    #region Public Classes
    //-----------------------------------------------------------------------------
    // Name: KeyEx (Public Static Class)
    // Desc: Contains useful extensions and utility functions for the 'Key' enum.
    //-----------------------------------------------------------------------------
    public static class KeyEx
    {
        #region Private Static Fields
        static Dictionary<Key, string> sKeyToText = new Dictionary<Key, string>();  // Maps a key to its text string
        static Dictionary<string, Key> sTextToKey = new Dictionary<string, Key>();  // Maps a text string to its corresponding key
        #endregion

        #region Static Constructors
        //-----------------------------------------------------------------------------
        // Name: KeyEx() (Static Constructor)
        // Desc: Static constructor responsible for initializing static data.
        //-----------------------------------------------------------------------------
        static KeyEx()
        {
            // Get all key values and store key-text mappings
            var keys = Enum.GetValues(typeof(Key));
            foreach (Key key in keys)
            {
                // Store key text
                string keyText = key.ToString();

                // Map key to text
                if (!sKeyToText.ContainsKey(key))
                    sKeyToText.Add(key, keyText);

                // Map text to key
                keyText = keyText.ToLower();
                if (!sTextToKey.ContainsKey(keyText))
                    sTextToKey.Add(keyText, key);
            }
            
            // Change number keys that belong to the alpha-numeric keys
            for (int i = 0; i < 10; ++i)
            {
                string text = i.ToString();
                sKeyToText[(Key)((int)Key.Digit0 + i)] = text;
                sTextToKey.Add(text, (Key)((int)Key.Digit0 + i));
            }

            // Update special keys
            sKeyToText[Key.LeftBracket] = "[";
            sKeyToText[Key.RightBracket] = "]";
            sKeyToText[Key.Equals] = "=";
            sKeyToText[Key.Comma] = ",";
            sKeyToText[Key.Semicolon] = ";";
            sKeyToText[Key.Period] = ".";
            sKeyToText[Key.Backslash] = "\\";
            sKeyToText[Key.Slash] = "/";
            sKeyToText[Key.Minus] = "-";
            sKeyToText[Key.Backquote] = "`";

            sTextToKey.Add("[", Key.LeftBracket);
            sTextToKey.Add("]", Key.RightBracket);
            sTextToKey.Add("=", Key.Equals);
            sTextToKey.Add(",", Key.Comma);
            sTextToKey.Add(";", Key.Semicolon);
            sTextToKey.Add(".", Key.Period);
            sTextToKey.Add("\\", Key.Backslash);
            sTextToKey.Add("/", Key.Slash);
            sTextToKey.Add("-", Key.Minus);
            sTextToKey.Add("`", Key.Backquote);
        }
        #endregion

        #region Public Extensions
        //-----------------------------------------------------------------------------
        // Name: KeyToText() (Public Extension)
        // Desc: Converts the key to its textual representation. For the most part, it
        //       works the same way as using the 'ToString()' function on the 'Key' enum.
        //       However, some keys will return a different text which is more suitable
        //       for display purposes.
        // Rtrn: The textual representation of the key.
        //-----------------------------------------------------------------------------
        public static string KeyToText(this Key key)
        {
            return sKeyToText[key];
        }
        #endregion

        #region Public Static Functions
        //-----------------------------------------------------------------------------
        // Name: KeyFromText() (Public Static Function)
        // Desc: Returns a member of the 'Key' enum from the specified text.
        // Parm: keyText - The key text. Valid text is text returned by calling 'ToString()'
        //                 or 'KeyToText()' on each member of the 'Key' enum. Case doesn't
        //                 matter.
        //       key     - Returns the key which corresponds to the specified text.
        // Rtrn: True on success and false on failure. If the function fails, the value
        //       of 'key' should be ignored.
        //-----------------------------------------------------------------------------
        public static bool KeyFromText(string keyText, out Key key)
        {
            return sTextToKey.TryGetValue(keyText.ToLower(), out key);
        }

        //-----------------------------------------------------------------------------
        // Name: KeyFromKeyCode() (Public Static Function)
        // Desc: Returns a 'Key' from the specified 'KeyCode'. This is handy for translating
        //       old Input System's 'KeyCode' to the new Input System's 'Key'.
        // Parm: keyCode - The 'KeyCode' to translate to 'Key'.
        // Rtrn: The 'Key' which corresponds to 'keyCode'. Some 'KeyCode' members don't
        //       have a mapping inside the 'Key' enum. For those keys, 'Key.None' will be
        //       returned.
        // Cred: https://discussions.unity.com/t/convert-from-old-keycode-to-the-new-key-enum/801672/2
        //-----------------------------------------------------------------------------
        public static Key KeyFromKeyCode(KeyCode keyCode)
        {
            Key unknownKey  = Key.None;
            Key mouseKey    = Key.None;
            Key joystickKey = Key.None;

            switch (keyCode)
            {
                case KeyCode.None:              return Key.None;
                case KeyCode.Backspace:         return Key.Backspace;
                case KeyCode.Delete:            return Key.Delete;
                case KeyCode.Tab:               return Key.Tab;
                case KeyCode.Clear:             return unknownKey;      // Conversion unknown.
                case KeyCode.Return:            return Key.Enter;
                case KeyCode.Pause:             return Key.Pause;
                case KeyCode.Escape:            return Key.Escape;
                case KeyCode.Space:             return Key.Space;
                case KeyCode.Keypad0:           return Key.Numpad0;
                case KeyCode.Keypad1:           return Key.Numpad1;
                case KeyCode.Keypad2:           return Key.Numpad2;
                case KeyCode.Keypad3:           return Key.Numpad3;
                case KeyCode.Keypad4:           return Key.Numpad4;
                case KeyCode.Keypad5:           return Key.Numpad5;
                case KeyCode.Keypad6:           return Key.Numpad6;
                case KeyCode.Keypad7:           return Key.Numpad7;
                case KeyCode.Keypad8:           return Key.Numpad8;
                case KeyCode.Keypad9:           return Key.Numpad9;
                case KeyCode.KeypadPeriod:      return Key.NumpadPeriod;
                case KeyCode.KeypadDivide:      return Key.NumpadDivide;
                case KeyCode.KeypadMultiply:    return Key.NumpadMultiply;
                case KeyCode.KeypadMinus:       return Key.NumpadMinus;
                case KeyCode.KeypadPlus:        return Key.NumpadPlus;
                case KeyCode.KeypadEnter:       return Key.NumpadEnter;
                case KeyCode.KeypadEquals:      return Key.NumpadEquals;
                case KeyCode.UpArrow:           return Key.UpArrow;
                case KeyCode.DownArrow:         return Key.DownArrow;
                case KeyCode.RightArrow:        return Key.RightArrow;
                case KeyCode.LeftArrow:         return Key.LeftArrow;
                case KeyCode.Insert:            return Key.Insert;
                case KeyCode.Home:              return Key.Home;
                case KeyCode.End:               return Key.End;
                case KeyCode.PageUp:            return Key.PageUp;
                case KeyCode.PageDown:          return Key.PageDown;
                case KeyCode.F1:                return Key.F1;
                case KeyCode.F2:                return Key.F2;
                case KeyCode.F3:                return Key.F3;
                case KeyCode.F4:                return Key.F4;
                case KeyCode.F5:                return Key.F5;
                case KeyCode.F6:                return Key.F6;
                case KeyCode.F7:                return Key.F7;
                case KeyCode.F8:                return Key.F8;
                case KeyCode.F9:                return Key.F9;
                case KeyCode.F10:               return Key.F10;
                case KeyCode.F11:               return Key.F11;
                case KeyCode.F12:               return Key.F12;
                case KeyCode.F13:               return unknownKey;      // Conversion unknown.
                case KeyCode.F14:               return unknownKey;      // Conversion unknown.
                case KeyCode.F15:               return unknownKey;      // Conversion unknown.
                case KeyCode.Alpha0:            return Key.Digit0;
                case KeyCode.Alpha1:            return Key.Digit1;
                case KeyCode.Alpha2:            return Key.Digit2;
                case KeyCode.Alpha3:            return Key.Digit3;
                case KeyCode.Alpha4:            return Key.Digit4;
                case KeyCode.Alpha5:            return Key.Digit5;
                case KeyCode.Alpha6:            return Key.Digit6;
                case KeyCode.Alpha7:            return Key.Digit7;
                case KeyCode.Alpha8:            return Key.Digit8;
                case KeyCode.Alpha9:            return Key.Digit9;
                case KeyCode.Exclaim:           return unknownKey;      // Conversion unknown.
                case KeyCode.DoubleQuote:       return unknownKey;      // Conversion unknown.
                case KeyCode.Hash:              return unknownKey;      // Conversion unknown.
                case KeyCode.Dollar:            return unknownKey;      // Conversion unknown.
                case KeyCode.Percent:           return unknownKey;      // Conversion unknown.
                case KeyCode.Ampersand:         return unknownKey;      // Conversion unknown.
                case KeyCode.Quote:             return Key.Quote;
                case KeyCode.LeftParen:         return unknownKey;      // Conversion unknown.
                case KeyCode.RightParen:        return unknownKey;      // Conversion unknown.
                case KeyCode.Asterisk:          return unknownKey;      // Conversion unknown.
                case KeyCode.Plus:              return Key.None;        
                case KeyCode.Comma:             return Key.Comma;
                case KeyCode.Minus:             return Key.Minus;
                case KeyCode.Period:            return Key.Period;
                case KeyCode.Slash:             return Key.Slash;
                case KeyCode.Colon:             return unknownKey;      // Conversion unknown.
                case KeyCode.Semicolon:         return Key.Semicolon;
                case KeyCode.Less:              return Key.None;
                case KeyCode.Equals:            return Key.Equals;
                case KeyCode.Greater:           return unknownKey;      // Conversion unknown.
                case KeyCode.Question:          return unknownKey;      // Conversion unknown.
                case KeyCode.At:                return unknownKey;      // Conversion unknown.
                case KeyCode.LeftBracket:       return Key.LeftBracket;
                case KeyCode.Backslash:         return Key.Backslash;
                case KeyCode.RightBracket:      return Key.RightBracket;
                case KeyCode.Caret:             return Key.None;        
                case KeyCode.Underscore:        return unknownKey;      // Conversion unknown.
                case KeyCode.BackQuote:         return Key.Backquote;
                case KeyCode.A:                 return Key.A;
                case KeyCode.B:                 return Key.B;
                case KeyCode.C:                 return Key.C;
                case KeyCode.D:                 return Key.D;
                case KeyCode.E:                 return Key.E;
                case KeyCode.F:                 return Key.F;
                case KeyCode.G:                 return Key.G;
                case KeyCode.H:                 return Key.H;
                case KeyCode.I:                 return Key.I;
                case KeyCode.J:                 return Key.J;
                case KeyCode.K:                 return Key.K;
                case KeyCode.L:                 return Key.L;
                case KeyCode.M:                 return Key.M;
                case KeyCode.N:                 return Key.N;
                case KeyCode.O:                 return Key.O;
                case KeyCode.P:                 return Key.P;
                case KeyCode.Q:                 return Key.Q;
                case KeyCode.R:                 return Key.R;
                case KeyCode.S:                 return Key.S;
                case KeyCode.T:                 return Key.T;
                case KeyCode.U:                 return Key.U;
                case KeyCode.V:                 return Key.V;
                case KeyCode.W:                 return Key.W;
                case KeyCode.X:                 return Key.X;             
                case KeyCode.Y:                 return Key.Y;
                case KeyCode.Z:                 return Key.Z;
                case KeyCode.LeftCurlyBracket:  return unknownKey;      // Conversion unknown.
                case KeyCode.Pipe:              return unknownKey;      // Conversion unknown.
                case KeyCode.RightCurlyBracket: return unknownKey;      // Conversion unknown.
                case KeyCode.Tilde:             return unknownKey;      // Conversion unknown.
                case KeyCode.Numlock:           return Key.NumLock;
                case KeyCode.CapsLock:          return Key.CapsLock;
                case KeyCode.ScrollLock:        return Key.ScrollLock;
                case KeyCode.RightShift:        return Key.RightShift;
                case KeyCode.LeftShift:         return Key.LeftShift;
                case KeyCode.RightControl:      return Key.RightCtrl;
                case KeyCode.LeftControl:       return Key.LeftCtrl;
                case KeyCode.RightAlt:          return Key.RightAlt;
                case KeyCode.LeftAlt:           return Key.LeftAlt;
                case KeyCode.LeftCommand:       return Key.LeftCommand;
                  // case KeyCode.LeftApple: (same as LeftCommand)
                case KeyCode.LeftWindows:       return Key.LeftWindows;
                case KeyCode.RightCommand:      return Key.RightCommand;
                  // case KeyCode.RightApple: (same as RightCommand)
                case KeyCode.RightWindows:      return Key.RightWindows;
                case KeyCode.AltGr:             return Key.AltGr;
                case KeyCode.Help:              return unknownKey;      // Conversion unknown.
                case KeyCode.Print:             return Key.PrintScreen;
                case KeyCode.SysReq:            return unknownKey;      // Conversion unknown.
                case KeyCode.Break:             return unknownKey;      // Conversion unknown.
                case KeyCode.Menu:              return Key.ContextMenu;
                case KeyCode.Mouse0:
                case KeyCode.Mouse1:
                case KeyCode.Mouse2:
                case KeyCode.Mouse3:
                case KeyCode.Mouse4:
                case KeyCode.Mouse5:
                case KeyCode.Mouse6:

                    return mouseKey; // Not supported anymore.

                // All other keys are joystick keys which do not
                // exist anymore in the new input system.
                default:

                    return joystickKey; // Not supported anymore.
            }
        }
        #endregion
    }
    #endregion
}
