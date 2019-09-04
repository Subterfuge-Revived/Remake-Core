using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SubterfugeCore.Shared.Content.Game.Objects;
using SubterfugeCore.Shared.Content.Game.Objects.Base;
using SubterfugeRemake.Shared.Content.Game.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace SubterfugeRemake.Shared.Content.Game.World
{
    /// <summary>
    ///  This class represents a single game within the application. It holds the game state, players list, and much more.
    /// </summary>
    class Match
    {
        private Camera camera;
        private GameState gameState;
        private List<GameObject> gameObjects = new List<GameObject>();

        public Match()
        {
            this.gameState = new GameState();
            this.camera = new Camera();
            gameObjects.Add(new Sub());
        }


        public void update(GameTime gameTime) {
            // Check to see if the camera has been moved.
            this.camera.update();

            // Call the update function on all game objects.
            foreach(GameObject gameObject in this.gameObjects)
            {
                gameObject.update(gameTime);
            }

        }
        public void render(SpriteBatch spriteBatch, GameTime gameTime)
        {
            this.camera.render(spriteBatch, gameTime, gameObjects);
        }
    }
}
