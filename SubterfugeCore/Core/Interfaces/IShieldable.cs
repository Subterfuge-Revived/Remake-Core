using System;
using System.Collections.Generic;
using System.Text;

namespace SubterfugeCore.Core.Interfaces.Outpost
{
    public interface IShieldable
    {
        int getShields();
        void setShields(int shieldValue);
        void removeShields(int shieldsToRemove);
        void toggleShield();
        bool isShieldActive();
        void addShield(int shields);
        int getShieldCapacity();
        void setShieldCapacity(int capactiy);
    }
}
