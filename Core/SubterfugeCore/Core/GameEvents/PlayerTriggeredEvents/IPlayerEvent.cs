using Subterfuge.Remake.Api.Network;
using Subterfuge.Remake.Core.Timing;

namespace Subterfuge.Remake.Core.GameEvents.PlayerTriggeredEvents
{
    public interface IPlayerEvent
    {
        bool BackwardAction(TimeMachine timeMachine, GameState.GameState state);
        bool ForwardAction(TimeMachine timeMachine, GameState.GameState state);
        GameEventData GetEventData();
        bool WasEventSuccessful();
    }
}