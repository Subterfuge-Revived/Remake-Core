using Newtonsoft.Json;
using Subterfuge.Remake.Api.Network;
using Subterfuge.Remake.Core.Timing;

namespace Subterfuge.Remake.Core.GameEvents.PlayerTriggeredEvents
{
    public class GameEndEvent : PlayerTriggeredEvent
    {
        public GameEndEvent(GameRoomEvent model) : base(model)
        {
        }
        
        public GameEndEventData GetEventData()
        {
            return JsonConvert.DeserializeObject<GameEndEventData>(Model.GameEventData.SerializedEventData);
        }

        public override bool ForwardAction(TimeMachine timeMachine, GameState state)
        {
            return true;
        }

        public override bool BackwardAction(TimeMachine timeMachine, GameState state)
        {
            return true;
        }

        public override bool WasEventSuccessful()
        {
            return true;
        }
    }
}