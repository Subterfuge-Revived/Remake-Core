using System;
using Microsoft.DotNet.PlatformAbstractions;
using Newtonsoft.Json;
using Subterfuge.Remake.Api.Network;
using Subterfuge.Remake.Core.GameEvents.Base;
using Subterfuge.Remake.Core.Players;
using Subterfuge.Remake.Core.Timing;

namespace Subterfuge.Remake.Core.GameEvents.PlayerTriggeredEvents
{
    public abstract class PlayerTriggeredEvent : GameEvent
    {
        protected readonly GameRoomEvent Model;
        protected PlayerTriggeredEvent(GameRoomEvent model) : base(new GameTick(model.GameEventData.OccursAtTick), Priority.PlayerIssuedCommand)
        {
            this.Model = model;
        }

        public Player IssuedBy()
        {
            return new Player(Model.IssuedBy);
        }

        public DateTime GetUnixTimeIssued()
        {
            return Model.TimeIssued;
        }

        public override string GetEventId()
        {
            return Model.Id;
        }

        protected GameRoomEvent GetBaseGameEventModel()
        {
            return new GameRoomEvent()
            {
                Id = GetEventId(),
                IssuedBy = IssuedBy().PlayerInstance,
                TimeIssued = Model.TimeIssued,
                GameEventData = Model.GameEventData
            };
        }

        public override bool Equals(object other)
        {
            PlayerTriggeredEvent asEvent = other as PlayerTriggeredEvent;
            if (asEvent == null)
                return false;

            return asEvent.OccursAt == this.OccursAt &&
                   asEvent.IssuedBy().PlayerInstance.Equals(this.IssuedBy().PlayerInstance) &&
                   asEvent.GetBaseGameEventModel().Id == this.GetBaseGameEventModel().Id;
        }

        public override int GetHashCode()
        {
            var hashBuilder = new HashCodeCombiner();
            hashBuilder.Add(IssuedBy().PlayerInstance);
            hashBuilder.Add(OccursAt);
            hashBuilder.Add(Priority);
            hashBuilder.Add(GetBaseGameEventModel().Id);
            return hashBuilder.GetHashCode();
        }
    }
}