using Microsoft.Xna.Framework.Input.Touch;
using SubterfugeFrontend.Shared.Content.Game.Events.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace SubterfugeFrontend.Shared.Content.Game.Events.Listeners
{
    class InputListener
    {
        private List<IListener> listeners = new List<IListener>();
        public static TouchListener touchListener { get; } = new TouchListener();
        public static KeyboardListener keyboardListener { get; } = new KeyboardListener();
        public static MouseListener mouseListener { get; } = new MouseListener();

        public InputListener()
        {
            this.listeners.Add(touchListener);
            this.listeners.Add(keyboardListener);
            this.listeners.Add(mouseListener);
        }

        public void addListener(IListener listener)
        {
            this.listeners.Add(listener);
        }

        public void listen()
        {
            foreach(IListener listener in this.listeners)
            {
                listener.listen();
            }
        }
    }
}
