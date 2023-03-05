using Newtonsoft.Json;
using Subterfuge.Remake.Api.Network;
using Subterfuge.Remake.Core.Entities.Components;
using Subterfuge.Remake.Core.Timing;

namespace Subterfuge.Remake.Core.GameEvents.PlayerTriggeredEvents
{
    public class ToggleShieldEvent : PlayerTriggeredEvent
    {
        public ToggleShieldEvent(GameRoomEvent model) : base(model)
        {
            
        }

        public ToggleShieldEventData GetEventData()
        {
            return JsonConvert.DeserializeObject<ToggleShieldEventData>(Model.GameEventData.SerializedEventData);
        }
        public override bool ForwardAction(TimeMachine timeMachine)
        {
            ShieldManager shieldManager = timeMachine.GetState().GetEntity(GetEventData().SourceId).GetComponent<ShieldManager>();
            DrillerCarrier drillerCarrier = timeMachine.GetState().GetEntity(GetEventData().SourceId).GetComponent<DrillerCarrier>();
            if (shieldManager != null && !drillerCarrier.GetOwner().IsEliminated())
            {
                shieldManager.ToggleShield();
                EventSuccess = true;
            }
            else
            {
                EventSuccess = false;
            }

            return EventSuccess;
        }

        public override bool BackwardAction(TimeMachine timeMachine)
        {
            if (EventSuccess)
            {
                ShieldManager shieldManager = timeMachine.GetState().GetEntity(GetEventData().SourceId).GetComponent<ShieldManager>();
                shieldManager.ToggleShield();
            }
            return EventSuccess;
        }

        public override bool WasEventSuccessful()
        {
            return EventSuccess;
        }
    }
}