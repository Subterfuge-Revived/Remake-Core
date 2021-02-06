using System.Numerics;
using SubterfugeCore.Core.Interfaces;
using SubterfugeCore.Core.Topologies;

namespace SubterfugeCore.Core.Entities.Base
{
    public abstract class GameObject
    {
        protected RftVector Position;
        public GameObject()
        {

        }

        public GameObject(RftVector position)
        {
            this.Position = position;
        }

        public abstract RftVector GetPosition();
    }
}
