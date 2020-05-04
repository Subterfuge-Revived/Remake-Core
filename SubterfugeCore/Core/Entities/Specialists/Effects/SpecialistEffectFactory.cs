using SubterfugeCore.Core.Entities.Specialists.Effects.Enums;

namespace SubterfugeCore.Core.Entities.Specialists.Effects
{
    /// <summary>
    /// A factory to create new specialist effects.
    /// </summary>
    public class SpecialistEffectFactory
    {
        /// <summary>
        /// Creates a new specialist effect with the specified parameters
        /// </summary>
        /// <param name="type">The effect type</param>
        /// <param name="value">The base value of the effect</param>
        /// <param name="target">The effect targets</param>
        /// <param name="trigger">The effect trigger</param>
        /// <param name="triggerRange">The detection of the effect's trigger</param>
        /// <returns></returns>
        public ISpecialistEffect createSpecialistEffect(SpecialistEffectConfiguration effectConfiguration)
        {
            SpecialistEffect effect = null;
            
            switch (effectConfiguration.EffectType)
            {
                case EffectType.AlterDriller:
                    AlterDrillerEffect alterDrillerEffect = new AlterDrillerEffect();
                    effect = alterDrillerEffect;
                    break;
            }

            if (effect.GetType() == typeof(NumericSpecialistEffect))
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
        private void setEffectValues(NumericSpecialistEffect effect, int value, EffectTarget target, EffectTrigger trigger,
            EffectTriggerRange triggerRange, SpecialistEffectScale scale)
        {
            effect._effectValue = value;
            effect._effectTarget = target;
            effect._effectTrigger = trigger;
            effect._effectTriggerRange = triggerRange;
            effect.EffectScale = new SpecialistEffectScale();
            effect.EffectScale = scale;
        }
    }
}