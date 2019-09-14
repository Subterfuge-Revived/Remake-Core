using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using SubterfugeRemake.Shared.Content.Game.Graphics.GameObjects;
using System;
using System.Collections.Generic;
using SubterfugeCore.Shared;
using Color = Microsoft.Xna.Framework.Color;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using SubterfugeCore.GameObjects.Base;

namespace SubterfugeRemake.Shared.Content.Game.Graphics
{
    class Camera
    {
        protected static Rectangle cameraBounds = new Rectangle(); //The area on the map that is visible
        protected Rectangle cameraScreenLocation = new Rectangle(); //The area of the screen the camera takes up
        private double widthRatio;
        private double heightRatio;
        private bool isActive;

        TouchCollection[] touchCollection = new TouchCollection[2];


        public Camera(GraphicsDevice device)
        {
            this.isActive = true;
            cameraBounds = new Rectangle(0, 0, device.Viewport.Width, device.Viewport.Height);
        }

        public Rectangle getCameraBounds()
        {
            return cameraBounds;
        }

        public Rectangle getScreenLocation()
        {
            return this.cameraScreenLocation;
        }

        public void update()
        {
            // Determine if the camera's position should be updated.
            touchCollection[0] = touchCollection[1];
            touchCollection[1] = TouchPanel.GetState();

            if (touchCollection[1].Count > 0)
            {
                if (touchCollection[1][0].State == TouchLocationState.Moved)
                {
                    TouchLocation location = touchCollection[1][0];
                    TouchLocation lastLocation = touchCollection[0][0];
                    Vector2 delta = lastLocation.Position - location.Position;

                    // Update the camera based on the delta.
                    Rectangle currentBounds = cameraBounds;
                    cameraBounds = new Rectangle((int)Math.Round(currentBounds.X + delta.X), (int)Math.Round(currentBounds.Y + delta.Y), currentBounds.Width, currentBounds.Height);

                }
                else
                {
                    touchCollection[0] = touchCollection[1];
                }
            }

        }

        public void render(SpriteBatch spriteBatch, GameTime gameTime, List<GameObject> gameObjects)
        {

            // Draw the background.
            spriteBatch.Draw(
                texture: SubterfugeApp.SpriteLoader.getSprite("Sea"),
                destinationRectangle: new Rectangle(0, 0, cameraBounds.Width, cameraBounds.Height), color: Color.Cyan);

            // If the camera is not enabled, do not render.
            if (!this.isActive)
            {
                return;
            }
            // Call the render function on all game objects
            foreach (GameObject gameObject in gameObjects)
            {
                if (cameraBounds.Contains(new Point((int)gameObject.getPosition().X, (int)gameObject.getPosition().Y)))
                {
                    TexturedGameObject texturedObject = TextureFactory.getTexturedObject(gameObject);
                    texturedObject.render(spriteBatch);
                }
            }
        }

        // Determines the gameObject's relative location on the screen based on the camera's position.
        public static Rectangle getRelativeLocation(TexturedGameObject gameObject)
        {
            return new Rectangle((int)gameObject.getPosition().X - cameraBounds.X, (int)gameObject.getPosition().Y - cameraBounds.Y, gameObject.Width, gameObject.Height);
        }

        // Determines the gameObject's relative location on the screen based on the camera's position.
        public static Vector2 getRelativePosition(Vector2 objectPosition)
        {
            return new Vector2(objectPosition.X - cameraBounds.X, objectPosition.Y - cameraBounds.Y);
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
