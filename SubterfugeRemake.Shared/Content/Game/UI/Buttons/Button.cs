using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SubterfugeFrontend.Shared.Content.Game.Events.Events;
using SubterfugeFrontend.Shared.Content.Game.Events.Listeners;
using System;
using System.Collections.Generic;
using System.Text;

namespace SubterfugeFrontend.Shared.Content.Game.UI
{
    class Button : IGuiComponent
    {
        private String buttonText;
        private Texture2D buttonTexture;
        private Color fontColor;
        private Rectangle buttonBounds;

        // Button State
        private MouseState _currentMouse;
        private SpriteFont _font;
        private bool _isHovering = false;
        private MouseState _previousState;

        // Event delegate to register click functions.
        public event EventHandler Click;

        public Button(String buttonText, SpriteFont font, Texture2D texture, Color fontColor, Rectangle buttonBounds)
        {
            this.buttonText = buttonText;
            this.buttonTexture = texture;
            this.fontColor = fontColor;
            this.buttonBounds = buttonBounds;
            this._font = font;

            // Input listeners
            InputListener.Release += Released;
            InputListener.Press += Pressed;
        }

        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            Color color = Color.White;

            if (_isHovering)
                color = Color.Gray;

            spriteBatch.Draw(
                texture: this.buttonTexture,
                destinationRectangle: this.buttonBounds,
                color: color,
                sourceRectangle: this.buttonTexture.Bounds,
                effects: new SpriteEffects(),
                layerDepth: 0.0f,
                rotation: 0,
                origin: new Vector2(0, 0));

            if (!String.IsNullOrEmpty(this.buttonText))
            {
                Vector2 size = _font.MeasureString(this.buttonText);

                // Provide some button padding (10% each edge):
                int verticalPadding = (int)(this.buttonBounds.Height * 0.1);
                int horizontalPadding = (int)(this.buttonBounds.Width * 0.1);
                Rectangle paddedButtonBoundary = new Rectangle(this.buttonBounds.X + horizontalPadding, this.buttonBounds.Y + verticalPadding, this.buttonBounds.Width - 2 * horizontalPadding, this.buttonBounds.Height - 2 * verticalPadding);

                float xScale = (paddedButtonBoundary.Width / size.X);
                float yScale = (paddedButtonBoundary.Height / size.Y);

                // Taking the smaller scaling value will result in the text always fitting in the boundaires.
                float scale = Math.Min(xScale, yScale);

                // Figure out the location to absolutely-center it in the boundaries rectangle.
                int strWidth = (int)Math.Round(size.X * scale);
                int strHeight = (int)Math.Round(size.Y * scale);
                Vector2 position = new Vector2();
                position.X = (((this.buttonBounds.Width - strWidth) / 2) + this.buttonBounds.X);
                position.Y = (((this.buttonBounds.Height - strHeight) / 2) + this.buttonBounds.Y);

                // A bunch of settings where we just want to use reasonable defaults.
                float rotation = 0.0f;
                Vector2 spriteOrigin = new Vector2(0, 0);
                float spriteLayer = 0.0f; // all the way in the front
                SpriteEffects spriteEffects = new SpriteEffects();

                // Draw the string to the sprite batch!
                spriteBatch.DrawString(_font, this.buttonText, position, Color.White, rotation, spriteOrigin, scale, spriteEffects, spriteLayer);

            }
        }

        public void Pressed(object sender, EventArgs e)
        {
            // Determine if the clicked location was this button.
            TouchPressEvent press = (TouchPressEvent)e;
            if (this.buttonBounds.Contains(press.getTouchLocation().Position))
            {
                // Button was pressed.
                this._isHovering = true;
            }
        }

        public void Released(object sender, EventArgs e)
        {
            // Determine if the clicked location was this button.
            TouchReleaseEvent release = (TouchReleaseEvent)e;
            if (_isHovering)
            {
                if (this.buttonBounds.Contains(release.getTouchLocation().Position))
                {
                    // Press release was on the button. Emit event.
                    Click?.Invoke(this, new EventArgs());
                }
            }
            this._isHovering = false;
        }
    }
}
