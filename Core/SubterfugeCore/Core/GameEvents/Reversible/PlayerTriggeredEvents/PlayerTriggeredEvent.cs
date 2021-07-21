using System;
using Google.Protobuf;
using SubterfugeCore.Core.GameEvents.Base;
using SubterfugeCore.Core.GameEvents.ReversibleEvents;
using SubterfugeCore.Core.Players;
using SubterfugeCore.Core.Timing;
using SubterfugeRemakeService;

namespace SubterfugeCore.Core.GameEvents.Reversible.PlayerTriggeredEvents
{
    public abstract class PlayerTriggeredEvent : IReversible
    {
        protected GameEventModel model;
        protected PlayerTriggeredEvent(GameEventModel eventModel)
        {
            model = eventModel;
            parseGameEventModel(model);
        }
        
        protected PlayerTriggeredEvent(
            String eventId,
            EventType eventType,
            Player issuedBy,
            GameTick occursAtTick,
            ByteString eventData
        ) {
            model = new GameEventModel()
            {
                Id = eventId,
                EventType = eventType,
                IssuedBy = issuedBy.GetId(),
                OccursAtTick = occursAtTick.GetTick(),
                UnixTimeIssued = DateTime.Now.ToFileTimeUtc(),
                EventData = eventData,
            };
            parseGameEventModel(model);
        }
        public abstract void ForwardAction(TimeMachine timeMachine);

        public abstract void BackwardAction(TimeMachine timeMachine);

        public abstract void parseGameEventModel(GameEventModel eventModel);

        public GameEventModel ToGameEventModel()
        {
            return model;
        }
    }
}