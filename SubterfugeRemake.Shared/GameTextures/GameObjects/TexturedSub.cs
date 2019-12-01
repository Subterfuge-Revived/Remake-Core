using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SubterfugeCore.Entities;
using SubterfugeCore.Entities.Base;

namespace SubterfugeFrontend.Shared.Content.Game.Graphics.GameObjects
{
    class TexturedSub : TexturedGameObject
    {

        public TexturedSub(GameObject gameObject) : base(gameObject, SubterfugeApp.SpriteLoader.getSprite("SubFill"),
            100, 100)
        {
            this.rotation = (float)Math.PI / 4f;
        }

        public override void render(SpriteBatch spriteBatch)
        {
            Color playerColor;

            if (((Sub)this.gameObject).getOwner() == null)
            {
                return;
            }

            switch (((Sub)this.gameObject).getOwner().getId())
            {
                case 1:
                    playerColor = Color.Blue;
                    break;
                case 2:
                    playerColor = Color.Red;
                    break;
                case 3:
                    playerColor = Color.Teal;
                    break;
                case 4:
                    playerColor = Color.Purple;
                    break;
                case 5:
                    playerColor = Color.Green;
                    break;
                case 6:
                    playerColor = Color.Orange;
                    break;
                case 7:
                    playerColor = Color.Yellow;
                    break;
                case 8:
                    playerColor = Color.Black;
                    break;
                default:
                    playerColor = Color.DarkGray;
                    break;
            }



            spriteBatch.Draw(
                texture: this.getTexture(),
                destinationRectangle: Camera.getRelativeScreenBoundary(this),
                sourceRectangle: this.getTexture().Bounds,
                effects: new SpriteEffects(),
                layerDepth: 1,
                color: playerColor,
                origin: this.getTexture().Bounds.Size.ToVector2() / 2f,
                rotation: this.rotation + (float)((Sub)gameObject).getRotation()); ;


            SpriteFont font = SubterfugeApp.FontLoader.getFont("Arial");
            string drillerCount = ((Sub) this.gameObject).getDrillerCount().ToString();
            Vector2 stringSize = font.MeasureString(drillerCount);


            spriteBatch.DrawString(
                spriteFont: SubterfugeApp.FontLoader.getFont("Arial"),
                text: drillerCount,
                position: Camera.getRelativeScreenPosition(this.getPosition()),
                color: Color.White,
                rotation: this.rotation + (float)((Sub)gameObject).getRotation() + (float)(Math.PI / 2),
                origin: stringSize / 2f,
                layerDepth: 1f,
                scale: 1.5f,
                effects: SpriteEffects.None);
        }
    }
}
