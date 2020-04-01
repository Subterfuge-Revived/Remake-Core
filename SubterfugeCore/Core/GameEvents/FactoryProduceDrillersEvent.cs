using SubterfugeCore.Core.Entities.Locations;
using SubterfugeCore.Core.GameEvents.Validators;
using System;
using System.Collections.Generic;
using System.Text;
using SubterfugeCore.Core.GameEvents.Base;
using SubterfugeCore.Core.Timing;

namespace SubterfugeCore.Core.GameEvents
{
    /// <summary>
    /// Driller production event
    /// </summary>
    public class FactoryProduceDrillersEvent : GameEvent
    {
        /// <summary>
        /// The outpost producing the drillers
        /// </summary>
        Outpost outpost;
        
        /// <summary>
        /// The tick the outpost is expected to produce drillers on
        /// </summary>
        GameTick productionTick;
        
        /// <summary>
        /// If the event was successful
        /// </summary>
        bool eventSuccess = false;
        
        /// <summary>
        /// If the net production event was added to the time machine
        /// </summary>
        bool addedNextProduction = false;
        
        /// <summary>
        /// A reference to the next production event.
        /// </summary>
        GameEvent nextEvent;

        /// <summary>
        /// Constructor for the driller production event
        /// </summary>
        /// <param name="outpost">The outpost to produce drillers at</param>
        /// <param name="tick">The gameTick to produce them at</param>
        public FactoryProduceDrillersEvent(Outpost outpost, GameTick tick)
        {
            this.outpost = outpost;
            this.productionTick = tick;
            this.eventName = "Factory Production Event";
        }
        
        /// <summary>
        /// Undo a production event
        /// </summary>
        public override bool backwardAction()
        {
            if (eventSuccess)
            {
                outpost.removeDrillers(6);
                Game.timeMachine.removeEvent(this.nextEvent);
                this.nextEvent = null;
            }

            return this.eventSuccess;
        }

        public override bool wasEventSuccessful()
        {
            return this.eventSuccess;
        }

        public override string toJSON()
        {
            // Production events don't need to be in the database. No need for JSON.
            throw new NotImplementedException();
        }

        /// <summary>
        /// Forward production event
        /// </summary>
        public override bool forwardAction()
        {
            if (Validator.validateOutpost(outpost))
            {
                if(this.outpost.getOwner() != null && this.outpost.getOutpostType() == OutpostType.FACTORY)
                {
                    this.nextEvent = new FactoryProduceDrillersEvent(this.outpost, this.productionTick.advance(40));
                    Game.timeMachine.addEvent(this.nextEvent);
                    outpost.addDrillers(6);
                    eventSuccess = true;
                }
                else
                {
                    this.eventSuccess = false;
                }
            }
            else
            {
                this.eventSuccess = false;
            }

            return this.eventSuccess;
        }
        
        /// <summary>
        /// Get the tick of the production
        /// </summary>
        /// <returns>The production event's tick</returns>
        public override GameTick getTick()
        {
            return this.productionTick;
        }
    }
}
