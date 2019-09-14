using Microsoft.Xna.Framework;

namespace SubterfugeCore.GameObjects.Base
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

        public Vector2 getPosition()
        {
            return this.position;
        }

        public void setPosition(Vector2 newPosition)
        {
            this.position = newPosition;
        }

    }
}
