using System;
using System.Collections.Generic;
using System.Text;

namespace SubterfugeCore.Core.Entities.Specialists
{
    public class SpecialistManager
    {
        private int capacity;
        List<Specialist> specialists = new List<Specialist>();

        public SpecialistManager()
        {
            this.capacity = 3;  // default capacity
        }

        public SpecialistManager(int capacity)
        {
            this.capacity = capacity;
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
            if(this.specialists.Count < capacity)
                this.specialists.Add(specialist);
        }

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

        public void removeSpecialist(Specialist specialist)
        {
            this.specialists.Remove(specialist);
        }

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
