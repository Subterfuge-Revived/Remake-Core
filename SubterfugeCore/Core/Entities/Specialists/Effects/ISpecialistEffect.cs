using SubterfugeCore.Core.Entities.Specialists.Effects.Enums;
using SubterfugeCore.Core.Interfaces;

namespace SubterfugeCore.Core.Entities.Specialists.Effects
{
    public interface ISpecialistEffect
    {
        void ForwardEffect(ICombatable friendly, ICombatable enemy);
        void BackwardEffect(ICombatable friendly, ICombatable enemy);
    }
}
