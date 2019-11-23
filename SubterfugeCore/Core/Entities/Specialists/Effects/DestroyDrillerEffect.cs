using System;
using System.Collections.Generic;
using System.Text;
using SubterfugeCore.Core.Interfaces.Outpost;

namespace SubterfugeCore.Core.Entities.Specialists
{
    class DestroyDrillerEffect : ISpecialistEffect, IScalableEffect
    {
        private bool killsFriendly;
        private int drillerCount;

        public DestroyDrillerEffect(int drillerCount)
        {
            this.drillerCount = drillerCount;
            this.killsFriendly = false;
        }

        public void destroysFriendly()
        {
            this.killsFriendly = true;
        }

        public void scale(int factor)
        {
            this.drillerCount = this.drillerCount * factor;
        }

        public void forwardEffect(ICombatable combatant)
        {
            throw new NotImplementedException();
        }

        public void backwardEffect(ICombatable combatant)
        {
            throw new NotImplementedException();
        }
    }
}
