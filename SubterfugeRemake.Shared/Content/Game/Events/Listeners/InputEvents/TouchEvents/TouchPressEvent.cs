using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;
using System;

namespace SubterfugeFrontend.Shared.Content.Game.Events.Events
{
    public class TouchPressEvent : EventArgs
    {

        public TouchLocation touch { get; }

        public TouchPressEvent(TouchLocation touchLocation)
        {
            this.touch = touchLocation;
        }
    }
}
