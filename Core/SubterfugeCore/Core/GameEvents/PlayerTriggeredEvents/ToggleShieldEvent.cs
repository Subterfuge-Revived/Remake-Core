using GameEventModels;
using Google.Protobuf;
using SubterfugeCore.Core.Interfaces;
using SubterfugeRemakeService;

namespace SubterfugeCore.Core.GameEvents
{
    public class ToggleShieldEvent : PlayerTriggeredEvent
    {
        public ToggleShieldEvent(GameEventModel model) : base(model)
        {
            
        }

        public ToggleShieldEventData GetEventData()
        {
            return ToggleShieldEventData.Parser.ParseFrom(model.EventData);
        }
        public override bool ForwardAction(GameState state)
        {
            ICombatable source = state.GetCombatableById(GetEventData().SourceId);
            if (source != null)
            {
                source.GetShieldManager().ToggleShield();
                EventSuccess = true;
            }
            else
            {
                EventSuccess = false;
            }

            return EventSuccess;
        }

        public override bool BackwardAction(GameState state)
        {
            if (EventSuccess)
            {
                ICombatable source = state.GetCombatableById(GetEventData().SourceId);
                source.GetShieldManager().ToggleShield();
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