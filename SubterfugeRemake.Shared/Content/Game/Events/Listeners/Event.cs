using SubterfugeFrontend.Shared.Content.Game.Events.Base;

namespace SubterfugeFrontend.Shared.Content.Game.Events
{
    public abstract class Event
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
