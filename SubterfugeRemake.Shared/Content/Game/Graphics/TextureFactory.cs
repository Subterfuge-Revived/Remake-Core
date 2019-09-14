using SubterfugeCore.GameObjects;
using SubterfugeCore.GameObjects.Base;
using SubterfugeRemake.Shared.Content.Game.Graphics.GameObjects;

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
