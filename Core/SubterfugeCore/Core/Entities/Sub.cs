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

namespace SubterfugeCore.Core.Entities
{
    /// <summary>
    /// An instance of a Sub
    /// </summary>
    public class Sub : ICombatable, ILocalEventWatcher
    {
        /// <summary>
        /// Unique identifier for each sub
        /// </summary>
        private string _id;

        /// <summary>
        /// How many drillers are on the sub
        /// </summary>
        private DrillerManager _drillerManager;
        
        /// <summary>
        /// Where the sub is travelling from
        /// </summary>
        private ISubLauncher _source;
        
        /// <summary>
        /// Where the sub is travelling to
        /// </summary>
        private ITargetable _destination;
        
        /// <summary>
        /// The time the sub launched from its source
        /// </summary>
        private GameTick _launchTime;
        
        /// <summary>
        /// The sub's speed
        /// </summary>
        private float _speed = 1f;
        
        /// <summary>
        /// The owner of the sub
        /// </summary>
        private Player _owner;
        
        /// <summary>
        /// Specialist manager for the sub
        /// </summary>
        private SpecialistManager _specialistManager;
        
        /// <summary>
        /// The shield manager for the outpost.
        /// </summary>
        private ShieldManager _shieldManager;

        /// <summary>
        /// If the sub is captured.
        /// </summary>
        public bool IsCaptured { get; set; } = false;
        
        /// <summary>
        /// The sub's initial position.
        /// </summary>
        private RftVector StartPosition;

        /// <summary>
        /// A queue holding the game events this object will observe during its lifetime.
        /// </summary>
        private PriorityQueue<GameEvent> gameEvents;

        /// <summary>
        /// Sub constructor
        /// </summary>
        /// <param name="source">The initial location of the sub</param>
        /// <param name="destination">The destination of the sub</param>
        /// <param name="launchTime">The time of launch</param>
        /// <param name="drillerCount">The amount of drillers to launch</param>
        /// <param name="owner">The owner</param>
        public Sub(string id, ISubLauncher source, ITargetable destination, GameTick launchTime, int drillerCount, Player owner) : base()
        {
            this._id = id;
            this._source = source;
            this._destination = destination;
            this._launchTime = launchTime;
            this._drillerManager = new DrillerManager(drillerCount);
            this.StartPosition = source.GetCurrentPosition(launchTime);
            this._owner = owner;
            this._specialistManager = new SpecialistManager(3);
            _shieldManager = new ShieldManager(5);
        }

        /// <summary>
        /// Gets the specialist manager of the sub
        /// </summary>
        /// <returns>The specialist manager</returns>
        public SpecialistManager GetSpecialistManager()
        {
            return this._specialistManager;
        }

        /// <summary>
        /// Returns the sub's owner
        /// </summary>
        /// <returns>The sub's owner</returns>
        public Player GetOwner()
        {
            return this._owner;
        }

        /// <summary>
        /// Gets the rotation of the sub
        /// </summary>
        /// <returns>Gets the angle of the sub's path</returns>
        public double GetRotationRadians()
        {
            // Determine direction vector
            RftVector direction = GetDirection();
            return Math.Atan2(direction.Y, direction.X);
        }
        
        /// <summary>
        /// Gets the speed of the sub
        /// </summary>
        /// <returns>The sub's speed</returns>
        public float GetSpeed()
        {
            return this._speed;
        }

        /// <summary>
        /// Gets the initial position of the sub
        /// </summary>
        /// <returns>Gets the subs initial position</returns>
        public RftVector GetInitialPosition()
        {
            return this.StartPosition;
        }

        /// <summary>
        /// Gets the position of the sub's destination
        /// </summary>
        /// <returns>The destination of the sub</returns>
        public RftVector GetDestinationLocation()
        {
            return this._destination.GetInterceptionPosition(this.StartPosition, this.GetSpeed());
        }
        
