using System;
using System.Diagnostics;
using System.Drawing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace SubterfugeCore.Shared.Content.Game.Objects.Base
{
    abstract class GameObject
    {

        protected RectangleF boundingBox;
        protected Texture2D objectTexture;
        protected Vector2 position;
        protected float rotation = 0f;

        public GameObject(Texture2D objectTexture)
        {
            this.objectTexture = objectTexture;
            this.boundingBox = new RectangleF(this.position.X - (this.objectTexture.Width / 2), this.position.Y - (this.objectTexture.Height / 2), this.objectTexture.Width, this.objectTexture.Height);
        }

        private void updateObject(GameTime gameTime)
        {
            // Update the position of the bounding box
            if (this.objectTexture != null)
            {
                this.boundingBox = new RectangleF(this.position.X - (this.objectTexture.Width / 2), this.position.Y - (this.objectTexture.Height / 2), this.objectTexture.Width, this.objectTexture.Height);
            }

            // Call object's update function
            this.update(gameTime);
        }

        public abstract void update(GameTime gameTime);

        public Texture2D getTexture()
        {
            return this.objectTexture;
        }

        public RectangleF getBoundingBox()
        {
            return this.boundingBox;
        }

        public Boolean isColliding(GameObject gameObject, float timeDelta)
        {
            RectangleF boundingBox = gameObject.getBoundingBox();
            if (this.boundingBox.Contains(boundingBox) || this.boundingBox.IntersectsWith(boundingBox))
            {
                return true;
            }

            return false;
        }

        public Vector2 getPosition()
        {
            return this.position;
        }

    }
}
