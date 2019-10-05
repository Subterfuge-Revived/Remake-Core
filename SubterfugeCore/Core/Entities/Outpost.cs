using Microsoft.Xna.Framework;
using SubterfugeCore.Components;
using SubterfugeCore.Components.Outpost;
using SubterfugeCore.Core.Components.Outpost;
using SubterfugeCore.Entities.Base;
using SubterfugeCore.Players;
using SubterfugeCore.Timing;

namespace SubterfugeCore.Entities
{
    public class Outpost : GameObject, IOwnable, ITargetable, IHasDrillers
    {
        private Player outpostOwner;
        int drillerCount;

        public Outpost(Vector2 outpostLocation, Player outpostOwner)
        {
            this.position = outpostLocation;
            this.drillerCount = 42;
            this.outpostOwner = outpostOwner;
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

        public Vector2 getCurrentLocation()
        {
            return this.getPosition();
        }

        public int getDrillerCount()
        {
            return this.drillerCount;
        }

        public void setDrillerCount(int drillerCount)
        {
            this.drillerCount = drillerCount;
        }

        public bool hasDrillers(int drillers)
        {
            return this.drillerCount >= drillers;
        }
    }
}
