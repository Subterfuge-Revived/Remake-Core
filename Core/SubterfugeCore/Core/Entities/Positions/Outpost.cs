using System;
using System.Collections.Generic;
using System.Numerics;
using GameEventModels;
using SubterfugeCore.Core.Entities.Managers;
using SubterfugeCore.Core.Entities.Specialists;
using SubterfugeCore.Core.GameEvents;
using SubterfugeCore.Core.GameEvents.Base;
using SubterfugeCore.Core.Interfaces;
using SubterfugeCore.Core.Players;
using SubterfugeCore.Core.Timing;
using SubterfugeCore.Core.Topologies;

namespace SubterfugeCore.Core.Entities.Positions
{
	/// <summary>
	/// Outpost class
	/// </summary>
    public abstract class Outpost : IOwnable, ITargetable, IDrillerCarrier, ISubLauncher, ICombatable, IShieldable, IVision
    {
        /// <summary>
        /// A unique identifier for the outpost
        /// </summary>
        private string _id;

        /// <summary>
        /// A unique name that identifies the outpost.
        /// </summary>
        public string Name { get; set; } = "Undefined";

        /// <summary>
        /// Is the outpost destroyed. WARNING: any modifications to this variable should be accomplished via GameState.GetCombatableById in case the object in the game state has changed.
        /// </summary>
        private bool _isDestroyed;

        /// <summary>
        /// The owner of the outpost
        /// </summary>
        private Player _outpostOwner;
        
        /// <summary>
        /// The outposts's specialist manager
        /// </summary>
        private SpecialistManager _specialistManager;

        /// <summary>
        /// The shield manager for the outpost.
        /// </summary>
        private ShieldManager _shieldManager;

        /// <summary>
        /// Manages raw driller counts.
        /// </summary>
        private SubLauncher _subLauncher;

        private RftVector Position;

        /// <summary>
        /// Outpost constructor
        /// </summary>
        /// <param name="outpostStartPosition">The position of the outpost</param>
        public Outpost(string id, RftVector outpostStartPosition)
        {
            this._id = id;
            this._isDestroyed = false;
            this.Position = outpostStartPosition;
            _subLauncher = new SubLauncher();
            this._outpostOwner = null;
            this._specialistManager = new SpecialistManager(100);
            _shieldManager = new ShieldManager(10);
		}

        /// <summary>
        /// Outpost constructor
        /// </summary>
        /// <param name="outpostStartPosition">The outpost position</param>
        /// <param name="outpostOwner">The outpost's owner</param>
        public Outpost(string id, RftVector outpostStartPosition, Player outpostOwner)
        {
            this._id = id;
            this._isDestroyed = false;
            this.Position = outpostStartPosition;
            _subLauncher = outpostOwner == null ? new SubLauncher() : new SubLauncher(40);
            this._outpostOwner = outpostOwner;
            this._specialistManager = new SpecialistManager(100);
            _shieldManager = new ShieldManager(10);
        }

