using System;
using System.Collections.Generic;
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

        /// <summary>
        /// List of players who can see this event
        /// </summary>
        protected List<Player> VisibleTo;

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

        /// <summary>
        /// Gets the list of players whose FOV contains the event
        /// </summary>
        /// <returns>A list of players who can see the event</returns>
        public List<Player> GetVisibleTo()
        {
            return this.VisibleTo;
        }

        /// <summary>
        /// Gets whether the PlayerTriggeredEvent is visible to a specific player.
        /// </summary>
        /// <param name="player">The player to check visibility of the event</param>
        /// <returns>True if the event is visible to the passed player, and false otherwise.</returns>
        public bool IsVisibleTo(Player player)
        {
            return this.VisibleTo.Contains(player);
        }

        /// <summary>
        /// Determines the players whose FOV contains the event.
        /// PRE: the game state is on the same tick as the event.
        /// </summary>
        public abstract void DetermineVisibility();
        
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