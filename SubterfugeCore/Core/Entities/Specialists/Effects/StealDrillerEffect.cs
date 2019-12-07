using System;
using System.Collections.Generic;
using System.Text;
using SubterfugeCore.Core.Interfaces.Outpost;

namespace SubterfugeCore.Core.Entities.Specialists.Effects
{
    class StealDrillerEffect : ISpecialistEffect
    {
        private float stealPercent;
        EffectTrigger trigger;

        public StealDrillerEffect(float stealPercent, EffectTrigger trigger)
        {
            this.stealPercent = stealPercent;
            this.trigger = trigger;
        }

        public void forwardEffect(ICombatable friendly, ICombatable enemy)
        {
            enemy.removeDrillers((int)(enemy.getDrillerCount() * stealPercent));
        }

        public void backwardEffect(ICombatable friendly, ICombatable enemy)
        {
            // Can't do this. Need an event that stores the initial steal amount O_O ...
            // enemy.addDrillers(drillerCount * stealPercent);
        }

        public EffectTrigger getEffectTrigger()
        {
            return this.trigger;
        }
    }
}
