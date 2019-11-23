using System.Collections.Generic;
using Microsoft.Xna.Framework;
using SubterfugeCore.Components;
using SubterfugeCore.Components.Outpost;
using SubterfugeCore.Core.Components.Outpost;
using SubterfugeCore.Core.Entities.Specialists;
using SubterfugeCore.Core.Interfaces.Outpost;
using SubterfugeCore.Entities.Base;
using SubterfugeCore.Players;
using SubterfugeCore.Timing;

namespace SubterfugeCore.Entities
{
    public class Outpost : GameObject, IOwnable, ITargetable, IDrillerCarrier, ILaunchable, ICombatable, IShieldable
    {
        private Player outpostOwner;
        private SpecialistManager specialistManager;
        int drillerCount;

        int shields;
        bool shieldActive;
        int shieldCapacity;
        // shield recharge rate when implemented

        public Outpost(Vector2 outpostLocation, Player outpostOwner)
        {
            this.position = outpostLocation;
            this.drillerCount = 42;
            this.outpostOwner = outpostOwner;
            this.specialistManager = new SpecialistManager(100);
            this.shieldActive = true;
            this.shieldCapacity = 10;
            this.shields = 10;
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

        public Sub launchSub(int drillerCount, ITargetable destination)
        {
            if (this.hasDrillers(drillerCount))
            {
                this.removeDrillers(drillerCount);
                Sub launchedSub = new Sub(this, destination, GameServer.timeMachine.currentTick, drillerCount, this.getOwner());
                GameServer.timeMachine.getState().addSub(launchedSub);
                return launchedSub;
            }
            return null;
        }

        public void undoLaunch(Sub sub)
        {
            this.addDrillers(sub.getDrillerCount());
            GameServer.timeMachine.getState().removeSub(sub);
        }

        public void performSpecialistPhase(ICombatable combatable)
        {
            throw new System.NotImplementedException();
        }

        public void undoSpecialstPhase(ICombatable combatable)
        {
            throw new System.NotImplementedException();
        }

        public void attack(ICombatable combatable)
        {
            throw new System.NotImplementedException();
        }

        public void undoAttack(ICombatable combatable)
        {
            throw new System.NotImplementedException();
        }

        public SpecialistManager getSpecialistManager()
        {
            return this.specialistManager;
        }

        public int getShields()
        {
            return this.shields;
        }

        public void setShields(int shieldValue)
        {
            if(shieldValue > this.shieldCapacity)
            {
                this.shields = this.shieldCapacity;
            } else
            {
                this.shields = shieldValue;
            }
        }

        public void removeShields(int shieldsToRemove)
        {
            if (this.shields - shieldsToRemove < 0)
            {
                this.shields = 0;
            }
            else
            {
                this.shields -= shieldsToRemove;
            }
        }

        public void toggleShield()
        {
            this.shieldActive = !this.shieldActive;
        }

        public bool isShieldActive()
        {
            return this.shieldActive;
        }

        public void addShield(int shields)
        {
            if(this.shields + shields > this.shieldCapacity)
            {
                this.shields = this.shieldCapacity;
            } else
            {
                this.shields += shields;
            }
        }

        public int getShieldCapacity()
        {
            return this.shieldCapacity;
        }

        public void setShieldCapacity(int capactiy)
        {
            this.shieldCapacity = capactiy;
        }
    }
}
