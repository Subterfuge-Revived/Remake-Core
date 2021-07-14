using System.Collections.Generic;
using GameEventModels;
using Google.Protobuf;
using SubterfugeCore.Core.GameEvents.Base;
using SubterfugeCore.Core.Interfaces;
using SubterfugeCore.Core.Players;
using SubterfugeCore.Core.Timing;
using SubterfugeRemakeService;

namespace SubterfugeCore.Core.GameEvents
{
    public class ToggleShieldEvent : PlayerTriggeredEvent
    {
        private ICombatable _shieldEntity;

        public ToggleShieldEvent(GameEventModel model) : base(model)
        {
            
        }

        public ToggleShieldEventData GetEventData()
        {
            return ToggleShieldEventData.Parser.ParseFrom(model.EventData);
        }
        public override bool ForwardAction(TimeMachine timeMachine, GameState state)
        {
            ICombatable source = state.GetCombatableById(GetEventData().SourceId);
            if (source != null && !source.GetOwner().IsEliminated())
            {
                this._shieldEntity = source;
                source.GetShieldManager().ToggleShield();
                EventSuccess = true;
            }
            else
            {
                EventSuccess = false;
            }

            return EventSuccess;
        }

        public override bool BackwardAction(TimeMachine timeMachine, GameState state)
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

		public override void DetermineVisibility()
		{
            this.VisibleTo = new List<Player>(this._shieldEntity.GetVisibleTo());
		}

		public override Priority GetPriority()
		{
            return Priority.NATURAL_PRIORITY_9;
		}

		public override GameEventModel ToGameEventModel()
        {
            GameEventModel baseModel = GetBaseGameEventModel();
            baseModel.EventData = GetEventData().ToByteString();
            return baseModel;
        }
    }
}