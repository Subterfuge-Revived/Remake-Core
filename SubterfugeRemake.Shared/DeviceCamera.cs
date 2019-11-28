using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SubterfugeFrontend.Shared.Content.Gui;
using System;
using System.Collections.Generic;
using System.Text;

namespace SubterfugeFrontend.Shared
{
    class DeviceCamera
    {

        // Device parameters for rendering.
        public static int DEVICE_WIDTH;
        public static int DEVICE_HEIGHT;
        public static float HORIZONTAL_SCALING;
        public static float VERTICAL_SCALING;
        public static Vector2 TARGET_RESOLUTION = new Vector2(750, 1334); // This is the targeted resolution for the game.
        public static Matrix TRANSFORMATION_MATRIX;

        // ApplicationState to begin rendering the app on the user's device.
        private ApplicationState applicationState;

        public DeviceCamera(GraphicsDevice device)
        {
            DEVICE_WIDTH = device.PresentationParameters.BackBufferWidth;
            DEVICE_HEIGHT = device.PresentationParameters.BackBufferHeight;

            HORIZONTAL_SCALING = DEVICE_WIDTH / TARGET_RESOLUTION.X;
            VERTICAL_SCALING = DEVICE_HEIGHT / TARGET_RESOLUTION.Y;

            Vector3 screenScalingFactor = new Vector3(HORIZONTAL_SCALING, VERTICAL_SCALING, 1);
            TRANSFORMATION_MATRIX = Matrix.CreateScale(screenScalingFactor);

            applicationState = new ApplicationState();
        }

        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            spriteBatch.Begin(transformMatrix: TRANSFORMATION_MATRIX);
            applicationState.Draw(spriteBatch, gameTime);
            spriteBatch.End();
        }

        public void Update(GameTime gameTime)
        {
            applicationState.Update(gameTime);
        }

        public static Point getWorldLocation(Point deviceLocation)
        {
            return new Point((int)(deviceLocation.X / HORIZONTAL_SCALING), (int)(deviceLocation.Y / VERTICAL_SCALING));
        }


        public static Point getWorldLocation(Vector2 deviceLocation)
        {
            return new Point((int)(deviceLocation.X / HORIZONTAL_SCALING), (int)(deviceLocation.Y / VERTICAL_SCALING));
        }



    }
}
