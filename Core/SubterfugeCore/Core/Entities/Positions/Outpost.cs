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
    public class Outpost : IOwnable, ITargetable, IDrillerCarrier, ISubLauncher, ICombatable, IShieldable, ILocalEventWatcher
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
        /// The owner of the outpost
        /// </summary>
        private Player _outpostOwner;
        
        /// <summary>
        /// The outposts's specialist manager
        /// </summary>
        private SpecialistManager _specialistManager;

        /// <summary>
        /// The outpost type
        /// </summary>
        OutpostType _type;

        /// <summary>
        /// The shield manager for the outpost.
        /// </summary>
        private ShieldManager _shieldManager;

        /// <summary>
        /// Manages raw driller counts.
        /// </summary>
        private DrillerManager _drillerManager;

        private RftVector Position;
        
        /// <summary>
        /// A queue holding the game events this object will observe during its lifetime.
        /// </summary>
        private PriorityQueue<GameEvent> gameEvents;

        /// <summary>
        /// Outpost constructor
        /// </summary>
        /// <param name="outpostStartPosition">The position of the outpost</param>
        public Outpost(string id, RftVector outpostStartPosition)
        {
            this._id = id;
            this.Position = outpostStartPosition;
            _drillerManager = new DrillerManager();
            this._outpostOwner = null;
            this._specialistManager = new SpecialistManager(100);
            _shieldManager = new ShieldManager(10);
        }

        /// <summary>
        /// Outpost constructor
        /// </summary>
        /// <param name="outpostStartPosition">The outpost position</param>
        /// <param name="type">The type of outpost to create</param>
        public Outpost(string id, RftVector outpostStartPosition, OutpostType type)
        {
            this._id = id;
            this.Position = outpostStartPosition;
            _drillerManager = new DrillerManager();
            this._outpostOwner = null;
            this._specialistManager = new SpecialistManager(100);
            _shieldManager = new ShieldManager(10);
            this._type = type;
        }

        /// <summary>
        /// Outpost constructor
        /// </summary>
        /// <param name="outpostStartPosition">The outpost position</param>
        /// <param name="outpostOwner">The outpost's owner</param>
        /// <param name="type">The type of outpost to create</param>
        public Outpost(string id, RftVector outpostStartPosition, Player outpostOwner, OutpostType type)
        {
            this._id = id;
            this.Position = outpostStartPosition;
            _drillerManager = outpostOwner == null ? new DrillerManager() : new DrillerManager(40);
            this._outpostOwner = outpostOwner;
            this._specialistManager = new SpecialistManager(100);
            _shieldManager = new ShieldManager(10);
            this._type = type;
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

        public ICombatable LaunchSub(GameState state, LaunchEvent launchEventData)
        {
            return launchEventData.ForwardAction(state);
            LaunchEventData eventData = launchEventData.GetEventData();
            ICombatable destination = state.GetCombatableById(eventData.DestinationId);
            
            if (destination != null && this.HasDrillers(eventData.DrillerCount))
            {
                this.RemoveDrillers(eventData.DrillerCount);
                Sub launchedSub = new Sub(this._id, this, destination, state.GetCurrentTick(), eventData.DrillerCount, this.GetOwner());
                launchEventData.ForwardAction(state);
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

        
        // Driller Carrier Interface
        public int GetDrillerCount()
        {
            return _drillerManager.GetDrillerCount();
        }

        public void SetDrillerCount(int drillerCount)
        {
            _drillerManager.SetDrillerCount(drillerCount);
        }

        public void AddDrillers(int drillersToAdd)
        {
            _drillerManager.AddDrillers(drillersToAdd);
        }

        public void RemoveDrillers(int drillersToRemove)
        {
            _drillerManager.RemoveDrillers(drillersToRemove);
        }

        public bool HasDrillers(int drillers)
        {
            return _drillerManager.HasDrillers(drillers);
        }

        public GameEvent GetNextEvent()
        {
            return this.gameEvents.Peek();
        }

        public List<GameEvent> GetEvents()
        {
            return this.gameEvents.GetQueue();
        }
    }
}
