using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;
using System;

namespace SubterfugeFrontend.Shared.Content.Game.Events.Events
{
    public class TouchReleaseEvent : Event
    {

        private TouchLocation touch;

        public TouchReleaseEvent(TouchLocation touchLocation) : base(Base.EventType.OnTouchReleaseEvent)
        {
            this.touch = touchLocation;
        }

        public TouchLocation getTouchLocation()
        {
            return this.touch;
        }
    }
}
