using System;
using System.Collections.Generic;
using System.Text;
using SubterfugeCore.Core.Entities.Specialists.Effects;
using SubterfugeCore.Core.Interfaces;

namespace SubterfugeCore.Core.Entities.Specialists
{
    /// <summary>
    /// Specialist management class to facilitate adding and removing specialists from ISpecialistCarrier classes.
    /// </summary>
    public class SpecialistManager
    {
        /// <summary>
        /// The maximum number of specialists that can be stored in this carrier
        /// </summary>
        private int _capacity;
        
        /// <summary>
        /// A list of specialists in the carrier
        /// </summary>
        List<Specialist> _specialists = new List<Specialist>();

        /// <summary>
        /// Constructor for the specialist manager. Sets the capacity to 3 by default.
        /// </summary>
        public SpecialistManager()
        {
            this._capacity = 3;  // default capacity
        }
        
        /// <summary>
        /// Constructor with a specific capacity
        /// </summary>
        /// <param name="capacity">The capacity of the manager</param>
        public SpecialistManager(int capacity)
        {
            this._capacity = capacity;
        }
        
        /// <summary>
        /// Gets a list of the held specialists
        /// </summary>
        /// <returns>A list of the held specialists</returns>
        public List<Specialist> GetSpecialists()
        {
            return this._specialists;
        }

        /// <summary>
        /// Determines if a specialist can be added to this location
        /// </summary>
        /// <returns>If a specialist can be added</returns>
        public bool CanAddSpecialist()
        {
            return this._specialists.Count < _capacity;
        }

        /// <summary>
        /// Adds a specialist to the location
        /// </summary>
        /// <param name="specialist">The specialist to add to the location</param>
        public void AddSpecialist(Specialist specialist)
        {
            if(this._specialists.Count < _capacity)
                this._specialists.Add(specialist);
        }

        /// <summary>
        /// Adds a list of specialists
        /// </summary>
        /// <param name="specialists">A list of specialists to add</param>
        public void AddSpecialists(List<Specialist> specialists)
        {
            foreach(Specialist s in specialists)
            {
                if(this._specialists.Count < _capacity)
                {
                    this._specialists.Add(s);
                }
            }
        }

        /// <summary>
        /// Removes a specialist
        /// </summary>
        /// <param name="specialist">The specialist to remove</param>
        public void RemoveSpecialist(Specialist specialist)
        {
            this._specialists.Remove(specialist);
        }

        /// <summary>
        /// Removes a list of specialsits
        /// </summary>
        /// <param name="specialists">The list of specialists to remove</param>
        public void RemoveSpecialists(List<Specialist> specialists)
        {
            foreach(Specialist s in specialists)
            {
                if (this._specialists.Contains(s))
                {
                    this._specialists.Remove(s);
                }
            }
        }

        /// <summary>
        /// Get the capacity
        /// </summary>
        /// <returns>The capacity of specialists</returns>
        public int GetCapacity()
        {
            return this._capacity;
        }

        /// <summary>
        /// Sets the capacity
        /// </summary>
        /// <param name="capacity">The capacity to set</param>
        public void SetCapacity(int capacity)
        {
            this._capacity = capacity;
        }

        
        /// <summary>
        /// Returns the current number of specialists
        /// </summary>
        /// <returns>The current number of specialists</returns>
        public int GetSpecialistCount()
        {
            return this._specialists.Count;
        }
    }
}
