using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace SubterfugeFrontend.Shared.Content.Game.Events.Listeners.InputEvents.MouseEvents
{
    class MouseMoveEvent : EventArgs
    {
        public MouseState currentState { get; }
        public MouseState previousState { get; }

        public MouseMoveEvent(MouseState currentState, MouseState previousState)
        {
            this.currentState = currentState;
            this.previousState = previousState;
        }

        public Point getDelta()
        {
            return previousState.Position - currentState.Position;
        }
    }
}
