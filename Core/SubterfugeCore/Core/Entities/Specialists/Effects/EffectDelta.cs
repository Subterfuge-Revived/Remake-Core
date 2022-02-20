using System.Collections;
using SubterfugeCore.Core.Components;
using SubterfugeCore.Core.Entities.Specialists.Effects.Enums;
using SubterfugeCore.Core.Interfaces;

namespace SubterfugeCore.Core.Entities.Specialists.Effects
{
    /// <summary>
    /// An effect delta. This is a single addition or subtraction to apply.
    /// </summary>
    public class EffectDelta
    {

        /// <summary>
        /// The value to add to the effector.
        /// </summary>
        public int Value { get; set; } = 0;

        /// <summary>
        /// What effect is being modified.
        /// </summary>
        public EffectModifier Modifier { get; set; }
        
        /// <summary>
        /// A reference to the target to apply the effects to.
        /// </summary>
        public IEntity EffectMe { get; set; }

        public EffectDelta(int value, IEntity effectMe, EffectModifier modifier)
        {
            this.Value = value;
            this.Modifier = modifier;
            this.EffectMe = effectMe;
        }

        /// <summary>
        /// Applies the effect to the effector.
        /// </summary>
        /// <param name="effectMe">The target to effect</param>
        public void ApplyForwardEffect()
        {
            switch (Modifier)
            {
                case EffectModifier.Driller:
                    EffectMe.GetComponent<DrillerCarrier>().AddDrillers(Value);
                    break;
                case EffectModifier.SpecialistCapacity:
                    int capacity = EffectMe.GetComponent<SpecialistManager>().GetCapacity();
                    EffectMe.GetComponent<SpecialistManager>().SetCapacity(capacity + Value);
                    break;
                case EffectModifier.Vision:
                case EffectModifier.ShieldValue:
                case EffectModifier.ShieldRegeneration:
                case EffectModifier.ShieldMaxValue:
                    // TODO apply effect deltas here.
                    break;
                case EffectModifier.Speed:
                    // TODO apply sped effects here.
                    break;
            }
        }

        /// <summary>
        /// Undoes the effect to the effector.
        /// </summary>
        /// <param name="effectMe">The target to effect</param>
        public void ApplyBackwardsEffect()
        {
            switch (Modifier)
            {
                case EffectModifier.Driller:
                    EffectMe.GetComponent<DrillerCarrier>().RemoveDrillers(Value);
                    break;
                case EffectModifier.SpecialistCapacity:
                    int capacity = EffectMe.GetComponent<SpecialistManager>().GetCapacity();
                    EffectMe.GetComponent<SpecialistManager>().SetCapacity(capacity - Value);
                    break;
                case EffectModifier.Vision:
                case EffectModifier.ShieldValue:
                case EffectModifier.ShieldRegeneration:
                case EffectModifier.ShieldMaxValue:
                    // TODO: apply effect deltas here.
                    break;
                case EffectModifier.Speed:
                    // TODO: apply speed effects here.
                    break;
            }
        }


    }
}