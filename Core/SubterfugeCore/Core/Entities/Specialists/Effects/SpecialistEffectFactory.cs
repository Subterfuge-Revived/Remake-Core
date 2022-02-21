using SubterfugeCore.Core.Entities.Specialists.Effects.Enums;

namespace SubterfugeCore.Core.Entities.Specialists.Effects
{
    /// <summary>
    /// A factory to create new specialist effects.
    /// </summary>
    public class SpecialistEffectFactory
    {
        /// <summary>
        /// Creates a specialist effect
        /// </summary>
        /// <param name="effectConfiguration">The specialist effect configuration parameters</param>
        /// <returns>The specialist effect</returns>
        public ISpecialistEffect CreateSpecialistEffect(SpecialistEffectConfiguration effectConfiguration)
        {
            SpecialistEffect effect = null;
            NumericSpecialistEffect numericSpecialistEffect = new NumericSpecialistEffect();

            switch (effectConfiguration.EffectType)
            {
                case EffectModifier.Driller:
                case EffectModifier.Speed:
                case EffectModifier.Vision:
                case EffectModifier.ShieldRegeneration:
                case EffectModifier.ShieldValue:
                case EffectModifier.ShieldMaxValue:
                    effect = new NumericSpecialistEffect();
                    break;
                case EffectModifier.KillPlayer:
                case EffectModifier.KillSpecialist:
                case EffectModifier.VictoryPlayer:
                    // TODO
                    // effect = new ToggleableSpecialistEffect();
                    effect = new NumericSpecialistEffect();
                    break;
            }
            
            numericSpecialistEffect.Effector = effectConfiguration.EffectType;

            if (effect != null && effect.GetType() == typeof(NumericSpecialistEffect))
            {
                this.setEffectValues(((NumericSpecialistEffect)effect), effectConfiguration.Value, effectConfiguration.EffectTarget, effectConfiguration.EffectTrigger, effectConfiguration.EffectTriggerRange, effectConfiguration.GetEffectScale());   
            }

            return effect;
        }

        /// <summary>
        /// Helper function to create the specialist effect from the populated values
        /// </summary>
        /// <param name="effect">The SpecialistEffect to set values for</param>
        /// <param name="value">The base effect value</param>
        /// <param name="target">The effect's targets</param>
        /// <param name="trigger">The effect's trigger</param>
        /// <param name="triggerRange">The effect's trigger detection range</param>
        /// <param name="scale">How to scale the effect</param>
        private void setEffectValues(NumericSpecialistEffect effect, int value, EffectTarget target, EffectTrigger trigger,
            EffectTriggerRange triggerRange, SpecialistEffectScale scale)
        {
            effect.EffectValue = value;
            effect.EffectTarget = target;
            effect.EffectTrigger = trigger;
            effect.EffectTriggerRange = triggerRange;
            effect.EffectScale = new SpecialistEffectScale();
            effect.EffectScale = scale;
        }
    }
}