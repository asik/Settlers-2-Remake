using System;
using OpenTK.Input;

namespace Game {
    class GameKeyboard {
        KeyboardDevice keyboard;
        bool[] keyStates;

        public GameKeyboard(KeyboardDevice Keyboard) {
            keyboard = Keyboard;
            keyboard.KeyDown += keyboard_KeyDown;
            keyboard.KeyUp += keyboard_KeyUp;
            keyStates = new bool[keyboard.NumberOfKeys];
        }

        public bool IsKeyDown(Key key) {
            return keyStates[(int)key];
        }

        void keyboard_KeyUp(object sender, KeyboardKeyEventArgs e) {
            keyStates[(int)e.Key] = false;
        }

        void keyboard_KeyDown(object sender, KeyboardKeyEventArgs e) {
            keyStates[(int)e.Key] = true;
        }

    }
}
