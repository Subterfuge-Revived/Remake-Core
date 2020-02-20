using SubterfugeCore.Core.Interfaces;

namespace SubterfugeCore.Core.Entities.Specialists.Effects
{
    public interface ISpecialistEffect
    {
        void forwardEffect(ICombatable friendly, ICombatable enemy);
        void backwardEffect(ICombatable friendly, ICombatable enemy);
        EffectTrigger getEffectTrigger();
    }
}
