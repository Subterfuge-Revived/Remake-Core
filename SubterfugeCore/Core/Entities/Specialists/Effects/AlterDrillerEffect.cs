using SubterfugeCore.Core.Entities.Specialists.Effects.Enums;
using SubterfugeCore.Core.Interfaces;

namespace SubterfugeCore.Core.Entities.Specialists.Effects
{
    public class AlterDrillerEffect : ISpecialistEffect
    {
        private int _drillerCount;
        private EffectTrigger _trigger;
        private EffectTarget _target;
        private EffectType _effectType;

        public AlterDrillerEffect(int drillerCount, EffectTrigger trigger, EffectTarget effectTarget)
        {
            this._drillerCount = drillerCount;
            this._trigger = trigger;
            this._target = effectTarget;
        }

        public void ForwardEffect(ICombatable friendly, ICombatable enemy)
        {
            friendly.AddDrillers(_drillerCount);
        }

        public void BackwardEffect(ICombatable friendly, ICombatable enemy)
        {
            friendly.RemoveDrillers(_drillerCount);
        }

        public EffectTrigger GetEffectTrigger()
        {
            return this._trigger;
        }

        public void SetEffectTrigger(EffectTrigger effectTrigger)
        {
            this._trigger = effectTrigger;
        }

        public EffectTarget GetEffectTarget()
        {
            return this._target;
        }

        public void SetEffectTarget(EffectTarget effectTarget)
        {
            this._target = effectTarget;
        }

        public EffectType GetEffectType()
        {
            return this._effectType;
        }

        public void SetEffectType(EffectType effectType)
        {
            this._effectType = effectType;
        }
    }
}