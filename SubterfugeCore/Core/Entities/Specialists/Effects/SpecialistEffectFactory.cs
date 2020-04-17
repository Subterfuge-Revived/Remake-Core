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
        public ISpecialistEffect createSpecialistEffect(EffectType type, int value, EffectTarget target, EffectTrigger trigger, EffectTriggerRange triggerRange)
        {
            SpecialistEffect effect = null;
            
            switch (type)
            {
                case EffectType.AlterDriller:
                    AlterDrillerEffect alterDrillerEffect = new AlterDrillerEffect();
                    effect = alterDrillerEffect;
                    break;
                case EffectType.StealDriller:
                    StealDrillerEffect stealDrillerEffect = new StealDrillerEffect(value, trigger);
                    effect = stealDrillerEffect;
                    break;
            }
            
            this.setEffectValues(effect, value, target, trigger, triggerRange);

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
        private void setEffectValues(SpecialistEffect effect, int value, EffectTarget target, EffectTrigger trigger,
            EffectTriggerRange triggerRange)
        {
            effect._effectValue = value;
            effect._effectTarget = target;
            effect._effectTrigger = trigger;
            effect._effectTriggerRange = triggerRange;
        }
    }
}