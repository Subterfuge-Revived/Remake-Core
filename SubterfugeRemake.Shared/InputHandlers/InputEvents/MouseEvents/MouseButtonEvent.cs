using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace SubterfugeFrontend.Shared.Content.Game.Events.Listeners.InputEvents.MouseEvents
{
    class MouseButtonEvent
    {
        public MouseState previousState { get; }
        public MouseState currentState { get; }
        public MouseButton pressedButton { get; }

        public MouseButtonEvent(MouseState previousState, MouseState currentState, MouseButton button)
        {
            this.previousState = previousState;
            this.currentState = currentState;
            this.pressedButton = button;
        }

        public Point getMouseLocation()
        {
            return this.currentState.Position;
        }
    }
}
