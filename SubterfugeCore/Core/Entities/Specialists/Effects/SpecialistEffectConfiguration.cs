using SubterfugeCore.Core.Entities.Specialists.Effects.Enums;

namespace SubterfugeCore.Core.Entities.Specialists.Effects
{
    public class SpecialistEffectConfiguration
    {
        /// <summary>
        /// The value to apply to the specialist effect.
        /// </summary>
        public int Value { get; set; } = 0;
        
        /// <summary>
        /// How the effect is triggered
        /// </summary>
        public EffectTrigger EffectTrigger { get; set; } = EffectTrigger.None;
        
        /// <summary>
        /// Who the effect targets
        /// </summary>
        public EffectTarget EffectTarget { get; set; } = EffectTarget.None;
        
        /// <summary>
        /// How large to apply the trigger effect to search for event triggers.
        /// </summary>
        public EffectTriggerRange EffectTriggerRange { get; set; } = EffectTriggerRange.Self;

        /// <summary>
        /// The type of effect occurs.
        /// </summary>
        public EffectType EffectType { get; set; } = EffectType.None;
        
        
        //////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////////////
        /// These variables are for scaling effects only.
        //////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////////////
        
        /// <summary>
        /// The Type of scaling that should be applied to the event.
        /// This applies a multiplication on the event's base value to determine the total effect.
        /// </summary>
        public EffectScale EffectScale { get; set; } = EffectScale.None;

        /// <summary>
        /// The target when considering how much to scale the effect by.
        /// </summary>
        public EffectTarget ScaleTarget { get; set; } = EffectTarget.None;

        /// <summary>
        /// The range to apply when considering how much to scale the effect by.
        /// </summary>
        public EffectTriggerRange ScaleRange { get; set; } = EffectTriggerRange.Self;

        public SpecialistEffectScale GetEffectScale()
        {
            SpecialistEffectScale scale = new SpecialistEffectScale();
            scale.EffectScale = EffectScale;
            scale.ScaleRange = ScaleRange;
            scale.ScaleTarget = ScaleTarget;
            return scale;
        }
    }
}