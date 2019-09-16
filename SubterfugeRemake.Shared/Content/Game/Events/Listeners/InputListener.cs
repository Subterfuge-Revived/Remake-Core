using Microsoft.Xna.Framework.Input.Touch;
using SubterfugeFrontend.Shared.Content.Game.Events.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace SubterfugeFrontend.Shared.Content.Game.Events.Listeners
{
    class InputListener
    {

        private TouchCollection[] touchCollection = new TouchCollection[2];
        private bool isTouch = false;
        public void listen()
        {
            // Determine if the camera's position should be updated.
            touchCollection[0] = touchCollection[1];
            touchCollection[1] = TouchPanel.GetState();

            if (touchCollection[1].Count > 0)
            {
                if (touchCollection[1][0].State == TouchLocationState.Moved)
                {
                    new DragEvent(touchCollection);
                }
                else
                {
                    touchCollection[0] = touchCollection[1];
                }
                if(touchCollection[1][0].State == TouchLocationState.Pressed && !this.isTouch)
                {
                    Console.WriteLine("Touched");
                    this.isTouch = true;
                    new TouchPressEvent(touchCollection[1][0]);
                }
                if (touchCollection[1][0].State == TouchLocationState.Released)
                {
                    Console.WriteLine("Released Touch");
                    new TouchReleaseEvent(touchCollection[1][0]);
                    this.isTouch = false;
                }
            }
        }
    }
}
