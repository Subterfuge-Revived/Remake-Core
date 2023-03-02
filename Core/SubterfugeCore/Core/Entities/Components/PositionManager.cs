using System;
using System.Collections.Generic;
using System.Linq;
using Subterfuge.Remake.Core.GameEvents.Combat;
using Subterfuge.Remake.Core.GameEvents.EventPublishers;
using Subterfuge.Remake.Core.Timing;
using Subterfuge.Remake.Core.Topologies;

namespace Subterfuge.Remake.Core.Entities.Components
{
    public class PositionManager : EntityComponent, ICombatEventPublisher
    {
        private IEntity _destination;
        private readonly IEntity _source;
        private readonly RftVector _initialLocation;
        public RftVector CurrentLocation { get; private set; }
        private readonly GameTick _startTime;
        private readonly SpeedManager _speedManager;

        private List<CombatEvent> localCombatEvents = new List<CombatEvent>();
        
        public PositionManager(
            IEntity parent,
            IEntity initialLocation,
            IEntity destination,
            GameTick startTime,
            TimeMachine timeMachine
        ) : base(parent) {
            this._destination = destination;
            this._source = initialLocation;
            this._initialLocation = _source.GetComponent<PositionManager>().CurrentLocation;
            _speedManager = parent.GetComponent<SpeedManager>();
            this._startTime = startTime;
            CurrentLocation = this._initialLocation;

            // Register to move this object on every tick.
            timeMachine.OnTick += OnTickCheck;
        }
        
        public PositionManager(
            IEntity parent,
            RftVector location,
            GameTick startTime,
            TimeMachine timeMachine
        ) : base(parent) {
            this._destination = null;
            this._source = null;
            this._initialLocation = location;
            this._startTime = startTime;
            _speedManager = parent.GetComponent<SpeedManager>();
            CurrentLocation = this._initialLocation;

            // Register to move this object on every tick.
            timeMachine.OnTick += OnTickCheck;
        }

        /// <summary>
        /// Moves the object 1 unit every tick scaled by speed.
        /// </summary>
        /// <param name="sender">The TimeMachine instance that published the event</param>
        /// <param name="onTick">On Tick event args</param>
        private void OnTickCheck(object sender, OnTickEventArgs onTick)
        {
            if (_destination != null)
            {
                RftVector direction = GetDirection().Normalize();

                if (onTick.Direction == TimeMachineDirection.FORWARD)
                {
                    this.CurrentLocation += (direction * _speedManager.GetSpeed());
                }
                else
                {
                    this.CurrentLocation -= (direction * _speedManager.GetSpeed());
                }
            }
            
            // Check for combat events.
            if (Parent is Sub)
            {
                CheckCombat(sender as TimeMachine, onTick);
            }
        }

        private void CheckCombat(TimeMachine timeMachine, OnTickEventArgs onTick)
        {
            timeMachine
                .GetState()
                .GetAllGameObjects()
                .Where(entity => entity != Parent)
                .Where(entity =>
                {
                    // If the sub has just launched, ignore it's source.
                    // Otherwise, it could be returning to the source.
                    if (onTick.CurrentTick.GetTick() - this.GetSpawnTick().GetTick() > 2)
                    {
                        return entity != _source;
                    }
                    return true;
                })
                .Where(entity =>
                    (entity.GetComponent<PositionManager>().CurrentLocation - this.CurrentLocation).Length() < Parent.GetComponent<SpeedManager>().GetSpeed())
                .ToList()
                .ForEach(entityToFight =>
                {
                    if (onTick.Direction == TimeMachineDirection.FORWARD)
                    {
                        var combat = new CombatEvent(this.Parent, entityToFight,
                            timeMachine.GetCurrentTick());
                        
                        // Allow any listenint specialists to modify the combat object.
                        OnPreCombat?.Invoke(this, new OnPreCombatEventArgs()
                        {
                            Direction = onTick.Direction,
                            CurrentState = timeMachine.GetState(),
                            CombatEvent = combat,
                        });
                        
                        combat.ForwardAction(timeMachine, timeMachine.GetState());
                        localCombatEvents.Add(combat);
                        
                        // Notify of a completed combat.
                        OnPostCombat?.Invoke(this, new PostCombatEventArgs()
                        {
                            Direction = onTick.Direction,
                            CombatResolution = combat.GetCombatResolution(),
                            CurrentState = timeMachine.GetState()
                        });
                    }
                });
            
            // Also check to undo events...
            if (onTick.Direction == TimeMachineDirection.REVERSE)
            {
                localCombatEvents
                    .Where(it => it.OccursAt == onTick.CurrentTick.Advance(1))
                    .ToList()
                    .ForEach(it =>
                    {
                        it.BackwardAction(timeMachine, timeMachine.GetState());
                        localCombatEvents.Remove(it);
                    });
            }
        }

