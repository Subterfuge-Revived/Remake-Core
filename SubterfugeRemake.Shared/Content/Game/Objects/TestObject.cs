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
    class TestObject : GameObject
    {
        public TestObject(Texture2D objectTexture) : base(objectTexture)
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