        /// <summary>
        /// Get the destination
        /// </summary>
        /// <returns>The sub's destination</returns>
        public ITargetable GetDestination()
        {
            return this._destination;
        }

        /// <summary>
        /// Get the tick the sub launched
        /// </summary>
        /// <returns>The tick the sub launched</returns>
        public GameTick GetLaunchTick()
        {
            return this._launchTime;
        }

        /// <summary>
        /// Get the combat location if this object is targeted by another
        /// </summary>
        /// <param name="targetFrom">The location that this sub is being targeted from</param>
        /// <param name="chaserSpeed">The speed this is being targeted by</param>
        /// <returns>The location of combat</returns>
		public RftVector GetInterceptionPosition(RftVector targetFrom, float chaserSpeed)
        {
            // https://stackoverflow.com/questions/37250215/intersection-of-two-moving-objects-with-latitude-longitude-coordinates
            
            var distanceBetweenTargets = (StartPosition - targetFrom).Magnitude();
            var directionBetweenTargets = Math.Atan2(StartPosition.Y - targetFrom.Y, StartPosition.X - targetFrom.X);
            var alpha = Math.PI + directionBetweenTargets - Math.Atan2(this.GetDirection().Y, this.GetDirection().X);

            if (chaserSpeed == _speed)
            {
                if (Math.Cos(alpha) < 0)
                {
                    return null;
                }
                var radians = (directionBetweenTargets + alpha) % (Math.PI * 2);
                return new RftVector((float)(Math.Cos(radians) * distanceBetweenTargets), (float)(Math.Sin(radians) * distanceBetweenTargets));
            }

            // Solve with quadratic formula
            var a = Math.Pow(chaserSpeed, 2) - Math.Pow(GetSpeed(), 2);
            var b = 2 * distanceBetweenTargets * GetSpeed() * Math.Cos(alpha);
            var c = -Math.Pow(distanceBetweenTargets, 2);

            var discriminant = Math.Pow(b, 2) - (4 * a * c);
            if (discriminant < 0)
                return null;

            var time = (Math.Sqrt(discriminant) - b) / (2 * a);
            var x = StartPosition.X +
                    (GetSpeed() * time) * Math.Cos(Math.Atan2(this.GetDirection().Y, this.GetDirection().X));
            var y = StartPosition.Y +
                    (GetSpeed() * time) * Math.Sin(Math.Atan2(this.GetDirection().Y, this.GetDirection().X));
            
            return new RftVector((float)x, (float)y);
            
		    // if (targetFrom == this.StartPosition)
            //     return targetFrom;
            //
            // if (Math.Abs(chaserSpeed) < 0.001)
            //     return targetFrom;
            //
            // // Determine target's distance to travel to destination:
            // RftVector targetDestination = this.GetDestinationLocation();
            //
            // // Check if the chaser can get there before it.
            // RftVector chaserDestination = targetDestination - targetFrom;
            //
            // float ticksToDestination = targetDestination.Magnitude()/ this.GetSpeed();
            // float chaserTicksToDestination = chaserDestination.Magnitude() / chaserSpeed;
            //
            // if (ticksToDestination > chaserTicksToDestination)
            // {
            //     // Can intercept.
            //     // Determine interception point.
            //
            //     int scalar = 1;
            //     bool searching = true;
            //     while (searching)
            //     {
            //         Vector2 destination = (this.GetDestinationLocation() - this.GetCurrentPosition()).Normalize();
            //
            //         RftVector runnerLocation = this.GetCurrentPosition() + new RftVector(RftVector.Map, (destination * scalar));
            //         Vector2 chaserDirection = (runnerLocation - targetFrom).Normalize();
            //         RftVector chaserPosition = targetFrom + new RftVector(RftVector.Map, (chaserDirection * scalar));
            //
            //         if((chaserPosition - runnerLocation).Magnitude() < 1)
            //         {
            //             return chaserPosition;
            //         }
            //         if(runnerLocation.Magnitude() > this.GetDestinationLocation().Magnitude())
            //         {
            //             return targetFrom;
            //         }
            //         scalar++;
            //     }
            //     return targetFrom;
            // }
            // return targetFrom;
		}
        
