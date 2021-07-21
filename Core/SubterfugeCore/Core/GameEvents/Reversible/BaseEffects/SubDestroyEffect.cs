using SubterfugeCore.Core.Entities;
using SubterfugeCore.Core.GameEvents.ReversibleEvents;
using SubterfugeCore.Core.Interfaces;
using SubterfugeCore.Core.Timing;

namespace SubterfugeCore.Core.GameEvents.Reversible
{
    public class SubDestroyEffect : IReversible
    {
        private readonly Sub _sub;
        private readonly TimeMachine _timeMachine;
        
        public SubDestroyEffect(Sub sub, TimeMachine timeMachine)
        {
            _sub = sub;
            _timeMachine = timeMachine;
        }

        public void ForwardAction(TimeMachine timeMachine)
        {
            _timeMachine.GetState().RemoveSub(_sub);
        }

        public void BackwardAction(TimeMachine timeMachine)
        {
            _timeMachine.GetState().AddSub(_sub);
        }
    }
}