using GameEventModels;
using Google.Protobuf;
using SubterfugeCore.Core.Components;
using SubterfugeCore.Core.Entities;
using SubterfugeCore.Core.Timing;
using SubterfugeRemakeService;

namespace SubterfugeCore.Core.GameEvents.PlayerTriggeredEvents
{
    public class ToggleShieldEvent : PlayerTriggeredEvent
    {
        public ToggleShieldEvent(GameEventModel model) : base(model)
        {
            
        }

        public ToggleShieldEventData GetEventData()
        {
            return ToggleShieldEventData.Parser.ParseFrom(Model.EventData);
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

        public override GameEventModel ToGameEventModel()
        {
            GameEventModel baseModel = GetBaseGameEventModel();
            baseModel.EventData = GetEventData().ToByteString();
            return baseModel;
        }
    }
}