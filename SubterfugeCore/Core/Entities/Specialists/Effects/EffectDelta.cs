using System.Collections;
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
        public EffectEffector Effector { get; set; }
        
        /// <summary>
        /// A reference to the target to apply the effects to.
        /// </summary>
        public ICombatable EffectMe { get; set; }

        public EffectDelta(int value, ICombatable effectMe, EffectEffector effector)
        {
            this.Value = value;
            this.Effector = effector;
            this.EffectMe = effectMe;
        }

        /// <summary>
        /// Applies the effect to the effector.
        /// </summary>
        /// <param name="effectMe">The target to effect</param>
        public void ApplyForwardEffect()
        {
            switch (Effector)
            {
                case EffectEffector.Driller:
                    EffectMe.AddDrillers(Value);
                    break;
                case EffectEffector.SpecialistCapacity:
                    int capacity = EffectMe.GetSpecialistManager().GetCapacity();
                    EffectMe.GetSpecialistManager().SetCapacity(capacity + Value);
                    break;
                case EffectEffector.Vision:
                case EffectEffector.ShieldValue:
                case EffectEffector.ShieldRegeneration:
                case EffectEffector.ShieldMaxValue:
                    // TODO apply effect deltas here.
                    break;
            }
        }

        /// <summary>
        /// Undoes the effect to the effector.
        /// </summary>
        /// <param name="effectMe">The target to effect</param>
        public void ApplyBackwardsEffect()
        {
            switch (Effector)
            {
                case EffectEffector.Driller:
                    EffectMe.RemoveDrillers(Value);
                    break;
                case EffectEffector.SpecialistCapacity:
                    int capacity = EffectMe.GetSpecialistManager().GetCapacity();
                    EffectMe.GetSpecialistManager().SetCapacity(capacity - Value);
                    break;
                case EffectEffector.Vision:
                case EffectEffector.ShieldValue:
                case EffectEffector.ShieldRegeneration:
                case EffectEffector.ShieldMaxValue:
                    // TODO: apply effect deltas here.
                    break;
            }
        }


    }
}