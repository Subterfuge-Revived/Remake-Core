using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SubterfugeCore.Entities.Base;
using System.Drawing;

namespace SubterfugeFrontend.Shared.Content.Game.Graphics.GameObjects
{
    abstract class TexturedGameObject
    {
        protected GameObject gameObject;
        protected Texture2D objectTexture;
        protected float rotation = 0f;
        public int Height { get; set; }
        public int Width { get; set; }

        // Class to convert from a GameObject from the GameServer to a renderable object.
        public TexturedGameObject(GameObject gameObject, Texture2D texture, int width, int height)
        {
            this.Width = width;
            this.Height = height;
            this.gameObject = gameObject;
            this.objectTexture = texture;
        }

        public abstract void render(SpriteBatch spriteBatch);


        public RectangleF getBoundingBox()
        {
            return new RectangleF(this.gameObject.getPosition().X - (this.Width / 2), this.gameObject.getPosition().Y - (this.Height / 2), this.Width, this.Height);
        }

        public Texture2D getTexture()
        {
            return this.objectTexture;
        }

        public Vector2 getPosition()
        {
            return this.gameObject.getPosition();
        }
    }
}
