using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace SubterfugeRemake.Shared.Content.Game.Graphics
{
    class SpriteLoader
    {
        private Dictionary<String, Texture2D> sprites = new Dictionary<string, Texture2D>();

        public SpriteLoader()
        {
            
        }

        public void addSprite(String spriteName, Texture2D sprite)
        {
            this.sprites[spriteName] = sprite;
        }

        public Texture2D getSprite(String spriteName)
        {
            if(this.sprites.ContainsKey(spriteName))
            {
                return this.sprites[spriteName];
            }
            return null;
        }

    }
}
