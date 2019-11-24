using System;
using System.Collections.Generic;
using System.Text;
using SubterfugeCore.Core.Entities.Specialists.Effects;
using SubterfugeCore.Core.Interfaces.Outpost;

namespace SubterfugeCore.Core.Entities.Specialists
{
    class DestroyDrillerEffect : ISpecialistEffect, IScalableEffect
    {
        private bool killsFriendly;
        private int drillerCount;
        EffectTrigger trigger;
        int scaleFactor = 1;

        public DestroyDrillerEffect(int drillerCount, EffectTrigger trigger)
        {
            this.drillerCount = drillerCount;
            this.killsFriendly = false;
            this.trigger = trigger;
        }

        public void destroysFriendly()
        {
            this.killsFriendly = true;
        }

        public void forwardEffect(ICombatable combatant)
        {
            combatant.removeDrillers(drillerCount * scaleFactor);
        }

        public void backwardEffect(ICombatable combatant)
        {
            combatant.addDrillers(drillerCount * scaleFactor);
        }

        public EffectTrigger getEffectTrigger()
        {
            return this.trigger;
        }

        public void scale(int scale)
        {
            this.scaleFactor = scale;
        }
    }
}
