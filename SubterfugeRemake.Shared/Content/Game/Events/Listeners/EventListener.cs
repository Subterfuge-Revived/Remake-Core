using System;
using System.Collections.Generic;
using System.Text;

using SubterfugeCore.Shared.Content.Game.Events.Base;

namespace SubterfugeCore.Shared.Content.Game.Events
{
    public class EventListener
    {
        private EventType eventType;

        public EventListener(EventType eventType)
        {
            this.eventType = eventType;
        }

        public EventType getEventType()
        {
            return this.eventType;
        }

        // This method should be overridden
        public void onEvent(Event e)
        {
        }

    }
}
