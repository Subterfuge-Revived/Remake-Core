using System;
using System.Collections.Generic;
using System.Linq;
using Subterfuge.Remake.Core.Entities.Specialists;
using Subterfuge.Remake.Core.GameEvents.EventPublishers;
using Subterfuge.Remake.Core.Players;
using Subterfuge.Remake.Core.Timing;

namespace Subterfuge.Remake.Core.Entities.Components
{
    /// <summary>
    /// Specialist management class to facilitate adding and removing specialists from ISpecialistCarrier classes.
    /// </summary>
    public class SpecialistManager : EntityComponent, ISpecialistEventPublisher
    {
        /// <summary>
        /// The maximum number of specialists that can be stored in this carrier
        /// </summary>
        private int _capacity;
        
        /// <summary>
        /// A list of specialists in the carrier
        /// </summary>
        List<Specialist> _specialists = new List<Specialist>();

        public bool CanLoadCapturedSpecialists { get; set; } = false;
        private int _canHireAtLocation = 0;

        public event EventHandler<OnSpecialistHireEventArgs>? OnSpecialistHire;
        public event EventHandler<OnSpecialistsCapturedEventArgs>? OnSpecialistCapture;
        public event EventHandler<OnSpecialistPromotionEventArgs>? OnSpecialistPromotion;
        public event EventHandler<OnSpecialistTransferEventArgs>? OnSpecialistTransfer;
        

        /// <summary>
        /// Constructor with a specific capacity
        /// </summary>
        /// <param name="parent">The parent entity</param>
        /// <param name="capacity">The capacity of the manager</param>
        public SpecialistManager(IEntity parent, int capacity = 3) : base(parent)
        {
            _capacity = capacity;
        }
        
        /// <summary>
        /// Gets a list of the held specialists
        /// </summary>
        /// <returns>A list of the held specialists</returns>
        public List<Specialist> GetSpecialists()
        {
            return _specialists;
        }

        /// <summary>
        /// Determines if a specialist can be added to this location
        /// </summary>
        /// <returns>If a specialist can be added</returns>
        public bool CanAddSpecialists(int specsToAdd = 1)
        {
            return GetUncapturedSpecialistCount() + specsToAdd <= _capacity;
        }

        public bool CanAddSpecialist(Specialist specialist)
        {
            if (GetUncapturedSpecialistCount() < _capacity)
                return false;

            if (Parent is Sub && specialist.IsCaptured() && !CanLoadCapturedSpecialists)
                return false;

            return true;
        }

        public void AllowHireFromLocation()
        {
            _canHireAtLocation++;
        }

        public void DisallowHireFromLocation()
        {
            _canHireAtLocation--;
        }

        public bool HireSpecialist(Specialist specialist, TimeMachine timeMachine)
        {
            if (_canHireAtLocation == 0 && !specialist.IsHero)
                return false;
            
            if (GetUncapturedSpecialistCount() < _capacity)
            {
                _specialists.Add(specialist);
                // Must register event listener before triggering other events.
                specialist.ArriveAtLocation(Parent, timeMachine);
                OnSpecialistHire?.Invoke(this, new OnSpecialistHireEventArgs()
                {
                    Direction = TimeMachineDirection.FORWARD,
                    HiredSpecialist = specialist,
                    HireLocation = Parent,
                });
                return true;
            }
            return false;
        }

        public bool UndoHireSpecialist(Specialist specialist, TimeMachine timeMachine)
        {
            _specialists.Remove(specialist);
            // Reverse must trigger events before unregistering listeners
            OnSpecialistHire?.Invoke(this, new OnSpecialistHireEventArgs()
            {
                Direction = TimeMachineDirection.REVERSE,
                HiredSpecialist = specialist,
                HireLocation = Parent,
            });
            specialist.LeaveLocation(Parent, timeMachine);
            return true;
        }

        /// <summary>
        /// Get the capacity
        /// </summary>
        /// <returns>The capacity of specialists</returns>
        public int GetCapacity()
        {
            return _capacity;
        }

        /// <summary>
        /// Sets the capacity
        /// </summary>
        /// <param name="capacity">The capacity to set</param>
        public int AlterCapacity(int delta)
        {
            var initialCapacity = _capacity;
            _capacity = Math.Max(0, _capacity + delta);
            return _capacity - initialCapacity;
        }

        
        /// <summary>
        /// Returns the current number of specialists
        /// </summary>
        /// <returns>The current number of specialists</returns>
        public int GetUncapturedSpecialistCount()
        {
            return _specialists.Where(it => !it.IsCaptured()).ToList().Count;
        }

