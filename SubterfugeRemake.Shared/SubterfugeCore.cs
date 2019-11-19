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

#endregion

namespace SubterfugeFrontend.Shared
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class SubterfugeApp : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        InputListener inputListener = new InputListener();
        private static SpriteLoader spriteLoader = new SpriteLoader();
        private static FontLoader fontLoader = new FontLoader();
        private List<IGuiComponent> guiComponents = new List<IGuiComponent>();
        // private GuiSystem guiSystem;

        Match match;

        internal static SpriteLoader SpriteLoader { get => spriteLoader; set => spriteLoader = value; }
        internal static FontLoader FontLoader { get => fontLoader; set => fontLoader = value; }
        internal Match Match { get => match; set => match = value; }

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
            match = new Match(graphics.GraphicsDevice);
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            /* 
             * 
             * Code for when we add Monogame.Extended.Gui library
             * 
            // var viewportAdapter = new DefaultViewportAdapter(GraphicsDevice);
            // var guiRenderer = new GuiSpriteBatchRenderer(GraphicsDevice, () => Matrix.Identity);

            // Create a new screen
            // Screen screen = new MainMenu();
            // this.guiSystem = new GuiSystem(viewportAdapter, guiRenderer) { ActiveScreen = screen };
            */

            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            SpriteLoader.loadSprites(Content);
            FontLoader.loadFonts(Content);

            Console.WriteLine(NtpConnector.GetNetworkTime().ToLongDateString());

            Button button = new Button("Click Me!", fontLoader.getFont("Arial"), SpriteLoader.getSprite("Sea"), Color.Blue, new Rectangle(200, 200, 200, 200));
            button.Click += onClick;

            guiComponents.Add(button); ;
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
            inputListener.listen();
            match.update(gameTime);

            // guiSystem.Update(gameTime);
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            spriteBatch.Begin();
            match.render(spriteBatch, gameTime);
            foreach (IGuiComponent component in this.guiComponents)
            {
                component.Draw(spriteBatch, gameTime);
            }
            spriteBatch.End();
            // guiSystem.Draw(gameTime);

            base.Draw(gameTime);
        }
    }
}
