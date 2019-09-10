#region Using Statements
using System;
using Android.OS;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
// using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Input;
using SubterfugeCore.Shared.Content.Game.Events;
using SubterfugeCore.Shared.Content.Game.Objects;
using SubterfugeRemake.Shared.Content.Game.Core.Timing;
using SubterfugeRemake.Shared.Content.Game.Graphics;
using SubterfugeRemake.Shared.Content.Game.World;

#endregion

namespace SubterfugeCore.Shared
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class SubterfugeApp : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        public static EventObserver eventObserver;
        private static SpriteLoader spriteLoader = new SpriteLoader();
        private static FontLoader fontLoader = new FontLoader();
        Match match;

        internal static SpriteLoader SpriteLoader { get => spriteLoader; set => spriteLoader = value; }
        internal static FontLoader FontLoader { get => fontLoader; set => fontLoader = value; }

        public SubterfugeApp()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            graphics.IsFullScreen = true;
            eventObserver = new EventObserver();
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            base.Initialize();
            match = new Match(graphics.GraphicsDevice);
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

            Console.WriteLine(NtpConnector.GetNetworkTime().ToLongDateString());
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
            match.update(gameTime);
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            spriteBatch.Begin();
            match.render(spriteBatch, gameTime);
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
