using System;
using SubterfugeCore.Core.GameEvents.Base;
using SubterfugeCore.Core.Players;
using SubterfugeCore.Core.Timing;
using SubterfugeCore.Models.GameEvents;

namespace SubterfugeCore.Core.GameEvents.PlayerTriggeredEvents
{
    public abstract class PlayerTriggeredEvent : GameEvent
    {
        protected readonly GameEventData Model;
        protected PlayerTriggeredEvent(GameEventData model)
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

        protected GameEventData GetBaseGameEventModel()
        {
            return new GameEventData()
            {
                Id = GetEventId(),
                IssuedBy = IssuedBy().ToUser(),
                OccursAtTick = GetOccursAt().GetTick(),
                UnixTimeIssued = GetUnixTimeIssued().ToFileTimeUtc(),
            };
        }
    }
}