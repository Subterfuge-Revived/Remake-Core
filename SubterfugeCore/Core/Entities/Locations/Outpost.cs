using System;
using System.Numerics;
using SubterfugeCore.Core.Entities.Base;
using SubterfugeCore.Core.Entities.Specialists;
using SubterfugeCore.Core.Interfaces;
using SubterfugeCore.Core.Players;

namespace SubterfugeCore.Core.Entities.Locations
{
    /// <summary>
    /// Outpost class
    /// </summary>
    public class Outpost : GameObject, IOwnable, ITargetable, IDrillerCarrier, ILaunchable, ICombatable, IShieldable
    {
        private Guid guid;
        private Player outpostOwner;
        private SpecialistManager specialistManager;
        int drillerCount;
        OutpostType type;

        int shields;
        bool shieldActive;
        int shieldCapacity;
        // shield recharge rate when implemented

        /// <summary>
        /// Outpost constructor
        /// </summary>
        /// <param name="outpostLocation">The location of the outpost</param>
        public Outpost(Vector2 outpostLocation)
        {
            this.guid = Guid.NewGuid();
            this.position = outpostLocation;
            this.drillerCount = 0;
            this.outpostOwner = null;
            this.specialistManager = new SpecialistManager(100);
            this.shieldActive = true;
            this.shieldCapacity = 10;
            this.shields = 0;
        }

        /// <summary>
        /// Outpost constructor
        /// </summary>
        /// <param name="outpostLocation">The outpost location</param>
        /// <param name="type">The type of outpost to create</param>
        public Outpost(Vector2 outpostLocation, OutpostType type)
        {
            this.guid = Guid.NewGuid();
            this.position = outpostLocation;
            this.drillerCount = 0;
            this.outpostOwner = null;
            this.specialistManager = new SpecialistManager(100);
            this.shieldActive = true;
            this.shieldCapacity = 10;
            this.shields = 0;
            this.type = type;
        }

        /// <summary>
        /// Outpost constructor
        /// </summary>
        /// <param name="outpostLocation">The outpost location</param>
        /// <param name="outpostOwner">The outpost's owner</param>
        /// <param name="type">The type of outpost to create</param>
        public Outpost(Vector2 outpostLocation, Player outpostOwner, OutpostType type)
        {
            this.guid = Guid.NewGuid();
            this.position = outpostLocation;
            this.drillerCount = outpostOwner == null ? 0 : 40;
            this.outpostOwner = outpostOwner;
            this.specialistManager = new SpecialistManager(100);
            this.shieldActive = true;
            this.shieldCapacity = 10;
            this.shields = 0;
            this.type = type;
        }

        /// <summary>
        /// The position of the outpost
        /// </summary>
        /// <returns>The outpost position</returns>
        public override Vector2 getPosition()
        {
            return this.position;
        }

        /// <summary>
        /// Set the outpost owner
        /// </summary>
        /// <param name="newOwner">The owner of the outpost</param>
        public void setOwner(Player newOwner)
        {
            this.outpostOwner = newOwner;
        }

        /// <summary>
        /// Gets the owner of the outpost
        /// </summary>
        /// <returns>The outpost owner</returns>
        public Player getOwner()
        {
            return this.outpostOwner;
        }

        /// <summary>
        /// Adds drillers to the outpost
        /// </summary>
        /// <param name="drillers">The number of drillers to add to the outpost</param>
        public void addDrillers(int drillers)
        {
            this.drillerCount += drillers;
        }
        
        /// <summary>
        /// Remove drillers from the outpost
        /// </summary>
        /// <param name="drillers">The number of drillers to remove</param>
        public void removeDrillers(int drillers)
        {
            this.drillerCount -= drillers;
        }

        /// <summary>
        /// The combat location if this object is targeted.
        /// </summary>
        /// <param name="targetFrom">The location being targeted from</param>
        /// <param name="speed">The speed of the attacker</param>
        /// <returns>The combat location</returns>
        public Vector2 getTargetLocation(Vector2 targetFrom, double speed)
        {
            return this.getPosition();
        }

