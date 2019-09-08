using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SubterfugeCore.Shared;
using SubterfugeCore.Shared.Content.Game.Objects.Base;

namespace SubterfugeRemake.Shared.Content.Game.Graphics.GameObjects
{
    class TexturedSub : TexturedGameObject
    {
        public TexturedSub(GameObject gameObject) : base(gameObject, SubterfugeApp.SpriteLoader.getSprite("riot"), 64, 64)
        {
        }

        public override void update(GameTime gameTime)
        {
            // TODO: convert gameTime to a dateTime
        }
    }
}
