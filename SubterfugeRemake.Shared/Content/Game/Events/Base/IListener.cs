using System;
using System.Collections.Generic;
using System.Text;

namespace SubterfugeFrontend.Shared.Content.Game.Events.Base
{
    public interface IListener
    {
        void onEvent(Event e);
        void registerListener();
        void unregisterListener();
    }
}
