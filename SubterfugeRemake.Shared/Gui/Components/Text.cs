using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SubterfugeFrontend.Shared.Content.Game.UI
{
    class Text : IGuiComponent
    {
        string text;
        Alignment alignment;

        Text(string text, Alignment alignment)
        {
            this.text = text;
        }

        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            throw new NotImplementedException();
        }
    }
}
