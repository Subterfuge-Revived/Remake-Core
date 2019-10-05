using System;
using System.Collections.Generic;
using System.Text;

namespace SubterfugeCore.Core.Components.Outpost
{
    public interface IHasDrillers
    {
        int getDrillerCount();
        void setDrillerCount(int drillerCount);
        void addDrillers(int drillersToAdd);
        void removeDrillers(int drillersToRemove);
        bool hasDrillers(int drillers);
    }
}
