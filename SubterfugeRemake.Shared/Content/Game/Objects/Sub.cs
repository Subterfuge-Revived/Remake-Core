using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Remoting.Contexts;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SubterfugeCore.Shared.Content.Game.Objects.Base;

namespace SubterfugeCore.Shared.Content.Game.Objects
{
    class Sub : GameObject
    {
        public Sub() : base(SubterfugeCore.Shared.SubterfugeApp.SpriteLoader.getSprite("riot"))
        {
            this.position = new Vector2(0, 0);
        }

        public override void update(GameTime gameTime)
        {
            Vector2 position = this.position;
            this.position = new Vector2(position.X + 1, position.Y + 1);
        }
    }
}
