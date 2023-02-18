using Subterfuge.Remake.Api.Network;

namespace Subterfuge.Remake.Core.Entities.Specialists.Effects
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

            switch (effectConfiguration.EffectModifier)
            {
                case EffectModifier.Driller:
                case EffectModifier.Speed:
                case EffectModifier.VisionRange:
                case EffectModifier.ShieldRegenerationRate:
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

            numericSpecialistEffect.configuration = effectConfiguration;

            return effect;
        }
    }
}