using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;
using System;

namespace SubterfugeFrontend.Shared.Content.Game.Events.Events
{
    public class TouchPressEvent : Event
    {

        private TouchLocation touch;

        public TouchPressEvent(TouchLocation touchLocation) : base(Base.EventType.OnTouchPressEvent)
        {
            this.touch = touchLocation;
        }

        public TouchLocation getTouchLocation()
        {
            return this.touch;
        }
    }
}
