using SubterfugeCore.Core.Entities.Specialists.Effects.Enums;
using SubterfugeCore.Core.Interfaces;

namespace SubterfugeCore.Core.Entities.Specialists.Effects
{
    /// <summary>
    /// An effect to alter the number of drillers at a location.
    /// This effect applies a simple addition or subtraction.
    /// </summary>
    public class AlterDrillerEffect : SpecialistEffect
    {
        public override void ForwardEffect(ICombatable friendly, ICombatable enemy)
        {
            friendly.AddDrillers((int)getEffectValue(friendly, enemy));
        }

        public override void BackwardEffect(ICombatable friendly, ICombatable enemy)
        {
            friendly.RemoveDrillers((int)getEffectValue(friendly, enemy));
        }

    }
}