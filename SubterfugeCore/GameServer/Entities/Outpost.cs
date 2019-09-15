using Microsoft.Xna.Framework;
using SubterfugeCore.Components;
using SubterfugeCore.Entities.Base;
using SubterfugeCore.Players;

namespace SubterfugeCore.Entities
{
    public class Outpost : GameObject, SubterfugeCore.Components.IOwnable
    {
        private Player outpostOwner;

        public Outpost(Vector2 outpostLocation)
        {
            this.position = outpostLocation;
        }

        public Vector2 getLocation()
        {
            return this.position;
        }

        public void setOwner(Player newOwner)
        {
            this.outpostOwner = newOwner;
        }

        Player IOwnable.getOwner()
        {
            return this.outpostOwner;
        }
    }
}
