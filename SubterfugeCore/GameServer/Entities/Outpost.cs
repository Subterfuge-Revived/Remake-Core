using Microsoft.Xna.Framework;
using SubterfugeCore.Components;
using SubterfugeCore.Components.Outpost;
using SubterfugeCore.Entities.Base;
using SubterfugeCore.Players;
using SubterfugeCore.Timing;

namespace SubterfugeCore.Entities
{
    public class Outpost : GameObject, Components.IOwnable, Components.Outpost.ITargetable
    {
        private Player outpostOwner;
        int drillerCount;

        public Outpost(Vector2 outpostLocation)
        {
            this.position = outpostLocation;
            this.drillerCount = 42;
        }

        public override Vector2 getPosition()
        {
            return this.position;
        }

        public void setOwner(Player newOwner)
        {
            this.outpostOwner = newOwner;
        }

        public Player getOwner()
        {
            return this.outpostOwner;
        }

        public int getDrillersAtOutpost()
        {
            return this.drillerCount;
        }

        public bool hasDrillers(int drillers)
        {
            return this.drillerCount >= drillers;
        }

        public void addDrillers(int drillers)
        {
            this.drillerCount += drillers;
        }
        public void removeDrillers(int drillers)
        {
            this.drillerCount -= drillers;
        }

        public Vector2 getTargetLocation(Vector2 targetFrom, double speed)
        {
            return this.getPosition();
        }
    }
}
