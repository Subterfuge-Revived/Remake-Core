using System;
using System.Collections.Generic;
using System.Linq;
using Subterfuge.Remake.Core.Entities.Specialists;
using Subterfuge.Remake.Core.GameEvents.EventPublishers;
using Subterfuge.Remake.Core.Players;

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

        public event EventHandler<OnSpecialistHireEventArgs>? OnSpecialistHire;
        public event EventHandler<OnSpecialistsCapturedEventArgs>? OnCaptured;
        public event EventHandler<OnSpecialistsCapturedEventArgs>? OnUncaptured;
        public event EventHandler<OnSpecialistPromotionEventArgs>? OnSpecialistPromotion;
        public event EventHandler<OnAddSpecialistEventArgs>? OnSpecialistArrive;
        public event EventHandler<OnRemoveSpecialistEventArgs>? OnSpecialistLeave;
        public event EventHandler<OnSpecialistCapacityChangeEventArgs>? OnSpecialistCapacityChange;
        

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
            return (_specialists.Count + specsToAdd) <= _capacity;
        }

        public bool HireSpecialist(Specialist specialist)
        {
            if (_specialists.Count < _capacity)
            {
                _specialists.Add(specialist);
                specialist.ArriveAt(Parent);
                OnSpecialistHire?.Invoke(this, new OnSpecialistHireEventArgs()
                {
                    HiredSpecialist = specialist,
                    HireLocation = Parent,
                });
                return true;
            }
            return false;
        }

        /// <summary>
        /// Adds a specialist to the location
        /// </summary>
        /// <param name="specialist">The specialist to add to the location</param>
        public bool AddFriendlySpecialist(Specialist specialist)
        {
            if (_specialists.Count < _capacity)
            {
                _specialists.Add(specialist);
                specialist.ArriveAt(Parent);
                OnSpecialistArrive?.Invoke(this, new OnAddSpecialistEventArgs()
                {
                    AddedSpecialist = specialist,
                    AddedTo = Parent,
                });
                return true;
            }
            return false;
        }

        /// <summary>
        /// Adds a list of specialists
        /// </summary>
        /// <param name="specialists">A list of specialists to add</param>
        public int AddFriendlySpecialists(List<Specialist> specialists)
        {
            var addedSpecialists = 0;
            foreach(Specialist s in specialists)
            {
                if(_specialists.Count < _capacity)
                {
                    addedSpecialists++;
                    _specialists.Add(s);
                    s.ArriveAt(Parent);
                    OnSpecialistArrive?.Invoke(this, new OnAddSpecialistEventArgs()
                    {
                        AddedSpecialist = s,
                        AddedTo = Parent,
                    });
                }
            }

            return addedSpecialists;
        }

        /// <summary>
        /// Removes a specialist
        /// </summary>
        /// <param name="specialist">The specialist to remove</param>
        public bool RemoveFriendlySpecialist(Specialist specialist)
        {
            if (_specialists.Contains(specialist))
            {
                _specialists.Remove(specialist);
                specialist.LeaveLocation(Parent);
                OnSpecialistLeave?.Invoke(this, new OnRemoveSpecialistEventArgs()
                {
                    RemovedSpecialist = specialist,
                    RemovedFrom = Parent,
                });
                return true;
            }
            return false;
        }

        /// <summary>
        /// Removes a list of specialsits
        /// </summary>
        /// <param name="specialists">The list of specialists to remove</param>
        public int RemoveFriendlySpecialists(List<Specialist> specialists)
        {
            var removedSpecialists = 0;
            foreach(Specialist s in specialists)
            {
                if (_specialists.Contains(s))
                {
                    removedSpecialists++;
                    _specialists.Remove(s);
                    s.LeaveLocation(Parent);
                    OnSpecialistLeave?.Invoke(this, new OnRemoveSpecialistEventArgs()
                    {
                        RemovedSpecialist = s,
                        RemovedFrom = Parent,
                    });
                }
            }

            return removedSpecialists;
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
        public void AlterCapacity(int delta)
        {
            var initialCapacity = _capacity;
            _capacity = Math.Max(0, _capacity + delta);
            OnSpecialistCapacityChange?.Invoke(this, new OnSpecialistCapacityChangeEventArgs()
            {
                CapacityDelta = _capacity - initialCapacity
            });
        }

        
        /// <summary>
        /// Returns the current number of specialists
        /// </summary>
        /// <returns>The current number of specialists</returns>
        public int GetSpecialistCount()
        {
            return _specialists.Count;
        }

        /// <summary>
        /// Transfers all of the specialists from this specialist manager to the target specialist manager.
        /// </summary>
        /// <param name="specialistManager"></param>
        public bool TransferSpecialistsTo(SpecialistManager specialistManager)
        {
            if (specialistManager.CanAddSpecialists(this._specialists.Count))
            {
                List<Specialist> toRemove = new List<Specialist>(_specialists);
                RemoveFriendlySpecialists(toRemove);
                specialistManager.AddFriendlySpecialists(toRemove);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Transfers all of the specialists from this specialist manager to the target specialist manager.
        /// </summary>
        /// <param name="destinationSpecialistManager">Specialist manager to transfer subs to</param>
        /// <param name="specialistIds">List of specialist Ids to transfer</param>
        public bool TransferSpecialistsById(SpecialistManager destinationSpecialistManager, List<string> specialistIds)
        {
            var specialistsMatchingId = _specialists.Where(it => specialistIds.Contains(it.GetId())).ToList();
            if (destinationSpecialistManager.CanAddSpecialists(specialistsMatchingId.Count))
            {
                RemoveFriendlySpecialists(specialistsMatchingId);
                destinationSpecialistManager.AddFriendlySpecialists(specialistsMatchingId);
                return true;
            }

            return false;
        }
        
        /// <summary>
        /// Sets all of the specialists within this specialist manager to be captured.
        /// </summary>
        public void CaptureAll()
        {
            foreach(Specialist s in _specialists)
            {
                s.SetCaptured(true);
                s.OnCaptured(Parent);
            }
            OnCaptured?.Invoke(this, new OnSpecialistsCapturedEventArgs()
            {
                CaptureLocation = Parent,
            });
        }
        
        /// <summary>
        /// Sets all of the specialists within this specialist manager to be captured.
        /// </summary>
        public void KillAll()
        {
            foreach(Specialist s in _specialists)
            {
                this._specialists.Remove(s);
            }
        }

        /// <summary>
        /// Uncaptures all of the specialists within this specialist manager.
        /// </summary>
        public void UncaptureAll()
        {
            foreach(Specialist s in _specialists)
            {
                s.SetCaptured(false);
            }
            OnUncaptured?.Invoke(this, new OnSpecialistsCapturedEventArgs()
            {
                CaptureLocation = Parent,
            });
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
                if (s.GetOwner() == player)
                {
                    playerSpecs.Add(s);
                }
            }

            return playerSpecs;
        }
    }
}