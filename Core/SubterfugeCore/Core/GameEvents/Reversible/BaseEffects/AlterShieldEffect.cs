using SubterfugeCore.Core.GameEvents.ReversibleEvents;
using SubterfugeCore.Core.Interfaces;
using SubterfugeCore.Core.Timing;

namespace SubterfugeCore.Core.GameEvents.Reversible
{
    public class AlterShieldEffect : IReversible
    {
        private IShieldable _shieldable;
        private int _modifyBy;
        
        public AlterShieldEffect(IShieldable shieldable, int modifyBy)
        {
            _shieldable = shieldable;
            _modifyBy = modifyBy;
        }

        public void ForwardAction(TimeMachine timeMachine)
        {
            _shieldable.GetShieldManager().AddShield(_modifyBy);
        }

        public void BackwardAction(TimeMachine timeMachine)
        {
            _shieldable.GetShieldManager().RemoveShields(_modifyBy);
        }
    }
}