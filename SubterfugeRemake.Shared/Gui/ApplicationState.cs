using Android.Transitions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SubterfugeFrontend.Shared.Content.Game.Events.Listeners;
using SubterfugeFrontend.Shared.Content.Gui.Scenes;

namespace SubterfugeFrontend.Shared.Content.Gui
{
    class ApplicationState
    {
        private InputListener inputListener = new InputListener();
        public static Scenes.Scene currentScene;
        DisplayMode currentDisplay = SubterfugeApp.graphics.GraphicsDevice.DisplayMode;

        public ApplicationState()
        {
            currentScene = new MenuScene();
        }

        public void Update(GameTime gametime)
        {
            currentScene.Update(gametime);
            inputListener.listen();
        }

        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            currentScene.Draw(spriteBatch, gameTime);
        }

        public static void setScene(Scenes.Scene scene)
        {
            currentScene.unloadEventListeners();
            currentScene = scene;
        }

    }
}