        /// <summary>
        /// Gets the rotation of the sub
        /// </summary>
        /// <returns>Gets the angle of the sub's path</returns>
        public double GetRotationRadians()
        {
            RftVector direction = GetDirection();
            return Math.Atan2(direction.Y, direction.X);
        }

        public GameTick GetSpawnTick()
        {
            return _startTime;
        }

        public RftVector GetExpectedDestination()
        {
            // If there is no destination, return to the source.
            if (_destination == null)
            {
                return _initialLocation;
            }
            
            // If the target is not moving, interpolate the current position from the destination.
            // Linear interpolation
            if (Math.Abs(_destination.GetComponent<SpeedManager>().GetSpeed()) < 0.001)
            {
                return _destination.GetComponent<PositionManager>().CurrentLocation;
            }
            else
            {
                // If this is not moving, but the target is, show the destination as here...
                if (Math.Abs(_speedManager.GetSpeed()) < 0.001)
                {
                    return this.CurrentLocation;
                }
                
                // Get the target's interception point using the initial position & the speed.
                var interceptionPoint = _destination.GetComponent<PositionManager>()
                    .GetInterceptionPosition(_initialLocation, _speedManager.GetSpeed());
                return interceptionPoint;
            }
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
            
            var distanceBetweenTargets = (CurrentLocation - targetFrom).Length();
            var directionBetweenTargets = Math.Atan2(CurrentLocation.Y - targetFrom.Y, CurrentLocation.X - targetFrom.X);
            var alpha = Math.PI + directionBetweenTargets - Math.Atan2(GetDirection().Y, GetDirection().X);

            if (Math.Abs(chaserSpeed - _speedManager.GetSpeed()) < 0.001)
            {
                if (Math.Cos(alpha) < 0)
                {
                    return null;
                }
                // This determines what direction the chaser needs to go.
                // Create a Normalized vector from this.
                var radians = (directionBetweenTargets + alpha) % (Math.PI * 2);
                // With isosceles triangle, determine the other two angles:
                var otherAngles = ((Math.PI * 2) - radians) / 2.0;
                var distanceToTravelBeforeInterception = (float)(Math.Abs(Math.Cos(otherAngles) * distanceBetweenTargets));
                var timeToReach = distanceToTravelBeforeInterception / _speedManager.GetSpeed();
                var dir = GetDirection().Normalize();
                return new RftVector(CurrentLocation.X + (dir.X * timeToReach), CurrentLocation.Y + (dir.Y * timeToReach));
            }

            // Solve with quadratic formula
            var a = Math.Pow(chaserSpeed, 2) - Math.Pow(this._speedManager.GetSpeed(), 2);
            var b = 2 * distanceBetweenTargets * this._speedManager.GetSpeed() * Math.Cos(alpha);
            var c = -Math.Pow(distanceBetweenTargets, 2);

            var discriminant = Math.Pow(b, 2) - (4 * a * c);
            if (discriminant < 0)
                return null;

            var time = (Math.Sqrt(discriminant) - b) / (2 * a);
            var normalizedDirection = GetDirection().Normalize();
            var x = _initialLocation.X +
                    (this._speedManager.GetSpeed() * time) * Math.Cos(Math.Atan2(normalizedDirection.Y, normalizedDirection.X));
            var y = _initialLocation.Y +
                    (this._speedManager.GetSpeed() * time) * Math.Sin(Math.Atan2(normalizedDirection.Y, normalizedDirection.X));
            
            return new RftVector((float)x, (float)y);
        }

        public RftVector GetDirection()
        {
            if (_destination == null)
            {
                return new RftVector(0, 0);
            }

            return (GetExpectedDestination() - _initialLocation);
        }

        public GameTick GetExpectedArrival()
        {
            if (_destination == null || _speedManager.GetSpeed() < 0.001)
            {
                return null;
            }
            RftVector direction = GetDirection();
            int ticksToArrive = (int) Math.Floor(direction.Length() / _speedManager.GetSpeed());
            return _startTime.Advance(ticksToArrive);
        }

        public IEntity GetDestination()
        {
            return _destination;
        }

        public RftVector GetInitialPosition()
        {
            return this._initialLocation;
        }

        public void SetDestination(IEntity destination)
        {
            this._destination = destination;
        }

        public IEntity GetSource()
        {
            return _source;
        }

        public void OnTargeted(IEntity targetedBy)
        {
            OnLocationTargeted?.Invoke(this, new OnLocationTargetedEventArgs()
            {
                Destination = Parent,
                TargetedBy = targetedBy,
            });
        }

        public event EventHandler<OnPreCombatEventArgs>? OnPreCombat;
        public event EventHandler<PostCombatEventArgs>? OnPostCombat;
        public event EventHandler<OnLocationTargetedEventArgs>? OnLocationTargeted;
    }
}