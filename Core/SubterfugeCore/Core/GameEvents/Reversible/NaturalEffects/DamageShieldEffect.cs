using SubterfugeCore.Core.Entities;
using SubterfugeCore.Core.GameEvents.ReversibleEvents;
using SubterfugeCore.Core.Interfaces;
using SubterfugeCore.Core.Timing;

namespace SubterfugeCore.Core.GameEvents.Reversible
{
    public class DamageShieldEffect : IReversible
    {
        private IShieldable _shieldable;
        private IDrillerCarrier _attacker;
        private int _originalDrillerCount;
        private int _originalShieldCount;
        
        public DamageShieldEffect(IDrillerCarrier attacker, IShieldable shieldable)
        {
            _attacker = attacker;
            _shieldable = shieldable;
        }

        public void ForwardAction(TimeMachine timeMachine)
        {
            ShieldManager shieldManager = _shieldable.GetShieldManager();
                
            if (shieldManager.IsShieldActive())
            {
                _originalDrillerCount = _attacker.GetDrillerCount();
                _originalShieldCount = shieldManager.GetShields();
                
                _attacker.RemoveDrillers(shieldManager.GetShields());
                shieldManager.RemoveShields(_originalDrillerCount);
            }
        }

        public void BackwardAction(TimeMachine timeMachine)
        {
            ShieldManager shieldManager = _shieldable.GetShieldManager();
                
            if (shieldManager.IsShieldActive())
            {
                _attacker.SetDrillerCount(_originalDrillerCount);
                shieldManager.SetShields(_originalShieldCount);
            }
        }
    }
}