        /// <summary>
        /// The current location of the outpost
        /// </summary>
        /// <returns>The current location of the outpost</returns>
        public Vector2 getCurrentLocation()
        {
            return this.getPosition();
        }

        
        /// <summary>
        /// Get the number of drillers at the location
        /// </summary>
        /// <returns>The number of drillers at the outpost</returns>
        public int getDrillerCount()
        {
            return this.drillerCount;
        }

        /// <summary>
        /// Set the number of drillers
        /// </summary>
        /// <param name="drillerCount">The number of drillers to set</param>
        public void setDrillerCount(int drillerCount)
        {
            this.drillerCount = drillerCount;
        }

        /// <summary>
        /// Checks if the outpost has the drillers specified
        /// </summary>
        /// <param name="drillers">The number of drillers to check for</param>
        /// <returns>if the outpost has the drillers</returns>
        public bool hasDrillers(int drillers)
        {
            return this.drillerCount >= drillers;
        }

        /// <summary>
        /// Launches a sub from this location
        /// </summary>
        /// <param name="drillerCount">The number of drillers to launch</param>
        /// <param name="destination">The destination</param>
        /// <returns>A reference to the launched sub</returns>
        public Sub launchSub(int drillerCount, ITargetable destination)
        {
            if (this.hasDrillers(drillerCount))
            {
                this.removeDrillers(drillerCount);
                Sub launchedSub = new Sub(this, destination, Game.timeMachine.currentTick, drillerCount, this.getOwner());
                Game.timeMachine.getState().addSub(launchedSub);
                return launchedSub;
            }
            return null;
        }

        /// <summary>
        /// Undoes a sub launch
        /// </summary>
        /// <param name="sub"> The sub to undo</param>
        public void undoLaunch(Sub sub)
        {
            this.addDrillers(sub.getDrillerCount());
            Game.timeMachine.getState().removeSub(sub);
        }

        /// <summary>
        /// Gets the specialist manager for the outpost
        /// </summary>
        /// <returns>the specialist manager</returns>
        public SpecialistManager getSpecialistManager()
        {
            return this.specialistManager;
        }

        /// <summary>
        /// Gets the sheilds at this location
        /// </summary>
        /// <returns>The number of shields at this location</returns>
        public int getShields()
        {
            return this.shields;
        }

        /// <summary>
        /// Sets the sheilds at the location
        /// </summary>
        /// <param name="shieldValue">The value of shields to set</param>
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

        /// <summary>
        /// Removes shields from the location
        /// </summary>
        /// <param name="shieldsToRemove">The number of shields to remove</param>
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

        /// <summary>
        /// Toggles shields
        /// </summary>
        public void toggleShield()
        {
            this.shieldActive = !this.shieldActive;
        }

        /// <summary>
        /// Determines if the shields are active
        /// </summary>
        /// <returns>If the shields are enabled</returns>
        public bool isShieldActive()
        {
            return this.shieldActive;
        }

        /// <summary>
        /// Adds shields to the outpost
        /// </summary>
        /// <param name="shields">The number of shields to add</param>
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

        /// <summary>
        /// Gets the shield capacity
        /// </summary>
        /// <returns>The maximum shield amount</returns>
        public int getShieldCapacity()
        {
            return this.shieldCapacity;
        }

        /// <summary>
        /// Sets the sheild capacity
        /// </summary>
        /// <param name="capactiy">The capacity of the sheilds</param>
        public void setShieldCapacity(int capactiy)
        {
            this.shieldCapacity = capactiy;
        }

        /// <summary>
        /// Gets the outpost type
        /// </summary>
        /// <returns>The type of outpost</returns>
        public OutpostType getOutpostType()
        {
            return this.type;
        }

        /// <summary>
        /// Gets the globally unique indentifier for the Outpost.
        /// </summary>
        /// <returns>The Outpost's Guid</returns>
        public Guid getGuid()
        {
            return this.guid;
        }
    }
}
