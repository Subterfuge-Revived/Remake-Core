using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace SubterfugeFrontend.Shared.Content.Game.Graphics
{
    class FontLoader
    {
        private Dictionary<String, SpriteFont> fonts = new Dictionary<string, SpriteFont>();

        public FontLoader()
        {

        }

        public void addFont(String fontName, SpriteFont font)
        {
            this.fonts[fontName] = font;
        }

        public SpriteFont getFont(String fontName)
        {
            if (this.fonts.ContainsKey(fontName))
            {
                return this.fonts[fontName];
            }

            return null;
        }

        public void loadFonts(ContentManager content)
        {
            this.addFont("Arial", content.Load<SpriteFont>("Assets/Fonts/Arial"));
        }
    }
}
