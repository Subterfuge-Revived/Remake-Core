using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;
using System;

namespace SubterfugeFrontend.Shared.Content.Game.Events.Events
{
    public class DragEvent : Event
    {

        private TouchCollection[] touchCollection;

        public DragEvent(TouchCollection[] touchCollection) : base(Base.EventType.OnDragEvent)
        {
            this.touchCollection = touchCollection;
            EventObserver.triggerEvent(this);
        }

        public Vector2 getDelta()
        {
            TouchLocation location = this.touchCollection[1][0];
            TouchLocation lastLocation = this.touchCollection[0][0];
            return lastLocation.Position - location.Position;
        }
    }
}
