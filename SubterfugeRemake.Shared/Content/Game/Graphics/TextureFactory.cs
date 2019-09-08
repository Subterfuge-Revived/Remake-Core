using SubterfugeCore.Shared.Content.Game.Objects;
using SubterfugeCore.Shared.Content.Game.Objects.Base;
using SubterfugeRemake.Shared.Content.Game.Graphics.GameObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace SubterfugeRemake.Shared.Content.Game.Graphics
{
    class TextureFactory
    {

        public static TexturedGameObject getTexturedObject(GameObject gameObject)
        {
            if (gameObject.GetType().Equals(typeof(Sub)))
            {
                return new TexturedSub(gameObject);
            }
            return null;
        }

    }
}
