using System;
using SubterfugeCore.Core.Entities;
using SubterfugeCore.Core.Entities.Positions;
using SubterfugeCore.Core.Interfaces;

namespace SubterfugeCore.Core.GameEvents.ReversibleEvents
{
    public class ShieldCombat : IReversible
    {
        private IShieldable _friendly;
        private ICombatable _enemy;
        
        public ShieldCombat(IShieldable shielded, ICombatable combatant2)
        {
            _friendly = shielded;
            _enemy = combatant2;
        }
        public bool ForwardAction()
        {            
            _friendly.GetShieldManager().CombatShields(_enemy.GetDrillerCount());
            return true;
        }

        public bool BackwardAction()
        {
            // No need to reverse this event. The shield Manager does it automatically when calling .getShields().
            return true;
        }

        public bool WasEventSuccessful()
        {
            throw new System.NotImplementedException();
        }
    }
}