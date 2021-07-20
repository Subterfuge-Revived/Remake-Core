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
        public EffectModifier EffectType { get; set; } = EffectModifier.None;

        /// <summary>
        /// Applies the event's forward action
        /// </summary>
        /// <param name="state">The game state to get the effects for.</param>
        /// <param name="friendly">The friendly participant. Null if none.</param>
        /// <param name="enemy">The enemy participant. Null if none.</param>
        public abstract List<EffectDelta> GetForwardEffectDeltas(GameState state, ICombatable friendly, ICombatable enemy);
        
        /// <summary>
        /// Applies the event's backwards action
        /// </summary>
        /// <param name="state">The game state to get the effects for.</param>
        /// <param name="friendly">The friendly participant. Null if none.</param>
        /// <param name="enemy">The enemy participant. Null if none.</param>
        public abstract List<EffectDelta> GetBackwardEffectDeltas(GameState state, ICombatable friendly, ICombatable enemy);

        public List<ICombatable> getEffectTargets(GameState state, ICombatable friendly, ICombatable enemy)
        {
            List<ICombatable> targets = new List<ICombatable>();
            
            // Filter based on the trigger range first
            switch (_effectTriggerRange)
            {
                case EffectTriggerRange.Self:
                    targets.Add(friendly);
                    targets.Add(enemy);
                    break;
                case EffectTriggerRange.Local:
                    targets.Add(friendly);
                    targets.Add(enemy);
                    break;
                case EffectTriggerRange.ConstantRange:
                    // TODO: get all ICombatable within range of friendly.
                    break;
                case EffectTriggerRange.Global:
                    targets.AddRange(state.GetAllGameObjects());
                    break;
                case EffectTriggerRange.LocationVisionRange:
                    // TODO: Get all ICombatable within outpost's vision.
                    break;
                case EffectTriggerRange.PlayerVisionRange:
                    // TODO: Get all ICombatable within player's vision
                    break;
            }
            
            // Filter based on the target
            switch (_effectTarget)
            {
                case EffectTarget.All:
                    break;
                case EffectTarget.Both:
                    break;
                case EffectTarget.Enemy:
                    targets = targets.FindAll(x => x.GetOwner() != friendly.GetOwner());
                    break;
                case EffectTarget.Friendly:
                    targets = targets.FindAll(x => x.GetOwner() == friendly.GetOwner());
                    break;
                case EffectTarget.None:
                    targets.RemoveAll(x => true);
                    break;
            }

            return targets;
        }
    }
}