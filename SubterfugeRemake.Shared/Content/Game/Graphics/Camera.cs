using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using SubterfugeCore.Shared.Content.Game.Objects.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace SubterfugeRemake.Shared.Content.Game.Graphics
{
    class Camera
    {
        protected Rectangle cameraBounds = new Rectangle(); //The area on the map that is visible
        protected Rectangle cameraScreenLocation = new Rectangle(); //The area of the screen the camera takes up
        private double widthRatio;
        private double heightRatio;
        private bool isActive;

        TouchCollection[] touchCollection = new TouchCollection[2];


        public Camera()
        {
            this.isActive = true;
            this.cameraBounds = new Rectangle(0, 0, 480, 920);
        }

        public Rectangle getCameraBounds()
        {
            return this.cameraBounds;
        }

        public Rectangle getScreenLocation()
        {
            return this.cameraScreenLocation;
        }

        public void update()
        {
            /**
            // Determine if the camera's position should be updated.
            touchCollection[0] = touchCollection[1];
            touchCollection[1] = TouchPanel.GetState();

            if (touchCollection[1].Count > 0)
            {
                if(touchCollection[1][0].State == TouchLocationState.Moved)
                {
                    // We want to get the difference between the most recent touch (last in the collection)
                    // and the one before it.
                    TouchLocation currentLocation = touchCollection[1][touchCollection[1].Count - 1];
                    TouchLocation lastLocation = touchCollection[1][touchCollection[1].Count - 2];

                    Vector2 delta = currentLocation.Position - lastLocation.Position;
                } else
                {
                    touchCollection[0] = touchCollection[1];
                }
                
                // Update the camera based on the delta.
                Rectangle currentBounds = this.cameraBounds;
                this.cameraBounds = new Rectangle((int)Math.Round(currentBounds.X + delta.X), (int)Math.Round(currentBounds.Y + delta.Y), currentBounds.Width, currentBounds.Height);
            }
    **/

        }

        public void render(SpriteBatch spriteBatch, GameTime gameTime, List<GameObject> gameObjects)
        {
            // If the camera is not enabled, do not render.
            if(!this.isActive)
            {
                return;
            }
            // Call the render function on all game objects
            foreach (GameObject gameObject in gameObjects)
            {
                spriteBatch.Draw(gameObject.getTexture(), this.getRelativeLocation(gameObject), Microsoft.Xna.Framework.Color.White);
            }
        }

        // Determines the gameObject's relative location on the screen based on the camera's position.
        private Rectangle getRelativeLocation(GameObject gameObject)
        {
            Rectangle drawLocation = new Rectangle((int)(gameObject.getPosition().X - (gameObject.getTexture().Width / 2)),
    (int)(gameObject.getPosition().Y - (gameObject.getTexture().Height / 2)), gameObject.getTexture().Width,
    height: gameObject.getTexture().Height);

            return new Rectangle(drawLocation.X - this.cameraBounds.X, drawLocation.Y - this.cameraBounds.Y, drawLocation.Height, drawLocation.Width);
        }

        public void setActive()
        {
            this.isActive = true;
        }

        public void setInactive()
        {
            this.isActive = false;
        }

    }
}
