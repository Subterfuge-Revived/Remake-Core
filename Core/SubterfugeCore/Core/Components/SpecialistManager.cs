using System;
using System.Collections.Generic;
using System.Linq;
using SubterfugeCore.Core.Entities;
using SubterfugeCore.Core.Entities.Specialists;
using SubterfugeCore.Core.EventArgs;
using SubterfugeCore.Core.Players;

namespace SubterfugeCore.Core.Components
{
    /// <summary>
    /// Specialist management class to facilitate adding and removing specialists from ISpecialistCarrier classes.
    /// </summary>
    public class SpecialistManager : EntityComponent
    {
        /// <summary>
        /// The maximum number of specialists that can be stored in this carrier
        /// </summary>
        private int _capacity;
        
        /// <summary>
        /// A list of specialists in the carrier
        /// </summary>
        List<Specialist> _specialists = new List<Specialist>();

        
        public event EventHandler<OnAddSpecialistEventArgs> OnAddSpecialist;
        public event EventHandler<OnRemoveSpecialistEventArgs> OnRemoveSpecialist;
        public event EventHandler<OnSpecialistCapacityChangeEventArgs> OnSpecialistCapacityChange;

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

        /// <summary>
        /// Adds a specialist to the location
        /// </summary>
        /// <param name="specialist">The specialist to add to the location</param>
        public bool AddSpecialist(Specialist specialist)
        {
            if (_specialists.Count < _capacity)
            {
                _specialists.Add(specialist);
                OnAddSpecialist?.Invoke(this, new OnAddSpecialistEventArgs()
                {
                    AddedSpecialist = specialist,
                    AddedTo = this,
                });
                return true;
            }
            return false;
        }

        /// <summary>
        /// Adds a list of specialists
        /// </summary>
        /// <param name="specialists">A list of specialists to add</param>
        public int AddSpecialists(List<Specialist> specialists)
        {
            var addedSpecialists = 0;
            foreach(Specialist s in specialists)
            {
                if(_specialists.Count < _capacity)
                {
                    addedSpecialists++;
                    _specialists.Add(s);
                    OnAddSpecialist?.Invoke(this, new OnAddSpecialistEventArgs()
                    {
                        AddedSpecialist = s,
                        AddedTo = this,
                    });
                }
            }

            return addedSpecialists;
        }

        /// <summary>
        /// Removes a specialist
        /// </summary>
        /// <param name="specialist">The specialist to remove</param>
        public bool RemoveSpecialist(Specialist specialist)
        {
            if (_specialists.Contains(specialist))
            {
                _specialists.Remove(specialist);
                OnRemoveSpecialist?.Invoke(this, new OnRemoveSpecialistEventArgs()
                {
                    RemovedSpecialist = specialist,
                    RemovedFrom = this,
                });
                return true;
            }
            return false;
        }

        /// <summary>
        /// Removes a list of specialsits
        /// </summary>
        /// <param name="specialists">The list of specialists to remove</param>
        public int RemoveSpecialists(List<Specialist> specialists)
        {
            var removedSpecialists = 0;
            foreach(Specialist s in specialists)
            {
                if (_specialists.Contains(s))
                {
                    removedSpecialists++;
                    _specialists.Remove(s);
                    OnRemoveSpecialist?.Invoke(this, new OnRemoveSpecialistEventArgs()
                    {
                        RemovedSpecialist = s,
                        RemovedFrom = this,
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
        public void SetCapacity(int capacity)
        {
            var previousCapacity = _capacity;
            _capacity = capacity;
            OnSpecialistCapacityChange?.Invoke(this, new OnSpecialistCapacityChangeEventArgs()
            {
                NewCapacity = capacity,
                PreviousCapacity = previousCapacity,
                SpecialistManager = this,
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
                foreach (Specialist s in toRemove)
                {
                    RemoveSpecialist(s);
                    specialistManager.AddSpecialist(s);
                }

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
                foreach (Specialist s in specialistsMatchingId)
                {
                    RemoveSpecialist(s);
                    destinationSpecialistManager.AddSpecialist(s);
                }

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
        }
        
        /// <summary>
        /// Gets all of the specialists beloging to a specific player.
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