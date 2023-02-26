using Newtonsoft.Json;
using Subterfuge.Remake.Api.Network;
using Subterfuge.Remake.Core.Timing;

namespace Subterfuge.Remake.Core.GameEvents.PlayerTriggeredEvents
{
    public class PromoteSpecialistEvent : PlayerTriggeredEvent
    {
        public PromoteSpecialistEvent(GameRoomEvent model) : base(model)
        {
        }
        
        public PromoteSpecialistEventData GetEventData()
        {
            return JsonConvert.DeserializeObject<PromoteSpecialistEventData>(Model.GameEventData.SerializedEventData);
        }

        public override bool ForwardAction(TimeMachine timeMachine, GameState state)
        {
            throw new System.NotImplementedException();
        }

        public override bool BackwardAction(TimeMachine timeMachine, GameState state)
        {
            throw new System.NotImplementedException();
        }

        public override bool WasEventSuccessful()
        {
            throw new System.NotImplementedException();
        }
    }
}