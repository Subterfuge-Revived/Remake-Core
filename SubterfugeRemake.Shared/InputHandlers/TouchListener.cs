using Microsoft.Xna.Framework.Input.Touch;
using SubterfugeFrontend.Shared.Content.Game.Events.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace SubterfugeFrontend.Shared.Content.Game.Events.Listeners
{
    class TouchListener : IListener
    {
        private TouchCollection[] touchCollection = new TouchCollection[2];
        private bool isTouch = false;

        // Event delegates to allow components to listen to inputs.
        public event EventHandler<TouchPressEvent> Press;
        public event EventHandler<TouchReleaseEvent> Release;
        public event EventHandler<TouchDragEvent> Drag;

        public void listen()
        {
            // Determine if the camera's position should be updated.
            touchCollection[0] = touchCollection[1];
            touchCollection[1] = TouchPanel.GetState();

            if (touchCollection[1].Count > 0)
            {
                if (touchCollection[1][0].State == TouchLocationState.Moved)
                {
                    Drag?.Invoke(this, new TouchDragEvent(touchCollection));
                }
                else
                {
                    touchCollection[0] = touchCollection[1];
                }
                if (touchCollection[1][0].State == TouchLocationState.Pressed && !this.isTouch)
                {
                    Console.WriteLine("Touched");
                    Press?.Invoke(this, new TouchPressEvent(touchCollection[1][0]));
                    this.isTouch = true;
                }
                if (touchCollection[1][0].State == TouchLocationState.Released)
                {
                    Console.WriteLine("Released Touch");
                    Release?.Invoke(this, new TouchReleaseEvent(touchCollection[1][0]));
                    this.isTouch = false;
                }
            }
        }

    }
}
