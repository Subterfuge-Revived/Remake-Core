using SubterfugeCore.Core.Components;
using SubterfugeCore.Core.Entities;
using SubterfugeCore.Core.Timing;
using SubterfugeCore.Models.GameEvents;

namespace SubterfugeCore.Core.GameEvents.PlayerTriggeredEvents
{
    public class ToggleShieldEvent : PlayerTriggeredEvent
    {
        public ToggleShieldEvent(GameEventData model) : base(model)
        {
            
        }

        public ToggleShieldEventData GetEventData()
        {
            return Model.EventData as ToggleShieldEventData;
        }
        public override bool ForwardAction(TimeMachine timeMachine, GameState.GameState state)
        {
            ShieldManager shieldManager = state.GetEntity(GetEventData().SourceId).GetComponent<ShieldManager>();
            DrillerCarrier drillerCarrier = state.GetEntity(GetEventData().SourceId).GetComponent<DrillerCarrier>();
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

        public override bool BackwardAction(TimeMachine timeMachine, GameState.GameState state)
        {
            if (EventSuccess)
            {
                ShieldManager shieldManager = state.GetEntity(GetEventData().SourceId).GetComponent<ShieldManager>();
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