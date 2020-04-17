using SubterfugeCore.Core.Entities.Specialists.Effects.Enums;
using SubterfugeCore.Core.Interfaces;

namespace SubterfugeCore.Core.Entities.Specialists.Effects
{
    public class AlterDrillerEffect : SpecialistEffect
    {

        public override void ForwardEffect(ICombatable friendly, ICombatable enemy)
        {
            friendly.AddDrillers(getEffectValue(friendly, enemy));
        }

        public override void BackwardEffect(ICombatable friendly, ICombatable enemy)
        {
            friendly.RemoveDrillers(getEffectValue(friendly, enemy));
        }

    }
}