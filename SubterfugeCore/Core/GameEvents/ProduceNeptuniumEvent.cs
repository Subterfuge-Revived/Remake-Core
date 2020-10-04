using Newtonsoft.Json;
using SubterfugeCore.Core.Config;
using SubterfugeCore.Core.Entities.Positions;
using SubterfugeCore.Core.GameEvents.Base;
using SubterfugeCore.Core.Players;
using SubterfugeCore.Core.Timing;

namespace SubterfugeCore.Core.GameEvents
{ 
    public class ProduceNeptuniumEvent : GameEvent
    {
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
                outpost.GetOwner().addNeptunium(Constants.BASE_NEPTUNIUM_PRODUCTION);
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
                outpost.GetOwner().removeNeptunium(Constants.BASE_NEPTUNIUM_PRODUCTION);
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