        public int GetTotalSpecialistCount()
        {
            return _specialists.Count;
        }

        public int GetCapturedSpecialistCount()
        {
            return _specialists.Where(it => it.IsCaptured()).ToList().Count;
        }

        /// <summary>
        /// Transfers all of the specialists from this specialist manager to the target specialist manager.
        /// </summary>
        /// <param name="specialistManager"></param>
        public bool TransferSpecialistsTo(SpecialistManager specialistManager, TimeMachine timeMachine)
        {
            if (specialistManager.CanAddSpecialists(this._specialists.Count))
            {
                List<Specialist> toRemove = new List<Specialist>(_specialists);
                _specialists.Clear();
                specialistManager._specialists.AddRange(toRemove);
                
                toRemove.ForEach(spec =>
                {
                    spec.LeaveLocation(Parent, timeMachine);
                    spec.ArriveAtLocation(specialistManager.Parent, timeMachine);
                });
                
                OnSpecialistTransfer?.Invoke(this , new OnSpecialistTransferEventArgs()
                {
                    AddedTo = specialistManager.Parent,
                    Direction = TimeMachineDirection.FORWARD,
                    RemovedFrom = this.Parent,
                    specialist = toRemove,
                });
                
                return true;
            }
            return false;
        }

        /// <summary>
        /// Transfers all of the specialists from this specialist manager to the target specialist manager.
        /// </summary>
        /// <param name="destinationSpecialistManager">Specialist manager to transfer subs to</param>
        /// <param name="specialistIds">List of specialist Ids to transfer</param>
        public bool TransferSpecialistsById(SpecialistManager destinationSpecialistManager, List<string> specialistIds, TimeMachine timeMachine)
        {
            var specialistsMatchingId = _specialists.Where(it => specialistIds.Contains(it.GetId())).ToList();
            if (destinationSpecialistManager.CanAddSpecialists(specialistsMatchingId.Count))
            {
                _specialists = _specialists.Where(it => !specialistIds.Contains(it.GetId())).ToList();
                destinationSpecialistManager._specialists.AddRange(specialistsMatchingId);
                
                specialistsMatchingId.ForEach(spec =>
                {
                    spec.LeaveLocation(Parent, timeMachine);
                    spec.ArriveAtLocation(destinationSpecialistManager.Parent, timeMachine);
                });
                
                OnSpecialistTransfer?.Invoke(this , new OnSpecialistTransferEventArgs()
                {
                    AddedTo = destinationSpecialistManager.Parent,
                    Direction = TimeMachineDirection.FORWARD,
                    RemovedFrom = this.Parent,
                    specialist = specialistsMatchingId,
                });
                
                return true;
            }

            return false;
        }

        public void CaptureAllForward(TimeMachine timeMachine, IEntity captureLocation)
        {
            foreach(Specialist s in _specialists)
            {
                s.SetCaptured(true, timeMachine, captureLocation);
            }
            
            OnSpecialistCapture?.Invoke(this, new OnSpecialistsCapturedEventArgs()
            {
                Direction = TimeMachineDirection.FORWARD,
                Location = captureLocation,
                TimeMachine = timeMachine,
            });
        }

        /// <summary>
        /// Sets all of the specialists within this specialist manager to be captured.
        /// </summary>
        public void KillAll()
        {
            _specialists.Clear();
        }

        public void ReviveAll(List<Specialist> killedSpecialists)
        {
            _specialists.AddRange(killedSpecialists);
        }

        /// <summary>
        /// Uncaptures all of the specialists within this specialist manager.
        /// </summary>
        public void UncaptureAll(TimeMachine timeMachine, IEntity captureLocation)
        {
            foreach(Specialist s in _specialists)
            {
                s.SetCaptured(false, timeMachine, captureLocation);
            }
        }
        
        /// <summary>
        /// Gets all of the specialists belonging to a specific player.
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public List<Specialist> GetPlayerSpecialists(Player player)
        {
            List<Specialist> playerSpecs = new List<Specialist>();
            foreach(Specialist s in _specialists)
            {
                if (Equals(s.GetOwner(), player))
                {
                    playerSpecs.Add(s);
                }
            }

            return playerSpecs;
        }
    }
}