using Microsoft.Xna.Framework;
using SubterfugeCore.Shared.Content.Game.Objects.Base;

namespace SubterfugeCore.Shared.Content.Game.Objects
{
    class Sub : GameObject
    {
        private int drillerCount = 42;

        public Sub() : base()
        {
            this.position = new Vector2(0, 0);
        }

        public int getDrillerCount()
        {
            return this.drillerCount;
        }
    }
}
