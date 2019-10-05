using Microsoft.Xna.Framework;
using SubterfugeCore.Timing;

namespace SubterfugeCore.Entities.Base
{
    public abstract class GameObject
    {
        protected Vector2 position;
        public GameObject()
        {

        }

        public GameObject(Vector2 position)
        {
            this.position = position;
        }

        public abstract Vector2 getPosition();
    }
}
