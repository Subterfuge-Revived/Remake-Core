using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SubterfugeCore.Entities.Base;
using SubterfugeFrontend.Shared.Content.Game.Events.Listeners;

namespace SubterfugeFrontend.Shared.Content.Game.Graphics.GameObjects
{
    class TexturedOutpost : TexturedGameObject
    {

        public TexturedOutpost(GameObject gameObject) : base(gameObject, SubterfugeApp.SpriteLoader.getSprite("GeneratorFill"),
            100, 100)
        {
        }

        public override void render(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(
                texture: this.getTexture(),
                destinationRectangle: Camera.getRelativeLocation(this),
                color: Color.Blue,
                origin: this.getTexture().Bounds.Size.ToVector2() / 2f);
        }
    }
}
