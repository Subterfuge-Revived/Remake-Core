using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace SubterfugeFrontend.Shared.Content.Game.Events.Listeners.InputEvents.KeyboardEvents
{
    class KeyUpEvent : EventArgs
    {
        public Keys keyPressed { get; }
        public KeyboardState keyboardState { get; }
        public KeyUpEvent(Keys key, KeyboardState currentState)
        {
            this.keyPressed = key;
            this.keyboardState = currentState;
        }
    }
}
