using System.Collections.Generic;
using SubterfugeCore.Core.Components;
using SubterfugeRemakeService;

namespace SubterfugeCore.Core.Entities.Specialists.Effects
{
    /// <summary>
    /// An abstract class for implementing a specialist effect
    /// </summary>
    public abstract class SpecialistEffect : ISpecialistEffect
    {
        public SpecialistEffectConfiguration configuration;

        /// <summary>
        /// Applies the event's forward action
        /// </summary>
        /// <param name="state">The game state to get the effects for.</param>
        /// <param name="friendly">The friendly participant. Null if none.</param>
        /// <param name="enemy">The enemy participant. Null if none.</param>
        public abstract List<EffectDelta> GetForwardEffectDeltas(GameState.GameState state, Entity friendly, Entity enemy);
        
        /// <summary>
        /// Applies the event's backwards action
        /// </summary>
        /// <param name="state">The game state to get the effects for.</param>
        /// <param name="friendly">The friendly participant. Null if none.</param>
        /// <param name="enemy">The enemy participant. Null if none.</param>
        public abstract List<EffectDelta> GetBackwardEffectDeltas(GameState.GameState state, Entity friendly, Entity enemy);

        protected List<IEntity> GetEffectTargets(GameState.GameState state, Entity friendly, Entity enemy)
        {
            List<IEntity> targets = new List<IEntity>();
            
            // Filter based on the trigger range first
            switch (configuration.EffectTarget)
            {
                case EffectTarget.Friendly:
                    targets.Add(friendly);
                    break;
                case EffectTarget.Enemy:
                    targets.Add(enemy);
                    break;
                case EffectTarget.All:
                    // Check if the effect is an Aoe Effect. If it is, we need to get all friendly/enemy subs in the AoE
                    targets.Add(friendly);
                    targets.Add(enemy);
                    // TODO: get all ICombatable within range of friendly.
                    break;
                case EffectTarget.NoTarget:
                    break;
            }

            return targets;
        }
    }
}