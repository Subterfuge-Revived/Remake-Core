using SubterfugeCore.Core.Entities;
using SubterfugeCore.Core.GameEvents.ReversibleEvents;
using SubterfugeCore.Core.Interfaces;
using SubterfugeCore.Core.Timing;

namespace SubterfugeCore.Core.GameEvents.Reversible
{
    public class AlterShieldCapacityEffect : IReversible
    {
        private IShieldable _shieldable;
        private int _modifyBy;
        
        public AlterShieldCapacityEffect(IShieldable shieldable, int modifyBy)
        {
            _shieldable = shieldable;
            _modifyBy = modifyBy;
        }

        public void ForwardAction(TimeMachine timeMachine)
        {
            ShieldManager shieldManager = _shieldable.GetShieldManager();
            int shieldCapacity = shieldManager.GetShieldCapacity() + _modifyBy;
            shieldManager.SetShieldCapacity(shieldManager.GetShieldCapacity() + _modifyBy);
            new AlterShieldEffect(_shieldable, _modifyBy).ForwardAction(timeMachine);
        }

        public void BackwardAction(TimeMachine timeMachine)
        {
            ShieldManager shieldManager = _shieldable.GetShieldManager();
            shieldManager.SetShieldCapacity(shieldManager.GetShieldCapacity() - _modifyBy);
        }
    }
}