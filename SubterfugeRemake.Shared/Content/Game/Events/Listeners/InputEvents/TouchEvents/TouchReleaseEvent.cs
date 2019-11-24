using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;
using System;

namespace SubterfugeFrontend.Shared.Content.Game.Events.Events
{
    public class TouchReleaseEvent : EventArgs
    {

        public TouchLocation touch { get; }

        public TouchReleaseEvent(TouchLocation touchLocation)
        {
            this.touch = touchLocation;
        }
    }
}
