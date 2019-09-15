using SubterfugeCore.Entities;
using SubterfugeCore.Entities.Base;
using SubterfugeFrontend.Shared.Content.Game.Graphics.GameObjects;

namespace SubterfugeFrontend.Shared.Content.Game.Graphics
{
    class TextureFactory
    {

        public static TexturedGameObject getTexturedObject(GameObject gameObject)
        {
            if (gameObject.GetType().Equals(typeof(Sub)))
            {
                return new TexturedSub(gameObject);
            }
            if (gameObject.GetType().Equals(typeof(Outpost)))
            {
                return new TexturedOutpost(gameObject);
            }
            return null;
        }

    }
}
