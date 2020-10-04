using Newtonsoft.Json;
using SubterfugeCore.Core.Entities.Positions;
using SubterfugeCore.Core.GameEvents.Base;
using SubterfugeCore.Core.Players;
using SubterfugeCore.Core.Timing;

namespace SubterfugeCore.Core.GameEvents
{ 
    public class ProduceNeptuniumEvent : GameEvent
    {
        private const int NEPTUNIUM_PER_EVENT = 1;

        /// <summary>
        /// The outpost producing the neptunium
        /// </summary>
        private Outpost outpost;
        
        /// <summary>
        /// Constructor for a neptunium production event.
        /// </summary>
        /// <param name="outpost">The outpost producing the neptunium</param>
        /// <param name="tick">The tick the outpost produces the neptunium</param>
        public ProduceNeptuniumEvent(Outpost outpost, GameTick tick) : base(tick)
        {
            if (outpost.GetOutpostType() == OutpostType.Mine)
            {
                this.outpost = outpost;
            }
            else
            {
                this.EventSuccess = false;
            }
        }

        /// <summary>
        /// Produces the neptunium
        /// </summary>
        /// <returns>If the event was successful</returns>
        public override bool ForwardAction()
        {
            if (outpost.GetOutpostType() == OutpostType.Mine)
            {
                outpost.GetOwner().addNeptunium(NEPTUNIUM_PER_EVENT);
                EventSuccess = true;
            }
            else
            {
                EventSuccess = false;
            }

            return EventSuccess;
        }

        /// <summary>
        /// Undoes the neptunium production
        /// </summary>
        /// <returns>If the event was successful</returns>
        public override bool BackwardAction()
        {
            if (EventSuccess)
            {
                outpost.GetOwner().removeNeptunium(NEPTUNIUM_PER_EVENT);
            }

            return EventSuccess;
        }

        /// <summary>
        /// If the event was successful
        /// </summary>
        /// <returns>If the event was successful</returns>
        public override bool WasEventSuccessful()
        {
            return EventSuccess;
        }
    }
}