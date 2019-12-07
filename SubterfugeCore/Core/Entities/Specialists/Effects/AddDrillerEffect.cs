using SubterfugeCore.Core.Interfaces.Outpost;
using System;
using System.Collections.Generic;
using System.Text;

namespace SubterfugeCore.Core.Entities.Specialists.Effects
{
    class AddDrillerEffect : ISpecialistEffect, IScalableEffect
    {
        private int drillerCount;
        EffectTrigger trigger;
        int scaleFactor = 1;

        public AddDrillerEffect(int drillerCount, EffectTrigger trigger)
        {
            this.drillerCount = drillerCount;
            this.trigger = trigger;
        }

        public void forwardEffect(ICombatable friendly, ICombatable enemy)
        {
            friendly.addDrillers(drillerCount * scaleFactor);
        }

        public void backwardEffect(ICombatable friendly, ICombatable enemy)
        {
            friendly.removeDrillers(drillerCount * scaleFactor);
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
