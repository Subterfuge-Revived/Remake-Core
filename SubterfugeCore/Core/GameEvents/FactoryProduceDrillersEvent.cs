using SubterfugeCore.Core.Entities.Locations;
using SubterfugeCore.Core.GameEvents.Validators;
using SubterfugeCore.Entities;
using SubterfugeCore.GameEvents;
using SubterfugeCore.Timing;
using System;
using System.Collections.Generic;
using System.Text;

namespace SubterfugeCore.Core.GameEvents
{
    class FactoryProduceDrillersEvent : GameEvent
    {
        Outpost outpost;
        GameTick productionTick;
        bool eventSuccess = false;
        bool addedNextProduction = false;

        public FactoryProduceDrillersEvent(Outpost outpost, GameTick tick)
        {
            this.outpost = outpost;
            this.productionTick = tick;
            this.eventName = "Factory Production Event";
        }
        public override void eventBackwardAction()
        {
            if (eventSuccess)
            {
                outpost.removeDrillers(6);
            }
        }

        public override void eventForwardAction()
        {
            if (Validator.validateOutpost(outpost))
            {
                if(this.outpost.getOwner() != null && this.outpost.getOutpostType() == OutpostType.FACTORY)
                {
                    // After this cycle is produced, add another production event.
                    if (!addedNextProduction)
                    {
                        GameServer.timeMachine.addEvent(new FactoryProduceDrillersEvent(this.outpost, this.productionTick.advance(40)));
                        this.addedNextProduction = true;
                    }
                    outpost.addDrillers(6);
                    eventSuccess = true;
                }
            }
        }

        public override GameTick getTick()
        {
            return this.productionTick;
        }
    }
}
