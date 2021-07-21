using System.Collections.Generic;
using System.Linq;
using SubterfugeCore.Core.GameEvents.ReversibleEvents;
using SubterfugeCore.Core.Timing;

namespace SubterfugeCore.Core.GameEvents.Reversible
{
    public abstract class GameStateEffect : IReversible
    {
        public abstract List<IReversible> GetEvents();
        
        public void ForwardAction(TimeMachine timeMachine)
        {
            GetEvents().ForEach(it => it.ForwardAction(timeMachine));
        }

        public void BackwardAction(TimeMachine timeMachine)
        {
            List<IReversible> events = GetEvents();
            events.Reverse();
            events.ForEach(it => it.BackwardAction(timeMachine));
        }
    }
}