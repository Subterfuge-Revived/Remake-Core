using System;
using System.Collections.Generic;
using System.Text;

namespace SubterfugeCore.Core.Entities.Specialists
{
    public class SpecialistManager
    {
        private int capacity;
        List<Specialist> specialists = new List<Specialist>();

        public SpecialistManager(int capacity)
        {
            this.capacity = 3;  // default capacity
        }

        public List<Specialist> getSpecialists()
        {
            return this.specialists;
        }

        public bool canAddSpecialist()
        {
            return this.specialists.Count < capacity;
        }

        public void addSpecialist(Specialist specialist)
        {
            this.specialists.Add(specialist);
        }

        public void addSpecialists(List<Specialist> specialists)
        {
            this.specialists.AddRange(specialists);
        }

        public void removeSpecialist(Specialist specialist)
        {
            this.specialists.Remove(specialist);
        }

        public int getCapacity()
        {
            return this.capacity;
        }

        public void setCapacity(int capacity)
        {
            this.capacity = capacity;
        }

        public int getSpecialistCount()
        {
            return this.specialists.Count;
        }

    }
}
