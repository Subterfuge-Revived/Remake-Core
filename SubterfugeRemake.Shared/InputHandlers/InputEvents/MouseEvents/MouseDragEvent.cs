using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace SubterfugeFrontend.Shared.Content.Game.Events.Listeners.InputEvents.MouseEvents
{
    class MouseDragEvent : EventArgs
    {
        public MouseState currentState { get; }
        public MouseState previousState { get; }
        public MouseButton pressedButton { get; }

        public MouseDragEvent(MouseState currentState, MouseState previousState, MouseButton button)
        {
            this.currentState = currentState;
            this.previousState = previousState;
            this.pressedButton = button;
        }

        public Point getDelta()
        {
            return previousState.Position - currentState.Position;
        }
    }
}
