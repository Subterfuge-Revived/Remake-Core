using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace SubterfugeFrontend.Shared.Content.Gui.Scenes
{
    public abstract class Scene
    {
        private String sceneName;

        public Scene(String sceneName)
        {
            this.sceneName = sceneName;
        }

        public String getSceneName()
        {
            return this.sceneName;
        }

        public abstract void Draw(SpriteBatch spriteBatch, GameTime gameTime);

        public abstract void Update(GameTime gameTime);
    }
}
