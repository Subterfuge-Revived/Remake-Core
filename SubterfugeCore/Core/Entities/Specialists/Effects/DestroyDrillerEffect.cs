using SubterfugeCore.Core.Interfaces;

namespace SubterfugeCore.Core.Entities.Specialists.Effects
{
    class DestroyDrillerEffect : ISpecialistEffect, IScalableEffect
    {
        private bool _killsFriendly;
        private int _drillerCount;
        EffectTrigger _trigger;
        int _scaleFactor = 1;

        public DestroyDrillerEffect(int drillerCount, EffectTrigger trigger)
        {
            this._drillerCount = drillerCount;
            this._killsFriendly = false;
            this._trigger = trigger;
        }

        public void DestroysFriendly()
        {
            this._killsFriendly = true;
        }

        public void ForwardEffect(ICombatable friendly, ICombatable enemy)
        {
            enemy.RemoveDrillers(_drillerCount * _scaleFactor);
        }

        public void BackwardEffect(ICombatable friendly, ICombatable enemy)
        {
            enemy.AddDrillers(_drillerCount * _scaleFactor);
        }

        public EffectTrigger GetEffectTrigger()
        {
            return this._trigger;
        }

        public void Scale(int scale)
        {
            this._scaleFactor = scale;
        }
    }
}
