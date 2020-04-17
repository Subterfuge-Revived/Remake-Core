using SubterfugeCore.Core.Entities.Specialists.Effects.Enums;
using SubterfugeCore.Core.Interfaces;

namespace SubterfugeCore.Core.Entities.Specialists.Effects
{
    public abstract class SpecialistEffect : ISpecialistEffect
    {
        public int _effectValue { get; set; } = 0;
        public float _scalar = 1;
        public EffectScale _effectScale { get; set; } = EffectScale.None;
        public EffectTrigger _effectTrigger { get; set; } = EffectTrigger.None;
        public EffectTarget _effectTarget { get; set; } = EffectTarget.None;
        public EffectTriggerRange _effectTriggerRange { get; set; } = EffectTriggerRange.Self;
        public abstract void ForwardEffect(ICombatable friendly, ICombatable enemy);
        public abstract void BackwardEffect(ICombatable friendly, ICombatable enemy);

        public int getEffectValue(ICombatable friendly, ICombatable enemy)
        {
            return (int)(this._effectValue * _scalar);
        }
    }
}