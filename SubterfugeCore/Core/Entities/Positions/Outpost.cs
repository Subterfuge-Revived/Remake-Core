using System;
using System.Numerics;
using SubterfugeCore.Core.Entities.Base;
using SubterfugeCore.Core.Entities.Specialists;
using SubterfugeCore.Core.Generation;
using SubterfugeCore.Core.Interfaces;
using SubterfugeCore.Core.Players;
using SubterfugeCore.Core.Topologies;

namespace SubterfugeCore.Core.Entities.Positions
{
	/// <summary>
	/// Outpost class
	/// </summary>
    public class Outpost : GameObject, IOwnable, ITargetable, IDrillerCarrier, ILaunchable, ICombatable, IShieldable
    {
        /// <summary>
        /// A unique identifier for the outpost
        /// </summary>
        private int _id;
        
        /// <summary>
        /// The owner of the outpost
        /// </summary>
        private Player _outpostOwner;
        
        /// <summary>
        /// The outposts's specialist manager
        /// </summary>
        private SpecialistManager _specialistManager;
        
        /// <summary>
        /// The number of drillers at the outpost
        /// </summary>
        int _drillerCount;
        
        /// <summary>
        /// The outpost type
        /// </summary>
        OutpostType _type;

        private ShieldManager _shieldManager; 

        /// <summary>
        /// Outpost constructor
        /// </summary>
        /// <param name="outpostPosition">The position of the outpost</param>
        public Outpost(RftVector outpostPosition)
        {
            this._id = IdGenerator.GetNextId();
            this.Position = outpostPosition;
            this._drillerCount = 0;
            this._outpostOwner = null;
            this._specialistManager = new SpecialistManager(100);
            this._shieldManager = new ShieldManager();

        }

        /// <summary>
        /// Outpost constructor
        /// </summary>
        /// <param name="outpostPosition">The outpost position</param>
        /// <param name="type">The type of outpost to create</param>
        public Outpost(RftVector outpostPosition, OutpostType type)
        {
            this._id = IdGenerator.GetNextId();
            this.Position = outpostPosition;
            this._drillerCount = 0;
            this._outpostOwner = null;
            this._specialistManager = new SpecialistManager(100);
            this._shieldManager = new ShieldManager();
            this._type = type;
        }

        /// <summary>
        /// Outpost constructor
        /// </summary>
        /// <param name="outpostPosition">The outpost position</param>
        /// <param name="outpostOwner">The outpost's owner</param>
        /// <param name="type">The type of outpost to create</param>
        public Outpost(RftVector outpostPosition, Player outpostOwner, OutpostType type)
        {
            this._id = IdGenerator.GetNextId();
            this.Position = outpostPosition;
            this._drillerCount = outpostOwner == null ? 0 : 40;
            this._outpostOwner = outpostOwner;
            this._specialistManager = new SpecialistManager(100);
            this._shieldManager = new ShieldManager();
            this._type = type;
        }

        /// <summary>
        /// The position of the outpost
        /// </summary>
        /// <returns>The outpost position</returns>
        public override RftVector GetPosition()
        {
            return this.Position;
        }

        /// <summary>
        /// Set the outpost owner
        /// </summary>
        /// <param name="newOwner">The owner of the outpost</param>
        public void SetOwner(Player newOwner)
        {
            this._outpostOwner = newOwner;
        }

        /// <summary>
        /// Gets the owner of the outpost
        /// </summary>
        /// <returns>The outpost owner</returns>
        public Player GetOwner()
        {
            return this._outpostOwner;
        }

        /// <summary>
        /// Adds drillers to the outpost
        /// </summary>
        /// <param name="drillers">The number of drillers to add to the outpost</param>
        public void AddDrillers(int drillers)
        {
            this._drillerCount += drillers;
        }
        
        /// <summary>
        /// Remove drillers from the outpost
        /// </summary>
        /// <param name="drillers">The number of drillers to remove</param>
        public void RemoveDrillers(int drillers)
        {
            this._drillerCount -= drillers;
        }

        /// <summary>
        /// The combat position if this object is targeted.
        /// </summary>
        /// <param name="targetFrom">The position being targeted from</param>
        /// <param name="speed">The speed of the attacker</param>
        /// <returns>The combat position</returns>
        public RftVector GetInterceptionPoint(RftVector targetFrom, float speed)
        {
            return this.GetPosition();
        }

        /// <summary>
        /// The current position of the outpost
        /// </summary>
        /// <returns>The current position of the outpost</returns>
        public RftVector GetCurrentPosition()
        {
            return this.GetPosition();
        }

        
        /// <summary>
        /// Get the number of drillers at the position
        /// </summary>
        /// <returns>The number of drillers at the outpost</returns>
        public int GetDrillerCount()
        {
            return this._drillerCount;
        }

        /// <summary>
        /// Set the number of drillers
        /// </summary>
        /// <param name="drillerCount">The number of drillers to set</param>
        public void SetDrillerCount(int drillerCount)
        {
            this._drillerCount = drillerCount;
        }

        /// <summary>
        /// Checks if the outpost has the drillers specified
        /// </summary>
        /// <param name="drillers">The number of drillers to check for</param>
        /// <returns>if the outpost has the drillers</returns>
        public bool HasDrillers(int drillers)
        {
            return this._drillerCount >= drillers;
        }

        /// <summary>
        /// Launches a sub from this position
        /// </summary>
        /// <param name="drillerCount">The number of drillers to launch</param>
        /// <param name="destination">The destination</param>
        /// <returns>A reference to the launched sub</returns>
        public Sub LaunchSub(int drillerCount, ITargetable destination)
        {
            if (this.HasDrillers(drillerCount))
            {
                this.RemoveDrillers(drillerCount);
                Sub launchedSub = new Sub(this, destination, Game.TimeMachine.CurrentTick, drillerCount, this.GetOwner());
                Game.TimeMachine.GetState().AddSub(launchedSub);
                return launchedSub;
            }
            return null;
        }

        /// <summary>
        /// Undoes a sub launch
        /// </summary>
        /// <param name="sub"> The sub to undo</param>
        public void UndoLaunch(Sub sub)
        {
            this.AddDrillers(sub.GetDrillerCount());
            Game.TimeMachine.GetState().RemoveSub(sub);
        }

        /// <summary>
        /// Gets the specialist manager for the outpost
        /// </summary>
        /// <returns>the specialist manager</returns>
        public SpecialistManager GetSpecialistManager()
        {
            return this._specialistManager;
        }

        /// <summary>
        /// Gets the shield manager for the outpost.
        /// </summary>
        /// <returns>The shield manager.</returns>
        public ShieldManager GetShieldManager()
        {
            return this._shieldManager;
        }

        /// <summary>
        /// Gets the outpost type
        /// </summary>
        /// <returns>The type of outpost</returns>
        public OutpostType GetOutpostType()
        {
            return this._type;
        }

        /// <summary>
        /// Gets the globally unique indentifier for the Outpost.
        /// </summary>
        /// <returns>The Outpost's Guid</returns>
        public int GetId()
        {
            return this._id;
        }
    }
}
