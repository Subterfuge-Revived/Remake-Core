using SubterfugeFrontend.Shared.Content.Game.Events.Base;
using SubterfugeFrontend.Shared.Content.Game.Events.Listeners;

namespace SubterfugeFrontend.Shared.Content.Game.Events
{
    public class EventObserver
    {
        public delegate void onEvent(Event e);  // Observer to store listeners
        public static event onEvent eventListener;     // event to trigger the listeners

        public EventObserver()
        {
        }

        public static void triggerEvent(Event e)
        {
            eventListener(e);
        }

        public static void addEventHandler(IListener listener)
        {
            eventListener += listener.onEvent;
        }

        public static void removeEventHandler(IListener listener)
        {
            eventListener -= listener.onEvent;
        }
    }
}
