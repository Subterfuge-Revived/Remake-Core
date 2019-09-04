using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Input.Touch;
using SubterfugeCore.Shared.Content.Game.Events.Base;

namespace SubterfugeCore.Shared.Content.Game.Events
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
