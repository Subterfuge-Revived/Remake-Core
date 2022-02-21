using System;
using SubterfugeCore.Core.GameEvents.Base;
using SubterfugeCore.Core.Players;
using SubterfugeCore.Core.Timing;
using SubterfugeRemakeService;

namespace SubterfugeCore.Core.GameEvents.PlayerTriggeredEvents
{
    public abstract class PlayerTriggeredEvent : GameEvent
    {
        protected readonly GameEventModel Model;
        protected PlayerTriggeredEvent(GameEventModel model)
        {
            this.Model = model;
        }

        public Player IssuedBy()
        {
            return new Player(Model.IssuedBy);
        }

        public override GameTick GetOccursAt()
        {
            return new GameTick(Model.OccursAtTick);
        }

        public EventType GetEventType()
        {
            return Model.EventType;
        }

        public DateTime GetUnixTimeIssued()
        {
            return new DateTime(Model.UnixTimeIssued);
        }

        public override string GetEventId()
        {
            return Model.Id;
        }

        public override Priority GetPriority()
        {
            return Priority.PlayerIssuedCommand;
        }
        
        public abstract GameEventModel ToGameEventModel();

        protected GameEventModel GetBaseGameEventModel()
        {
            return new GameEventModel()
            {
                Id = GetEventId(),
                EventType = GetEventType(),
                IssuedBy = IssuedBy().GetId(),
                OccursAtTick = GetOccursAt().GetTick(),
                UnixTimeIssued = GetUnixTimeIssued().ToFileTimeUtc(),
            };
        }
    }
}