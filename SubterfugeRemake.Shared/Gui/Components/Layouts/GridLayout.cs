using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SubterfugeFrontend.Shared.Content.Game.UI.Base;

namespace SubterfugeFrontend.Shared.Content.Game.UI.Layouts
{
    public class GridLayout : GuiComponent
    {
        int cols;
        int rows;

        public GridLayout(int cols, int rows) : base(Alignment.CENTER)
        {
            this.cols = cols;
            this.rows = rows;
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            int counter = 0;
            foreach(GuiComponent child in this.children)
            {
                if(counter + 1 > cols)
                {
                    // Add new row.
                }
            }
            throw new NotImplementedException();
        }

        public override void unregisterEvents()
        {
            throw new NotImplementedException();
        }
    }
}
