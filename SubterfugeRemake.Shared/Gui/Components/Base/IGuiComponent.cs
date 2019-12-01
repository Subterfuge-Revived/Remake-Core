using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SubterfugeFrontend.Shared.Content.Game.UI.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace SubterfugeFrontend.Shared.Content.Game.UI
{
    interface IGuiComponent
    {

        void Draw(SpriteBatch spriteBatch, GameTime gameTime);
        void unregisterEvents();
        void addChild();
        void removeChild();
        void getOffset();
        List<IGuiComponent> getChildren();
    }
}
