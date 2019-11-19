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

            spriteBatch.Draw(this.buttonTexture, this.buttonBounds, color);
            if (!String.IsNullOrEmpty(this.buttonText))
            {
                // Determine location to draw text
                var x = (this.buttonBounds.X + (this.buttonBounds.Width / 2)) - (_font.MeasureString(this.buttonText).X / 2);
                var y = (this.buttonBounds.Y + (this.buttonBounds.Height / 2)) - (_font.MeasureString(this.buttonText).Y / 2);

                spriteBatch.DrawString(_font, this.buttonText, new Vector2(x, y), this.fontColor);
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
            if (this.buttonBounds.Contains(release.getTouchLocation().Position))
            {
                // Press release was on the button. Emit event.
                Click?.Invoke(this, new EventArgs());
            }
            this._isHovering = false;
        }
    }
}
