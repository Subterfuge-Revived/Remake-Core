using Subterfuge.Remake.Core.GameEvents.Base;
using Subterfuge.Remake.Core.Timing;

namespace Subterfuge.Remake.Core.GameEvents.EventPublishers
{
    public interface IReversibleEventHandler
    {
        bool ForwardEvent(TimeMachine timeMachine, GameEvent gameEvent);
        bool BackwardsEvent(TimeMachine timeMachine, GameEvent gameEvent);
    }
}