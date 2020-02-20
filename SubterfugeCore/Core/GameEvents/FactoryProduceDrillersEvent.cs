﻿using SubterfugeCore.Core.Entities.Locations;
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
    class FactoryProduceDrillersEvent : GameEvent
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
        public override void eventBackwardAction()
        {
            if (eventSuccess)
            {
                outpost.removeDrillers(6);
                Game.timeMachine.removeEvent(this.nextEvent);
                this.nextEvent = null;
            }
        }

        /// <summary>
        /// Forward production event
        /// </summary>
        public override void eventForwardAction()
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
            }
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
