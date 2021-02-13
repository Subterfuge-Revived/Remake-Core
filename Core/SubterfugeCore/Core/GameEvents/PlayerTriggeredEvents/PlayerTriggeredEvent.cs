using System;
using GameEventModels;
using SubterfugeCore.Core.GameEvents.Base;
using SubterfugeCore.Core.Players;
using SubterfugeCore.Core.Timing;
using SubterfugeRemakeService;

namespace SubterfugeCore.Core.GameEvents
{
    public abstract class PlayerTriggeredEvent : GameEvent
    {
        protected GameEventModel model;
        protected PlayerTriggeredEvent(GameEventModel model) : base()
        {
            this.model = model;
        }

        public Player IssuedBy()
        {
            return new Player(model.IssuedBy);
        }

        public override GameTick GetOccursAt()
        {
            return new GameTick(model.OccursAtTick);
        }

        public EventType GetEventType()
        {
            return model.EventType;
        }

        public DateTime GetUnixTimeIssued()
        {
            return new DateTime(model.UnixTimeIssued);
        }

        public override string GetEventId()
        {
            return model.EventId;
        }

        public override Priority GetPriority()
        {
            return Priority.PLAYER_ISSUED_COMMAND;
        }
        
        public abstract GameEventModel ToGameEventModel();

        protected GameEventModel GetBaseGameEventModel()
        {
            return new GameEventModel()
            {
                EventId = GetEventId(),
                EventType = GetEventType(),
                IssuedBy = IssuedBy().GetId(),
                OccursAtTick = GetOccursAt().GetTick(),
                UnixTimeIssued = GetUnixTimeIssued().ToFileTimeUtc(),
            };
        }
    }
}