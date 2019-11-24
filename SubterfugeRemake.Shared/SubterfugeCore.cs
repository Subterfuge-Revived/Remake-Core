#region Using Statements
using System;
using Android.OS;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SubterfugeFrontend.Shared.Content.Game.Events;
using SubterfugeFrontend.Shared.Content.Game.Graphics;
using SubterfugeFrontend.Shared.Content.Game.World;
using SubterfugeCore.Timing;
using SubterfugeFrontend.Shared.Content.Game.Events.Listeners;
using SubterfugeFrontend.Shared.Content.Game.UI;
using System.Collections.Generic;
using SubterfugeFrontend.Shared.Content.Gui;
using System.IO;

#endregion

namespace SubterfugeFrontend.Shared
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class SubterfugeApp : Game
    {
        public static GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        ApplicationState applicationState;
        Vector2 baseScreenSize = new Vector2(750, 1334); // This is the targeted resolution for the game.
        public static Matrix globalTransformation;            // THis is the transform matrix that spriteBatch will apply to properly draw
        private static SpriteLoader spriteLoader = new SpriteLoader();
        private static FontLoader fontLoader = new FontLoader();


        internal static SpriteLoader SpriteLoader { get => spriteLoader; set => spriteLoader = value; }
        internal static FontLoader FontLoader { get => fontLoader; set => fontLoader = value; }
        public SubterfugeApp()
        {

            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            graphics.IsFullScreen = true;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            SpriteLoader.loadSprites(Content);
            FontLoader.loadFonts(Content);

            applicationState = new ApplicationState();
            Console.WriteLine(NtpConnector.GetNetworkTime().ToLongDateString());

            //Work out how much we need to scale our graphics to fill the screen
            float horScaling = GraphicsDevice.PresentationParameters.BackBufferWidth / baseScreenSize.X;
            float verScaling = GraphicsDevice.PresentationParameters.BackBufferHeight / baseScreenSize.Y;
            Vector3 screenScalingFactor = new Vector3(horScaling, verScaling, 1);
            globalTransformation = Matrix.CreateScale(screenScalingFactor);
        }

        public void onClick(object sender, EventArgs e)
        {
            Console.WriteLine("Button Clicked");
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {

            // For Mobile devices, this logic will close the Game when the Back button is pressed
            // Exit() is obsolete on iOS
#if !__IOS__ && !__TVOS__
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
                Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                Exit();
            }
#endif
            // TODO: Add your update logic here			
            applicationState.Update(gameTime);
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // Applying a transformation matrix breaks my imput listeners for some reason :(
            // spriteBatch.Begin(transformMatrix: globalTransformation);
            spriteBatch.Begin();
            applicationState.Draw(spriteBatch, gameTime);
            base.Draw(gameTime);
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
