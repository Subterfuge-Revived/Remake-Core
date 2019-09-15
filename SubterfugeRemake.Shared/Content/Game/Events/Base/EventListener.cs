using System;
using System.Collections.Generic;
using System.Text;

namespace SubterfugeFrontend.Shared.Content.Game.Events.Listeners
{
    public abstract class EventListener
    {

        public delegate void eventHandler(Event e);

        public EventListener()
        {
            EventObserver.addEventHandler(this);
        }

        public abstract void onEvent(Event e);

    }
}