        /// <summary>
        /// Outpost constructor; should only be used when it is necessary to change the type of an outpost (i.e. when converting an outpost to a mine)
        /// </summary>
        /// <param name="id">The id of the outpost</param>
        /// <param name="name">The name of the outpost</param>
        /// <param name="isDestroyed">Whether the outpost is destroyed</param>
        /// <param name="owner">The owner of the outpost</param>
        /// <param name="specialists">The SpecialistManger for an outpost</param>
        /// <param name="shields">The ShieldManager for an outpost</param>
        /// <param name="drillers">The SubLauncher for an outpost</param>
        /// <param name="pos">The position of an outpost</param>
        public Outpost(Outpost o)
        {
            this._id = o.GetId();
            this.Name = o.Name;
            this._isDestroyed = o.IsDestroyed();
            this._outpostOwner = o.GetOwner();
            this._specialistManager = o.GetSpecialistManager();
            this._shieldManager = o.GetShieldManager();
            this._subLauncher = o.GetSubLauncher();
            this.Position = o.GetCurrentPosition();
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
        /// Returns if the outpost is destroyed
        /// </summary>
        /// <returns>True if the outpost is destroyed, and false otherwise</returns>
        public bool IsDestroyed()
        {
            return _isDestroyed;
        }
        
        /// <summary>
        /// Destroys the outpost
        /// </summary>
        public void destroyOutpost()
        {
            this._isDestroyed = true;
        }

        /// <summary>
        /// Restores (undestroys) the outpost
        /// </summary>
        public void restoreOutpost()
        {
            this._isDestroyed = false;
        }

        /// <summary>
        /// The combat position if this object is targeted.
        /// </summary>
        /// <param name="targetFrom">The position being targeted from</param>
        /// <param name="chaserSpeed">The speed of the attacker</param>
        /// <returns>The combat position</returns>
        public RftVector GetInterceptionPosition(RftVector targetFrom, float chaserSpeed)
        {
            return Position;
        }

        /// <summary>
        /// The current position of the outpost
        /// </summary>
        /// <returns>The current position of the outpost</returns>
        public RftVector GetCurrentPosition()
        {
            return Position;
        }

        /// <summary>
        /// Gets the specialist manager for the outpost
        /// </summary>
        /// <returns>the specialist manager</returns>
        public SpecialistManager GetSpecialistManager()
        {
            return this._specialistManager;
        }

        public ShieldManager GetShieldManager()
        {
            return _shieldManager;
        }

        /// <summary>
        /// Gets the globally unique indentifier for the Outpost.
        /// </summary>
        /// <returns>The Outpost's Guid</returns>
        public string GetId()
        {
            return this._id;
        }

        public float GetSpeed()
        {
            return 0f;
        }

        public Vector2 GetDirectionNormalized()
        {
            return new Vector2(0, 0);
        }

        RftVector ITargetable.GetDirection()
        {
            return new RftVector(0, 0);
        }

        public GameTick GetExpectedArrival()
        {
            return null;
        }

        public RftVector GetCurrentPosition(GameTick currentTick)
        {
            return Position;
        }

        public Sub LaunchSub(GameState state, LaunchEvent launchEventData)
        {
            LaunchEventData eventData = launchEventData.GetEventData();
            ICombatable destination = state.GetCombatableById(eventData.DestinationId);
            
            if (destination != null && this.HasDrillers(eventData.DrillerCount))
            {
                this.RemoveDrillers(eventData.DrillerCount);
                Sub launchedSub = new Sub(this._id, this, destination, state.GetCurrentTick(), eventData.DrillerCount, this.GetOwner());
                state.AddSub(launchedSub);
                return launchedSub;
            }
            return null;
        }

        public void UndoLaunch(GameState state, LaunchEvent launchEventData)
        {
            // Determine any specialist effects if a specialist left the sub.
            LaunchEventData launchData = launchEventData.GetEventData();

            if (launchEventData.GetActiveSub() != null && launchEventData.GetActiveSub() is Sub)
            {
                Sub launchedSub = launchEventData.GetActiveSub() as Sub;
                this.AddDrillers(launchData.DrillerCount);
                launchedSub.GetSpecialistManager().transferSpecialistsTo(this._specialistManager);
                state.RemoveSub(launchEventData.GetActiveSub() as Sub);
            }
        }

        public SubLauncher GetSubLauncher()
        {
            return _subLauncher;
        }

        
        // Driller Carrier Interface
        public int GetDrillerCount()
        {
            return _subLauncher.GetDrillerCount();
        }

        public void SetDrillerCount(int drillerCount)
        {
            _subLauncher.SetDrillerCount(drillerCount);
        }

        public void AddDrillers(int drillersToAdd)
        {
            _subLauncher.AddDrillers(drillersToAdd);
        }

        public void RemoveDrillers(int drillersToRemove)
        {
            _subLauncher.RemoveDrillers(drillersToRemove);
        }

        public bool HasDrillers(int drillers)
        {
            return _subLauncher.HasDrillers(drillers);
        }

        public bool isInVisionRange(GameTick tick, IPosition position)
        {
            return Vector2.Distance(this.GetCurrentPosition(tick).ToVector2(), position.GetCurrentPosition(tick).ToVector2()) <= GetVisionRange();
        }

        // abstract methods

        public abstract float GetVisionRange();

        /// <summary>
        /// Gets the outpost type
        /// </summary>
        /// <returns>The type of outpost</returns>
        public abstract OutpostType GetOutpostType();
    }
}
