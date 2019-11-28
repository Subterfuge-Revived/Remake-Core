using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SubterfugeFrontend.Shared.Content.Game.Network;
using SubterfugeFrontend.Shared.Content.Game.UI;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace SubterfugeFrontend.Shared.Content.Gui.Scenes
{
    class MenuScene : Scene
    {

        List<IGuiComponent> renderables = new List<IGuiComponent>();

        private EventHandler playClick;
        private EventHandler networkClick;

        public MenuScene() : base("Main Menu")
        {
            Button playButton = new Button("Play Game", SubterfugeApp.FontLoader.getFont("Arial"), SubterfugeApp.SpriteLoader.getSprite("blue-button"), Color.Blue, new Rectangle(100, 100, 400, 200));
            Button checkNetworkButton = new Button("Check Network", SubterfugeApp.FontLoader.getFont("Arial"), SubterfugeApp.SpriteLoader.getSprite("blue-button"), Color.Blue, new Rectangle(100, 300, 400, 200));

            playButton.Click += onPlayClicked;
            checkNetworkButton.Click += checkNetwork;

            renderables.Add(playButton);
            renderables.Add(checkNetworkButton);
        }

        public void onPlayClicked(object sender, EventArgs e)
        {
            ApplicationState.setScene(new GameScene());
        }

        public async void checkNetwork(object sender, EventArgs e)
        {
            Api api = new Api();
            HttpResponseMessage response = await api.login("Test", "$2y$10$gAPTQXr.bwRRgjXoJQM7kOa3hAAAfQngaDVGWcOTJqvfBdgl.uILK");
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            foreach(IGuiComponent component in renderables)
            {
                component.Draw(spriteBatch, gameTime);
            }
        }

        public override void Update(GameTime gameTime)
        {

        }

        public override void unloadEventListeners()
        {
            foreach(IGuiComponent guiComponent in this.renderables)
            {
                guiComponent.unregisterEvents();
            }
        }
    }
}
