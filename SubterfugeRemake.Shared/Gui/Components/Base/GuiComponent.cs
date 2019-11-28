using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SubterfugeFrontend.Shared.Content.Game.UI.Base
{
    public abstract class GuiComponent : IGuiComponent
    {
        protected Alignment alignment { get; set; }
        protected List<GuiComponent> children = new List<GuiComponent>();

        public GuiComponent(Alignment alignment)
        {
            this.alignment = alignment;
        }

        public void addChild(GuiComponent child)
        {
            this.children.Add(child);
        }

        public void removeChild(GuiComponent child)
        {
            this.children.Remove(child);
        }

        public abstract void Draw(SpriteBatch spriteBatch, GameTime gameTime);

        public abstract void unregisterEvents();
    }
}
