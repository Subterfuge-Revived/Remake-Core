using SubterfugeFrontend.Shared.Content.Game.Events.Base;
using System;

namespace SubterfugeFrontend.Shared.Content.Game.Events
{
    public abstract class Event : EventArgs
    {

        public EventType eventType;

        public Event(EventType eventType)
        {
            this.eventType = eventType;
        }

        public EventType getEventType()
        {
            return this.eventType;
        }
    }
}
