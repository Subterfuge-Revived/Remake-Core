using Microsoft.Xna.Framework.Input;
using SubterfugeFrontend.Shared.Content.Game.Events.Listeners.InputEvents.KeyboardEvents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SubterfugeFrontend.Shared.Content.Game.Events.Listeners
{
    class KeyboardListener : IListener
    {
        private Array _keysValues = Enum.GetValues(typeof(Keys));

        private bool _isInitial;
        private TimeSpan _lastPressTime;

        private Keys _previousKey;
        private KeyboardState _previousState;

        private KeyboardState _currentKeyboardState;
        private KeyboardState _previousKeyboardState;

        // Event delegates to allow components to listen to inputs.
        public event EventHandler<KeyDownEvent> KeyDown;
        public event EventHandler<KeyUpEvent> KeyUp;

        public void listen()
        {
            _previousKeyboardState = _currentKeyboardState;
            _currentKeyboardState = Keyboard.GetState();

            RaisePressedEvents(_currentKeyboardState);
            RaiseReleasedEvents(_currentKeyboardState);
        }

        private void RaisePressedEvents(KeyboardState currentState)
        {
            if (!currentState.IsKeyDown(Keys.LeftAlt) && !currentState.IsKeyDown(Keys.RightAlt))
            {
                var pressedKeys = _keysValues.Cast<Keys>().Where(key => currentState.IsKeyDown(key) && _previousState.IsKeyUp(key));

                foreach (var key in pressedKeys)
                {
                    var args = new KeyDownEvent(key, currentState);

                    KeyDown?.Invoke(this, args);

                    _previousKey = key;
                    _isInitial = true;
                }
            }
        }

        private void RaiseReleasedEvents(KeyboardState currentState)
        {
            var releasedKeys = _keysValues.Cast<Keys>().Where(key => currentState.IsKeyUp(key) && _previousState.IsKeyDown(key));

            foreach (var key in releasedKeys)
                KeyUp?.Invoke(this, new KeyUpEvent(key, currentState));
        }
    }
}
