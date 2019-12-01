using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SubterfugeFrontend.Shared.Content.Game.Graphics;
using SubterfugeFrontend.Shared.Gui.Components.Base;
using SubterfugeFrontend.Shared.Gui.Helper;

namespace SubterfugeFrontend.Shared.Content.Game.UI.Base
{
    public abstract class GuiComponent : IRectangular
    {
        protected List<GuiComponent> children = new List<GuiComponent>();

        protected Color BackgroundColor { get; set; }
        protected Color TextColor { get; set; }
        protected bool IsEnabled{get; set;}
        protected bool IsVisible { get; set; }
        protected Point Origin { get; set; }
        public Thickness Margin { get; set; }
        public Thickness Padding { get; set; }
        public bool IsLayoutRequired { get; set; }
        public Alignment HorizontalAlignment { get; set; } = Alignment.STRETCH;
        public Alignment VerticalAlignment { get; set; } = Alignment.STRETCH;
        public Alignment HorizontalTextAlignment { get; set; } = Alignment.CENTER;
        public Alignment VerticalTextAlignment { get; set; } = Alignment.CENTER;
        public string Text { get; set; }
        public int FontSize { get; set; } = 12;

        public Texture2D Texture { get; set; } = null;

        public Rectangle BoundingRectangle { get; set; }

        protected GuiComponent()
        {
            BackgroundColor = Color.White;
            TextColor = Color.White;
            IsEnabled = true;
            IsVisible = true;
            Origin = Point.Zero;
        }

        public Rectangle ContentRectangle
        {
            get
            {
                var r = BoundingRectangle;
                return new Rectangle(r.Left + Padding.Left, r.Top + Padding.Top, r.Width - Padding.Width, r.Height - Padding.Height);
            }
        }

        public void addChild(GuiComponent child)
        {
            this.children.Add(child);
        }

        public void removeChild(GuiComponent child)
        {
            this.children.Remove(child);
        }

        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            // Debugging rectangles.
            DrawHelper.DrawBorder(spriteBatch, this.BoundingRectangle, 2, Color.Red);
            DrawHelper.DrawBorder(spriteBatch, this.ContentRectangle, 2, Color.Maroon);

            if (this.Texture == null) {
                DrawHelper.DrawFilledRectangle(spriteBatch, this.BoundingRectangle, this.BackgroundColor);
            } else
            {
                spriteBatch.Draw(this.Texture, this.BoundingRectangle, this.Texture.Bounds, this.BackgroundColor, 0, new Vector2(this.Texture.Bounds.Center.X, this.Texture.Bounds.Center.Y), new SpriteEffects(), 0);
            }

            if(this.Text != null)
            {
                // Determine where to draw the text based on alignment method.

                // TODO. For now it centers all text.

                spriteBatch.DrawString(SubterfugeApp.FontLoader.getFont("Arial"), this.Text, new Vector2(this.ContentRectangle.Center.X, this.ContentRectangle.Center.Y), this.TextColor);
            }
        }

        public abstract void unregisterEvents();
    }
}
