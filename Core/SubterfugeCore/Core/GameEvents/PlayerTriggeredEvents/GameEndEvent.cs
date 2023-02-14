using Newtonsoft.Json;
using SubterfugeCore.Core.Timing;
using SubterfugeCore.Models.GameEvents;

namespace SubterfugeCore.Core.GameEvents.PlayerTriggeredEvents
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

        public override bool ForwardAction(TimeMachine timeMachine, GameState.GameState state)
        {
            return true;
        }

        public override bool BackwardAction(TimeMachine timeMachine, GameState.GameState state)
        {
            return true;
        }

        public override bool WasEventSuccessful()
        {
            return true;
        }
    }
}