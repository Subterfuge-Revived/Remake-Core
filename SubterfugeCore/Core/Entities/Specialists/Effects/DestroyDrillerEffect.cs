using SubterfugeCore.Core.Interfaces;

namespace SubterfugeCore.Core.Entities.Specialists.Effects
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

        public void forwardEffect(ICombatable friendly, ICombatable enemy)
        {
            enemy.removeDrillers(drillerCount * scaleFactor);
        }

        public void backwardEffect(ICombatable friendly, ICombatable enemy)
        {
            enemy.addDrillers(drillerCount * scaleFactor);
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
