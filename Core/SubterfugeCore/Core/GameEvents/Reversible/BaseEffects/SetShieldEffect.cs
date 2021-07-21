using SubterfugeCore.Core.GameEvents.ReversibleEvents;
using SubterfugeCore.Core.Interfaces;
using SubterfugeCore.Core.Timing;

namespace SubterfugeCore.Core.GameEvents.Reversible
{
    public class SetShieldEffect : IReversible
    {
        private IShieldable _shieldable;
        private int _setTo;
        private int _setFrom;
        
        public SetShieldEffect(IShieldable shieldable, int setTo)
        {
            _shieldable = shieldable;
            _setTo = setTo;
        }

        public void ForwardAction(TimeMachine timeMachine)
        {
            _setFrom = _shieldable.GetShieldManager().GetShields();
            _shieldable.GetShieldManager().SetShields(_setTo);
        }

        public void BackwardAction(TimeMachine timeMachine)
        {
            _shieldable.GetShieldManager().SetShields(_setFrom);
        }
    }
}