        /// <summary>
        /// Set the owner
        /// </summary>
        /// <param name="newOwner">The new owner</param>

        public void SetOwner(Player newOwner)
        {
            this._owner = newOwner;
        }

        /// <summary>
        /// The subs direction
        /// </summary>
        /// <returns>The sub's direction vector</returns>
        public Vector2 GetDirectionNormalized()
        {
            return (this.GetDestinationLocation() - this.StartPosition).Normalize();
        }
        
        /// <summary>
        /// The subs direction
        /// </summary>
        /// <returns>The sub's direction vector</returns>
        public RftVector GetDirection()
        {
            return (this.GetDestinationLocation() - this.StartPosition);
        }

        public GameTick GetExpectedArrival()
        {
            RftVector direction = this.GetDirection();
            int ticksToArrive = (int)Math.Floor(direction.Magnitude() / this.GetSpeed());
            return this._launchTime.Advance(ticksToArrive);
        }

        /// <summary>
        /// Get the launch source
        /// </summary>
        /// <returns>The source of the sub launch</returns>
        public ISubLauncher GetSource()
        {
            return this._source;
        }

        /// <summary>
        /// Undoes launching a sub from this object
        /// </summary>
        /// <param name="sub"></param>
        public void UndoLaunch(ICombatable sub)
        {
            return;
        }
        
        /// <summary>
        /// Gets the globally unique indentifier for the Sub.
        /// </summary>
        /// <returns>The Sub's Guid</returns>
        public string GetId()
        {
            return this._id;
        }

        public ICombatable LaunchSub(GameState state, LaunchEvent launchEvent)
        {
            // Determine any specialist effects if a specialist left the sub.
            LaunchEventData launchData = launchEvent.GetEventData();
            ICombatable destination = state.GetCombatableById(launchData.DestinationId);

            if (destination != null && this.HasDrillers(launchData.DrillerCount))
            {
                this.RemoveDrillers(launchData.DrillerCount);
                Sub launchedSub = new Sub(launchEvent.GetEventId(), this, destination, state.GetCurrentTick(), launchData.DrillerCount, this._owner);
                state.AddSub(launchedSub);
                return launchedSub;
            }

            return null;
        }

        public void UndoLaunch(GameState state, LaunchEvent launchEvent)
        {
            // Determine any specialist effects if a specialist left the sub.
            LaunchEventData launchData = launchEvent.GetEventData();
            ICombatable destination = state.GetCombatableById(launchData.DestinationId);

            if (launchEvent.GetActiveSub() != null && launchEvent.GetActiveSub() is Sub)
            {
                Sub launchedSub = launchEvent.GetActiveSub() as Sub;
                this.AddDrillers(launchData.DrillerCount);
                launchedSub.GetSpecialistManager().transferSpecialistsTo(this._specialistManager);
                state.RemoveSub(launchEvent.GetActiveSub() as Sub);
            }
        }

        public ShieldManager GetShieldManager()
        {
            return this._shieldManager;
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

        public RftVector GetCurrentPosition(GameTick currentTick)
        {
            // Determine number of ticks after launch:
            int elapsedTicks = currentTick - this._launchTime;

            // Determine direction vector
            Vector2 direction = (this._destination.GetInterceptionPosition(this.StartPosition, this.GetSpeed()) - this._source.GetCurrentPosition(currentTick)).Normalize();

            if(elapsedTicks > 0)
            {
                this.StartPosition = this._source.GetCurrentPosition(currentTick) + new RftVector(RftVector.Map, (direction * (elapsedTicks * this.GetSpeed())));
                return this.StartPosition;
            }
            else
            {
                return new RftVector(RftVector.Map);
            }
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
