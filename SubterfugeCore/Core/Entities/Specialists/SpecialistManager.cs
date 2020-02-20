using System;
using System.Collections.Generic;
using System.Text;

namespace SubterfugeCore.Core.Entities.Specialists
{
    /// <summary>
    /// Specialist management class
    /// </summary>
    public class SpecialistManager
    {
        
        private int capacity;    // The number of specialists that can be held
        List<Specialist> specialists = new List<Specialist>();  // A list of held specialists

        /// <summary>
        /// Constructor for the specialist manager.
        /// </summary>
        public SpecialistManager()
        {
            this.capacity = 3;  // default capacity
        }
        
        /// <summary>
        /// Constructor with a specific capacity
        /// </summary>
        /// <param name="capacity">The capacity of the manager</param>
        public SpecialistManager(int capacity)
        {
            this.capacity = capacity;
        }
        
        /// <summary>
        /// Gets a list of the held specialists
        /// </summary>
        /// <returns>A list of the held specialists</returns>
        public List<Specialist> getSpecialists()
        {
            return this.specialists;
        }

        /// <summary>
        /// Determines if a specialist can be added to this location
        /// </summary>
        /// <returns>If a specialist can be added</returns>
        public bool canAddSpecialist()
        {
            return this.specialists.Count < capacity;
        }

        /// <summary>
        /// Adds a specialist to the location
        /// </summary>
        /// <param name="specialist">The specialist to add to the location</param>
        public void addSpecialist(Specialist specialist)
        {
            if(this.specialists.Count < capacity)
                this.specialists.Add(specialist);
        }

        /// <summary>
        /// Adds a list of specialists
        /// </summary>
        /// <param name="specialists">A list of specialists to add</param>
        public void addSpecialists(List<Specialist> specialists)
        {
            foreach(Specialist s in specialists)
            {
                if(this.specialists.Count < capacity)
                {
                    this.specialists.Add(s);
                }
            }
        }

        /// <summary>
        /// Removes a specialist
        /// </summary>
        /// <param name="specialist">The specialist to remove</param>
        public void removeSpecialist(Specialist specialist)
        {
            this.specialists.Remove(specialist);
        }

        /// <summary>
        /// Removes a list of specialsits
        /// </summary>
        /// <param name="specialists">The list of specialists to remove</param>
        public void removeSpecialists(List<Specialist> specialists)
        {
            foreach(Specialist s in specialists)
            {
                if (this.specialists.Contains(s))
                {
                    this.specialists.Remove(s);
                }
            }
        }

        /// <summary>
        /// Get the capacity
        /// </summary>
        /// <returns>The capacity of specialists</returns>
        public int getCapacity()
        {
            return this.capacity;
        }

        /// <summary>
        /// Sets the capacity
        /// </summary>
        /// <param name="capacity">The capacity to set</param>
        public void setCapacity(int capacity)
        {
            this.capacity = capacity;
        }

        
        /// <summary>
        /// Returns the current number of specialists
        /// </summary>
        /// <returns>The current number of specialists</returns>
        public int getSpecialistCount()
        {
            return this.specialists.Count;
        }

    }
}
