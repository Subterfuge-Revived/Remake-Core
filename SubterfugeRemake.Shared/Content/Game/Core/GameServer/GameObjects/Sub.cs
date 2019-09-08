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
        public Sub() : base()
        {
            this.position = new Vector2(0, 0);
        }
    }
}
