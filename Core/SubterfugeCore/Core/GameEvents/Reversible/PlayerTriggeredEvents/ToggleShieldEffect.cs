using System;
using GameEventModels;
using Google.Protobuf;
using SubterfugeCore.Core.Entities.Positions;
using SubterfugeCore.Core.GameEvents.Reversible.PlayerTriggeredEvents;
using SubterfugeCore.Core.GameEvents.ReversibleEvents;
using SubterfugeCore.Core.Interfaces;
using SubterfugeCore.Core.Timing;
using SubterfugeRemakeService;

namespace SubterfugeCore.Core.GameEvents.Reversible
{
    public class ToggleShieldEffect : PlayerTriggeredEvent
    {

        private ToggleShieldEventData _toggleShieldEventData;
        
        public ToggleShieldEffect(GameEventModel eventModel) : base(eventModel)
        {
        }

        public ToggleShieldEffect(IShieldable toggledOutpost, GameTick occursAt) : base(
            Guid.NewGuid().ToString(),
            EventType.ToggleShieldEvent,
            toggledOutpost.GetOwner(),
            occursAt,
            new ToggleShieldEventData()
            {
                SourceId = toggledOutpost.GetId()
            }.ToByteString()
        ) {}

        public override void ForwardAction(TimeMachine timeMachine)
        {
            ICombatable source = timeMachine.GetState().GetCombatableById(_toggleShieldEventData.SourceId);
            if (source != null && !source.GetOwner().IsEliminated())
            {
                source.GetShieldManager().ToggleShield();
            }
        }

        public override void BackwardAction(TimeMachine timeMachine)
        {
            ICombatable source = timeMachine.GetState().GetCombatableById(_toggleShieldEventData.SourceId);
            if (source != null && !source.GetOwner().IsEliminated())
            {
                source.GetShieldManager().ToggleShield();
            }
        }

        public override void parseGameEventModel(GameEventModel eventModel)
        {
            if (eventModel.EventType == EventType.ToggleShieldEvent)
            {
                this._toggleShieldEventData = ToggleShieldEventData.Parser.ParseFrom(eventModel.EventData);
            }
        }
    }
}