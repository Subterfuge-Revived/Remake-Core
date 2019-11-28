using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using SubterfugeFrontend.Shared.Content.Game.Graphics.GameObjects;
using System;
using System.Collections.Generic;
using SubterfugeFrontend.Shared;
using Color = Microsoft.Xna.Framework.Color;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using SubterfugeCore.Entities.Base;
using SubterfugeFrontend.Shared.Content.Game.Events.Listeners;
using SubterfugeFrontend.Shared.Content.Game.Events;
using SubterfugeFrontend.Shared.Content.Game.Events.Events;
using SubterfugeCore;
using SubterfugeCore.Timing;
using System.Drawing;
using SubterfugeCore.Entities;
using SubterfugeCore.GameEvents;

namespace SubterfugeFrontend.Shared.Content.Game.Graphics
{
    class Camera
    {
        protected static Rectangle cameraBounds = new Rectangle(); //The area on the map that is visible
        protected Rectangle cameraScreenLocation = new Rectangle(); //The area of the screen the camera takes up
        private double widthRatio;
        private double heightRatio;
        private bool isActive;

        private bool _isTouchingGameObject = false;
        private TexturedGameObject initialPress = null;

        TouchCollection[] touchCollection = new TouchCollection[2];


        public Camera(GraphicsDevice device)
        {
            this.isActive = true;
            cameraBounds = new Rectangle(0, 0, device.Viewport.Width, device.Viewport.Height);
            cameraScreenLocation = new Rectangle(0, 0, device.Viewport.Width, device.Viewport.Height);

            InputListener.touchListener.Press += onPress;
            InputListener.touchListener.Drag += onDrag;
            InputListener.touchListener.Release += onRelease;
        }

        public Rectangle getCameraBounds()
        {
            return cameraBounds;
        }

        public Rectangle getScreenLocation()
        {
            return this.cameraScreenLocation;
        }

        public void startRender(SpriteBatch spriteBatch)
        {
            // Draw the background.
            spriteBatch.Draw(
                texture: SubterfugeApp.SpriteLoader.getSprite("Sea"),
                sourceRectangle: SubterfugeApp.SpriteLoader.getSprite("Sea").Bounds,
                destinationRectangle: cameraScreenLocation,
                color: Color.Cyan);

        }

        public void render(SpriteBatch spriteBatch, GameTime gameTime)
        {
            // If the camera is not enabled, do not render.
            if (!this.isActive)
            {
                return;
            }
            // Call the render function on all game objects
            foreach (GameObject gameObject in GameServer.timeMachine.getState().getOutposts())
            {
                TexturedGameObject texturedObject = TextureFactory.getTexturedObject(gameObject);
                RectangleF boudningBox = texturedObject.getBoundingBox();
                Rectangle RectBox = new Rectangle((int)boudningBox.X, (int)boudningBox.Y, (int)boudningBox.Width, (int)boudningBox.Height);
                if (cameraBounds.Contains(RectBox) || cameraBounds.Intersects(RectBox))
                {
                    texturedObject.render(spriteBatch);
                }
            }
            // Call the render function on all game objects
            foreach (GameObject gameObject in GameServer.timeMachine.getState().getSubList())
            {
                TexturedGameObject texturedObject = TextureFactory.getTexturedObject(gameObject);
                RectangleF boudningBox = texturedObject.getBoundingBox();
                Rectangle RectBox = new Rectangle((int)boudningBox.X, (int)boudningBox.Y, (int)boudningBox.Width, (int)boudningBox.Height);
                if (cameraBounds.Contains(RectBox) || cameraBounds.Intersects(RectBox))
                {
                    texturedObject.render(spriteBatch);
                }
            }
        }

        // Determines the gameObject's relative location on the screen based on the camera's position.
        public static Rectangle getRelativeScreenBoundary(TexturedGameObject gameObject)
        {
            return new Rectangle((int)gameObject.getPosition().X - cameraBounds.X, (int)gameObject.getPosition().Y - cameraBounds.Y, gameObject.Width, gameObject.Height);
        }

        // Determines the gameObject's relative location on the screen based on the camera's position.
        public static Vector2 getRelativeScreenPosition(Vector2 objectPosition)
        {
            return new Vector2(objectPosition.X - cameraBounds.X, objectPosition.Y - cameraBounds.Y);
        }

        public static Vector2 getWorldPosition(Vector2 relativePosition)
        {
            return new Vector2(relativePosition.X + cameraBounds.X, relativePosition.Y + cameraBounds.Y);
        }

        public void setActive()
        {
            this.isActive = true;
        }

        public void setInactive()
        {
            this.isActive = false;
        }

        public void onPress(object sender, TouchPressEvent e)
        {

            Console.WriteLine("Touch detected.");
            Point pressLocation = DeviceCamera.getWorldLocation(e.touch.Position);
            Vector2 worldLocation = getWorldPosition(new Vector2(pressLocation.X, pressLocation.Y));

            bool found = false;
            foreach (Outpost outpost in GameServer.timeMachine.getState().getOutposts())
            {
                // Get the textured version to determine the bounding box.
                TexturedOutpost textured = new TexturedOutpost(outpost);
                if (textured.getBoundingBox().Contains(new PointF((int)(worldLocation.X), (int)(worldLocation.Y))))
                {
                    _isTouchingGameObject = true;
                    initialPress = textured;
                    found = true;
                }
            }
            if (!found)
            {
                _isTouchingGameObject = false;
                initialPress = null;
            }
        }

        public void onDrag(object sender, TouchDragEvent e)
        {
            if (!_isTouchingGameObject) {

                Point delta = DeviceCamera.getWorldLocation(e.getDelta());

                // Update the camera based on the delta.
                Rectangle currentBounds = cameraBounds;
                cameraBounds = new Rectangle((int)Math.Round((double)(currentBounds.X + delta.X)), (int)Math.Round((double)(currentBounds.Y + delta.Y)), currentBounds.Width, currentBounds.Height);
            }
        }

        public void onRelease(object sender, TouchReleaseEvent e)
        {
            if (_isTouchingGameObject)
            {
                Point pressLocation = DeviceCamera.getWorldLocation(e.touch.Position);
                Vector2 worldLocation = getWorldPosition(new Vector2(pressLocation.X, pressLocation.Y));
                TexturedGameObject releaseLocation = null;

                bool found = false;
                // Check if released on a game object.
                foreach (Outpost outpost in GameServer.timeMachine.getState().getOutposts())
                {
                    // Get the textured version to determine the bounding box.
                    TexturedOutpost textured = new TexturedOutpost(outpost);
                    if (textured.getBoundingBox().Contains(new PointF((int)(worldLocation.X), (int)(worldLocation.Y))))
                    {
                        releaseLocation = textured;
                        found = true;
                    }
                }

                if (found)
                {
                    // Launch a new sub!!!
                    LaunchEvent newLaunch = new LaunchEvent(GameServer.timeMachine.getState().getCurrentTick().getNextTick(), (Outpost)initialPress.gameObject, 1, (Outpost)releaseLocation.gameObject);
                    GameServer.timeMachine.addEvent(newLaunch);
                }
            }
        }
    }
}
