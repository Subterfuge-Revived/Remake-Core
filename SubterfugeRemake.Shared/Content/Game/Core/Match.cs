using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SubterfugeCore;
using SubterfugeFrontend.Shared.Content.Game.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace SubterfugeFrontend.Shared.Content.Game.World
{
    /// <summary>
    ///  This class represents a single game within the application. It holds the game state, players list, and much more.
    /// </summary>
    class Match
    {
        private Camera camera;
        private GameServer gameServer;

        public Match(GraphicsDevice device)
        {
            this.gameServer = new GameServer();
            this.camera = new Camera(device);
        }


        public void update(GameTime gameTime) {
            this.gameServer.GetGameState();
            // Check to see if the camera has been moved.

        }
        public void render(SpriteBatch spriteBatch, GameTime gameTime)
        {
            this.camera.startRender(spriteBatch);
            this.camera.render(spriteBatch, gameTime, gameServer.GetGameState().getSubList());
            this.camera.render(spriteBatch, gameTime, gameServer.GetGameState().getOutposts());
        }
    }
}
