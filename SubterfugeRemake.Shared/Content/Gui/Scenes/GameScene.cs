using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SubterfugeFrontend.Shared.Content.Game.World;

namespace SubterfugeFrontend.Shared.Content.Gui.Scenes
{
    class GameScene : Scene
    {
        private Match match;

        public GameScene() : base("Game")
        {
            this.match = new Match(SubterfugeApp.graphics.GraphicsDevice);
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            match.render(spriteBatch, gameTime);
        }

        public override void Update(GameTime gameTime)
        {
            match.update(gameTime);
        }
    }
}
