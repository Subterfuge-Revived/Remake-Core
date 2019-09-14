using System.Collections.Generic;

namespace SubterfugeFrontend.Shared.Content.Game.Events
{
    public class EventObserver
    {
        private List<EventListener> eventListeners = new List<EventListener>();

        public EventObserver()
        {
        }

        public void triggerEvent(Event e)
        {
            foreach (EventListener listener in eventListeners)
            {
                if (listener.getEventType() == e.getEventType())
                {
                    listener.onEvent(e);
                }
            }
        }

        public void addEventListener(EventListener listener)
        {
            this.eventListeners.Add(listener);
        }

        public void removeEventListener(EventListener listener)
        {
            this.eventListeners.Remove(listener);
        }
    }
}
