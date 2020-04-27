using System;
using System.Collections.Generic;
using System.Linq;
using SubterfugeCore.Core.Entities.Positions;
using SubterfugeCore.Core.Entities.Specialists.Effects.Enums;
using SubterfugeCore.Core.Interfaces;
using SubterfugeCore.Core.Players;

namespace SubterfugeCore.Core.Entities.Specialists.Effects
{
    /// <summary>
    /// An abstract class for implementing a specialist effect
    /// </summary>
    public abstract class SpecialistEffect : ISpecialistEffect
    {
        /// <summary>
        /// How the effect is triggered
        /// </summary>
        public EffectTrigger _effectTrigger { get; set; } = EffectTrigger.None;
        
        /// <summary>
        /// Who the effect targets
        /// </summary>
        public EffectTarget _effectTarget { get; set; } = EffectTarget.None;
        
        /// <summary>
        /// How large to apply the trigger effect to search for event triggers.
        /// </summary>
        public EffectTriggerRange _effectTriggerRange { get; set; } = EffectTriggerRange.Self;

        /// <summary>
        /// The type of effect occurs.
        /// </summary>
        public EffectType EffectType { get; set; } = EffectType.None;

        /// <summary>
        /// Applies the event's forward action
        /// </summary>
        /// <param name="friendly">The friendly participant. Null if none.</param>
        /// <param name="enemy">The enemy participant. Null if none.</param>
        public abstract List<EffectDelta> GetForwardEffectDeltas(ICombatable friendly, ICombatable enemy);
        
        /// <summary>
        /// Applies the event's backwards action
        /// </summary>
        /// <param name="friendly">The friendly participant. Null if none.</param>
        /// <param name="enemy">The enemy participant. Null if none.</param>
        public abstract List<EffectDelta> GetBackwardEffectDeltas(ICombatable friendly, ICombatable enemy);
    }
}